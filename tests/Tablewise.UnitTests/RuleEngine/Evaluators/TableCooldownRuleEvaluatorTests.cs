using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.UnitTests.RuleEngine.Evaluators;

/// <summary>
/// TableCooldownRuleEvaluator birim testleri.
/// </summary>
public class TableCooldownRuleEvaluatorTests
{
    private readonly Mock<ILogger<TableCooldownRuleEvaluator>> _loggerMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;

    public TableCooldownRuleEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<TableCooldownRuleEvaluator>>();
        _dbContextMock = new Mock<IApplicationDbContext>();
    }

    [Fact]
    public async Task EvaluateAsync_WhenCooldownViolation_ReturnsBlockOutcome()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        var today = DateTime.UtcNow.Date;
        var previousReservationEnd = today.AddHours(18); // 18:00'da biten rezervasyon
        var newReservationStart = today.AddHours(18).AddMinutes(15); // 18:15'te başlayan yeni rezervasyon (30dk bekleme süresi ihlali)

        var previousReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = venueId,
            TableId = tableId,
            Status = ReservationStatus.Confirmed,
            ReservedFor = today.AddHours(16),
            EndTime = previousReservationEnd,
            PartySize = 4,
            GuestName = "Previous Guest",
            GuestPhone = "5551111111"
        };

        SetupDbContext(new[] { previousReservation });

        var rule = CreateRule(
            conditions: """{"version":1,"cooldownMinutes":30}""",
            actions: """{"version":1,"message":"Masa temizlik için hazırlanıyor"}"""
        );
        var context = CreateContext(tenantId, venueId, tableId, newReservationStart);

        var evaluator = new TableCooldownRuleEvaluator(_loggerMock.Object, _dbContextMock.Object);

        // Act
        var result = await evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RuleActionType.Block, result.ActionType);
        Assert.Equal("Masa temizlik için hazırlanıyor", result.Message);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCooldownPassed_ReturnsNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        var today = DateTime.UtcNow.Date;
        var previousReservationEnd = today.AddHours(18); // 18:00'da biten rezervasyon
        var newReservationStart = today.AddHours(19); // 19:00'da başlayan yeni rezervasyon (30dk geçmiş)

        var previousReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = venueId,
            TableId = tableId,
            Status = ReservationStatus.Completed,
            ReservedFor = today.AddHours(16),
            EndTime = previousReservationEnd,
            PartySize = 4,
            GuestName = "Previous Guest",
            GuestPhone = "5551111111"
        };

        SetupDbContext(new[] { previousReservation });

        var rule = CreateRule(
            conditions: """{"version":1,"cooldownMinutes":30}""",
            actions: """{"version":1,"message":"Engellendi"}"""
        );
        var context = CreateContext(tenantId, venueId, tableId, newReservationStart);

        var evaluator = new TableCooldownRuleEvaluator(_loggerMock.Object, _dbContextMock.Object);

        // Act
        var result = await evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoTable_ReturnsNull()
    {
        // Arrange
        var rule = CreateRule(
            conditions: """{"version":1,"cooldownMinutes":30}""",
            actions: """{"version":1,"message":"Engellendi"}"""
        );
        var context = CreateContextWithoutTable();

        var evaluator = new TableCooldownRuleEvaluator(_loggerMock.Object, _dbContextMock.Object);

        // Act
        var result = await evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoPreviousReservation_ReturnsNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        SetupDbContext(Array.Empty<Reservation>()); // Önceki rezervasyon yok

        var rule = CreateRule(
            conditions: """{"version":1,"cooldownMinutes":30}""",
            actions: """{"version":1,"message":"Engellendi"}"""
        );
        var context = CreateContext(tenantId, venueId, tableId, DateTime.UtcNow.Date.AddHours(20));

        var evaluator = new TableCooldownRuleEvaluator(_loggerMock.Object, _dbContextMock.Object);

        // Act
        var result = await evaluator.EvaluateAsync(rule, context);

        // Assert
        Assert.Null(result);
    }

    private void SetupDbContext(Reservation[] reservations)
    {
        var reservationsList = reservations.ToList().AsQueryable();
        var mockSet = new Mock<DbSet<Reservation>>();

        mockSet.As<IAsyncEnumerable<Reservation>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Reservation>(reservationsList.GetEnumerator()));

        mockSet.As<IQueryable<Reservation>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Reservation>(reservationsList.Provider));

        mockSet.As<IQueryable<Reservation>>()
            .Setup(m => m.Expression)
            .Returns(reservationsList.Expression);

        mockSet.As<IQueryable<Reservation>>()
            .Setup(m => m.ElementType)
            .Returns(reservationsList.ElementType);

        mockSet.As<IQueryable<Reservation>>()
            .Setup(m => m.GetEnumerator())
            .Returns(reservationsList.GetEnumerator());

        _dbContextMock.Setup(x => x.Reservations).Returns(mockSet.Object);
    }

    private static Rule CreateRule(string conditions, string actions)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Table Cooldown Rule",
            RuleType = "table_cooldown",
            ConditionsJson = conditions,
            ActionsJson = actions,
            Priority = 1,
            IsActive = true
        };
    }

    private static ReservationContext CreateContext(Guid tenantId, Guid venueId, Guid tableId, DateTime reservedFor)
    {
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
                TableId = tableId,
                PartySize = 4,
                ReservedFor = reservedFor,
                EndTime = reservedFor.AddHours(2),
                GuestName = "New Guest",
                GuestPhone = "5551234567"
            },
            Table = new Table
            {
                Id = tableId,
                TenantId = tenantId,
                VenueId = venueId,
                Name = "Table 1",
                Capacity = 6,
                IsActive = true
            },
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }

    private static ReservationContext CreateContextWithoutTable()
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
                GuestName = "New Guest",
                GuestPhone = "5551234567"
            },
            Table = null, // Masa yok
            DaysInAdvance = 3,
            CurrentOccupancyRate = 0.5
        };
    }
}

#region Test Async Query Helpers

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}

internal class TestAsyncQueryProvider<TEntity> : Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executeMethod = typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
            .MakeGenericMethod(resultType);

        var result = executeMethod.Invoke(_inner, new object[] { expression });
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { result })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    { }

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

#endregion
