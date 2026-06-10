# Özel Limitler — Prompt 1: Backend

## Bağlam

`GET /api/v1/tenant/me/plan-limits` endpoint'i zaten mevcut ve çalışıyor.
`GetPlanLimitsQueryHandler` plan'ın `LimitsJson`'ını okuyor.
`Tenant` entity'sinde henüz `CustomLimitsJson` alanı yok.

Bu prompt'ta yapılacaklar:
1. `Tenant` entity'sine `CustomLimitsJson` ekle + migration
2. `GetPlanLimitsQueryHandler`'ı custom override'ı okuyacak şekilde güncelle
3. Super Admin'in tenant'a özel limit atayabileceği endpoint'i ekle

---

## Adım 1 — `Tenant` Entity Güncellemesi

**Dosya:** `src/Tablewise.Domain/Entities/Tenant.cs`

`IsActive` property'sinin altına ekle:

```csharp
/// <summary>
/// Tenant'a özel limit override'ları (JSONB).
/// Null ise plan limitleri geçerlidir.
/// Dolu ise ilgili anahtar için plan limitini override eder.
/// Format: { "maxVenues": 10, "maxTables": -1 }
/// Sadece override edilmek istenen anahtarlar girilir — diğerleri plan'dan okunur.
/// </summary>
public string? CustomLimitsJson { get; set; }
```

---

## Adım 2 — Migration

```bash
cd src/Tablewise.Api
dotnet ef migrations add AddTenantCustomLimitsJson --project ../Tablewise.Infrastructure
dotnet ef database update
```

Migration sonrası `CustomLimitsJson` kolonu `Tenant` tablosuna `nullable text` olarak eklenmeli.

---

## Adım 3 — `GetPlanLimitsQueryHandler` Güncellemesi

**Dosya:** `src/Tablewise.Application/Features/Tenant/Queries/GetPlanLimitsQuery.cs`

Mevcut `Handle` metodunda tenant sorgusu `CustomLimitsJson`'ı da çekecek şekilde zaten çalışıyor (entity tüm alanları alıyor). Sadece `ReadPlanLimit` metodunu ve çağrılarını güncelle:

```csharp
// ESKİ çağrılar:
MaxVenues = ReadPlanLimit(featuresJson, limitsJson, "maxVenues"),

// YENİ çağrılar (customLimitsJson parametresi eklendi):
MaxVenues = ReadPlanLimit(tenant.CustomLimitsJson, limitsJson, "maxVenues"),
MaxTables = ReadPlanLimit(tenant.CustomLimitsJson, limitsJson, "maxTables"),
MaxRules  = ReadPlanLimit(tenant.CustomLimitsJson, limitsJson, "maxRules"),
MaxReservationsPerMonth = ReadPlanLimit(tenant.CustomLimitsJson, limitsJson, "maxReservationsPerMonth"),
```

`ReadPlanLimit` metodunu şu şekilde güncelle — custom limit varsa onu kullan, yoksa plan limitini:

```csharp
/// <summary>
/// Limit çözümleme: önce customLimitsJson (tenant özel), sonra planLimitsJson (plan geneli).
/// Negatif değer = sınırsız (null döner).
/// </summary>
private static int? ReadPlanLimit(string? customLimitsJson, string? planLimitsJson, string key)
{
    // 1. Tenant özel limit
    var custom = TryReadIntProperty(customLimitsJson, key);
    if (custom.HasValue)
        return NormalizeLimit(custom.Value);

    // 2. Plan limiti
    var fromPlan = TryReadIntProperty(planLimitsJson, key);
    return fromPlan.HasValue ? NormalizeLimit(fromPlan.Value) : null;
}
```

`PlanLimitsDto`'ya `IsCustom` flag'i ekle — Admin Panel'de "Özel limit uygulandı" badge'i için:

**Dosya:** `src/Tablewise.Application/DTOs/Tenant/PlanLimitsDto.cs` (veya neredeyse o dosya)

```csharp
public class PlanLimitsDto
{
    public int? MaxVenues { get; set; }
    public int CurrentVenueCount { get; set; }
    public int? MaxTables { get; set; }
    public int CurrentTableCount { get; set; }
    public int? MaxRules { get; set; }
    public int CurrentRuleCount { get; set; }
    public int? MaxReservationsPerMonth { get; set; }
    public int CurrentReservationCount { get; set; }

    // YENI: Tenant'a özel limit uygulandı mı?
    public bool HasCustomLimits { get; set; }
}
```

Handler'da `HasCustomLimits` değerini doldur:

```csharp
HasCustomLimits = !string.IsNullOrWhiteSpace(tenant.CustomLimitsJson)
                 && tenant.CustomLimitsJson != "{}"
```

---

## Adım 4 — Super Admin Endpoint: Tenant Custom Limit Güncelleme

**Dosya:** `src/Tablewise.Api/Controllers/PlatformTenantsController.cs`

Mevcut controller'a yeni endpoint ekle:

```csharp
/// <summary>
/// Tenant'a özel limit override'larını günceller.
/// Null göndermek mevcut custom limitleri temizler (plan limitine döner).
/// </summary>
[HttpPut("{tenantId:guid}/custom-limits")]
[RequirePlatformRole(PlatformRole.SuperAdmin)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> UpdateCustomLimits(
    Guid tenantId,
    [FromBody] UpdateTenantCustomLimitsDto dto,
    CancellationToken cancellationToken)
{
    await _mediator.Send(new UpdateTenantCustomLimitsCommand(tenantId, dto), cancellationToken);
    return NoContent();
}
```

**DTO:** `src/Tablewise.Application/DTOs/Platform/TenantDtos.cs` içine ekle:

```csharp
public record UpdateTenantCustomLimitsDto(
    int? MaxVenues,                  // null = bu limiti plan'dan al
    int? MaxTables,                  // null = bu limiti plan'dan al
    int? MaxRules,                   // null = bu limiti plan'dan al
    int? MaxReservationsPerMonth,    // null = bu limiti plan'dan al
    int? MaxStaffAccounts);          // null = bu limiti plan'dan al
// Not: -1 = sınırsız, pozitif = belirtilen limit, null = plan limitine dön
```

**Command + Handler:**

Yeni dosya: `src/Tablewise.Application/Features/Platform/Commands/UpdateTenantCustomLimitsCommand.cs`

```csharp
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed record UpdateTenantCustomLimitsCommand(
    Guid TenantId,
    UpdateTenantCustomLimitsDto Dto) : IRequest;

public sealed class UpdateTenantCustomLimitsCommandHandler
    : IRequestHandler<UpdateTenantCustomLimitsCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateTenantCustomLimitsCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateTenantCustomLimitsCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Tenant bulunamadı.");

        var dto = request.Dto;

        // Tüm null ise custom limitleri temizle
        if (dto.MaxVenues is null && dto.MaxTables is null && dto.MaxRules is null
            && dto.MaxReservationsPerMonth is null && dto.MaxStaffAccounts is null)
        {
            tenant.CustomLimitsJson = null;
        }
        else
        {
            // Sadece null olmayan değerleri JSON'a yaz
            var dict = new Dictionary<string, int>();
            if (dto.MaxVenues.HasValue)             dict["maxVenues"] = dto.MaxVenues.Value;
            if (dto.MaxTables.HasValue)             dict["maxTables"] = dto.MaxTables.Value;
            if (dto.MaxRules.HasValue)              dict["maxRules"] = dto.MaxRules.Value;
            if (dto.MaxReservationsPerMonth.HasValue) dict["maxReservationsPerMonth"] = dto.MaxReservationsPerMonth.Value;
            if (dto.MaxStaffAccounts.HasValue)      dict["maxStaffAccounts"] = dto.MaxStaffAccounts.Value;

            tenant.CustomLimitsJson = JsonSerializer.Serialize(dict);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Tamamlanma Kriterleri

- [ ] Migration başarıyla çalıştı, `Tenant` tablosunda `CustomLimitsJson` kolonu var
- [ ] `GET /api/v1/tenant/me/plan-limits` → custom limit varsa onu, yoksa plan limitini döndürüyor
- [ ] `PlanLimitsDto.HasCustomLimits` doğru çalışıyor
- [ ] `PUT /api/platform/tenants/{id}/custom-limits` → 204 döndürüyor
- [ ] Tüm null gönderilince `CustomLimitsJson` null oluyor (plan limitine dönüş)
- [ ] `dotnet build` hatasız

---

## Çalıştırma

```bash
cd src/Tablewise.Api
dotnet ef migrations add AddTenantCustomLimitsJson --project ../Tablewise.Infrastructure
dotnet ef database update
dotnet run
```
