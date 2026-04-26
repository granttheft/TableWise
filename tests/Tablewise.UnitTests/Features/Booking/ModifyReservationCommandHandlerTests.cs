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
/// ModifyReservationCommandHandler unit testleri.
/// </summary>
public class ModifyReservationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISlotAvailabilityService> _slotServiceMock;
    private readonly Mock<IRuleEvaluator> _ruleEvaluatorMock;
    private readonly Mock<IDistributedLockService> _lockServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ModifyReservationCommandHandler>> _loggerMock;

    public ModifyReservationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _slotServiceMock = new Mock<ISlotAvailabilityService>();
        _ruleEvaluatorMock = new Mock<IRuleEvaluator>();
        _lockServiceMock = new Mock<IDistributedLockService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ModifyReservationCommandHandler>>();
    }

    /// <summary>
    /// 23 saat sonra değişiklik yapılamaz (24 saat deadline).
    /// </summary>
    [Fact]
    public async Task Handle_When23HoursBeforeReservation_ThrowsBusinessRuleException()
    {
        // Arrange
        var reservationTime = DateTime.UtcNow.AddHours(23); // 23 saat sonra
        var reservation = CreateConfirmedReservation(reservationTime);

        SetupReservationMock(reservation);

        var handler = CreateHandler();
        var command = new ModifyReservationCommand
        {
            ConfirmCode = reservation.ConfirmCode,
            NewDateTime = reservationTime.AddDays(1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("24 saat", exception.Message);
        Assert.Contains("23", exception.Message); // Kalan süre bilgisi
    }

    /// <summary>
    /// 25 saat sonra değişiklik yapılabilir.
    /// </summary>
    [Fact]
    public async Task Handle_When25HoursBeforeReservation_Succeeds()
    {
        // Arrange
        var reservationTime = DateTime.UtcNow.AddHours(25); // 25 saat sonra
        var reservation = CreateConfirmedReservation(reservationTime);

        SetupReservationMock(reservation);
        SetupLockMock();
        SetupSlotAvailableMock();
        SetupRuleAllowMock();
        SetupAllRepositoriesMock(reservation);

        var handler = CreateHandler();
        var command = new ModifyReservationCommand
        {
            ConfirmCode = reservation.ConfirmCode,
            NewDateTime = reservationTime.AddDays(1)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(reservation.ConfirmCode, result.ConfirmCode); // Yeni kod üretilmeli
        Assert.Equal(8, result.ConfirmCode.Length);
    }

    /// <summary>
    /// Tam 24 saat öncesinde değişiklik yapılabilir.
    /// </summary>
    [Fact]
    public async Task Handle_WhenExactly24HoursBeforeReservation_Succeeds()
    {
        // Arrange
        var reservationTime = DateTime.UtcNow.AddHours(24).AddMinutes(1); // 24 saat 1 dakika sonra
        var reservation = CreateConfirmedReservation(reservationTime);

        SetupReservationMock(reservation);
        SetupLockMock();
        SetupSlotAvailableMock();
        SetupRuleAllowMock();
        SetupAllRepositoriesMock(reservation);

        var handler = CreateHandler();
        var command = new ModifyReservationCommand
        {
            ConfirmCode = reservation.ConfirmCode,
            NewDateTime = reservationTime.AddDays(1)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// Sadece Confirmed durumundaki rezervasyonlar değiştirilebilir.
    /// </summary>
    [Fact]
    public async Task Handle_WhenReservationNotConfirmed_ThrowsBusinessRuleException()
    {
        // Arrange
        var reservationTime = DateTime.UtcNow.AddDays(3);
        var reservation = CreateConfirmedReservation(reservationTime);
        reservation.Status = ReservationStatus.Cancelled; // İptal edilmiş

        SetupReservationMock(reservation);

        var handler = CreateHandler();
        var command = new ModifyReservationCommand
        {
            ConfirmCode = reservation.ConfirmCode,
            NewDateTime = reservationTime.AddDays(1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("onaylanmış", exception.Message.ToLower());
    }

    /// <summary>
    /// En az bir değişiklik yapılmalı.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoChangesProvided_ThrowsBusinessRuleException()
    {
        // Arrange
        var reservationTime = DateTime.UtcNow.AddDays(3);
        var reservation = CreateConfirmedReservation(reservationTime);

        SetupReservationMock(reservation);

        var handler = CreateHandler();
        var command = new ModifyReservationCommand
        {
            ConfirmCode = reservation.ConfirmCode
            // Hiçbir değişiklik yok
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("değişiklik", exception.Message.ToLower());
    }

    /// <summary>
    /// Eski rezervasyon Modified durumuna geçmeli.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSuccessful_OldReservationStatusBecomesModified()
    {
        // Arrange
        var reservationTime = DateTime.UtcNow.AddDays(3);
        var reservation = CreateConfirmedReservation(reservationTime);

        SetupReservationMock(reservation);
        SetupLockMock();
        SetupSlotAvailableMock();
        SetupRuleAllowMock();
        SetupAllRepositoriesMock(reservation);

        var handler = CreateHandler();
        var command = new ModifyReservationCommand
        {
            ConfirmCode = reservation.ConfirmCode,
            NewDateTime = reservationTime.AddDays(1)
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ReservationStatus.Modified, reservation.Status);
    }

    #region Helper Methods

    private ModifyReservationCommandHandler CreateHandler()
    {
        return new ModifyReservationCommandHandler(
            _unitOfWorkMock.Object,
            _slotServiceMock.Object,
            _ruleEvaluatorMock.Object,
            _lockServiceMock.Object,
            _cacheServiceMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    private static Reservation CreateConfirmedReservation(DateTime reservedFor)
    {
        var venueId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        return new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = venueId,
            ConfirmCode = "TEST1234",
            Status = ReservationStatus.Confirmed,
            GuestName = "Test User",
            GuestPhone = "+905551234567",
            PartySize = 4,
            ReservedFor = reservedFor,
            EndTime = reservedFor.AddMinutes(90),
            Venue = new Venue
            {
                Id = venueId,
                TenantId = tenantId,
                Name = "Test Venue",
                SlotDurationMinutes = 90
            }
        };
    }

    private void SetupReservationMock(Reservation reservation)
    {
        var reservationRepoMock = new Mock<IRepository<Reservation>>();
        var list = new List<Reservation> { reservation }.AsQueryable();

        var mockDbSet = new Mock<DbSet<Reservation>>();
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Reservation>(list.Provider));
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(list.Expression);
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(list.ElementType);
        mockDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(list.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<Reservation>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Reservation>(list.GetEnumerator()));

        reservationRepoMock.Setup(x => x.Query()).Returns(mockDbSet.Object);
        reservationRepoMock.Setup(x => x.Add(It.IsAny<Reservation>()));
        _unitOfWorkMock.Setup(x => x.Reservations).Returns(reservationRepoMock.Object);
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

    private void SetupSlotAvailableMock()
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
            .ReturnsAsync(SlotAvailabilityResult.Available(Guid.NewGuid()));
    }

    private void SetupRuleAllowMock()
    {
        _ruleEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<RuleEvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RuleEvaluationResult.Allow());
    }

    private void SetupAllRepositoriesMock(Reservation existingReservation)
    {
        // StatusLog
        var statusLogRepoMock = new Mock<IRepository<ReservationStatusLog>>();
        statusLogRepoMock.Setup(x => x.Add(It.IsAny<ReservationStatusLog>()));
        _unitOfWorkMock.Setup(x => x.ReservationStatusLogs).Returns(statusLogRepoMock.Object);

        // AuditLog
        var auditLogRepoMock = new Mock<IRepository<AuditLog>>();
        auditLogRepoMock.Setup(x => x.Add(It.IsAny<AuditLog>()));
        _unitOfWorkMock.Setup(x => x.AuditLogs).Returns(auditLogRepoMock.Object);

        // Tables
        var tableRepoMock = new Mock<IRepository<Table>>();
        var emptyTables = new List<Table>().AsQueryable();
        var mockTableDbSet = new Mock<DbSet<Table>>();
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Table>(emptyTables.Provider));
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.Expression).Returns(emptyTables.Expression);
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.ElementType).Returns(emptyTables.ElementType);
        mockTableDbSet.As<IQueryable<Table>>().Setup(m => m.GetEnumerator()).Returns(emptyTables.GetEnumerator());
        mockTableDbSet.As<IAsyncEnumerable<Table>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Table>(emptyTables.GetEnumerator()));
        tableRepoMock.Setup(x => x.Query()).Returns(mockTableDbSet.Object);
        _unitOfWorkMock.Setup(x => x.Tables).Returns(tableRepoMock.Object);

        // TableCombinations
        var comboRepoMock = new Mock<IRepository<TableCombination>>();
        var emptyCombos = new List<TableCombination>().AsQueryable();
        var mockComboDbSet = new Mock<DbSet<TableCombination>>();
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TableCombination>(emptyCombos.Provider));
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.Expression).Returns(emptyCombos.Expression);
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.ElementType).Returns(emptyCombos.ElementType);
        mockComboDbSet.As<IQueryable<TableCombination>>().Setup(m => m.GetEnumerator()).Returns(emptyCombos.GetEnumerator());
        mockComboDbSet.As<IAsyncEnumerable<TableCombination>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<TableCombination>(emptyCombos.GetEnumerator()));
        comboRepoMock.Setup(x => x.Query()).Returns(mockComboDbSet.Object);
        _unitOfWorkMock.Setup(x => x.TableCombinations).Returns(comboRepoMock.Object);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _slotServiceMock.Setup(x => x.InvalidateCacheAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    #endregion
}
