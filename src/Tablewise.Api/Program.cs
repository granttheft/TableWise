using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sentry;
using Serilog;
using Serilog.Events;
using Tablewise.Api.Middleware;
using Tablewise.Application.Validators.Auth;
using Tablewise.Infrastructure;
using Tablewise.Infrastructure.Auth;
using Tablewise.Infrastructure.Persistence;
using Tablewise.Infrastructure.Persistence.SeedData;

// Serilog early initialization
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Tablewise API başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog configuration
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("Application", "Tablewise.Api")
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        // Production'da Seq/Elasticsearch'e yaz
        .WriteTo.Conditional(
            _ => !context.HostingEnvironment.IsDevelopment(),
            wt => wt.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341")));

    // Sentry configuration
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = builder.Configuration["Sentry:Dsn"];
        options.Environment = builder.Environment.EnvironmentName;
        options.TracesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.2;
        options.ProfilesSampleRate = 0.1;
        options.SendDefaultPii = false; // KVKK/GDPR - PII gönderme
    });

    // ===== SERVICES =====

    // Infrastructure (DbContext, Repositories, Redis, R2, Context Services)
    builder.Services.AddInfrastructure(builder.Configuration);

    // JWT Settings
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
    builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(AuthSettings.SectionName));

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
        ?? throw new InvalidOperationException("JWT ayarları bulunamadı.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("X-Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // MediatR
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Tablewise.Application.Interfaces.IAuthService).Assembly);
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterTenantDtoValidator>();
    builder.Services.AddFluentValidationAutoValidation();

    // Controllers
    builder.Services.AddControllers();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["https://tablewise.com.tr", "https://app.tablewise.com.tr"];

            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("X-Token-Expired", "X-Correlation-Id");
        });
    });

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Auth endpoint'leri: 10 req/dakika (IP bazlı)
        options.AddPolicy("auth", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIpAddress(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        // Reserve endpoint: 5 req/dakika (IP bazlı)
        options.AddPolicy("reserve", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIpAddress(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        // Booking endpoint'leri: 30 req/dakika (IP bazlı)
        options.AddPolicy("booking", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIpAddress(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2
                }));

        // Authenticated: 200 req/dakika (user bazlı)
        options.AddPolicy("authenticated", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User?.Identity?.Name ?? GetClientIpAddress(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 200,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                }));

        // Staff invite: 10 req/saat (tenant bazlı spam önleme)
        options.AddPolicy("staff-invite", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.FindFirst("tenant_id")?.Value ?? GetClientIpAddress(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromHours(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        // Accept invite: 5 req/saat (token bazlı brute-force önleme)
        options.AddPolicy("accept-invite", httpContext =>
        {
            var token = httpContext.Request.RouteValues["token"]?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"accept:{token}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromHours(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        });

        // Global fallback
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIpAddress(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                }));

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc6585#section-4",
                title = "Çok fazla istek",
                status = 429,
                detail = "İstek limiti aşıldı. Lütfen biraz bekleyip tekrar deneyin."
            }, cancellationToken);
        };
    });

    // Exception Handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql")
        .AddRedis(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379", name: "redis");

    var app = builder.Build();

    // ===== MIDDLEWARE PIPELINE (SIRALAMA KRİTİK!) =====

    // 1. Exception Handler (en başta - tüm hataları yakalar)
    app.UseExceptionHandler();

    // 2. Serilog Request Logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());

            // PII loglanmaz - sadece ID'ler
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                diagnosticContext.Set("TenantId", httpContext.User.FindFirst("tenant_id")?.Value);
            }
        };

        // Belirli path'leri loglama
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (httpContext.Request.Path.StartsWithSegments("/health") ||
                httpContext.Request.Path.StartsWithSegments("/healthz") ||
                httpContext.Request.Path.StartsWithSegments("/metrics"))
            {
                return LogEventLevel.Verbose;
            }

            return ex != null ? LogEventLevel.Error :
                   elapsed > 1000 ? LogEventLevel.Warning :
                   LogEventLevel.Information;
        };
    });

    // 3. HTTPS Redirection
    app.UseHttpsRedirection();

    // 4. CORS
    app.UseCors();

    // 5. Rate Limiting
    app.UseRateLimiter();

    // 6. Authentication
    app.UseAuthentication();

    // 7. Authorization
    app.UseAuthorization();

    // 8. Tenant Resolver (JWT'den veya slug'dan tenant çözer)
    app.UseMiddleware<TenantResolverMiddleware>();

    // 9. İleride: IdempotencyMiddleware (POST /reserve için)
    // app.UseMiddleware<IdempotencyMiddleware>();

    // Development ortamında ek ayarlar
    if (app.Environment.IsDevelopment())
    {
        // Seed data
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<TablewiseDbContext>();
            var logger = services.GetRequiredService<ILogger<DbSeeder>>();

            await context.Database.MigrateAsync();

            var seeder = new DbSeeder(context, logger);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Seed data işlemi sırasında hata oluştu.");
        }
    }

    // 10. Controllers
    app.MapControllers();

    // 11. Health Checks
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/healthz");
    app.MapHealthChecks("/ready");

    Log.Information("Tablewise API başlatıldı. Environment: {Environment}", app.Environment.EnvironmentName);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılamadı!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Local helper function - İstemci IP adresini alır
static string GetClientIpAddress(HttpContext httpContext)
{
    var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrEmpty(forwardedFor))
    {
        return forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "unknown";
    }

    var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
    if (!string.IsNullOrEmpty(realIp))
    {
        return realIp;
    }

    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

// Integration tests için partial class
public partial class Program { }
