using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Venue entity konfigürasyonu.
/// </summary>
public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    /// <summary>
    /// Venue entity yapılandırması.
    /// </summary>
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");

        builder.HasKey(v => v.Id);

        // Properties
        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Address)
            .HasMaxLength(500);

        builder.Property(v => v.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Europe/Istanbul");

        builder.Property(v => v.SlotDurationMinutes)
            .HasDefaultValue(90);

        builder.Property(v => v.WorkingHours)
            .HasColumnType("jsonb");

        builder.Property(v => v.LogoUrl)
            .HasMaxLength(500);

        builder.Property(v => v.DepositEnabled)
            .HasDefaultValue(false);

        builder.Property(v => v.DepositAmount)
            .HasPrecision(10, 2);

        builder.Property(v => v.DepositPerPerson)
            .HasDefaultValue(false);

        builder.Property(v => v.DepositRefundPolicy)
            .HasConversion<string>();

        builder.Property(v => v.DepositPartialPercent)
            .HasPrecision(5, 2);

        builder.Property(v => v.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(v => v.Description)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(v => v.TenantId);

        builder.HasIndex(v => new { v.Name, v.TenantId });

        // Relationships
        builder.HasMany(v => v.Tables)
            .WithOne(t => t.Venue)
            .HasForeignKey(t => t.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Rules)
            .WithOne(r => r.Venue)
            .HasForeignKey(r => r.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Reservations)
            .WithOne(r => r.Venue)
            .HasForeignKey(r => r.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Closures)
            .WithOne(c => c.Venue)
            .HasForeignKey(c => c.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.CustomFields)
            .WithOne(c => c.Venue)
            .HasForeignKey(c => c.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.TableCombinations)
            .WithOne(tc => tc.Venue)
            .HasForeignKey(tc => tc.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
