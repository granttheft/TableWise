using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Tablewise.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory ile test ortamı.
/// Testcontainers kullanarak PostgreSQL ve Redis container'ları başlatır.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;

    /// <summary>
    /// Test tenant ID.
    /// </summary>
    public Guid TestTenantId { get; private set; }

    /// <summary>
    /// Test venue ID.
    /// </summary>
    public Guid TestVenueId { get; private set; }

    /// <summary>
    /// Test table ID.
    /// </summary>
    public Guid TestTableId { get; private set; }

    /// <summary>
    /// Test venue slug.
    /// </summary>
    public string TestSlug { get; } = "test-venue";

    /// <summary>
    /// CustomWebApplicationFactory constructor.
    /// </summary>
    public CustomWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("tablewise_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // PostgreSQL bağlantısını test container'ına yönlendir
            services.RemoveAll<DbContextOptions<TablewiseDbContext>>();
            services.RemoveAll<TablewiseDbContext>();

            services.AddDbContext<TablewiseDbContext>((sp, options) =>
            {
                var tenantContext = sp.GetRequiredService<ITenantContext>();
                var currentUser = sp.GetRequiredService<ICurrentUser>();

                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Redis bağlantısını test container'ına yönlendir
            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));
        });

        builder.UseEnvironment("Testing");
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync().ConfigureAwait(false);
        await _redisContainer.StartAsync().ConfigureAwait(false);

        // Veritabanını oluştur ve seed et
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);

        // Test verileri ekle
        await SeedTestDataAsync(dbContext).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync().ConfigureAwait(false);
        await _redisContainer.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Test verileri ekler.
    /// </summary>
    private async Task SeedTestDataAsync(TablewiseDbContext dbContext)
    {
        // Plan oluştur
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Test Plan",
            Tier = PlanTier.Pro,
            MonthlyPriceTry = 990,
            YearlyPriceTry = 9900,
            FeaturesJson = """{"smsEnabled": true, "depositEnabled": true}""",
            LimitsJson = """{"maxVenues": 10, "maxTables": 100, "maxRules": 100, "maxReservationsPerMonth": 10000}""",
            IsVisible = true
        };
        dbContext.Plans.Add(plan);

        // Tenant oluştur
        TestTenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = TestTenantId,
            Name = "Test Business",
            Slug = TestSlug,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            PlanId = plan.Id,
            PlanStatus = PlanStatus.Active,
            IsEmailVerified = true,
            IsActive = true
        };
        dbContext.Tenants.Add(tenant);

        // Venue oluştur
        TestVenueId = Guid.NewGuid();
        var venue = new Venue
        {
            Id = TestVenueId,
            TenantId = TestTenantId,
            Name = "Test Restaurant",
            TimeZone = "Europe/Istanbul",
            OpeningTime = new TimeSpan(10, 0, 0),
            ClosingTime = new TimeSpan(23, 0, 0),
            SlotDurationMinutes = 90,
            WorkingHours = """{"Monday":{"open":"10:00","close":"23:00"},"Tuesday":{"open":"10:00","close":"23:00"},"Wednesday":{"open":"10:00","close":"23:00"},"Thursday":{"open":"10:00","close":"23:00"},"Friday":{"open":"10:00","close":"23:00"},"Saturday":{"open":"10:00","close":"23:00"},"Sunday":{"open":"10:00","close":"23:00"}}"""
        };
        dbContext.Venues.Add(venue);

        // Table oluştur
        TestTableId = Guid.NewGuid();
        var table = new Table
        {
            Id = TestTableId,
            TenantId = TestTenantId,
            VenueId = TestVenueId,
            Name = "Masa 1",
            Capacity = 4,
            Location = TableLocation.Indoor,
            IsActive = true,
            SortOrder = 1
        };
        dbContext.Tables.Add(table);

        // Birkaç masa daha ekle
        for (var i = 2; i <= 5; i++)
        {
            dbContext.Tables.Add(new Table
            {
                Id = Guid.NewGuid(),
                TenantId = TestTenantId,
                VenueId = TestVenueId,
                Name = $"Masa {i}",
                Capacity = 4,
                Location = TableLocation.Indoor,
                IsActive = true,
                SortOrder = i
            });
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
