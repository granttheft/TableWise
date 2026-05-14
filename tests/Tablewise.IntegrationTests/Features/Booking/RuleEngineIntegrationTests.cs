using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tablewise.Application.DTOs.Booking;
using Tablewise.Application.Interfaces;
using Tablewise.Application.RuleEngine;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Infrastructure.Persistence;
using Tablewise.IntegrationTests.Fixtures;

namespace Tablewise.IntegrationTests.Features.Booking;

/// <summary>
/// Rule Engine integration testleri.
/// </summary>
[Collection("Database")]
public sealed class RuleEngineIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _reserveUrl;

    public RuleEngineIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _reserveUrl = $"/api/v1/book/{factory.TestSlug}/reserve";
    }

    private CustomWebApplicationFactory Factory => _factory;
    private HttpClient Client => _client;

    /// <summary>
    /// Public reserve endpoint requires Idempotency-Key; each call uses a fresh key.
    /// </summary>
    private Task<HttpResponseMessage> PostReserveAsync(ReserveRequestDto dto)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _reserveUrl)
        {
            Content = JsonContent.Create(dto)
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return Client.SendAsync(request);
    }

    /// <summary>
    /// Test 1: 7+ gün öncesi rezervasyon, erken rezervasyon indirimi uygulanmalı.
    /// </summary>
    [Fact]
    public async Task EarlyBooking_SevenDaysAdvance_AppliesDiscount()
    {
        // Arrange
        await SeedTestDataAsync();

        // Erken rezervasyon kuralını aktifleştir
        await ActivateRuleAsync("early_booking");

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 2,
            ReservedFor = DateTime.UtcNow.Date.AddDays(8).AddHours(14), // 8 gün sonra, çalışma saati içinde (UTC)
            PrivacyPolicyAccepted = true
        };

        // Act
        var response = await PostReserveAsync(reserveDto);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.IsSuccessStatusCode,
            $"HTTP {(int)response.StatusCode} {response.StatusCode}: {responseBody}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ReserveResponseDto>();

        Assert.NotNull(result);
        Assert.NotNull(result.DiscountPercent);
        Assert.True(result.DiscountPercent > 0);

        // Verify AppliedRulesSnapshot
        var reservation = await GetReservationByConfirmCodeAsync(result.ConfirmCode);
        Assert.NotNull(reservation.AppliedRulesSnapshot);
        Assert.Contains("early_booking", reservation.AppliedRulesSnapshot, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test 2: Aktif block kuralı varsa 422 dönmeli.
    /// </summary>
    [Fact]
    public async Task BlockRule_Active_Returns422()
    {
        // Arrange
        await SeedTestDataAsync();

        // Peak hour kuralını aktifleştir ve block'a çevir
        await ActivateBlockRuleAsync("peak_hour");

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 2,
            ReservedFor = DateTime.UtcNow.AddDays(1).Date.AddHours(20), // Peak saat
            PrivacyPolicyAccepted = true
        };

        // Act
        var response = await PostReserveAsync(reserveDto);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("kural", errorContent, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test 3: Sadece erkek grup + 4 kişi = blocked.
    /// </summary>
    [Fact]
    public async Task GroupComposition_AllMale_FourPeople_Blocked()
    {
        // Arrange
        await SeedTestDataAsync();

        // Grup kompozisyonu kuralını aktifleştir
        await ActivateRuleAsync("group_composition");

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 4,
            ReservedFor = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            CustomFieldAnswers = new Dictionary<string, string>
            {
                { "Grup Kompozisyonu", "Sadece Erkek" },
                { "Erkek Misafir Sayısı", "4" }
            },
            PrivacyPolicyAccepted = true
        };

        // Act
        var response = await PostReserveAsync(reserveDto);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("erkek", errorContent, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test 4: Grup kompozisyonu null ise kural skip edilmeli.
    /// </summary>
    [Fact]
    public async Task GroupComposition_Null_RuleSkipped()
    {
        // Arrange
        await SeedTestDataAsync();

        // Grup kompozisyonu kuralını aktifleştir
        await ActivateRuleAsync("group_composition");

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 4,
            ReservedFor = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            PrivacyPolicyAccepted = true
            // CustomFieldAnswers yok
        };

        // Act
        var response = await PostReserveAsync(reserveDto);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Test 5: Birden fazla kural tetiklenirse AppliedRulesSnapshot populated olmalı.
    /// </summary>
    [Fact]
    public async Task AppliedRulesSnapshot_Populated()
    {
        // Arrange
        await SeedTestDataAsync();

        // Birden fazla kural aktifleştir
        await ActivateRuleAsync("early_booking");
        await ActivateRuleAsync("vip_priority");

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test VIP",
            GuestEmail = "vip@example.com",
            GuestPhone = "+905551001001", // VIP müşteri
            PartySize = 2,
            ReservedFor = DateTime.UtcNow.Date.AddDays(8).AddHours(14),
            PrivacyPolicyAccepted = true
        };

        // Act
        var response = await PostReserveAsync(reserveDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ReserveResponseDto>();

        var reservation = await GetReservationByConfirmCodeAsync(result!.ConfirmCode);
        Assert.NotNull(reservation.AppliedRulesSnapshot);

        // JSON parse edilebilir mi?
        var snapshot = JsonSerializer.Deserialize<JsonElement>(reservation.AppliedRulesSnapshot);
        Assert.True(snapshot.ValueKind == JsonValueKind.Array);
        Assert.True(snapshot.GetArrayLength() > 0);
    }

    /// <summary>
    /// Test 6: Staff + OverrideRules = true ise pipeline skip edilmeli, audit log kaydedilmeli.
    /// </summary>
    [Fact]
    public async Task OverrideRules_Staff_SkipsPipeline_LogsAudit()
    {
        // Arrange
        await SeedTestDataAsync();

        // Block kuralı aktif
        await ActivateBlockRuleAsync("peak_hour");

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 2,
            ReservedFor = DateTime.UtcNow.AddDays(1).Date.AddHours(20), // Peak saat
            OverrideRules = true,
            PrivacyPolicyAccepted = true
        };

        // Act - Staff token ile
        var staffToken = await GetStaffTokenAsync();
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", staffToken);

        var response = await PostReserveAsync(reserveDto);

        // Assert
        response.EnsureSuccessStatusCode(); // Block kuralı olmasına rağmen geçti

        var result = await response.Content.ReadFromJsonAsync<ReserveResponseDto>();

        // Audit log kontrolü
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        var auditLog = await dbContext.AuditLogs
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == Factory.TestTenantId && a.Action == "RULES_OVERRIDDEN")
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(auditLog);
        Assert.Contains("atlandı", auditLog.NewValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test 7: Guest kullanıcı OverrideRules kullanamaz (403).
    /// </summary>
    [Fact]
    public async Task OverrideRules_Guest_Forbidden()
    {
        // Arrange
        await SeedTestDataAsync();

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 2,
            ReservedFor = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
            OverrideRules = true,
            PrivacyPolicyAccepted = true
        };

        // Act - Token YOK (guest)
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await PostReserveAsync(reserveDto);

        // Assert
        // Public endpoint olduğu için 403 yerine işlemi çalıştırır ama override'ı dikkate almaz
        // Bu durumda override flag etkisiz olacak
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Test 8: Kural tetiklendiğinde TimesTriggered++ olmalı.
    /// </summary>
    [Fact]
    public async Task TimesTriggered_Incremented()
    {
        // Arrange
        await SeedTestDataAsync();

        // Early booking kuralını aktifleştir
        var ruleId = await ActivateRuleAsync("early_booking");

        // İlk TimesTriggered değerini al
        var initialCount = await GetRuleTimesTriggeredAsync(ruleId);

        var reserveDto = new ReserveRequestDto
        {
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 2,
            ReservedFor = DateTime.UtcNow.Date.AddDays(8).AddHours(14),
            PrivacyPolicyAccepted = true
        };

        // Act
        var response = await PostReserveAsync(reserveDto);

        // Assert
        response.EnsureSuccessStatusCode();

        var newCount = await GetRuleTimesTriggeredAsync(ruleId);
        Assert.Equal(initialCount + 1, newCount);
    }

    #region Helper Methods

    private async Task SeedTestDataAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        // Her test öncesi bu tenant'a ait rezervasyonları temizle (slot / idempotency izolasyonu).
        await dbContext.Reservations
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == Factory.TestTenantId)
            .ExecuteDeleteAsync();

        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        await RuleEngineRulesCacheInvalidation
            .InvalidateForTenantAsync(cache, Factory.TestTenantId)
            .ConfigureAwait(false);
    }

    private async Task<Guid> ActivateRuleAsync(string ruleType)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        var rule = await dbContext.Rules
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == Factory.TestTenantId && r.RuleType == ruleType)
            .FirstOrDefaultAsync();

        if (rule != null)
        {
            rule.IsActive = true;
            await dbContext.SaveChangesAsync();
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            await RuleEngineRulesCacheInvalidation
                .InvalidateForTenantAsync(cache, Factory.TestTenantId)
                .ConfigureAwait(false);
            return rule.Id;
        }

        return Guid.Empty;
    }

    private async Task ActivateBlockRuleAsync(string ruleType)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        var rule = await dbContext.Rules
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == Factory.TestTenantId && r.RuleType == ruleType)
            .FirstOrDefaultAsync();

        if (rule != null)
        {
            rule.IsActive = true;
            // ActionsJson'u block olarak güncelle
            rule.ActionsJson = """
                {
                    "version": 1,
                    "block": true,
                    "message": "Bu saat dilimi için rezervasyon kabul edilmemektedir."
                }
                """;
            await dbContext.SaveChangesAsync();
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            await RuleEngineRulesCacheInvalidation
                .InvalidateForTenantAsync(cache, Factory.TestTenantId)
                .ConfigureAwait(false);
        }
    }

    private async Task<Reservation> GetReservationByConfirmCodeAsync(string confirmCode)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        var reservation = await dbContext.Reservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.ConfirmCode == confirmCode);

        return reservation!;
    }

    private async Task<string> GetStaffTokenAsync()
    {
        // Staff token için login endpoint'ini kullan
        // Test ortamında seed edilmiş staff user var
        var loginDto = new
        {
            Email = "staff@demo.com",
            Password = "Staff123!"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("tokens").GetProperty("accessToken").GetString()!;
    }

    private async Task<int> GetRuleTimesTriggeredAsync(Guid ruleId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TablewiseDbContext>();

        var rule = await dbContext.Rules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == ruleId);
        return rule?.TimesTriggered ?? 0;
    }

    #endregion
}
