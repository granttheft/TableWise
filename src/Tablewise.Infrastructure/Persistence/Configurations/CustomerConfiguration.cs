using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Customer entity konfigürasyonu.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <summary>
    /// Customer entity yapılandırması.
    /// </summary>
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.Tier)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.CustomerTier.Regular);

        builder.Property(c => c.IsBlacklisted)
            .HasDefaultValue(false);

        builder.Property(c => c.BlacklistReason)
            .HasMaxLength(1000);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.Property(c => c.TotalVisits)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(c => c.TenantId);

        builder.HasIndex(c => new { c.Phone, c.TenantId })
            .IsUnique();

        builder.HasIndex(c => c.Email);

        builder.HasIndex(c => c.Tier);

        // Relationships - Reservations relationship Reservation entity'de tanımlı
    }
}
