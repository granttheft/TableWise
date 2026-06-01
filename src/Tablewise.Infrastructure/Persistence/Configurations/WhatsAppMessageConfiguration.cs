using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// WhatsAppMessage entity EF Core yapılandırması.
/// </summary>
internal sealed class WhatsAppMessageConfiguration : IEntityTypeConfiguration<WhatsAppMessage>
{
    public void Configure(EntityTypeBuilder<WhatsAppMessage> builder)
    {
        builder.ToTable("WhatsAppMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ToPhone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Template)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ProviderMessageId)
            .HasMaxLength(64);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(500);

        builder.HasIndex(x => x.ReservationId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.TenantId, x.Template });

        builder.HasOne(x => x.Reservation)
            .WithMany()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
