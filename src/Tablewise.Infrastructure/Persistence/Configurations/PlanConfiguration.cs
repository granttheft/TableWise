using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Plan entity konfigürasyonu.
/// </summary>
public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    /// <summary>
    /// Plan entity yapılandırması.
    /// </summary>
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Tier)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.MonthlyPriceTry)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(p => p.YearlyPriceTry)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(p => p.FeaturesJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(p => p.LimitsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(p => p.IsVisible)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(p => p.Tier)
            .IsUnique();

        builder.HasIndex(p => p.IsVisible);

        // Relationships - Tenants ve Subscriptions relationships diğer entity'lerde tanımlı
    }
}
