using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tablewise.Application.DTOs.Auth;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Exceptions;
using Tablewise.Application.Settings;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Davet kabul komutu handler'ı.
/// </summary>
public sealed class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtService;
    private readonly AuthSettings _authSettings;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    /// <summary>
    /// AcceptInvitationCommandHandler constructor.
    /// </summary>
    public AcceptInvitationCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenService jwtService,
        IOptions<AuthSettings> authSettings,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        // Invitation bul
        var invitation = await _dbContext.UserInvitations
            .Include(inv => inv.Tenant)
                .ThenInclude(t => t!.Plan)
            .FirstOrDefaultAsync(inv => inv.Token == request.Token && !inv.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (invitation == null)
        {
            throw new BusinessRuleException(
                "Geçersiz davet linki.",
                "INVALID_INVITATION_TOKEN");
        }

        // Süresi dolmuş mu?
        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Davet linkinin süresi dolmuş. Lütfen yeni bir davet isteyin.",
                "INVITATION_EXPIRED");
        }

        // Zaten kabul edilmiş mi?
        if (invitation.AcceptedAt.HasValue)
        {
            throw new BusinessRuleException(
                "Bu davet zaten kabul edilmiş.",
                "INVITATION_ALREADY_ACCEPTED");
        }

        var tenant = invitation.Tenant!;

        // Email bu tenant'ta zaten kayıtlı mı? (race condition önleme)
        var existingUser = await _dbContext.Users
            .AnyAsync(u =>
                u.TenantId == tenant.Id &&
                u.Email.ToLower() == invitation.Email.ToLower() &&
                !u.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (existingUser)
        {
            throw new BusinessRuleException(
                "Bu email adresi zaten kayıtlı.",
                "EMAIL_ALREADY_EXISTS");
        }

        // Password hash
        var passwordHash = BCryptNet.HashPassword(request.Password, _authSettings.BcryptWorkFactor);

        // User oluştur
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = invitation.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = invitation.Role,
            IsActive = true,
            IsEmailVerified = true, // Davet ile geldiği için doğrulanmış sayılır
            InvitedAt = invitation.CreatedAt,
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);

        // Invitation'ı işaretle
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.UpdatedAt = DateTime.UtcNow;

        // Refresh token oluştur
        var (refreshToken, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RevocableRefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = refreshExpiresAt,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Set<RevocableRefreshToken>().Add(refreshTokenEntity);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            PerformedBy = user.Email,
            Action = "STAFF_JOINED",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            IpAddress = request.IpAddress,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Access token üret
        var planTier = tenant.Plan?.Tier ?? Domain.Enums.PlanTier.Starter;
        var (accessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user, tenant, planTier);

        _logger.LogInformation(
            "Personel daveti kabul edildi: UserId={UserId}, TenantId={TenantId}",
            user.Id, tenant.Id);

        return new AuthResultDto
        {
            Tokens = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshTokenExpiresAt = refreshExpiresAt
            },
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified
            },
            Tenant = new TenantInfoDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                PlanTier = planTier,
                PlanStatus = tenant.PlanStatus,
                TrialEndsAt = tenant.TrialEndsAt
            }
        };
    }
}
