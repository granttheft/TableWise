using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// EarlyBookingRuleEvaluator birim testleri.
/// </summary>
public class EarlyBookingRuleEvaluatorTests
{
    private readonly Mock<ILogger<EarlyBookingRuleEvaluator>> _loggerMock;
    private readonly EarlyBookingRuleEvaluator _evaluator;

    public EarlyBookingRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<EarlyBookingRuleEvaluator>>();
        _evaluator = new EarlyBookingRuleEvaluator(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WhenDaysInAdvanceMeetsMinimum_ReturnsDiscountOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minDaysInAdvance":7}""",
            actions: """{"version":1,"discountPercent":10,"label":"Erken rezervasyon %10 indirim"}"""
        );
        var context = CreateContext(daysInAdvance: 10);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Discount, result.ActionType);
        Assert.Equal("Erken rezervasyon %10 indirim", result.Message);
        Assert.Contains("\"discountPercent\":10", result.Payload);
    }

    [Fact]
    public async Task EvaluateAsync_WhenDaysInAdvanceBelowMinimum_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minDaysInAdvance":7}""",
            actions: """{"version":1,"discountPercent":10}"""
        );
        var context = CreateContext(daysInAdvance: 3);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenDaysInAdvanceExceedsMaximum_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minDaysInAdvance":7,"maxDaysInAdvance":30}""",
            actions: """{"version":1,"discountPercent":15}"""
        );
        var context = CreateContext(daysInAdvance: 45);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoLabel_UsesDefaultMessage()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minDaysInAdvance":5}""",
            actions: """{"version":1,"discountPercent":5}"""
        );
        var context = CreateContext(daysInAdvance: 7);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Erken rezervasyon indirimi uygulandı", result.Message);
    }

    private static Rule CreateRule(string conditions, string actions)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Early Booking Rule",
            RuleType = "early_booking",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(int daysInAdvance)
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
                PartySize = 4,
                ReservedFor = DateTime.UtcNow.AddDays(daysInAdvance),
                GuestName = "Test Guest",
                GuestPhone = "5551234567"
            },
            DaysInAdvance = daysInAdvance,
            CurrentOccupancyRate = 0.5
        };
    }
}
