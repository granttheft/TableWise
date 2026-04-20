using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Persistence.Configurations;

/// <summary>
/// UserInvitation entity konfigürasyonu.
/// </summary>
public class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable("UserInvitations");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Role).IsRequired().HasConversion<string>();
        builder.Property(u => u.Token).IsRequired().HasMaxLength(500);

        builder.HasIndex(u => u.Token).IsUnique();
        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.ExpiresAt);
    }
}

/// <summary>
/// VenueClosure entity konfigürasyonu.
/// </summary>
public class VenueClosureConfiguration : IEntityTypeConfiguration<VenueClosure>
{
    public void Configure(EntityTypeBuilder<VenueClosure> builder)
    {
        builder.ToTable("VenueClosures");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.IsFullDay).IsRequired();
        builder.Property(v => v.Reason).HasMaxLength(500);

        builder.HasIndex(v => new { v.VenueId, v.Date }).IsUnique();
        builder.HasIndex(v => v.TenantId);
    }
}

/// <summary>
/// VenueCustomField entity konfigürasyonu.
/// </summary>
public class VenueCustomFieldConfiguration : IEntityTypeConfiguration<VenueCustomField>
{
    public void Configure(EntityTypeBuilder<VenueCustomField> builder)
    {
        builder.ToTable("VenueCustomFields");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Label).IsRequired().HasMaxLength(200);
        builder.Property(v => v.FieldType).IsRequired().HasConversion<string>();
        builder.Property(v => v.Options).HasColumnType("jsonb");

        builder.HasIndex(v => v.VenueId);
        builder.HasIndex(v => v.TenantId);
    }
}

/// <summary>
/// Table entity konfigürasyonu.
/// </summary>
public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("Tables");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Capacity).IsRequired();
        builder.Property(t => t.Location).IsRequired().HasConversion<string>();
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.IsActive).HasDefaultValue(true);

        builder.HasIndex(t => t.VenueId);
        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => new { t.VenueId, t.IsActive });
    }
}

/// <summary>
/// TableCombination entity konfigürasyonu.
/// </summary>
public class TableCombinationConfiguration : IEntityTypeConfiguration<TableCombination>
{
    public void Configure(EntityTypeBuilder<TableCombination> builder)
    {
        builder.ToTable("TableCombinations");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.TableIds).IsRequired().HasColumnType("jsonb");
        builder.Property(t => t.CombinedCapacity).IsRequired();
        builder.Property(t => t.IsActive).HasDefaultValue(true);

        builder.HasIndex(t => t.VenueId);
        builder.HasIndex(t => t.TenantId);
    }
}

/// <summary>
/// AppliedRule entity konfigürasyonu.
/// </summary>
public class AppliedRuleConfiguration : IEntityTypeConfiguration<AppliedRule>
{
    public void Configure(EntityTypeBuilder<AppliedRule> builder)
    {
        builder.ToTable("AppliedRules");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActionType).IsRequired().HasConversion<string>();
        builder.Property(a => a.ActionPayload).HasColumnType("jsonb");

        builder.HasIndex(a => a.ReservationId);
        builder.HasIndex(a => a.RuleId);

        builder.HasOne(a => a.Rule)
            .WithMany(r => r.AppliedRules)
            .HasForeignKey(a => a.RuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// ReservationStatusLog entity konfigürasyonu.
/// </summary>
public class ReservationStatusLogConfiguration : IEntityTypeConfiguration<ReservationStatusLog>
{
    public void Configure(EntityTypeBuilder<ReservationStatusLog> builder)
    {
        builder.ToTable("ReservationStatusLogs");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FromStatus).IsRequired().HasConversion<string>();
        builder.Property(r => r.ToStatus).IsRequired().HasConversion<string>();
        builder.Property(r => r.ChangedBy).HasMaxLength(200);
        builder.Property(r => r.Reason).HasMaxLength(1000);

        builder.HasIndex(r => r.ReservationId);
        builder.HasIndex(r => r.ChangedByUserId);

        builder.HasOne(r => r.ChangedByUser)
            .WithMany()
            .HasForeignKey(r => r.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Subscription entity konfigürasyonu.
/// </summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status).IsRequired().HasConversion<string>();
        builder.Property(s => s.Amount).IsRequired().HasPrecision(10, 2);
        builder.Property(s => s.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("TRY");
        builder.Property(s => s.IyzicoSubscriptionId).HasMaxLength(200);
        builder.Property(s => s.IyzicoCustomerId).HasMaxLength(200);

        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.PlanId);
        builder.HasIndex(s => s.Status);
    }
}

/// <summary>
/// NotificationLog entity konfigürasyonu.
/// </summary>
public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Channel).IsRequired().HasConversion<string>();
        builder.Property(n => n.Type).IsRequired().HasConversion<string>();
        builder.Property(n => n.Recipient).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Status).IsRequired().HasMaxLength(50);
        builder.Property(n => n.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(n => n.TenantId);
        builder.HasIndex(n => n.ReservationId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.SentAt);
    }
}

/// <summary>
/// AuditLog entity konfigürasyonu.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.PerformedBy).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).HasMaxLength(100);
        builder.Property(a => a.EntityId).HasMaxLength(50);
        builder.Property(a => a.OldValue).HasColumnType("jsonb");
        builder.Property(a => a.NewValue).HasColumnType("jsonb");
        builder.Property(a => a.IpAddress).HasMaxLength(50);

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}

/// <summary>
/// IdempotencyKey entity konfigürasyonu.
/// </summary>
public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("IdempotencyKeys");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Key).IsRequired().HasMaxLength(500);
        builder.Property(i => i.ResponseJson).IsRequired().HasColumnType("jsonb");

        builder.HasIndex(i => i.Key).IsUnique();
        builder.HasIndex(i => i.TenantId);
        builder.HasIndex(i => i.ExpiresAt);
    }
}
