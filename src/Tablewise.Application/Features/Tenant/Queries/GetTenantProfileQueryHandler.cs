using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Tenant profil sorgusu handler'ı.
/// </summary>
public sealed class GetTenantProfileQueryHandler : IRequestHandler<GetTenantProfileQuery, TenantProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetTenantProfileQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<TenantProfileDto> Handle(GetTenantProfileQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var tenant = await _dbContext.Tenants
            .Include(t => t.Plan)
            .Where(t => t.Id == tenantId)
            .Select(t => new TenantProfileDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Email = t.Email,
                LogoUrl = t.Settings != null ? ExtractLogoUrl(t.Settings) : null,
                PlanTier = t.Plan!.Tier,
                PlanName = t.Plan.Name,
                PlanStatus = t.PlanStatus,
                TrialEndsAt = t.TrialEndsAt,
                PlanRenewsAt = t.PlanRenewsAt,
                IsEmailVerified = t.IsEmailVerified,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new NotFoundException("Tenant", tenantId);
        }

        return tenant;
    }

    private static string? ExtractLogoUrl(string settingsJson)
    {
        // Settings JSON'dan logoUrl çıkar (basit implementation)
        // Production'da System.Text.Json kullan
        return null; // TODO: JSON parse
    }
}
