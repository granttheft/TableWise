using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// VipPriorityRuleEvaluator birim testleri.
/// </summary>
public class VipPriorityRuleEvaluatorTests
{
    private readonly Mock<ILogger<VipPriorityRuleEvaluator>> _loggerMock;
    private readonly VipPriorityRuleEvaluator _evaluator;

    public VipPriorityRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<VipPriorityRuleEvaluator>>();
        _evaluator = new VipPriorityRuleEvaluator(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WhenVipCustomer_ReturnsSuggestOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minTier":"VIP"}""",
            actions: """{"version":1,"suggestBestTable":true}"""
        );
        var context = CreateContext(CustomerTier.VIP);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Suggest, result.ActionType);
        Assert.Equal("VIP misafiriniz için özel masa ayrıldı", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_WhenGoldCustomerAndMinTierGold_ReturnsSuggestOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minTier":"Gold"}""",
            actions: """{"version":1,"suggestBestTable":true}"""
        );
        var context = CreateContext(CustomerTier.Gold);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Suggest, result.ActionType);
    }

    [Fact]
    public async Task EvaluateAsync_WhenRegularCustomer_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minTier":"Gold"}""",
            actions: """{"version":1,"suggestBestTable":true}"""
        );
        var context = CreateContext(CustomerTier.Regular);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoCustomer_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minTier":"VIP"}""",
            actions: """{"version":1,"suggestBestTable":true}"""
        );
        var context = CreateContextWithoutCustomer();

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenBlacklistedCustomer_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minTier":"VIP"}""",
            actions: """{"version":1,"suggestBestTable":true}"""
        );
        var context = CreateContext(CustomerTier.Blacklisted);

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
            Name = "Test VIP Priority Rule",
            RuleType = "vip_priority",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(CustomerTier tier)
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
                CustomerId = customerId,
                PartySize = 4,
                ReservedFor = DateTime.UtcNow.AddDays(3),
                GuestName = "VIP Guest",
                GuestPhone = "5551234567"
            },
            Customer = new Customer
            {
                Id = customerId,
                TenantId = tenantId,
                FullName = "VIP Customer",
                Phone = "5551234567",
                Tier = tier
            },
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static ReservationContext CreateContextWithoutCustomer()
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
                ReservedFor = DateTime.UtcNow.AddDays(3),
                GuestName = "Guest Without Account",
                GuestPhone = "5551234567"
            },
            Customer = null,
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }
}
