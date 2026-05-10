using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// LargeGroupRuleEvaluator birim testleri.
/// </summary>
public class LargeGroupRuleEvaluatorTests
{
    private readonly Mock<ILogger<LargeGroupRuleEvaluator>> _loggerMock;
    private readonly LargeGroupRuleEvaluator _evaluator;

    public LargeGroupRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<LargeGroupRuleEvaluator>>();
        _evaluator = new LargeGroupRuleEvaluator(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WhenTableCapacitySufficient_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minPartySize":8}""",
            actions: """{"version":1,"requireCombination":true,"message":"Uygun masa yok"}"""
        );
        var context = CreateContext(partySize: 10, tableCapacity: 12);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCombinationAvailable_ReturnsSuggestOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minPartySize":8}""",
            actions: """{"version":1,"requireCombination":true,"message":"Uygun masa yok"}"""
        );
        var context = CreateContextWithCombination(partySize: 10, tableCapacity: 4, combinationCapacity: 12);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Suggest, result.ActionType);
        Assert.Contains("masa birleşimi önerilmektedir", result.Message);
        Assert.NotNull(result.Payload);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoCombinationAvailable_ReturnsBlockOutcome()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minPartySize":8}""",
            actions: """{"version":1,"requireCombination":true,"message":"Bu kadar kişilik grup kabul edilemiyor"}"""
        );
        var context = CreateContextWithoutCombination(partySize: 15, tableCapacity: 4);

        // Act
        var result = await _evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Block, result.ActionType);
        Assert.Equal("Bu kadar kişilik grup kabul edilemiyor", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_WhenPartySizeBelowMinimum_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"minPartySize":8}""",
            actions: """{"version":1,"requireCombination":true,"message":"Uygun masa yok"}"""
        );
        var context = CreateContext(partySize: 4, tableCapacity: 6);

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
            Name = "Test Large Group Rule",
            RuleType = "large_group",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(int partySize, int tableCapacity)
    {
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            OpeningTime = TimeSpan.FromHours(10),
            ClosingTime = TimeSpan.FromHours(22),
            TableCombinations = new List<TableCombination>()
        };

        return new ReservationContext
        {
            Tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test",
                Email = "test@test.com"
            },
            Venue = venue,
            Reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VenueId = venueId,
                TableId = tableId,
                PartySize = partySize,
                ReservedFor = DateTime.UtcNow.AddDays(3),
                GuestName = "Large Group",
                GuestPhone = "5551234567"
            },
            Table = new Table
            {
                Id = tableId,
                TenantId = tenantId,
                VenueId = venueId,
                Name = "Table 1",
                Capacity = tableCapacity,
                IsActive = true
            },
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static ReservationContext CreateContextWithCombination(int partySize, int tableCapacity, int combinationCapacity)
    {
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var combinationId = Guid.NewGuid();

        var combination = new TableCombination
        {
            Id = combinationId,
            TenantId = tenantId,
            VenueId = venueId,
            Name = "VIP Masa Birleşimi",
            CombinedCapacity = combinationCapacity,
            IsActive = true
        };

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            OpeningTime = TimeSpan.FromHours(10),
            ClosingTime = TimeSpan.FromHours(22),
            TableCombinations = new List<TableCombination> { combination }
        };

        return new ReservationContext
        {
            Tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test",
                Email = "test@test.com"
            },
            Venue = venue,
            Reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VenueId = venueId,
                TableId = tableId,
                PartySize = partySize,
                ReservedFor = DateTime.UtcNow.AddDays(3),
                GuestName = "Large Group",
                GuestPhone = "5551234567"
            },
            Table = new Table
            {
                Id = tableId,
                TenantId = tenantId,
                VenueId = venueId,
                Name = "Table 1",
                Capacity = tableCapacity,
                IsActive = true
            },
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static ReservationContext CreateContextWithoutCombination(int partySize, int tableCapacity)
    {
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        // Küçük kapasiteli birleşim ekle (yetersiz)
        var smallCombination = new TableCombination
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = venueId,
            Name = "Small Combination",
            CombinedCapacity = 8,
            IsActive = true
        };

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            OpeningTime = TimeSpan.FromHours(10),
            ClosingTime = TimeSpan.FromHours(22),
            TableCombinations = new List<TableCombination> { smallCombination }
        };

        return new ReservationContext
        {
            Tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test",
                Email = "test@test.com"
            },
            Venue = venue,
            Reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VenueId = venueId,
                TableId = tableId,
                PartySize = partySize,
                ReservedFor = DateTime.UtcNow.AddDays(3),
                GuestName = "Large Group",
                GuestPhone = "5551234567"
            },
            Table = new Table
            {
                Id = tableId,
                TenantId = tenantId,
                VenueId = venueId,
                Name = "Table 1",
                Capacity = tableCapacity,
                IsActive = true
            },
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }
}
