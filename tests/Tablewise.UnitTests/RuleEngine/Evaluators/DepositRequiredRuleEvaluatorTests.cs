using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// DepositRequiredRuleEvaluator birim testleri.
/// </summary>
public class DepositRequiredRuleEvaluatorTests
{
    private readonly Mock<ILogger<DepositRequiredRuleEvaluator>> _loggerMock;
    private readonly DepositRequiredRuleEvaluator _evaluator;

    public DepositRequiredRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<DepositRequiredRuleEvaluator>>();
        _evaluator = new DepositRequiredRuleEvaluator(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WhenScopeMatches_ReturnsDepositOutcome()
    {
        // Arrange - Cuma günü 20:00
        var rule = CreateRule(
            conditions: """{"version":1,"scopes":{"days":["Friday"],"times":["20:00"]}}""",
            actions: """{"version":1,"amount":100,"perPerson":false}"""
        );
        var friday20 = GetNextFriday().AddHours(20);
        var context = CreateContext(depositEnabled: true, reservedFor: friday20, partySize: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Deposit, result.ActionType);
        Assert.Contains("100", result.Payload);
    }

    [Fact]
    public async Task EvaluateAsync_WhenScopeDoesNotMatch_ReturnsNull()
    {
        // Arrange - Pazartesi günü (Cuma değil)
        var rule = CreateRule(
            conditions: """{"version":1,"scopes":{"days":["Friday","Saturday"]}}""",
            actions: """{"version":1,"amount":100}"""
        );
        var monday = GetNextMonday().AddHours(20);
        var context = CreateContext(depositEnabled: true, reservedFor: monday, partySize: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenUseVenueDefault_UsesVenueDepositAmount()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1}""",
            actions: """{"version":1,"useVenueDefault":true}"""
        );
        var context = CreateContextWithVenueDeposit(depositAmount: 50, perPerson: true, partySize: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Deposit, result.ActionType);
        Assert.Contains("200", result.Payload); // 50 * 4 = 200
    }

    [Fact]
    public async Task EvaluateAsync_WhenDepositDisabled_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1}""",
            actions: """{"version":1,"amount":100}"""
        );
        var context = CreateContext(depositEnabled: false, reservedFor: DateTime.UtcNow.AddDays(3), partySize: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenPerPerson_MultipliesByPartySize()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1}""",
            actions: """{"version":1,"amount":25,"perPerson":true}"""
        );
        var context = CreateContext(depositEnabled: true, reservedFor: DateTime.UtcNow.AddDays(3), partySize: 6);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("150", result.Payload); // 25 * 6 = 150
    }

    [Fact]
    public async Task EvaluateAsync_WhenMinPartySizeNotMet_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"scopes":{"minPartySize":8}}""",
            actions: """{"version":1,"amount":200}"""
        );
        var context = CreateContext(depositEnabled: true, reservedFor: DateTime.UtcNow.AddDays(3), partySize: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    private static Rule CreateRule(string conditions, string actions)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Deposit Rule",
            RuleType = "deposit_required",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(bool depositEnabled, DateTime reservedFor, int partySize)
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
                ClosingTime = TimeSpan.FromHours(22),
                DepositEnabled = depositEnabled,
                DepositAmount = 0,
                DepositPerPerson = false
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
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static ReservationContext CreateContextWithVenueDeposit(decimal depositAmount, bool perPerson, int partySize)
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
                ClosingTime = TimeSpan.FromHours(22),
                DepositEnabled = true,
                DepositAmount = depositAmount,
                DepositPerPerson = perPerson
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
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static DateTime GetNextFriday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilFriday == 0) daysUntilFriday = 7;
        return today.AddDays(daysUntilFriday);
    }

    private static DateTime GetNextMonday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        return today.AddDays(daysUntilMonday);
    }
}
