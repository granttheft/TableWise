using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.TableCombination.Commands;

/// <summary>
/// Masa kombinasyonu güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateTableCombinationCommandHandler : IRequestHandler<UpdateTableCombinationCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateTableCombinationCommandHandler> _logger;

    public UpdateTableCombinationCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateTableCombinationCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateTableCombinationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kombinasyon güncelleyebilir.");
        }

        // Kombinasyon bul
        var combination = await _dbContext.TableCombinations
            .FirstOrDefaultAsync(tc => 
                tc.Id == request.CombinationId && 
                tc.VenueId == request.VenueId && 
                tc.TenantId == tenantId && 
                !tc.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (combination == null)
        {
            throw new NotFoundException("TableCombination", request.CombinationId);
        }

        // Minimum 2 masa kontrolü
        if (request.TableIds.Count < 2)
        {
            throw new BusinessRuleException(
                "Kombinasyon için en az 2 masa seçilmelidir.",
                "MINIMUM_TWO_TABLES");
        }

        // Name değişiyorsa unique kontrolü
        if (combination.Name.ToLower() != request.Name.ToLower())
        {
            var nameExists = await _dbContext.TableCombinations
                .AnyAsync(tc => 
                    tc.VenueId == request.VenueId && 
                    tc.Name.ToLower() == request.Name.ToLower() && 
                    tc.Id != request.CombinationId &&
                    !tc.IsDeleted,
                    cancellationToken)
                .ConfigureAwait(false);

            if (nameExists)
            {
                throw new BusinessRuleException(
                    $"'{request.Name}' adında bir kombinasyon zaten mevcut.",
                    "COMBINATION_NAME_EXISTS");
            }
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

        // Kapasite kontrolü
        if (request.CombinedCapacity <= 0)
        {
            throw new BusinessRuleException(
                "Birleşik kapasite 0'dan büyük olmalıdır.",
                "INVALID_CAPACITY");
        }

        // Eski değerleri kaydet
        var oldValues = new
        {
            combination.Name,
            TableIds = combination.TableIds,
            combination.CombinedCapacity
        };

        // Güncelle
        combination.Name = request.Name;
        combination.TableIds = System.Text.Json.JsonSerializer.Serialize(request.TableIds);
        combination.CombinedCapacity = request.CombinedCapacity;
        combination.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "COMBINATION_UPDATED",
            EntityType = "TableCombination",
            EntityId = combination.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Name,
                request.TableIds,
                request.CombinedCapacity
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Masa kombinasyonu güncellendi: CombinationId={CombinationId}", combination.Id);

        return Unit.Value;
    }
}
