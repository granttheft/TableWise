using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;
using Tablewise.Infrastructure.Persistence.Interceptors;
using Tablewise.Infrastructure.Services;

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

        return services;
    }
}
