using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// RevocableRefreshToken entity için EF Core yapılandırması.
/// </summary>
public class RevocableRefreshTokenConfiguration : IEntityTypeConfiguration<RevocableRefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RevocableRefreshToken> builder)
    {
        builder.ToTable("revocable_refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ReplacedByToken)
            .HasMaxLength(256);

        builder.Property(x => x.RevokedBy)
            .HasMaxLength(256);

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(x => x.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        // Token üzerinde unique index
        builder.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token");

        // TenantId + UserId compound index (kullanıcının tüm token'larını çekmek için)
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("ix_refresh_tokens_tenant_user");

        // ExpiresAt index (expired token temizliği için)
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_refresh_tokens_expires_at");

        // User relationship
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tenant relationship
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
