using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// PeakHourRuleEvaluator birim testleri.
/// </summary>
public class PeakHourRuleEvaluatorTests
{
    private readonly Mock<ILogger<PeakHourRuleEvaluator>> _loggerMock;
    private readonly PeakHourRuleEvaluator _evaluator;

    public PeakHourRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<PeakHourRuleEvaluator>>();
        _evaluator = new PeakHourRuleEvaluator(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WhenInPeakHourAndBlock_ReturnsBlockOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"startTime":"19:00","endTime":"22:00"}""",
            actions: """{"version":1,"block":true,"message":"Bu saat diliminde rezervasyon kabul edilmiyor"}"""
        );
        var peakTime = DateTime.UtcNow.Date.AddHours(20); // 20:00
        var context = CreateContext(reservedFor: peakTime, occupancyRate: 0.8);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Block, result.ActionType);
        Assert.Equal("Bu saat diliminde rezervasyon kabul edilmiyor", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_WhenInPeakHourAndWarn_ReturnsWarnOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"startTime":"19:00","endTime":"22:00"}""",
            actions: """{"version":1,"warn":true,"message":"Yoğun saat - bekleme süresi uzun olabilir"}"""
        );
        var peakTime = DateTime.UtcNow.Date.AddHours(20); // 20:00
        var context = CreateContext(reservedFor: peakTime, occupancyRate: 0.8);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Warn, result.ActionType);
        Assert.Equal("Yoğun saat - bekleme süresi uzun olabilir", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_WhenOutsidePeakHour_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"startTime":"19:00","endTime":"22:00"}""",
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var offPeakTime = DateTime.UtcNow.Date.AddHours(14); // 14:00
        var context = CreateContext(reservedFor: offPeakTime, occupancyRate: 0.8);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenDayNotMatched_ReturnsNull()
    {
        // Arrange - Sadece Cuma-Cumartesi
        var rule = CreateRule(
            conditions: """{"version":1,"startTime":"19:00","endTime":"22:00","days":["Friday","Saturday"]}""",
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        // Pazartesi 20:00 (peak saat ama gün eşleşmiyor)
        var monday20 = GetNextMonday().AddHours(20);
        var context = CreateContext(reservedFor: monday20, occupancyRate: 0.8);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenOccupancyBelowMinimum_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"startTime":"19:00","endTime":"22:00","minOccupancyPercent":70}""",
            actions: """{"version":1,"block":true,"message":"Engellendi"}"""
        );
        var peakTime = DateTime.UtcNow.Date.AddHours(20); // 20:00
        var context = CreateContext(reservedFor: peakTime, occupancyRate: 0.5); // %50 doluluk

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenOccupancyAboveMinimum_ReturnsOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"startTime":"19:00","endTime":"22:00","minOccupancyPercent":70}""",
            actions: """{"version":1,"warn":true,"message":"Doluluk yüksek"}"""
        );
        var peakTime = DateTime.UtcNow.Date.AddHours(20); // 20:00
        var context = CreateContext(reservedFor: peakTime, occupancyRate: 0.85); // %85 doluluk

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Warn, result.ActionType);
    }

    private static Rule CreateRule(string conditions, string actions)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Peak Hour Rule",
            RuleType = "peak_hour",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(DateTime reservedFor, double occupancyRate)
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
                ReservedFor = reservedFor,
                GuestName = "Test Guest",
                GuestPhone = "5551234567"
            },
            DaysInAdvance = 3,
            CurrentOccupancyRate = occupancyRate
        };
    }

    private static DateTime GetNextMonday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        return today.AddDays(daysUntilMonday);
    }
}
