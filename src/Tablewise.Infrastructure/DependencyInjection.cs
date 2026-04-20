using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;
using Tablewise.Infrastructure.Persistence.Interceptors;
using Tablewise.Infrastructure.Services;
using Tablewise.Infrastructure.Storage;

namespace Tablewise.Infrastructure;

/// <summary>
/// Infrastructure katmanı için DI extension metodları.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Infrastructure servislerini DI container'a ekler.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HttpContextAccessor (ITenantContext ve ICurrentUser için gerekli)
        services.AddHttpContextAccessor();

        // Context Services
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // Interceptors
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        // DbContext
        services.AddDbContext<TablewiseDbContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
            var softDeleteInterceptor = serviceProvider.GetRequiredService<SoftDeleteInterceptor>();

            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);

                    npgsqlOptions.MigrationsAssembly(typeof(TablewiseDbContext).Assembly.FullName);
                })
                .AddInterceptors(auditInterceptor, softDeleteInterceptor);

            // Development'ta detailed errors
            if (configuration.GetValue<bool>("EnableDetailedErrors"))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        // Repository Pattern & Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        AddR2FileStorage(services, configuration);

        return services;
    }

    /// <summary>
    /// Cloudflare R2 (S3 uyumlu) istemcisi ve dosya depolama servisini kaydeder.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Uygulama yapılandırması.</param>
    private static void AddR2FileStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<R2StorageOptions>(configuration.GetSection(R2StorageOptions.SectionName));

        services.AddSingleton<AmazonS3Client>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<R2StorageOptions>>().Value;
            var credentials = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
            var accountId = string.IsNullOrWhiteSpace(opts.AccountId) ? "000000000000" : opts.AccountId.Trim();
            var serviceUrl = $"https://{accountId}.r2.cloudflarestorage.com";

            var awsConfig = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            return new AmazonS3Client(credentials, awsConfig);
        });

        services.AddSingleton<IAmazonS3>(sp => sp.GetRequiredService<AmazonS3Client>());
        services.AddScoped<IFileStorageService, R2FileStorageService>();
    }
}
