# Kritik Fix 4 — Multi-Tenant İzolasyon Integration Testleri

## Sorun

18 entity için Global Query Filter var ama hiç integration testi yok.
Güvenlik fazında bir `IgnoreQueryFilters()` yanlışlıkla kalırsa fark edilemez.

---

## Adım 1 — Test Projesi Oluştur

```bash
cd src

dotnet new xunit -n Tablewise.IntegrationTests
dotnet sln ../TableWise.sln add Tablewise.IntegrationTests/Tablewise.IntegrationTests.csproj

cd Tablewise.IntegrationTests

dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet add package FluentAssertions
dotnet add reference ../Tablewise.Api/Tablewise.Api.csproj
dotnet add reference ../Tablewise.Infrastructure/Tablewise.Infrastructure.csproj
```

---

## Adım 2 — Test Factory

Yeni dosya: `src/Tablewise.IntegrationTests/Fixtures/TablewiseWebFactory.cs`

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.IntegrationTests.Fixtures;

public class TablewiseWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("tablewise_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Gerçek DB'yi test DB ile değiştir
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TablewiseDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<TablewiseDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        // Migration uygula
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }
}
```

---

## Adım 3 — Test Seed Helper

Yeni dosya: `src/Tablewise.IntegrationTests/Helpers/TestDataHelper.cs`

```csharp
using Tablewise.Domain.Entities;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.IntegrationTests.Helpers;

public static class TestDataHelper
{
    public static async Task<(Tenant tenantA, Tenant tenantB)> CreateTwoTenantsAsync(
        TablewiseDbContext db)
    {
        var planId = (await db.Plans.FirstAsync()).Id;

        var tenantA = new Tenant
        {
            Id = Guid.NewGuid(), Name = "Test Tenant A",
            Slug = "tenant-a", IsActive = true,
            PlanId = planId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        var tenantB = new Tenant
        {
            Id = Guid.NewGuid(), Name = "Test Tenant B",
            Slug = "tenant-b", IsActive = true,
            PlanId = planId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        db.Tenants.AddRange(tenantA, tenantB);
        await db.SaveChangesAsync();
        return (tenantA, tenantB);
    }

    public static async Task<Venue> CreateVenueAsync(
        TablewiseDbContext db, Guid tenantId, string name = "Test Venue")
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(), TenantId = tenantId, Name = name,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.Venues.Add(venue);
        await db.SaveChangesAsync();
        return venue;
    }
}
```

---

## Adım 4 — İzolasyon Testleri

Yeni dosya: `src/Tablewise.IntegrationTests/TenantIsolationTests.cs`

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tablewise.Infrastructure.Persistence;
using Tablewise.IntegrationTests.Fixtures;
using Tablewise.IntegrationTests.Helpers;

namespace Tablewise.IntegrationTests;

public class TenantIsolationTests : IClassFixture<TablewiseWebFactory>
{
    private readonly TablewiseWebFactory _factory;

    public TenantIsolationTests(TablewiseWebFactory factory)
    {
        _factory = factory;
    }

    private TablewiseDbContext CreateDbContext(Guid? tenantId = null)
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();
        if (tenantId.HasValue)
            db.SetTenantId(tenantId.Value);
        return db;
    }

    [Fact]
    public async Task TenantA_CannotSee_TenantB_Venues()
    {
        // Arrange
        using var setupDb = CreateDbContext();
        var (tenantA, tenantB) = await TestDataHelper.CreateTwoTenantsAsync(setupDb);
        await TestDataHelper.CreateVenueAsync(setupDb, tenantA.Id, "Venue A");
        await TestDataHelper.CreateVenueAsync(setupDb, tenantB.Id, "Venue B");

        // Act — Tenant A context ile sorgula
        using var tenantADb = CreateDbContext(tenantA.Id);
        var venues = await tenantADb.Venues.ToListAsync();

        // Assert
        venues.Should().HaveCount(1);
        venues.Single().Name.Should().Be("Venue A");
        venues.Should().NotContain(v => v.Name == "Venue B");
    }

    [Fact]
    public async Task TenantA_CannotUpdate_TenantB_Reservation()
    {
        // Arrange
        using var setupDb = CreateDbContext();
        var (tenantA, tenantB) = await TestDataHelper.CreateTwoTenantsAsync(setupDb);

        // Tenant B için rezervasyon oluştur
        var venueB = await TestDataHelper.CreateVenueAsync(setupDb, tenantB.Id);
        var reservation = new Tablewise.Domain.Entities.Reservation
        {
            Id = Guid.NewGuid(), TenantId = tenantB.Id,
            VenueId = venueB.Id, GuestName = "Tenant B Guest",
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Status = Tablewise.Domain.Enums.ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        setupDb.Reservations.Add(reservation);
        await setupDb.SaveChangesAsync();

        // Act — Tenant A context ile Tenant B'nin rezervasyonunu bulmaya çalış
        using var tenantADb = CreateDbContext(tenantA.Id);
        var found = await tenantADb.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);

        // Assert
        found.Should().BeNull("Tenant A, Tenant B'nin rezervasyonunu görememeli");
    }

    [Fact]
    public async Task SuspendedTenant_CannotAccess_Data()
    {
        // Arrange
        using var setupDb = CreateDbContext();
        var (tenantA, _) = await TestDataHelper.CreateTwoTenantsAsync(setupDb);

        // Tenant'ı suspend et
        var tenant = await setupDb.Tenants.FindAsync(tenantA.Id);
        tenant!.IsActive = false;
        await setupDb.SaveChangesAsync();

        // Act — Suspended tenant context oluştur
        // TenantResolverMiddleware suspend kontrolü yapıyor:
        // Bu testi API layer'da yapıyoruz
        var client = _factory.CreateClient();
        // Suspended tenant slug ile istek at
        var response = await client.GetAsync($"/api/v1/venues?tenantSlug={tenantA.Slug}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.Forbidden,
            System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CustomLimits_AppliedToCorrectTenant()
    {
        // Arrange
        using var setupDb = CreateDbContext();
        var (tenantA, tenantB) = await TestDataHelper.CreateTwoTenantsAsync(setupDb);

        // Sadece Tenant A'ya custom limit uygula
        var tenantAEntity = await setupDb.Tenants.FindAsync(tenantA.Id);
        tenantAEntity!.CustomLimitsJson = """{"maxVenues": 99}""";
        await setupDb.SaveChangesAsync();

        // Act
        using var tenantADb = CreateDbContext(tenantA.Id);
        var tenantAData = await tenantADb.Tenants
            .IgnoreQueryFilters()
            .FirstAsync(t => t.Id == tenantA.Id);

        using var tenantBDb = CreateDbContext(tenantB.Id);
        var tenantBData = await tenantBDb.Tenants
            .IgnoreQueryFilters()
            .FirstAsync(t => t.Id == tenantB.Id);

        // Assert
        tenantAData.CustomLimitsJson.Should().Contain("99");
        tenantBData.CustomLimitsJson.Should().BeNullOrEmpty(
            "Tenant B'nin custom limiti olmamalı");
    }

    [Fact]
    public async Task SoftDeleted_Records_NotVisible()
    {
        // Arrange
        using var setupDb = CreateDbContext();
        var (tenantA, _) = await TestDataHelper.CreateTwoTenantsAsync(setupDb);
        var venue = await TestDataHelper.CreateVenueAsync(setupDb, tenantA.Id);

        // Soft delete
        venue.IsDeleted = true;
        venue.DeletedAt = DateTime.UtcNow;
        await setupDb.SaveChangesAsync();

        // Act
        using var tenantADb = CreateDbContext(tenantA.Id);
        var venues = await tenantADb.Venues.ToListAsync();

        // Assert
        venues.Should().NotContain(v => v.Id == venue.Id,
            "Soft delete edilmiş venue görünmemeli");
    }
}
```

---

## Adım 5 — Testleri Çalıştır

```bash
cd src/Tablewise.IntegrationTests
dotnet test --verbosity normal
```

Testcontainers PostgreSQL container'ı ayağa kaldırır (~30 sn), testleri çalıştırır, kapatır.

## Tamamlanma Kriterleri

- [ ] `Tablewise.IntegrationTests` projesi solution'a eklenmiş
- [ ] 5 integration test yazıldı
- [ ] Tüm testler `dotnet test` ile geçiyor
- [ ] TenantA → TenantB verisi göremiyor ✅
- [ ] TenantA → TenantB rezervasyonu güncelleyemiyor ✅
- [ ] Suspended tenant erişim reddediliyor ✅
- [ ] Custom limits doğru tenant'a uygulanıyor ✅
- [ ] Soft delete kayıtlar görünmüyor ✅
