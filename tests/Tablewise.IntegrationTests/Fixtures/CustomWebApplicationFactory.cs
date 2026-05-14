using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Tablewise.Application.Interfaces;
using Tablewise.Application.RuleEngine;
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

    /// <summary>
    /// Eşzamanlı rezervasyon testleri için venue'deki fazla masaları kaldırır; tek masa kalır.
    /// </summary>
    public async Task RestrictTestVenueToSingleTableAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        await dbContext.Reservations
            .IgnoreQueryFilters()
            .Where(r => r.VenueId == TestVenueId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        await dbContext.TableCombinations
            .IgnoreQueryFilters()
            .Where(tc => tc.VenueId == TestVenueId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        await dbContext.Tables
            .IgnoreQueryFilters()
            .Where(t => t.VenueId == TestVenueId && t.Id != TestTableId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        // Diğer testler kural motorunda kural bırakmış olabilir; public reserve senaryolarını izole et.
        var rules = await dbContext.Rules
            .IgnoreQueryFilters()
            .Where(r => r.VenueId == TestVenueId)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var rule in rules)
        {
            rule.IsActive = false;
            if (string.Equals(rule.RuleType, "peak_hour", StringComparison.Ordinal))
            {
                rule.ActionsJson = """{"version":1,"block":false,"warn":true,"message":"Yoğun saat"}""";
            }
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        await RuleEngineRulesCacheInvalidation
            .InvalidateForTenantAsync(cache, TestTenantId)
            .ConfigureAwait(false);
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

        var seedTime = DateTime.UtcNow;

        dbContext.Customers.Add(new Customer
        {
            Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"),
            TenantId = TestTenantId,
            FullName = "Test VIP Seed",
            Phone = "+905551001001",
            Email = "vip@example.com",
            Tier = CustomerTier.VIP,
            TotalVisits = 0,
            CreatedAt = seedTime
        });

        dbContext.Users.Add(new User
        {
            Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"),
            TenantId = TestTenantId,
            Email = "staff@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
            FirstName = "Staff",
            LastName = "User",
            Role = UserRole.Staff,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = seedTime
        });

        dbContext.Rules.AddRange(
            new Rule
            {
                Id = Guid.Parse("e0000001-0000-0000-0000-000000000001"),
                TenantId = TestTenantId,
                VenueId = TestVenueId,
                Name = "Erken Rezervasyon",
                RuleType = "early_booking",
                TriggerType = RuleTrigger.OnReservationCreate,
                Priority = 10,
                IsActive = false,
                ConditionsJson = """{"version":1,"minDaysInAdvance":7}""",
                ActionsJson = """{"version":1,"discountPercent":10,"label":"Erken rezervasyon indirimi"}""",
                CreatedAt = seedTime
            },
            new Rule
            {
                Id = Guid.Parse("e0000002-0000-0000-0000-000000000002"),
                TenantId = TestTenantId,
                VenueId = TestVenueId,
                Name = "Yoğun Saat",
                RuleType = "peak_hour",
                TriggerType = RuleTrigger.OnReservationCreate,
                Priority = 5,
                IsActive = false,
                ConditionsJson = """{"version":1,"startTime":"19:00","endTime":"22:00"}""",
                ActionsJson = """{"version":1,"block":false,"warn":true,"message":"Yoğun saat"}""",
                CreatedAt = seedTime
            },
            new Rule
            {
                Id = Guid.Parse("e0000003-0000-0000-0000-000000000003"),
                TenantId = TestTenantId,
                VenueId = TestVenueId,
                Name = "Grup Kompozisyonu",
                RuleType = "group_composition",
                TriggerType = RuleTrigger.OnReservationCreate,
                Priority = 2,
                IsActive = false,
                ConditionsJson = """
                {
                    "version": 1,
                    "operator": "and",
                    "rules": [
                        { "type": "composition", "blockedCompositions": ["AllMale"] },
                        { "type": "ratio", "minPartySize": 4 }
                    ]
                }
                """,
                ActionsJson = """
                {
                    "version": 1,
                    "block": true,
                    "message": "4 ve üzeri kişilik sadece erkek gruplar için rezervasyon kabul edilmemektedir."
                }
                """,
                CreatedAt = seedTime
            },
            new Rule
            {
                Id = Guid.Parse("e0000004-0000-0000-0000-000000000004"),
                TenantId = TestTenantId,
                VenueId = TestVenueId,
                Name = "VIP Öncelik",
                RuleType = "vip_priority",
                TriggerType = RuleTrigger.OnReservationCreate,
                Priority = 20,
                IsActive = false,
                ConditionsJson = """{"version":1,"minTier":"Gold"}""",
                ActionsJson = """{"version":1,"suggestBestTable":true}""",
                CreatedAt = seedTime
            });

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
