using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// CustomConditionRuleEvaluator birim testleri.
/// </summary>
public class CustomConditionRuleEvaluatorTests
{
    private readonly Mock<ILogger<CustomConditionRuleEvaluator>> _loggerMock;
    private readonly CustomConditionRuleEvaluator _evaluator;

    public CustomConditionRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<CustomConditionRuleEvaluator>>();
        _evaluator = new CustomConditionRuleEvaluator(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_PartySizeGreaterOrEqual_WhenMet_ReturnsOutcome()
    {
        // Arrange - partySize >= 6, partySize=7 → triggered
        var rule = CreateRule(
            conditions: """{"version":1,"operator":"and","conditions":[{"field":"partySize","op":">=","value":6}]}""",
            actions: """{"version":1,"block":true,"message":"Büyük grup engeli"}"""
        );
        var context = CreateContext(partySize: 7);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Block, result.ActionType);
        Assert.Equal("Büyük grup engeli", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_PartySizeGreaterOrEqual_WhenNotMet_ReturnsNull()
    {
        // Arrange - partySize >= 6, partySize=4 → not triggered
        var rule = CreateRule(
            conditions: """{"version":1,"operator":"and","conditions":[{"field":"partySize","op":">=","value":6}]}""",
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var context = CreateContext(partySize: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_GroupCompositionEquals_WhenNull_ReturnsFalse()
    {
        // Arrange - groupComposition == "AllMale", ctx.groupComposition=null → false (null safe)
        var rule = CreateRule(
            conditions: """{"version":1,"operator":"and","conditions":[{"field":"groupComposition","op":"==","value":"AllMale"}]}""",
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var context = CreateContext(partySize: 4, groupComposition: null);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result); // Null değer false döner, kural tetiklenmez
    }

    [Fact]
    public async Task EvaluateAsync_FemaleRatioLessThan_WhenTriggered_ReturnsOutcome()
    {
        // Arrange - femaleRatio < 0.30, femaleRatio=0.20 → triggered
        var rule = CreateRule(
            conditions: """{"version":1,"operator":"and","conditions":[{"field":"femaleRatio","op":"<","value":0.30}]}""",
            actions: """{"version":1,"warn":true,"message":"Kadın oranı düşük"}"""
        );
        var context = CreateContext(partySize: 10, maleCount: 8, femaleCount: 2); // femaleRatio = 0.20

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Warn, result.ActionType);
        Assert.Equal("Kadın oranı düşük", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_UnknownField_ReturnsFalse()
    {
        // Arrange - Bilinmeyen field → false
        var rule = CreateRule(
            conditions: """{"version":1,"operator":"and","conditions":[{"field":"unknownField","op":"==","value":"test"}]}""",
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var context = CreateContext(partySize: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result); // Bilinmeyen field null döner, koşul false olur
    }

    [Fact]
    public async Task EvaluateAsync_AndOperator_AllTrue_ReturnsOutcome()
    {
        // Arrange - operator="and": tümü true olmalı
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "and",
                "conditions": [
                    {"field": "partySize", "op": ">=", "value": 4},
                    {"field": "daysInAdvance", "op": "<=", "value": 7}
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Uyarı"}"""
        );
        var context = CreateContext(partySize: 5, daysInAdvance: 3);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Warn, result.ActionType);
    }

    [Fact]
    public async Task EvaluateAsync_AndOperator_OneFalse_ReturnsNull()
    {
        // Arrange - operator="and": biri false → tetiklenmez
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "and",
                "conditions": [
                    {"field": "partySize", "op": ">=", "value": 10},
                    {"field": "daysInAdvance", "op": "<=", "value": 7}
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Uyarı"}"""
        );
        var context = CreateContext(partySize: 5, daysInAdvance: 3); // partySize < 10

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_OrOperator_OneTrue_ReturnsOutcome()
    {
        // Arrange - operator="or": biri yeterli
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "conditions": [
                    {"field": "partySize", "op": ">=", "value": 10},
                    {"field": "daysInAdvance", "op": "<=", "value": 7}
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Uyarı"}"""
        );
        var context = CreateContext(partySize: 5, daysInAdvance: 3); // daysInAdvance <= 7

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EvaluateAsync_OrOperator_AllFalse_ReturnsNull()
    {
        // Arrange - operator="or": tümü false → tetiklenmez
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "or",
                "conditions": [
                    {"field": "partySize", "op": ">=", "value": 10},
                    {"field": "daysInAdvance", "op": ">=", "value": 30}
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Uyarı"}"""
        );
        var context = CreateContext(partySize: 5, daysInAdvance: 3);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_InOperator_ValueInArray_ReturnsOutcome()
    {
        // Arrange - dayOfWeek in ["Friday", "Saturday"]
        var rule = CreateRule(
            conditions: """
            {
                "version": 1,
                "operator": "and",
                "conditions": [
                    {"field": "reservation.reservedFor.dayOfWeek", "op": "in", "value": ["Friday", "Saturday"]}
                ]
            }
            """,
            actions: """{"version":1,"warn":true,"message":"Hafta sonu uyarısı"}"""
        );
        var friday = GetNextFriday();
        var context = CreateContextWithDate(partySize: 4, reservedFor: friday);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hafta sonu uyarısı", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_DiscountAction_ReturnsDiscountOutcome()
    {
        // Arrange - Discount aksiyonu
        var rule = CreateRule(
            conditions: """{"version":1,"operator":"and","conditions":[{"field":"daysInAdvance","op":">=","value":7}]}""",
            actions: """{"version":1,"discountPercent":15,"message":"Erken rezervasyon indirimi"}"""
        );
        var context = CreateContext(partySize: 4, daysInAdvance: 10);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Discount, result.ActionType);
        Assert.Contains("15", result.Payload);
    }

    [Fact]
    public async Task EvaluateAsync_CustomerTierEquals_ReturnsOutcome()
    {
        // Arrange - customer.tier == "VIP"
        var rule = CreateRule(
            conditions: """{"version":1,"operator":"and","conditions":[{"field":"customer.tier","op":"==","value":"VIP"}]}""",
            actions: """{"version":1,"suggest":true,"message":"VIP önceliği"}"""
        );
        var context = CreateContextWithCustomer(partySize: 4, customerTier: CustomerTier.VIP);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Suggest, result.ActionType);
    }

    private static Rule CreateRule(string conditions, string actions)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Custom Condition Rule",
            RuleType = "custom_condition",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(
        int partySize,
        int daysInAdvance = 3,
        string? groupComposition = null,
        int? maleCount = null,
        int? femaleCount = null)
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
                ReservedFor = DateTime.UtcNow.AddDays(daysInAdvance),
                GuestName = "Test Guest",
                GuestPhone = "5551234567"
            },
            DaysInAdvance = daysInAdvance,
            CurrentOccupancyRate = 0.5,
            GroupComposition = groupComposition,
            MaleCount = maleCount,
            FemaleCount = femaleCount
        };
    }

    private static ReservationContext CreateContextWithDate(int partySize, DateTime reservedFor)
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
                ReservedFor = reservedFor,
                GuestName = "Test Guest",
                GuestPhone = "5551234567"
            },
            DaysInAdvance = (int)(reservedFor.Date - DateTime.UtcNow.Date).TotalDays,
            CurrentOccupancyRate = 0.5
        };
    }

    private static ReservationContext CreateContextWithCustomer(int partySize, CustomerTier customerTier)
    {
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

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
                GuestName = "Test Guest",
                GuestPhone = "5551234567"
            },
            Customer = new Customer
            {
                Id = customerId,
                TenantId = tenantId,
                FullName = "VIP Customer",
                Phone = "5551234567",
                Tier = customerTier
            },
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static DateTime GetNextFriday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilFriday == 0) daysUntilFriday = 7;
        return today.AddDays(daysUntilFriday).AddHours(20);
    }
}
