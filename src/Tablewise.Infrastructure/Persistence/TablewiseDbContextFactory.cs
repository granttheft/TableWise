using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Services;

namespace Tablewise.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory. EF Core migrations için gerekli.
/// </summary>
public class TablewiseDbContextFactory : IDesignTimeDbContextFactory<TablewiseDbContext>
{
    /// <summary>
    /// DbContext instance oluşturur (design-time).
    /// </summary>
    public TablewiseDbContext CreateDbContext(string[] args)
    {
        // Configuration
        var basePath = Directory.GetCurrentDirectory();
        
        // Eğer Infrastructure klasöründeysek, Api klasörüne git
        if (basePath.Contains("Infrastructure"))
        {
            basePath = Path.Combine(basePath, "..", "Tablewise.Api");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<TablewiseDbContext>();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=tablewise;Username=tablewise_user;Password=dev_password";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(TablewiseDbContext).Assembly.FullName);
        });

        // Design-time services
        ITenantContext tenantContext = new DesignTimeTenantContext();
        ICurrentUser currentUser = new DesignTimeCurrentUser();

        return new TablewiseDbContext(optionsBuilder.Options, tenantContext, currentUser);
    }
}
