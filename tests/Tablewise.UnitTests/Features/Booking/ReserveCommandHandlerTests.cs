using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Application.Features.Booking.Commands;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.UnitTests.Features.Booking;

/// <summary>
/// ReserveCommandHandler unit testleri.
/// </summary>
public class ReserveCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISlotAvailabilityService> _slotServiceMock;
    private readonly Mock<IRuleEvaluator> _ruleEvaluatorMock;
    private readonly Mock<IDistributedLockService> _lockServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ReserveCommandHandler>> _loggerMock;

    public ReserveCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _slotServiceMock = new Mock<ISlotAvailabilityService>();
        _ruleEvaluatorMock = new Mock<IRuleEvaluator>();
        _lockServiceMock = new Mock<IDistributedLockService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ReserveCommandHandler>>();
    }

    /// <summary>
    /// Lock alınamadığında ConflictException fırlatılmalı.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLockCannotBeAcquired_ThrowsConflictException()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            SlotDurationMinutes = 90,
            Tenant = new Tenant { Id = tenantId, Slug = "test-venue", IsActive = true }
        };

        SetupVenueMock(venue);

        // Lock alınamıyor
        _lockServiceMock
            .Setup(x => x.WaitForLockAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IDistributedLockHandle?)null);

        var handler = CreateHandler();
        var command = CreateValidCommand("test-venue");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("eş zamanlı işlem", exception.Message);
    }

    /// <summary>
    /// Slot müsait değilse ConflictException fırlatılmalı.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSlotNotAvailable_ThrowsConflictException()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            SlotDurationMinutes = 90,
            Tenant = new Tenant { Id = tenantId, Slug = "test-venue", IsActive = true }
        };

        SetupVenueMock(venue);
        SetupLockMock();

        // Slot müsait değil
        _slotServiceMock
            .Setup(x => x.CheckSlotAvailabilityAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SlotAvailabilityResult.Unavailable("Masa dolu"));

        var handler = CreateHandler();
        var command = CreateValidCommand("test-venue");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("Masa dolu", exception.Message);
    }

    /// <summary>
    /// Kural motoru engellenirse BusinessRuleException fırlatılmalı.
    /// </summary>
    [Fact]
    public async Task Handle_WhenRuleEvaluatorBlocks_ThrowsBusinessRuleException()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            SlotDurationMinutes = 90,
            Tenant = new Tenant { Id = tenantId, Slug = "test-venue", IsActive = true }
        };

        SetupVenueMock(venue);
        SetupLockMock();
        SetupSlotAvailableMock();

        // Kural engeli
        _ruleEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<RuleEvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RuleEvaluationResult.Block("VIP masa sadece Gold üyelere"));

        var handler = CreateHandler();
        var command = CreateValidCommand("test-venue");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("VIP masa sadece Gold üyelere", exception.Message);
    }

    /// <summary>
    /// Başarılı rezervasyon oluşturulduğunda doğru response dönmeli.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsValidResponse()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            SlotDurationMinutes = 90,
            Tenant = new Tenant { Id = tenantId, Slug = "test-venue", IsActive = true }
        };

        SetupVenueMock(venue);
        SetupLockMock();
        SetupSlotAvailableMock(tableId);
        SetupRuleAllowMock();
        SetupCustomerMock();
        SetupReservationMock();

        var handler = CreateHandler();
        var command = CreateValidCommand("test-venue");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.ReservationId);
        Assert.NotEmpty(result.ConfirmCode);
        Assert.Equal(8, result.ConfirmCode.Length);
        Assert.Equal("Test Venue", result.VenueName);
        Assert.Equal(command.PartySize, result.PartySize);
    }

    /// <summary>
    /// ConfirmCode 8 karakter alphanumeric olmalı.
    /// </summary>
    [Fact]
    public async Task Handle_ConfirmCode_Is8CharAlphanumeric()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            SlotDurationMinutes = 90,
            Tenant = new Tenant { Id = tenantId, Slug = "test-venue", IsActive = true }
        };

        SetupVenueMock(venue);
        SetupLockMock();
        SetupSlotAvailableMock();
        SetupRuleAllowMock();
        SetupCustomerMock();
        SetupReservationMock();

        var handler = CreateHandler();
        var command = CreateValidCommand("test-venue");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(8, result.ConfirmCode.Length);
        Assert.True(result.ConfirmCode.All(c => char.IsLetterOrDigit(c)));
        Assert.True(result.ConfirmCode.All(c => char.IsUpper(c) || char.IsDigit(c)));
    }

    #region Helper Methods

    private ReserveCommandHandler CreateHandler()
    {
        return new ReserveCommandHandler(
            _unitOfWorkMock.Object,
            _slotServiceMock.Object,
            _ruleEvaluatorMock.Object,
            _lockServiceMock.Object,
            _cacheServiceMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    private ReserveCommand CreateValidCommand(string slug)
    {
        return new ReserveCommand
        {
            Slug = slug,
            IdempotencyKey = Guid.NewGuid().ToString(),
            GuestName = "Test User",
            GuestEmail = "test@example.com",
            GuestPhone = "+905551234567",
            PartySize = 4,
            ReservedFor = DateTime.UtcNow.AddDays(1).Date.AddHours(19)
        };
    }

    private void SetupVenueMock(Venue venue)
    {
        var venueRepoMock = new Mock<IRepository<Venue>>();
        var queryable = new List<Venue> { venue }.AsQueryable();

        var mockDbSet = new Mock<DbSet<Venue>>();
        mockDbSet.As<IQueryable<Venue>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Venue>(queryable.Provider));
        mockDbSet.As<IQueryable<Venue>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<Venue>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<Venue>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<Venue>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Venue>(queryable.GetEnumerator()));

        venueRepoMock.Setup(x => x.Query()).Returns(mockDbSet.Object);
        _unitOfWorkMock.Setup(x => x.Venues).Returns(venueRepoMock.Object);
    }

    private void SetupLockMock()
    {
        var lockHandleMock = new Mock<IDistributedLockHandle>();
        lockHandleMock.Setup(x => x.IsAcquired).Returns(true);

        _lockServiceMock
            .Setup(x => x.WaitForLockAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockHandleMock.Object);
    }

    private void SetupSlotAvailableMock(Guid? tableId = null)
    {
        _slotServiceMock
            .Setup(x => x.CheckSlotAvailabilityAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SlotAvailabilityResult.Available(tableId ?? Guid.NewGuid()));
    }

    private void SetupRuleAllowMock()
    {
        _ruleEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<RuleEvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RuleEvaluationResult.Allow());
    }

    private void SetupCustomerMock()
    {
        var customerRepoMock = new Mock<IRepository<Customer>>();
        var emptyList = new List<Customer>().AsQueryable();

        var mockDbSet = new Mock<DbSet<Customer>>();
        mockDbSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Customer>(emptyList.Provider));
        mockDbSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(emptyList.Expression);
        mockDbSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(emptyList.ElementType);
        mockDbSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(emptyList.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<Customer>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Customer>(emptyList.GetEnumerator()));

        customerRepoMock.Setup(x => x.Query()).Returns(mockDbSet.Object);
        customerRepoMock.Setup(x => x.Add(It.IsAny<Customer>()));
        _unitOfWorkMock.Setup(x => x.Customers).Returns(customerRepoMock.Object);
    }

    private void SetupReservationMock()
    {
        var reservationRepoMock = new Mock<IRepository<Reservation>>();
        var emptyList = new List<Reservation>().AsQueryable();

        var mockDbSet = new Mock<DbSet<Reservation>>();
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Reservation>(emptyList.Provider));
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(emptyList.Expression);
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(emptyList.ElementType);
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(emptyList.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<Reservation>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Reservation>(emptyList.GetEnumerator()));

        reservationRepoMock.Setup(x => x.Query()).Returns(mockDbSet.Object);
        reservationRepoMock.Setup(x => x.Add(It.IsAny<Reservation>()));
        _unitOfWorkMock.Setup(x => x.Reservations).Returns(reservationRepoMock.Object);

        // StatusLog ve AuditLog
        var statusLogRepoMock = new Mock<IRepository<ReservationStatusLog>>();
        statusLogRepoMock.Setup(x => x.Add(It.IsAny<ReservationStatusLog>()));
        _unitOfWorkMock.Setup(x => x.ReservationStatusLogs).Returns(statusLogRepoMock.Object);

        var auditLogRepoMock = new Mock<IRepository<AuditLog>>();
        auditLogRepoMock.Setup(x => x.Add(It.IsAny<AuditLog>()));
        _unitOfWorkMock.Setup(x => x.AuditLogs).Returns(auditLogRepoMock.Object);

        // Table
        var tableRepoMock = new Mock<IRepository<Table>>();
        var tableList = new List<Table>().AsQueryable();
        var mockTableDbSet = new Mock<DbSet<Table>>();
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Table>(tableList.Provider));
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.Expression).Returns(tableList.Expression);
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.ElementType).Returns(tableList.ElementType);
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.GetEnumerator()).Returns(tableList.GetEnumerator());
        mockTableDbSet.As<IAsyncEnumerable<Table>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Table>(tableList.GetEnumerator()));
        tableRepoMock.Setup(x => x.Query()).Returns(mockTableDbSet.Object);
        _unitOfWorkMock.Setup(x => x.Tables).Returns(tableRepoMock.Object);

        // TableCombination
        var comboRepoMock = new Mock<IRepository<TableCombination>>();
        var comboList = new List<TableCombination>().AsQueryable();
        var mockComboDbSet = new Mock<DbSet<TableCombination>>();
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TableCombination>(comboList.Provider));
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.Expression).Returns(comboList.Expression);
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.ElementType).Returns(comboList.ElementType);
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.GetEnumerator()).Returns(comboList.GetEnumerator());
        mockComboDbSet.As<IAsyncEnumerable<TableCombination>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<TableCombination>(comboList.GetEnumerator()));
        comboRepoMock.Setup(x => x.Query()).Returns(mockComboDbSet.Object);
        _unitOfWorkMock.Setup(x => x.TableCombinations).Returns(comboRepoMock.Object);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _slotServiceMock.Setup(x => x.InvalidateCacheAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheServiceMock.Setup(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    #endregion
}

#region Test Helpers

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(System.Linq.Expressions.Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(_inner, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

#endregion
