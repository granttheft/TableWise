using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Reservation entity konfigürasyonu.
/// </summary>
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    /// <summary>
    /// Reservation entity yapılandırması.
    /// </summary>
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.GuestName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.GuestEmail)
            .HasMaxLength(200);

        builder.Property(r => r.GuestPhone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.PartySize)
            .IsRequired();

        builder.Property(r => r.ReservedFor)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Source)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.ConfirmCode)
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(r => r.SpecialRequests)
            .HasMaxLength(2000);

        builder.Property(r => r.InternalNotes)
            .HasMaxLength(2000);

        builder.Property(r => r.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(r => r.AppliedRulesSnapshot)
            .HasColumnType("jsonb");

        builder.Property(r => r.CustomFieldAnswers)
            .HasColumnType("jsonb");

        builder.Property(r => r.DepositStatus)
            .HasConversion<string>();

        builder.Property(r => r.DepositAmount)
            .HasPrecision(10, 2);

        builder.Property(r => r.DepositPaymentRef)
            .HasMaxLength(200);

        builder.Property(r => r.CancellationReason)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(r => r.TenantId);

        builder.HasIndex(r => r.VenueId);

        builder.HasIndex(r => r.TableId);

        builder.HasIndex(r => r.CustomerId);

        builder.HasIndex(r => r.ConfirmCode)
            .IsUnique();

        builder.HasIndex(r => r.Status);

        builder.HasIndex(r => r.ReservedFor);

        builder.HasIndex(r => new { r.VenueId, r.ReservedFor });

        builder.HasIndex(r => new { r.TableId, r.ReservedFor });

        // Relationships
        builder.HasOne(r => r.Table)
            .WithMany(t => t.Reservations)
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.TableCombination)
            .WithMany(tc => tc.Reservations)
            .HasForeignKey(r => r.TableCombinationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ModifiedFromReservation)
            .WithMany(r => r.ModifiedReservations)
            .HasForeignKey(r => r.ModifiedFromReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.AppliedRules)
            .WithOne(ar => ar.Reservation)
            .HasForeignKey(ar => ar.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.StatusLogs)
            .WithOne(sl => sl.Reservation)
            .HasForeignKey(sl => sl.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.NotificationLogs)
            .WithOne(nl => nl.Reservation)
            .HasForeignKey(nl => nl.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
