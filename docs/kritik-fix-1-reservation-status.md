# Kritik Fix 1 — ReservationStatus + RuleAction Case Fix + Booking-UI Auth Interceptor

## Sorun

1. `ReservationStatus` enum backend, admin-panel ve booking-ui arasında case mismatch:
   - admin-panel `src/types/api.ts`: `Pending | Confirmed | Seated | Completed | Cancelled | NoShow` (PascalCase)
   - booking-ui `src/types/api.ts`: `pending | confirmed | cancelled | completed | no_show` (snake_case, Seated yok)
   - Backend hangi case'i serialize ediyor? Bu tek kaynaktan yönetilmeli.

2. `RuleAction` enum'unda da aynı sorun (Fable 5 bulgusu):
   - admin-panel: `'Block' | 'Warn' | 'Suggest' | 'Discount' | 'Deposit' | 'Redirect'` (PascalCase)
   - booking-ui: `'BLOCK' | 'WARN' | ...` (UPPERCASE) ve `'Redirect'` tipi booking-ui'de hiç yok
   - booking-ui `.toLowerCase()` ile runtime'da düzeltmeye çalışıyor (BookingPage çevresinde) — kırılgan, backend yeni action eklediğinde sessizce kırılır.

3. booking-ui `src/lib/api.ts`'de auth token interceptor yok — modify/cancel akışlarında reservation token header'a eklenmiyor.

---

## Adım 1 — Backend JSON Serializer Policy Denetimi

`src/Tablewise.Api/Program.cs` içinde JSON serializer konfigürasyonunu bul:

```csharp
// Şu an nasıl yapılandırılmış kontrol et:
// .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = ...)
// veya
// .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = ...)
```

Eğer `PropertyNamingPolicy` tanımlı değilse backend **PascalCase** döndürüyor demektir (.NET default).

Eğer `JsonNamingPolicy.CamelCase` varsa backend **camelCase** döndürüyor demektir.

Bulduğun sonucu not et — aşağıdaki adımlarda kullanacağız.

---

## Adım 2 — Backend'de Enum Serialization Standardize Et

`Program.cs`'de JSON konfigürasyonuna şunu ekle (PascalCase standardı):

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enum'ları string olarak serialize et (PascalCase — .NET default)
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
        // PropertyNamingPolicy null = PascalCase (default, değiştirme)
    });
```

Bu sayede `ReservationStatus.Pending` → JSON'da `"Pending"` olur.

---

## Adım 3 — admin-panel Tip Güncelleme

`frontend/admin-panel/src/types/api.ts` içinde `ReservationStatus` enum'unu kontrol et.
Şu an PascalCase ise dokunma — backend ile zaten uyumlu.

---

## Adım 4 — booking-ui Tip Güncelleme

`frontend/booking-ui/src/types/api.ts` içinde `ReservationStatus`'u şununla değiştir:

```typescript
// ESKİ (snake_case — yanlış):
export type ReservationStatus =
  | 'pending' | 'confirmed' | 'cancelled' | 'completed' | 'no_show'

// YENİ (PascalCase — backend ile uyumlu):
export type ReservationStatus =
  | 'Pending' | 'Confirmed' | 'Seated' | 'Completed' | 'Cancelled' | 'NoShow'
```

Dosyada `ReservationStatus` kullanan tüm yerleri tara:
```bash
grep -r "ReservationStatus\|reservation_status\|reservationStatus" frontend/booking-ui/src --include="*.ts" --include="*.tsx"
```

Bulunan tüm string karşılaştırmalarını (`=== 'pending'` → `=== 'Pending'` gibi) güncelle.

---

## Adım 4b — booking-ui RuleAction Tip Güncelleme (Fable bulgusu)

`frontend/booking-ui/src/types/api.ts` içinde `RuleAction`'ı şununla değiştir:

```typescript
// ESKİ (UPPERCASE, Redirect eksik — yanlış):
export type RuleAction =
  | 'BLOCK' | 'WARN' | 'SUGGEST' | 'DISCOUNT' | 'DEPOSIT'

// YENİ (PascalCase, backend ile uyumlu, Redirect dahil):
export type RuleAction =
  | 'Block' | 'Warn' | 'Suggest' | 'Discount' | 'Deposit' | 'Redirect'
```

`bookingMappers.ts` ve `BookingPage.tsx` çevresinde `RuleAction` için
yapılan `.toLowerCase()` / `.toUpperCase()` runtime dönüşümlerini bul:

```bash
grep -rn "RuleAction\|toLowerCase\|toUpperCase" frontend/booking-ui/src --include="*.ts" --include="*.tsx" | grep -i "rule\|action"
```

Bulunan tüm dönüşümleri kaldır — artık backend ile birebir aynı case
geldiği için gerek yok. `switch`/`if` karşılaştırmalarını PascalCase'e
güncelle (`case 'BLOCK':` → `case 'Block':`).

**Redirect aksiyonu için not:** RuleEnginePipeline.cs:287'de Redirect
mantığı henüz implement edilmemiş (`// TODO: v2'de redirect mantığı`).
Eğer admin panelde kural oluştururken Redirect seçeneği seçilebiliyorsa,
booking-ui'de bu action tipini handle eden bir case ekle ama davranışı
"Suggest" gibi ele al (fallback) — Redirect tam implement edilene kadar
booking akışını kırmasın.

---

## Adım 5 — booking-ui Auth Interceptor

`frontend/booking-ui/src/lib/api.ts` dosyasını aç.

Mevcut interceptor'ı bul ve reservation token desteği ekle:

```typescript
// Request interceptor — mevcut yoksa ekle, varsa genişlet
api.interceptors.request.use((config) => {
  // Idempotency-Key zaten set ediliyor — koru

  // Reservation token — localStorage veya sessionStorage'dan al
  // (modify/cancel akışlarında kullanılıyor)
  const reservationToken = sessionStorage.getItem('reservation_token')
    ?? localStorage.getItem('reservation_token')
  if (reservationToken) {
    config.headers['X-Reservation-Token'] = reservationToken
  }

  return config
})
```

Token'ın nerede set edildiğini bul (view-modify-cancel akışında):
```bash
grep -r "reservation_token\|reservationToken" frontend/booking-ui/src --include="*.ts" --include="*.tsx"
```

Token nasıl saklanıyorsa (sessionStorage/localStorage/context) interceptor'ı buna göre ayarla.

---

## Adım 6 — super-admin Tip Kontrolü

`frontend/super-admin/src/` içinde ReservationStatus veya RuleAction kullanımı varsa:
```bash
grep -rn "ReservationStatus\|RuleAction\|'pending'\|'confirmed'\|'BLOCK'\|'WARN'" frontend/super-admin/src --include="*.ts" --include="*.tsx"
```
Bulunursa PascalCase'e güncelle.

---

## Adım 7 — Doğrulama

```bash
# TypeScript hataları yok mu?
cd frontend/booking-ui && npx tsc --noEmit
cd frontend/admin-panel && npx tsc --noEmit

# Backend build
cd src/Tablewise.Api && dotnet build
```

Backend çalışırken:
- Booking UI'da bir rezervasyon oluştur
- Admin Panel'de aynı rezervasyonun durumunun doğru göründüğünü kontrol et
- Bir kural (rule) tetiklendiğinde (örn. Warn veya Block aksiyonu) booking UI'da doğru mesajın göründüğünü kontrol et

## Tamamlanma Kriterleri

- [ ] Backend PascalCase enum serialize ediyor (ReservationStatus + RuleAction)
- [ ] booking-ui ReservationStatus PascalCase
- [ ] booking-ui RuleAction PascalCase + Redirect tipi tanımlı
- [ ] admin-panel ReservationStatus ve RuleAction PascalCase
- [ ] booking-ui'deki toLowerCase/toUpperCase runtime dönüşümleri kaldırıldı
- [ ] booking-ui auth interceptor reservation token header'a ekliyor
- [ ] `tsc --noEmit` her iki frontend'de hatasız
- [ ] `dotnet build` hatasız
