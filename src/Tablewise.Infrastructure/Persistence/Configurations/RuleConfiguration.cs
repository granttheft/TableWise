using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Rule entity konfigürasyonu.
/// </summary>
public class RuleConfiguration : IEntityTypeConfiguration<Rule>
{
    /// <summary>
    /// Rule entity yapılandırması.
    /// </summary>
    public void Configure(EntityTypeBuilder<Rule> builder)
    {
        builder.ToTable("Rules");

        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.RuleType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.ConditionsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(r => r.ActionsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(r => r.Priority)
            .HasDefaultValue(100);

        builder.Property(r => r.TriggerType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.Property(r => r.ApplicableTimeSlots)
            .HasColumnType("jsonb");

        builder.Property(r => r.TimesTriggered)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(r => r.TenantId);

        builder.HasIndex(r => r.VenueId);

        builder.HasIndex(r => r.IsActive);

        builder.HasIndex(r => new { r.TenantId, r.IsActive, r.Priority });

        // Relationships - Venue relationship Venue entity'de tanımlı
    }
}
