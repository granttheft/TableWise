using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tenant entity konfigürasyonu.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    /// <summary>
    /// Tenant entity yapılandırması.
    /// </summary>
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.PlanId)
            .IsRequired();

        builder.Property(t => t.PlanStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Settings)
            .HasColumnType("jsonb");

        builder.Property(t => t.ReservationCountThisMonth)
            .HasDefaultValue(0);

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.Property(t => t.EmailVerificationToken)
            .HasMaxLength(500);

        builder.Property(t => t.PasswordResetToken)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.HasIndex(t => t.Email)
            .IsUnique();

        builder.HasIndex(t => t.PlanId);

        // Relationships
        builder.HasOne(t => t.Plan)
            .WithMany(p => p.Tenants)
            .HasForeignKey(t => t.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Venues)
            .WithOne(v => v.Tenant)
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Subscriptions)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
