using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.TableCombination.Commands;

/// <summary>
/// Masa kombinasyonu oluşturma komutu handler'ı.
/// </summary>
public sealed class CreateTableCombinationCommandHandler : IRequestHandler<CreateTableCombinationCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateTableCombinationCommandHandler> _logger;

    public CreateTableCombinationCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<CreateTableCombinationCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTableCombinationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kombinasyon oluşturabilir.");
        }

        // Venue kontrolü
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Minimum 2 masa kontrolü
        if (request.TableIds.Count < 2)
        {
            throw new BusinessRuleException(
                "Kombinasyon için en az 2 masa seçilmelidir.",
                "MINIMUM_TWO_TABLES");
        }

        // Name unique kontrolü (venue içinde)
        var nameExists = await _dbContext.TableCombinations
            .AnyAsync(tc => 
                tc.VenueId == request.VenueId && 
                tc.Name.ToLower() == request.Name.ToLower() && 
                !tc.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (nameExists)
        {
            throw new BusinessRuleException(
                $"'{request.Name}' adında bir kombinasyon zaten mevcut.",
                "COMBINATION_NAME_EXISTS");
        }

        // Masaları kontrol et
        var tables = await _dbContext.Tables
            .Where(t => 
                request.TableIds.Contains(t.Id) && 
                t.TenantId == tenantId && 
                !t.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tables.Count != request.TableIds.Count)
        {
            throw new BusinessRuleException(
                "Bazı masalar bulunamadı.",
                "TABLES_NOT_FOUND");
        }

        // Tüm masalar aynı venue'de mi?
        if (tables.Any(t => t.VenueId != request.VenueId))
        {
            throw new BusinessRuleException(
                "Tüm masalar aynı venue'de olmalıdır.",
                "TABLES_DIFFERENT_VENUE");
        }

        // Tüm masalar aktif mi?
        if (tables.Any(t => !t.IsActive))
        {
            throw new BusinessRuleException(
                "Kombinasyondaki tüm masalar aktif olmalıdır.",
                "INACTIVE_TABLES");
        }

        // Kapasite hesaplama (manuel veya otomatik)
        var combinedCapacity = request.CombinedCapacity ?? tables.Sum(t => t.Capacity);

        if (combinedCapacity <= 0)
        {
            throw new BusinessRuleException(
                "Birleşik kapasite 0'dan büyük olmalıdır.",
                "INVALID_CAPACITY");
        }

        // Kombinasyon oluştur
        var combination = new Domain.Entities.TableCombination
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = request.VenueId,
            Name = request.Name,
            TableIds = System.Text.Json.JsonSerializer.Serialize(request.TableIds),
            CombinedCapacity = combinedCapacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.TableCombinations.Add(combination);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "COMBINATION_CREATED",
            EntityType = "TableCombination",
            EntityId = combination.Id.ToString(),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Name,
                TableCount = request.TableIds.Count,
                combinedCapacity
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Masa kombinasyonu oluşturuldu: VenueId={VenueId}, CombinationId={CombinationId}, Name={Name}",
            request.VenueId, combination.Id, request.Name);

        return combination.Id;
    }
}
