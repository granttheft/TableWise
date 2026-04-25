using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tablewise.Application.DTOs.Auth;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Infrastructure.Persistence;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Tablewise.Infrastructure.Auth;

/// <summary>
/// Authentication ve authorization servisi.
/// Brute-force koruması, token rotation, audit logging dahil.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly TablewiseDbContext _dbContext;
    private readonly IJwtTokenService _jwtService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly AuthSettings _authSettings;
    private readonly ILogger<AuthService> _logger;

    private const string LoginFailKeyPrefix = "login_fail";

    /// <summary>
    /// AuthService constructor.
    /// </summary>
    public AuthService(
        TablewiseDbContext dbContext,
        IJwtTokenService jwtService,
        ICacheService cacheService,
        IEmailService emailService,
        IOptions<AuthSettings> authSettings,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _cacheService = cacheService;
        _emailService = emailService;
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> RegisterTenantAsync(
        RegisterTenantDto dto,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        // Email global unique kontrolü
        var emailLower = dto.Email.ToLowerInvariant();
        var emailExists = await _dbContext.Tenants
            .AnyAsync(t => t.Email.ToLower() == emailLower, cancellationToken)
            .ConfigureAwait(false);

        if (emailExists)
        {
            throw new BusinessRuleException(
                "Bu email adresi zaten kayıtlı.",
                "EMAIL_ALREADY_EXISTS");
        }

        var userEmailExists = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == emailLower, cancellationToken)
            .ConfigureAwait(false);

        if (userEmailExists)
        {
            throw new BusinessRuleException(
                "Bu email adresi zaten kayıtlı.",
                "EMAIL_ALREADY_EXISTS");
        }

        // Slug üret ve benzersizliği kontrol et
        var baseSlug = SlugGenerator.Generate(dto.BusinessName);
        var slug = baseSlug;
        var slugSuffix = 1;

        while (await _dbContext.Tenants
            .AnyAsync(t => t.Slug == slug, cancellationToken)
            .ConfigureAwait(false))
        {
            slugSuffix++;
            slug = SlugGenerator.MakeUnique(baseSlug, slugSuffix);
        }

        // Starter plan'ı al
        var starterPlan = await _dbContext.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Tier == PlanTier.Starter && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new BusinessRuleException(
                "Starter plan bulunamadı. Lütfen yönetici ile iletişime geçin.",
                "PLAN_NOT_FOUND");

        // Password hash
        var passwordHash = BCryptNet.HashPassword(dto.Password, _authSettings.BcryptWorkFactor);

        // Email verification token
        var emailVerificationToken = Guid.NewGuid().ToString("N");

        // Tenant oluştur
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = dto.BusinessName.Trim(),
            Slug = slug,
            Email = emailLower,
            PasswordHash = passwordHash,
            PlanId = starterPlan.Id,
            PlanStatus = PlanStatus.Trial,
            TrialEndsAt = DateTime.UtcNow.AddDays(_authSettings.TrialDays),
            IsEmailVerified = false,
            EmailVerificationToken = emailVerificationToken,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Tenants.Add(tenant);

        // Owner user oluştur
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = emailLower,
            PasswordHash = passwordHash,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Role = UserRole.Owner,
            PhoneNumber = dto.PhoneNumber?.Trim(),
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = emailVerificationToken,
            EmailVerificationExpiry = DateTime.UtcNow.AddHours(_authSettings.EmailVerificationTokenExpirationHours),
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            PerformedBy = user.Email,
            Action = "TENANT_REGISTERED",
            EntityType = "Tenant",
            EntityId = tenant.Id.ToString(),
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        // Refresh token oluştur
        var (refreshToken, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RevocableRefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = refreshExpiresAt,
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Set<RevocableRefreshToken>().Add(refreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Hoşgeldin emaili gönder (fire and forget - hata olsa bile kayıt tamamlandı)
        _ = SendWelcomeEmailSafeAsync(user, emailVerificationToken);

        // Access token üret
        var (accessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user, tenant, starterPlan.Tier);

        _logger.LogInformation(
            "Yeni tenant kaydı: {TenantId}, Slug: {Slug}, Email: {Email}",
            tenant.Id, tenant.Slug, "***");

        return BuildAuthResult(user, tenant, starterPlan.Tier, accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> LoginAsync(
        LoginDto dto,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var emailLower = dto.Email.ToLowerInvariant();

        // Brute-force koruması
        await CheckBruteForceProtectionAsync(ipAddress, emailLower, cancellationToken).ConfigureAwait(false);

        // Kullanıcıyı bul
        var user = await _dbContext.Users
            .Include(u => u.Tenant)
                .ThenInclude(t => t!.Plan)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            await RecordFailedLoginAsync(ipAddress, emailLower, cancellationToken).ConfigureAwait(false);
            throw new BusinessRuleException(
                "Email veya şifre hatalı.",
                "INVALID_CREDENTIALS");
        }

        // Şifre kontrolü
        if (!BCryptNet.Verify(dto.Password, user.PasswordHash))
        {
            await RecordFailedLoginAsync(ipAddress, emailLower, cancellationToken).ConfigureAwait(false);
            throw new BusinessRuleException(
                "Email veya şifre hatalı.",
                "INVALID_CREDENTIALS");
        }

        // Kullanıcı aktif mi?
        if (!user.IsActive)
        {
            throw new BusinessRuleException(
                "Hesabınız devre dışı bırakılmış. Destek ile iletişime geçin: " + _authSettings.SupportEmail,
                "ACCOUNT_DISABLED");
        }

        // Email doğrulandı mı?
        if (!user.IsEmailVerified)
        {
            throw new BusinessRuleException(
                "Email adresinizi doğrulamanız gerekiyor. Lütfen email kutunuzu kontrol edin.",
                "EMAIL_NOT_VERIFIED");
        }

        var tenant = user.Tenant!;

        // Tenant aktif mi?
        if (!tenant.IsActive)
        {
            throw new BusinessRuleException(
                "İşletme hesabınız devre dışı bırakılmış. Destek ile iletişime geçin: " + _authSettings.SupportEmail,
                "TENANT_DISABLED");
        }

        // Plan durumu kontrolü
        if (tenant.PlanStatus == PlanStatus.Suspended)
        {
            throw new BusinessRuleException(
                "Aboneliğiniz askıya alınmış. Ödeme yaparak hesabınızı aktifleştirin veya destek ile iletişime geçin: " + _authSettings.SupportEmail,
                "SUBSCRIPTION_SUSPENDED");
        }

        if (tenant.PlanStatus == PlanStatus.Cancelled)
        {
            throw new BusinessRuleException(
                "Aboneliğiniz iptal edilmiş. Yeni abonelik başlatmak için destek ile iletişime geçin: " + _authSettings.SupportEmail,
                "SUBSCRIPTION_CANCELLED");
        }

        // Başarılı giriş - fail counter'ı sıfırla
        await ClearFailedLoginAttemptsAsync(ipAddress, emailLower, cancellationToken).ConfigureAwait(false);

        // LastLoginAt güncelle
        user.LastLoginAt = DateTime.UtcNow;

        // Refresh token oluştur
        var (refreshToken, refreshExpiresAt) = _jwtService.GenerateRefreshToken(dto.RememberMe);

        var refreshTokenEntity = new RevocableRefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = refreshExpiresAt,
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Set<RevocableRefreshToken>().Add(refreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Access token üret
        var planTier = tenant.Plan?.Tier ?? PlanTier.Starter;
        var (accessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user, tenant, planTier);

        _logger.LogInformation(
            "Kullanıcı girişi: {UserId}, Tenant: {TenantId}",
            user.Id, tenant.Id);

        return BuildAuthResult(user, tenant, planTier, accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    /// <inheritdoc />
    public async Task<TokenResponseDto> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.Set<RevocableRefreshToken>()
            .Include(rt => rt.User)
                .ThenInclude(u => u!.Tenant)
                    .ThenInclude(t => t!.Plan)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (existingToken == null)
        {
            throw new UnauthorizedException("Geçersiz refresh token.");
        }

        if (!existingToken.IsActive)
        {
            // Token zaten kullanılmış veya revoke edilmiş - potansiyel token theft
            _logger.LogWarning(
                "Kullanılmış refresh token ile erişim denemesi. UserId: {UserId}, Token: {Token}",
                existingToken.UserId, "***");

            // Güvenlik: Bu kullanıcının tüm token'larını revoke et
            await RevokeAllUserTokensAsync(existingToken.UserId, "Token theft detected", cancellationToken)
                .ConfigureAwait(false);

            throw new UnauthorizedException("Oturumunuz sonlandırıldı. Lütfen tekrar giriş yapın.");
        }

        var user = existingToken.User!;
        var tenant = user.Tenant!;

        // Eski token'ı revoke et (rotation)
        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedBy = "Token Rotation";
        existingToken.RevokedByIp = ipAddress;

        // Yeni refresh token oluştur
        var (newRefreshToken, newRefreshExpiresAt) = _jwtService.GenerateRefreshToken();
        existingToken.ReplacedByToken = newRefreshToken;

        var newRefreshTokenEntity = new RevocableRefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = newRefreshExpiresAt,
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Set<RevocableRefreshToken>().Add(newRefreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Yeni access token üret
        var planTier = tenant.Plan?.Tier ?? PlanTier.Starter;
        var (accessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user, tenant, planTier);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = newRefreshExpiresAt
        };
    }

    /// <inheritdoc />
    public async Task LogoutAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.Set<RevocableRefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (existingToken == null)
        {
            return; // Token zaten yok/silinmiş - idempotent davranış
        }

        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedBy = "User Logout";
        existingToken.RevokedByIp = ipAddress;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Kullanıcı çıkışı: UserId: {UserId}", existingToken.UserId);
    }

    /// <inheritdoc />
    public async Task<bool> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(
                u => u.EmailVerificationToken == token && !u.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            throw new BusinessRuleException(
                "Geçersiz veya süresi dolmuş doğrulama linki.",
                "INVALID_VERIFICATION_TOKEN");
        }

        if (user.EmailVerificationExpiry.HasValue && user.EmailVerificationExpiry < DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Doğrulama linkinin süresi dolmuş. Lütfen yeni link isteyin.",
                "VERIFICATION_TOKEN_EXPIRED");
        }

        if (user.IsEmailVerified)
        {
            return true; // Zaten doğrulanmış
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationExpiry = null;

        // Tenant'ı da güncelle (owner ise)
        if (user.Role == UserRole.Owner && user.Tenant != null)
        {
            user.Tenant.IsEmailVerified = true;
            user.Tenant.EmailVerificationToken = null;
        }

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId,
            UserId = user.Id,
            PerformedBy = user.Email,
            Action = "EMAIL_VERIFIED",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Doğrulama başarılı emaili gönder
        _ = _emailService.SendEmailVerifiedNotificationAsync(user.Email, user.FirstName, cancellationToken);

        _logger.LogInformation("Email doğrulandı: UserId: {UserId}", user.Id);

        return true;
    }

    /// <inheritdoc />
    public async Task ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var emailLower = email.ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        // Güvenlik: Email bulunamasa bile başarılı döneriz (user enumeration engelleme)
        if (user == null)
        {
            _logger.LogInformation("Şifre sıfırlama isteği - email bulunamadı: {Email}", "***");
            return;
        }

        // Token oluştur
        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(_authSettings.PasswordResetTokenExpirationHours);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Email gönder
        var resetLink = $"{_authSettings.AdminPanelUrl}/reset-password?token={resetToken}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, resetLink, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Şifre sıfırlama emaili gönderildi: UserId: {UserId}", user.Id);
    }

    /// <inheritdoc />
    public async Task<bool> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            throw new BusinessRuleException(
                "Geçersiz veya süresi dolmuş şifre sıfırlama linki.",
                "INVALID_RESET_TOKEN");
        }

        if (user.PasswordResetExpiry.HasValue && user.PasswordResetExpiry < DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Şifre sıfırlama linkinin süresi dolmuş. Lütfen yeni link isteyin.",
                "RESET_TOKEN_EXPIRED");
        }

        // Yeni şifre hash
        user.PasswordHash = BCryptNet.HashPassword(newPassword, _authSettings.BcryptWorkFactor);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;

        // Güvenlik: Tüm refresh token'ları revoke et
        await RevokeAllUserTokensAsync(user.Id, "Password Reset", cancellationToken).ConfigureAwait(false);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId,
            UserId = user.Id,
            PerformedBy = user.Email,
            Action = "PASSWORD_RESET",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Şifre sıfırlandı: UserId: {UserId}", user.Id);

        return true;
    }

    #region Private Methods

    /// <summary>
    /// Brute-force koruması kontrolü.
    /// </summary>
    private async Task CheckBruteForceProtectionAsync(
        string? ipAddress,
        string email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return;
        }

        var key = $"{LoginFailKeyPrefix}:{ipAddress}:{email}";
        var failCount = await _cacheService.GetAsync<long>(key, cancellationToken).ConfigureAwait(false);

        if (failCount >= _authSettings.MaxFailedLoginAttempts)
        {
            var ttl = await _cacheService.GetTimeToLiveAsync(key, cancellationToken).ConfigureAwait(false);
            var remainingMinutes = ttl?.TotalMinutes ?? _authSettings.LockoutDurationMinutes;

            throw new BusinessRuleException(
                $"Çok fazla başarısız giriş denemesi. Lütfen {Math.Ceiling(remainingMinutes)} dakika sonra tekrar deneyin.",
                "ACCOUNT_LOCKED");
        }
    }

    /// <summary>
    /// Başarısız giriş kaydeder.
    /// </summary>
    private async Task RecordFailedLoginAsync(
        string? ipAddress,
        string email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return;
        }

        var key = $"{LoginFailKeyPrefix}:{ipAddress}:{email}";
        await _cacheService.IncrementAsync(
            key,
            TimeSpan.FromMinutes(_authSettings.LockoutDurationMinutes),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Başarısız giriş sayacını temizler.
    /// </summary>
    private async Task ClearFailedLoginAttemptsAsync(
        string? ipAddress,
        string email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return;
        }

        var key = $"{LoginFailKeyPrefix}:{ipAddress}:{email}";
        await _cacheService.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Kullanıcının tüm refresh token'larını revoke eder.
    /// </summary>
    private async Task RevokeAllUserTokensAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken)
    {
        var tokens = await _dbContext.Set<RevocableRefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedBy = reason;
        }
    }

    /// <summary>
    /// Hoşgeldin emaili güvenli gönderir (hata yutulur).
    /// </summary>
    private async Task SendWelcomeEmailSafeAsync(User user, string verificationToken)
    {
        try
        {
            var verificationLink = $"{_authSettings.AdminPanelUrl}/verify-email?token={verificationToken}";
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, verificationLink)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hoşgeldin emaili gönderilemedi: UserId: {UserId}", user.Id);
        }
    }

    /// <summary>
    /// AuthResult DTO oluşturur.
    /// </summary>
    private static AuthResultDto BuildAuthResult(
        User user,
        Tenant tenant,
        PlanTier planTier,
        string accessToken,
        DateTime accessExpiresAt,
        string refreshToken,
        DateTime refreshExpiresAt)
    {
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

    #endregion
}
