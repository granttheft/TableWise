using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;
using Tablewise.IntegrationTests.Fixtures;

namespace Tablewise.IntegrationTests.Features.Security;

/// <summary>
/// Multi-tenant izolasyon testleri. Global Query Filter'ın (TenantScopedEntity)
/// ve soft delete filtresinin gerçekten uygulandığını doğrular.
///
/// Risk: Güvenlik fazında bir handler'a yanlışlıkla IgnoreQueryFilters() eklenirse
/// bir tenant başka bir tenant'ın verisini görebilir. Bu testler o regresyonu yakalar.
/// </summary>
[Collection("Database")]
public sealed class TenantIsolationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TenantIsolationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Tenant context set edilmiş bir scope + DbContext döndürür.
    /// ITenantContext scoped olduğu için aynı scope'taki DbContext bu tenant'ı kullanır.
    /// tenantId null ise filtre uygulanmaz (kurulum/seed amaçlı).
    /// </summary>
    private (IServiceScope Scope, TablewiseDbContext Db) CreateScopedDb(Guid? tenantId)
    {
        var scope = _factory.Services.CreateScope();
        if (tenantId.HasValue)
        {
            scope.ServiceProvider.GetRequiredService<ITenantContext>().SetTenant(tenantId.Value);
        }
        var db = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();
        return (scope, db);
    }

    private static Tenant NewTenant(Guid planId, string suffix, PlanStatus status = PlanStatus.Active, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Name = $"Isolation Tenant {suffix}",
        Slug = $"iso-{suffix}-{Guid.NewGuid():N}",
        Email = $"iso-{suffix}-{Guid.NewGuid():N}@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
        PlanId = planId,
        PlanStatus = status,
        IsActive = isActive,
        IsEmailVerified = true
    };

    private static Venue NewVenue(Guid tenantId, string name) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Name = name,
        TimeZone = "Europe/Istanbul",
        OpeningTime = new TimeSpan(10, 0, 0),
        ClosingTime = new TimeSpan(23, 0, 0),
        SlotDurationMinutes = 90
    };

    private async Task<Guid> GetSeededPlanIdAsync()
    {
        var (scope, db) = CreateScopedDb(null);
        using (scope)
        {
            return (await db.Plans.FirstAsync()).Id;
        }
    }

    [Fact]
    public async Task TenantA_CannotSee_TenantB_Venues()
    {
        // Arrange
        var planId = await GetSeededPlanIdAsync();
        Tenant tenantA, tenantB;
        var (setupScope, setupDb) = CreateScopedDb(null);
        using (setupScope)
        {
            tenantA = NewTenant(planId, "a");
            tenantB = NewTenant(planId, "b");
            setupDb.Tenants.AddRange(tenantA, tenantB);
            setupDb.Venues.Add(NewVenue(tenantA.Id, "Venue A"));
            setupDb.Venues.Add(NewVenue(tenantB.Id, "Venue B"));
            await setupDb.SaveChangesAsync();
        }

        // Act — Tenant A context ile sorgula
        var (scopeA, dbA) = CreateScopedDb(tenantA.Id);
        using (scopeA)
        {
            var venues = await dbA.Venues.ToListAsync();

            // Assert
            Assert.Single(venues);
            Assert.Equal("Venue A", venues[0].Name);
            Assert.DoesNotContain(venues, v => v.Name == "Venue B");
        }
    }

    [Fact]
    public async Task TenantA_CannotSee_TenantB_Reservation()
    {
        // Arrange
        var planId = await GetSeededPlanIdAsync();
        Tenant tenantA, tenantB;
        Guid reservationBId;
        var (setupScope, setupDb) = CreateScopedDb(null);
        using (setupScope)
        {
            tenantA = NewTenant(planId, "a");
            tenantB = NewTenant(planId, "b");
            setupDb.Tenants.AddRange(tenantA, tenantB);

            var venueB = NewVenue(tenantB.Id, "Venue B");
            setupDb.Venues.Add(venueB);

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                VenueId = venueB.Id,
                GuestName = "Tenant B Guest",
                GuestPhone = "+905550000000",
                PartySize = 2,
                ReservedFor = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(90),
                Status = ReservationStatus.Pending,
                Source = ReservationSource.BookingUI,
                ConfirmCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
            };
            setupDb.Reservations.Add(reservation);
            await setupDb.SaveChangesAsync();
            reservationBId = reservation.Id;
        }

        // Act — Tenant A context ile Tenant B'nin rezervasyonunu bulmaya çalış
        var (scopeA, dbA) = CreateScopedDb(tenantA.Id);
        using (scopeA)
        {
            var found = await dbA.Reservations.FirstOrDefaultAsync(r => r.Id == reservationBId);

            // Assert
            Assert.Null(found);
        }
    }

    [Fact]
    public async Task CustomLimits_AppliedToCorrectTenant()
    {
        // Arrange
        var planId = await GetSeededPlanIdAsync();
        Tenant tenantA, tenantB;
        var (setupScope, setupDb) = CreateScopedDb(null);
        using (setupScope)
        {
            tenantA = NewTenant(planId, "a");
            tenantB = NewTenant(planId, "b");
            tenantA.CustomLimitsJson = """{"maxVenues": 99}""";
            setupDb.Tenants.AddRange(tenantA, tenantB);
            await setupDb.SaveChangesAsync();
        }

        // Act — Tenant kayıtlarını yeniden oku (Tenant tenant-scoped değil, Id ile erişilir)
        var (scope, db) = CreateScopedDb(null);
        using (scope)
        {
            var a = await db.Tenants.FirstAsync(t => t.Id == tenantA.Id);
            var b = await db.Tenants.FirstAsync(t => t.Id == tenantB.Id);

            // Assert
            Assert.NotNull(a.CustomLimitsJson);
            Assert.Contains("99", a.CustomLimitsJson!);
            Assert.True(string.IsNullOrEmpty(b.CustomLimitsJson), "Tenant B'nin custom limiti olmamalı");
        }
    }

    [Fact]
    public async Task SoftDeleted_Venue_NotVisible()
    {
        // Arrange
        var planId = await GetSeededPlanIdAsync();
        Tenant tenantA;
        Guid venueId;
        var (setupScope, setupDb) = CreateScopedDb(null);
        using (setupScope)
        {
            tenantA = NewTenant(planId, "a");
            setupDb.Tenants.Add(tenantA);

            var venue = NewVenue(tenantA.Id, "Soft Deleted Venue");
            setupDb.Venues.Add(venue);
            await setupDb.SaveChangesAsync();

            venue.IsDeleted = true;
            venue.DeletedAt = DateTime.UtcNow;
            await setupDb.SaveChangesAsync();
            venueId = venue.Id;
        }

        // Act
        var (scopeA, dbA) = CreateScopedDb(tenantA.Id);
        using (scopeA)
        {
            var venues = await dbA.Venues.ToListAsync();

            // Assert
            Assert.DoesNotContain(venues, v => v.Id == venueId);
        }
    }

    [Fact]
    public async Task SuspendedTenant_PublicBooking_Returns403()
    {
        // Arrange — Suspended (ama IsActive=true) tenant + venue oluştur
        var planId = await GetSeededPlanIdAsync();
        Tenant suspended;
        var (setupScope, setupDb) = CreateScopedDb(null);
        using (setupScope)
        {
            suspended = NewTenant(planId, "suspended", PlanStatus.Suspended);
            setupDb.Tenants.Add(suspended);
            setupDb.Venues.Add(NewVenue(suspended.Id, "Suspended Venue"));
            await setupDb.SaveChangesAsync();
        }

        // Act — Slug ile public booking config endpoint'i (TenantResolverMiddleware suspend kontrolü yapar)
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/book/{suspended.Slug}/config");

        // Assert — Suspended tenant 403 Forbidden almalı
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task InactiveTenant_PublicBooking_Returns404()
    {
        // Arrange — IsActive=false tenant; slug sorgusu (IsActive filtresi) onu bulamaz → 404
        var planId = await GetSeededPlanIdAsync();
        Tenant inactive;
        var (setupScope, setupDb) = CreateScopedDb(null);
        using (setupScope)
        {
            inactive = NewTenant(planId, "inactive", PlanStatus.Active, isActive: false);
            setupDb.Tenants.Add(inactive);
            setupDb.Venues.Add(NewVenue(inactive.Id, "Inactive Venue"));
            await setupDb.SaveChangesAsync();
        }

        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/book/{inactive.Slug}/config");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
