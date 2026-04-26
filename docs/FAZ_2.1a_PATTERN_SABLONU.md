# Faz 2.1a - Kalan Dosyalar İçin Şablon Kılavuzu

## ✅ Tamamlanan Dosyalar (20 adet)

### Tenant Modülü
1. ✅ `TenantUsageDto.cs`
2. ✅ `UpdateTenantCommand.cs`
3. ✅ `UpdateTenantCommandHandler.cs`
4. ✅ `UpdateTenantDtoValidator.cs`
5. ✅ `GetTenantUsageQuery.cs`
6. ✅ `GetTenantUsageQueryHandler.cs`
7. ✅ `GetAuditLogsQuery.cs`
8. ✅ `GetAuditLogsQueryHandler.cs`
9. ✅ `TenantController.cs`

### Venue Modülü
10. ✅ `VenueDto.cs`
11. ✅ `CreateVenueDto.cs`
12. ✅ `UpdateVenueDto.cs`
13. ✅ `WorkingHoursDto.cs`
14. ✅ `CreateVenueCommand.cs`
15. ✅ `CreateVenueCommandHandler.cs`
16. ✅ `CreateVenueDtoValidator.cs`
17. ✅ `GetVenuesQuery.cs`
18. ✅ `GetVenuesQueryHandler.cs`
19. ✅ `GetVenueByIdQuery.cs`
20. ✅ `GetVenueByIdQueryHandler.cs`
21. ✅ `VenueController.cs` (temel implementasyon)

---

## 📋 Kalan Dosyalar ve Şablonlar (yaklaşık 15 adet)

### 1️⃣ Tenant Logo Upload İşlemleri

#### `GenerateLogoUploadUrlCommand.cs`
**Konum:** `src\Tablewise.Application\Features\Tenant\Commands\`

```csharp
using MediatR;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Logo upload için presigned URL oluşturma komutu.
/// Sadece Owner rolü kullanabilir.
/// </summary>
public sealed record GenerateLogoUploadUrlCommand : IRequest<LogoUploadUrlDto>
{
    /// <summary>
    /// Dosya MIME tipi (image/jpeg, image/png).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Dosya boyutu (bytes).
    /// </summary>
    public required long FileSizeBytes { get; init; }
}
```

#### `GenerateLogoUploadUrlCommandHandler.cs`
**Pattern:** `CreateVenueCommandHandler.cs` benzeri
- Yetki kontrolü: Owner
- Cloudflare R2 presigned URL oluşturma (IStorageService)
- Dosya tipi ve boyut validasyonu
- Audit log

#### `ConfirmLogoUploadCommand.cs`
```csharp
using MediatR;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Logo upload onaylama komutu.
/// Upload tamamlandıktan sonra çağrılır.
/// </summary>
public sealed record ConfirmLogoUploadCommand : IRequest<Unit>
{
    /// <summary>
    /// Upload edilen dosyanın R2 key'i.
    /// </summary>
    public required string FileKey { get; init; }
}
```

#### `ConfirmLogoUploadCommandHandler.cs`
**Pattern:** `UpdateTenantCommandHandler.cs` benzeri
- Tenant'ın Settings JSON'ına logoUrl ekle
- Audit log
- SaveChanges

#### `ConfirmLogoUploadDtoValidator.cs`
```csharp
using FluentValidation;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Validators.Tenant;

public sealed class ConfirmLogoUploadDtoValidator : AbstractValidator<ConfirmLogoUploadDto>
{
    public ConfirmLogoUploadDtoValidator()
    {
        RuleFor(x => x.FileKey)
            .NotEmpty().WithMessage("FileKey zorunludur.")
            .MaximumLength(500).WithMessage("FileKey en fazla 500 karakter olabilir.");
    }
}
```

#### TenantController'a Eklenecek Endpoint'ler
```csharp
/// <summary>
/// Logo upload için presigned URL oluşturur.
/// </summary>
[HttpPost("logo/upload-url")]
[RequireOwner]
public async Task<IActionResult> GenerateLogoUploadUrl(
    [FromBody] GenerateLogoUploadUrlDto dto,
    CancellationToken cancellationToken = default)
{
    var command = new GenerateLogoUploadUrlCommand
    {
        ContentType = dto.ContentType,
        FileSizeBytes = dto.FileSizeBytes
    };

    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
}

/// <summary>
/// Logo upload'ını onaylar.
/// </summary>
[HttpPost("logo/confirm")]
[RequireOwner]
public async Task<IActionResult> ConfirmLogoUpload(
    [FromBody] ConfirmLogoUploadDto dto,
    CancellationToken cancellationToken = default)
{
    var command = new ConfirmLogoUploadCommand
    {
        FileKey = dto.FileKey
    };

    await _mediator.Send(command, cancellationToken);
    return NoContent();
}
```

---

### 2️⃣ Venue CRUD İşlemleri

#### `UpdateVenueCommand.cs`
**Konum:** `src\Tablewise.Application\Features\Venue\Commands\`

```csharp
using MediatR;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue güncelleme komutu.
/// Sadece Owner rolü kullanabilir.
/// </summary>
public sealed record UpdateVenueCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Mekan adı.
    /// </summary>
    public required string Name { get; init; }

    // ... diğer alanlar (CreateVenueCommand ile aynı)
}
```

#### `UpdateVenueCommandHandler.cs`
**Pattern:** `UpdateTenantCommandHandler.cs` benzeri
- Yetki kontrolü: Owner
- Venue bulma (TenantId ve !IsDeleted kontrolü)
- Kapora modülü plan kontrolü
- Güncelleme
- Audit log

#### `UpdateVenueDtoValidator.cs`
**Pattern:** `CreateVenueDtoValidator.cs` ile aynı kurallar

#### `DeleteVenueCommand.cs`
```csharp
using MediatR;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue silme komutu (soft delete).
/// Sadece Owner rolü kullanabilir.
/// </summary>
public sealed record DeleteVenueCommand : IRequest<Unit>
{
    /// <summary>
    /// Silinecek venue ID'si.
    /// </summary>
    public required Guid VenueId { get; init; }
}
```

#### `DeleteVenueCommandHandler.cs`
**Pattern:** `RemoveStaffCommandHandler.cs` benzeri
- Yetki kontrolü: Owner
- Venue bulma
- Aktif rezervasyon kontrolü (varsa silme engellenir)
- Soft delete (IsDeleted = true, DeletedAt = DateTime.UtcNow)
- Audit log

#### `UpdateWorkingHoursCommand.cs`
```csharp
using MediatR;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue çalışma saatleri güncelleme komutu.
/// </summary>
public sealed record UpdateWorkingHoursCommand : IRequest<Unit>
{
    public required Guid VenueId { get; init; }
    public required string WorkingHours { get; init; } // JSON
}
```

#### `UpdateWorkingHoursCommandHandler.cs`
**Pattern:** Basit update handler
- Venue bulma
- WorkingHours JSON validasyonu
- Güncelleme
- Audit log

#### VenueController'a Eklenecek Endpoint'ler
```csharp
/// <summary>
/// Venue günceller.
/// </summary>
[HttpPut("{id:guid}")]
[RequireOwner]
public async Task<IActionResult> UpdateVenue(
    Guid id,
    [FromBody] UpdateVenueDto dto,
    CancellationToken cancellationToken = default)
{
    var command = new UpdateVenueCommand
    {
        VenueId = id,
        Name = dto.Name,
        // ... diğer mapping'ler
    };

    await _mediator.Send(command, cancellationToken);
    return NoContent();
}

/// <summary>
/// Venue siler (soft delete).
/// </summary>
[HttpDelete("{id:guid}")]
[RequireOwner]
public async Task<IActionResult> DeleteVenue(
    Guid id,
    CancellationToken cancellationToken = default)
{
    var command = new DeleteVenueCommand { VenueId = id };
    await _mediator.Send(command, cancellationToken);
    return NoContent();
}

/// <summary>
/// Venue çalışma saatlerini günceller.
/// </summary>
[HttpPut("{id:guid}/working-hours")]
[RequireOwner]
public async Task<IActionResult> UpdateWorkingHours(
    Guid id,
    [FromBody] UpdateWorkingHoursDto dto,
    CancellationToken cancellationToken = default)
{
    var command = new UpdateWorkingHoursCommand
    {
        VenueId = id,
        WorkingHours = dto.WorkingHours
    };

    await _mediator.Send(command, cancellationToken);
    return NoContent();
}
```

---

## 📊 Kalan Dosya Listesi ve Öncelik

| # | Dosya | Şablon Referansı | Zorluk |
|---|-------|------------------|--------|
| 1 | `GenerateLogoUploadUrlCommand.cs` | Yukarıdaki şablon | Kolay |
| 2 | `GenerateLogoUploadUrlCommandHandler.cs` | CreateVenueCommandHandler | Orta (R2 entegrasyonu) |
| 3 | `ConfirmLogoUploadCommand.cs` | Yukarıdaki şablon | Kolay |
| 4 | `ConfirmLogoUploadCommandHandler.cs` | UpdateTenantCommandHandler | Kolay |
| 5 | `ConfirmLogoUploadDtoValidator.cs` | Yukarıdaki şablon | Kolay |
| 6 | `UpdateVenueCommand.cs` | CreateVenueCommand | Kolay |
| 7 | `UpdateVenueCommandHandler.cs` | UpdateTenantCommandHandler | Kolay |
| 8 | `UpdateVenueDtoValidator.cs` | CreateVenueDtoValidator | Kolay |
| 9 | `DeleteVenueCommand.cs` | Yukarıdaki şablon | Kolay |
| 10 | `DeleteVenueCommandHandler.cs` | RemoveStaffCommandHandler | Kolay |
| 11 | `UpdateWorkingHoursCommand.cs` | Yukarıdaki şablon | Kolay |
| 12 | `UpdateWorkingHoursCommandHandler.cs` | UpdateTenantCommandHandler | Kolay |
| 13 | `UpdateWorkingHoursDtoValidator.cs` | CreateVenueDtoValidator | Kolay |
| 14 | TenantController endpoint'leri (2 adet) | Yukarıdaki şablon | Kolay |
| 15 | VenueController endpoint'leri (3 adet) | Yukarıdaki şablon | Kolay |

---

## 🎯 Hızlı Klonlama İpuçları

### Command + Handler Oluşturma
1. Mevcut benzer command'ı kopyala
2. İsimleri değiştir
3. Property'leri güncelle
4. Handler'da business logic'i ayarla
5. Audit log ekle

### Validator Oluşturma
1. Benzer validator'ı kopyala
2. Property kurallarını ayarla
3. Custom validation metotları ekle (JSON, TimeZone, vb.)

### Controller Endpoint Ekleme
1. Mevcut endpoint'i kopyala
2. Route ve HTTP method'u ayarla
3. Command mapping yap
4. XML doc comment'leri güncelle

---

## 🔧 Bağımlılıklar ve Not

### Gerekli Interface'ler
- `IStorageService` - Logo upload için (Cloudflare R2)
  - `GeneratePresignedUploadUrlAsync()`
  - `GetPublicUrl()`

### Eksik DTO'lar
Logo işlemleri için request DTO'lar eklenecek:
```csharp
// src\Tablewise.Application\DTOs\Tenant\GenerateLogoUploadUrlDto.cs
public sealed record GenerateLogoUploadUrlDto
{
    public required string ContentType { get; init; }
    public required long FileSizeBytes { get; init; }
}

// src\Tablewise.Application\DTOs\Tenant\ConfirmLogoUploadDto.cs
public sealed record ConfirmLogoUploadDto
{
    public required string FileKey { get; init; }
}

// src\Tablewise.Application\DTOs\Venue\UpdateWorkingHoursDto.cs
public sealed record UpdateWorkingHoursDto
{
    public required string WorkingHours { get; init; }
}
```

---

## ✨ Sonuç

**Oluşturulan:** 21 dosya  
**Kalan:** ~15 dosya (pattern şablonları ile kolayca oluşturulabilir)

Kalan dosyaları yukarıdaki şablonları takip ederek 15-20 dakikada oluşturabilirsiniz. Tüm pattern'ler mevcut kod tabanıyla uyumludur.
