# Faz 1.8 - Personel Davet Sistemi

Bu fazda personel davet sistemi tamamen implement edilmiştir.

## Oluşturulan Dosyalar

### DTOs (6 dosya)
- `InviteStaffDto.cs`
- `AcceptInvitationDto.cs`
- `InvitationDto.cs`
- `StaffMemberDto.cs`
- `UpdateStaffRoleDto.cs`
- `InvitationPreviewDto.cs`

### CQRS Commands (12 dosya)
- `InviteStaffCommand.cs` + Handler
- `AcceptInvitationCommand.cs` + Handler
- `CancelInvitationCommand.cs` + Handler
- `ResendInvitationCommand.cs` + Handler
- `UpdateStaffRoleCommand.cs` + Handler
- `RemoveStaffCommand.cs` + Handler

### CQRS Queries (6 dosya)
- `ListInvitationsQuery.cs` + Handler
- `ListStaffQuery.cs` + Handler
- `GetInvitationPreviewQuery.cs` + Handler

### Authorization
- `RequireOwnerAttribute.cs` - Owner-only yetkilendirme
- `RequireOwnerOrStaffAttribute.cs` - Owner veya Staff yetkilendirme

### Validators (3 dosya)
- `InviteStaffDtoValidator.cs`
- `AcceptInvitationDtoValidator.cs`
- `UpdateStaffRoleDtoValidator.cs`

### Controllers (2 dosya)
- `StaffController.cs` - 7 endpoint (Owner-only)
- `InviteController.cs` - 2 endpoint (Public)

### Email Service
- `IEmailService.cs` - `SendStaffInvitationEmailAsync` metodu eklendi
- `PlaceholderEmailService.cs` - Implementation güncellendi

### Configuration
- `Program.cs` - MediatR + Rate limiting policies eklendi

## API Endpoints

### StaffController (Owner-only)
```
GET    /api/v1/staff                       → Personel listesi
GET    /api/v1/staff/invitations           → Davet listesi
POST   /api/v1/staff/invitations           → Davet gönder (10 req/saat)
POST   /api/v1/staff/invitations/{id}/resend → Daveti tekrar gönder
DELETE /api/v1/staff/invitations/{id}      → Daveti iptal et
PUT    /api/v1/staff/{id}/role             → Rol güncelle
DELETE /api/v1/staff/{id}                  → Personel sil (soft)
```

### InviteController (Public)
```
GET    /api/v1/invite/{token}              → Davet önizleme
POST   /api/v1/invite/{token}/accept       → Daveti kabul et (5 req/saat)
```

## Rate Limiting

| Policy | Limit | Window | Açıklama |
|--------|-------|--------|----------|
| `staff-invite` | 10 req | 1 saat | Tenant bazlı spam önleme |
| `accept-invite` | 5 req | 1 saat | Token bazlı brute-force önleme |

## Güvenlik Özellikleri

1. **Owner Yetki Kontrolü**: Sadece Owner rolü davet gönderebilir, rol değiştirebilir, silme yapabilir
2. **Son Owner Koruması**: Tek kalan Owner kullanıcısı silinemez veya Staff'a düşürülemez
3. **Kendini Silme/Düşürme Engeli**: Kullanıcı kendi hesabını silemez veya rolünü değiştiremez
4. **Davet Token Güvenliği**: 32 karakter hex, 7 gün geçerlilik
5. **Email Verification**: Davet ile gelen kullanıcılar otomatik verified
6. **Aktif Davet Kontrolü**: Aynı email'e birden fazla aktif davet gönderilemez
7. **Race Condition Önleme**: Davet kabul sırasında email duplicate kontrolü

## Business Rules

- Davet süresi: 7 gün
- Expire olan davetler kabul edilemez
- Zaten kabul edilmiş davetler tekrar kabul edilemez
- SuperAdmin rolü atanamaz
- Davet ile katılan kullanıcılar direkt giriş yapmış sayılır (JWT döner)
- Silinen kullanıcıların tüm refresh token'ları revoke edilir

## Audit Logging

Tüm işlemler audit log'a kaydedilir:
- `STAFF_INVITED`
- `STAFF_JOINED`
- `INVITATION_CANCELLED`
- `INVITATION_RESENT`
- `STAFF_ROLE_UPDATED`
- `STAFF_REMOVED`

## Toplam

**38 yeni dosya** oluşturuldu, personel davet sistemi production-ready.
