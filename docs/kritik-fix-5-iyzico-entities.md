# Kritik Fix 5 — İyzico Entity İskeletleri

## Sorun

Faz 7 başlangıcında plan revizyonu olmaması için entity iskeletleri
şimdiden eklenmeli. Henüz hiç logic yok — sadece migration.

---

## Adım 1 — Tenant Entity'ye IyzicoMerchantId Ekle

`src/Tablewise.Domain/Entities/Tenant.cs` içinde `CustomLimitsJson`
satırının altına ekle:

```csharp
/// <summary>
/// İyzico alt üye işyeri (sub-merchant) ID'si.
/// Null ise tenant henüz İyzico'ya kayıt olmamış demektir.
/// Kapora ödemeleri bu hesaba aktarılır.
/// </summary>
public string? IyzicoMerchantId { get; set; }

/// <summary>
/// İyzico sub-merchant başvuru durumu.
/// </summary>
public IyzicoMerchantStatus IyzicoMerchantStatus { get; set; }
    = IyzicoMerchantStatus.NotApplied;
```

---

## Adım 2 — IyzicoMerchantStatus Enum

Yeni dosya: `src/Tablewise.Domain/Enums/IyzicoMerchantStatus.cs`

```csharp
namespace Tablewise.Domain.Enums;

/// <summary>
/// İyzico alt üye işyeri başvuru durumu.
/// </summary>
public enum IyzicoMerchantStatus
{
    /// <summary>Başvuru yapılmadı.</summary>
    NotApplied = 0,

    /// <summary>Başvuru gönderildi, İyzico inceliyor (1-2 hafta).</summary>
    PendingApproval = 1,

    /// <summary>Onaylandı, ödeme alabilir.</summary>
    Approved = 2,

    /// <summary>Reddedildi.</summary>
    Rejected = 3,

    /// <summary>Askıya alındı.</summary>
    Suspended = 4
}
```

---

## Adım 3 — Payment Entity

Yeni dosya: `src/Tablewise.Domain/Entities/Payment.cs`

```csharp
using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Rezervasyon kapor ödemelerini ve platform abonelik ödemelerini temsil eder.
/// Her ödeme bir İyzico işlemine karşılık gelir.
/// </summary>
public sealed class Payment : TenantScopedEntity
{
    /// <summary>İlgili rezervasyon (kapora ödemeleri için).</summary>
    public Guid? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    /// <summary>İyzico payment ID (işlem referansı).</summary>
    public string? IyzicoPaymentId { get; set; }

    /// <summary>İyzico conversation ID (idempotency).</summary>
    public string? IyzicoConversationId { get; set; }

    /// <summary>Ödeme tutarı (TRY).</summary>
    public decimal Amount { get; set; }

    /// <summary>Para birimi — şimdilik sadece TRY.</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Ödeme durumu.</summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Ödeme tipi.</summary>
    public PaymentType Type { get; set; }

    /// <summary>Ödemenin tamamlandığı zaman.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>İyzico'dan dönen ham response (debug için).</summary>
    public string? IyzicoResponseJson { get; set; }

    /// <summary>Hata mesajı (başarısız ödemelerde).</summary>
    public string? ErrorMessage { get; set; }
}
```

---

## Adım 4 — PaymentStatus ve PaymentType Enum'ları

Yeni dosya: `src/Tablewise.Domain/Enums/PaymentEnums.cs`

```csharp
namespace Tablewise.Domain.Enums;

public enum PaymentStatus
{
    /// <summary>Ödeme bekleniyor.</summary>
    Pending = 0,

    /// <summary>Ödeme tamamlandı.</summary>
    Completed = 1,

    /// <summary>Ödeme başarısız.</summary>
    Failed = 2,

    /// <summary>İade edildi (tam).</summary>
    Refunded = 3,

    /// <summary>Kısmi iade.</summary>
    PartiallyRefunded = 4,

    /// <summary>İptal edildi.</summary>
    Cancelled = 5
}

public enum PaymentType
{
    /// <summary>Rezervasyon kapor ödemesi.</summary>
    ReservationDeposit = 0,

    /// <summary>Platform abonelik ödemesi (Faz 14+).</summary>
    PlatformSubscription = 1
}
```

---

## Adım 5 — Refund Entity

Yeni dosya: `src/Tablewise.Domain/Entities/Refund.cs`

```csharp
using Tablewise.Domain.Common;

namespace Tablewise.Domain.Entities;

/// <summary>
/// İade işlemlerini temsil eder.
/// Her iade bir Payment'a bağlıdır.
/// </summary>
public sealed class Refund : TenantScopedEntity
{
    /// <summary>İade edilen ödeme.</summary>
    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }

    /// <summary>İade tutarı (tam veya kısmi).</summary>
    public decimal Amount { get; set; }

    /// <summary>İyzico iade referans ID'si.</summary>
    public string? IyzicoRefundId { get; set; }

    /// <summary>İade nedeni.</summary>
    public string? Reason { get; set; }

    /// <summary>İadenin tamamlandığı zaman.</summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>İade başlatan personel.</summary>
    public Guid? InitiatedByStaffId { get; set; }
}
```

---

## Adım 6 — DbContext'e Ekle

`src/Tablewise.Infrastructure/Persistence/TablewiseDbContext.cs` içine ekle:

```csharp
public DbSet<Payment> Payments => Set<Payment>();
public DbSet<Refund> Refunds => Set<Refund>();
```

`OnModelCreating` içine Global Query Filter ekle:

```csharp
// Payment izolasyonu
modelBuilder.Entity<Payment>()
    .HasQueryFilter(e => e.TenantId == TenantFilterId.Value && !e.IsDeleted);

// Refund izolasyonu
modelBuilder.Entity<Refund>()
    .HasQueryFilter(e => e.TenantId == TenantFilterId.Value && !e.IsDeleted);
```

---

## Adım 7 — Migration

```bash
cd src/Tablewise.Api
dotnet ef migrations add AddIyzicoEntitySkeletons --project ../Tablewise.Infrastructure
dotnet ef database update
```

---

## Adım 8 — Doğrulama

```bash
dotnet build
dotnet ef migrations list
# En son migration: AddIyzicoEntitySkeletons görünmeli
```

## Tamamlanma Kriterleri

- [ ] `Tenant.IyzicoMerchantId` ve `IyzicoMerchantStatus` eklendi
- [ ] `IyzicoMerchantStatus` enum oluşturuldu
- [ ] `Payment` entity oluşturuldu
- [ ] `Refund` entity oluşturuldu
- [ ] `PaymentStatus` ve `PaymentType` enum'ları oluşturuldu
- [ ] DbContext'e `Payments` ve `Refunds` DbSet eklendi
- [ ] Global Query Filter'lar eklendi
- [ ] Migration başarıyla çalıştı
- [ ] `dotnet build` hatasız
