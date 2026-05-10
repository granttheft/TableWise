using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// GroupCompositionRuleEvaluator birim testleri.
/// </summary>
public class GroupCompositionRuleEvaluatorTests
{
    private readonly Mock<ILogger<GroupCompositionRuleEvaluator>> _loggerMock;
    private readonly GroupCompositionRuleEvaluator _evaluator;

    public GroupCompositionRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<GroupCompositionRuleEvaluator>>();
        _evaluator = new GroupCompositionRuleEvaluator(_loggerMock.Object);
    }

    /// <summary>
    /// Test 1: AllMale 4+ kişilik grup engellenme senaryosu.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_AllMaleGroupWith4PlusPeople_ReturnsBlockOutcome()
    {
        // Arrange - Sadece erkek, 4+ kişi AND operatörü
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "and",
                "rules": [
                    { "type": "composition", "blockedCompositions": ["AllMale"] },
                    { "type": "ratio", "minPartySize": 4 }
                ]
            }
            """,
            actions: """{"version":1,"block":true,"message":"4+ kişilik sadece erkek gruplar kabul edilmemektedir."}"""
        );
        var context = CreateContext(
            partySize: 5,
            maleCount: 5,
            femaleCount: 0,
            groupComposition: "AllMale"
        );

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Block, result.ActionType);
        Assert.Equal("4+ kişilik sadece erkek gruplar kabul edilmemektedir.", result.Message);
    }

    /// <summary>
    /// Test 2: Mixed grup - kural tetiklenmemeli.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_MixedGroup_ReturnsNull()
    {
        // Arrange - Mixed grup, AllMale engeli var ama Mixed değil
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "rules": [
                    { "type": "composition", "blockedCompositions": ["AllMale"] }
                ]
            }
            """,
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var context = CreateContext(
            partySize: 6,
            maleCount: 3,
            femaleCount: 3,
            groupComposition: "Mixed"
        );

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test 3: GroupComposition null - veri yok, skip edilmeli.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_NoGroupData_ReturnsNull()
    {
        // Arrange - Hiç grup verisi yok
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "rules": [
                    { "type": "composition", "blockedCompositions": ["AllMale"] }
                ]
            }
            """,
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var context = CreateContextWithoutGroupData(partySize: 6);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test 4: FemaleRatio düşük (0.20), minPartySize 6 - uyarı senaryosu.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_LowFemaleRatioWith6PlusPeople_ReturnsWarnOutcome()
    {
        // Arrange - %20 kadın, 6+ kişi, minimum %30 kadın gerekli
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "rules": [
                    { "type": "ratio", "minFemaleRatio": 0.30, "minPartySize": 6 }
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Grubunuzda en az %30 kadın misafir bulunması önerilmektedir."}"""
        );
        var context = CreateContext(
            partySize: 10,
            maleCount: 8,
            femaleCount: 2, // %20 kadın
            groupComposition: "Mixed"
        );

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Warn, result.ActionType);
        Assert.Equal("Grubunuzda en az %30 kadın misafir bulunması önerilmektedir.", result.Message);
    }

    /// <summary>
    /// Test 5: FemaleRatio düşük ama partySize minPartySize altında - skip edilmeli.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_LowFemaleRatioButBelowMinPartySize_ReturnsNull()
    {
        // Arrange - %25 kadın, 4 kişi, ama minPartySize 6
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "rules": [
                    { "type": "ratio", "minFemaleRatio": 0.30, "minPartySize": 6 }
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Uyarı"}"""
        );
        var context = CreateContext(
            partySize: 4,
            maleCount: 3,
            femaleCount: 1, // %25 kadın ama 4 kişi
            groupComposition: "Mixed"
        );

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test 6: MaxMaleRatio aşımı - uyarı senaryosu.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_HighMaleRatio_ReturnsWarnOutcome()
    {
        // Arrange - %90 erkek, maksimum %80 erkek izinli
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "rules": [
                    { "type": "ratio", "maxMaleRatio": 0.80 }
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Erkek oranı çok yüksek"}"""
        );
        var context = CreateContext(
            partySize: 10,
            maleCount: 9,
            femaleCount: 1, // %90 erkek
            groupComposition: "Mixed"
        );

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Warn, result.ActionType);
    }

    /// <summary>
    /// Test 7: AND operatörü - sadece biri ihlal, diğeri değil - pass.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_AndOperatorWithPartialViolation_ReturnsNull()
    {
        // Arrange - AllMale değil ama 4+ kişi - AND ile ikisi de ihlal olmalı
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "and",
                "rules": [
                    { "type": "composition", "blockedCompositions": ["AllMale"] },
                    { "type": "ratio", "minPartySize": 4 }
                ]
            }
            """,
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var context = CreateContext(
            partySize: 5,
            maleCount: 3,
            femaleCount: 2,
            groupComposition: "Mixed" // AllMale değil - ilk kural ihlal edilmiyor
        );

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test 8: AllowedCompositions kontrolü - izin verilmeyen kompozisyon.
    /// </summary>
    [Fact]
    public async Task EvaluateAsync_CompositionNotInAllowed_ReturnsBlockOutcome()
    {
        // Arrange - Sadece Mixed ve Family izinli, AllMale değil
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "rules": [
                    { "type": "composition", "allowedCompositions": ["Mixed", "Family"] }
                ]
            }
            """,
            actions: """{"version":1,"block":true,"message":"Bu grup tipi kabul edilmiyor"}"""
        );
        var context = CreateContext(
            partySize: 4,
            maleCount: 4,
            femaleCount: 0,
            groupComposition: "AllMale"
        );

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Block, result.ActionType);
    }

    private static Rule CreateRule(string conditions, string actions)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Group Composition Rule",
            RuleType = "group_composition",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(
        int partySize,
        int maleCount,
        int femaleCount,
        string groupComposition)
    {
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        return new ReservationContext
        {
            Tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test",
                Email = "test@test.com"
            },
            Venue = new Venue
            {
                Id = venueId,
                TenantId = tenantId,
                Name = "Test Venue",
                OpeningTime = TimeSpan.FromHours(10),
                ClosingTime = TimeSpan.FromHours(22)
            },
            Reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VenueId = venueId,
                PartySize = partySize,
                ReservedFor = DateTime.UtcNow.AddDays(3),
                GuestName = "Group Guest",
                GuestPhone = "5551234567"
            },
            MaleCount = maleCount,
            FemaleCount = femaleCount,
            GroupComposition = groupComposition,
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static ReservationContext CreateContextWithoutGroupData(int partySize)
    {
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        return new ReservationContext
        {
            Tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test",
                Email = "test@test.com"
            },
            Venue = new Venue
            {
                Id = venueId,
                TenantId = tenantId,
                Name = "Test Venue",
                OpeningTime = TimeSpan.FromHours(10),
                ClosingTime = TimeSpan.FromHours(22)
            },
            Reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VenueId = venueId,
                PartySize = partySize,
                ReservedFor = DateTime.UtcNow.AddDays(3),
                GuestName = "Group Guest",
                GuestPhone = "5551234567"
            },
            MaleCount = null, // Veri yok
            FemaleCount = null, // Veri yok
            GroupComposition = null, // Veri yok
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }
}
