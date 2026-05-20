# Faz 5 — Admin Panel (Tamamlandı)

**Tarih:** Mayıs 2026  
**Durum:** Sunum için kabul edilen sürüm — ileride refactor planlanabilir  
**Kaynak plan:** `docs/tablewise-cursor-prompts-v1.1.md` (Prompt 5.1–5.6)  
**Ürün dokümanı:** `docs/Tablewise_Urun_Dokumani_v1.1.docx`

---

## Özet

Faz 5 kapsamında **React 18 + Vite** admin paneli (`frontend/admin-panel`) backend API ile entegre edildi. Operatörler masaları, kuralları, rezervasyonları, müşterileri ve ekibi yönetebilir; kural motoru test edilebilir; manuel rezervasyon akışı çalışır durumdadır.

**Faz 6 (Booking UI)** bu dokümanın dışındadır — public rezervasyon arayüzü ayrı fazda ele alınacaktır.

---

## Teknoloji

| Katman | Stack |
|--------|--------|
| UI | React 18, TypeScript (strict), TailwindCSS, shadcn/ui |
| State | Zustand (`authStore`), TanStack Query v5 |
| Form | React Hook Form + Zod |
| API | Axios (`/api/v1/*`), Vite dev proxy → `http://localhost:5086` |
| Gerçek zamanlı | SignalR paketi kurulu (tam entegrasyon Faz 7+) |

---

## Modül Durumu (5.1 → 5.6)

### 5.1 — İskelet ve Auth

| Özellik | Durum |
|---------|--------|
| Vite proje yapısı, router, `AppLayout`, sidebar | ✅ |
| `lib/api.ts` — Bearer, 401/403 toast | ✅ |
| Login, Register, Forgot Password | ✅ |
| Reset Password (`/reset-password/:token`) | ✅ |
| Email doğrulama (`/verify-email/:token`) | ✅ |
| Davet kabul (`/invite/:token`) + otomatik giriş | ✅ |
| Protected routes, plan durumu yönlendirmeleri | ✅ |
| Dev Login (yalnızca geliştirme; gerçek API mutasyonları için kullanılmamalı) | ⚠️ |

### 5.2 — Dashboard

| Özellik | Durum |
|---------|--------|
| İstatistik kartları | ✅ |
| Haftalık grafik (Recharts) | ✅ |
| Bugünün rezervasyonları | ✅ |
| Son aktiviteler / top kurallar | ✅ |
| `useDashboardStats`, tenant stats API | ✅ |

### 5.3 — Masalar ve Yerleşim

| Özellik | Durum |
|---------|--------|
| Mekan seçici + masa listesi | ✅ |
| Masa CRUD, sıralama (dnd-kit), aktif/pasif | ✅ |
| Masa kombinasyonları | ✅ |
| Plan limiti (`usePlanLimits`) — masa sayısı | ✅ |

### 5.4 — Kural Builder

| Özellik | Durum |
|---------|--------|
| Kural listesi, öncelik sırası, aktif/pasif | ✅ |
| Şablon galerisi (erken rezervasyon, VIP, kapora, **grup dengesi**, özel kural vb.) | ✅ |
| Rule Builder adımları (koşul / aksiyon / önizleme) | ✅ |
| API ↔ UI mapping (`mapApiRuleDto`, `mapRuleFormToApiPayload`) | ✅ |
| İnsan okunur özet (`ruleHumanReadable`) | ✅ |
| Kural testi (kayıtlı kural) | ✅ |
| **Taslak kural testi** — `POST /api/v1/rules/test-draft` | ✅ |
| Özel kural (`custom_condition`) — JSON editör, alan referansı, validasyon | ✅ |
| Grup kompozisyonu (`group_composition`) — özel form + API JSON | ✅ |

### 5.5 — Rezervasyon Yönetimi

| Özellik | Durum |
|---------|--------|
| Günlük timeline görünümü | ✅ |
| Mekan / masa / durum filtreleri | ✅ |
| Rezervasyon detay drawer | ✅ |
| Manuel rezervasyon dialog | ✅ |
| Müşteri arama (CRM) | ✅ |
| **Kuralları kontrol et** — `POST /api/v1/reservations/evaluate` | ✅ |
| CSV export | ✅ |

### 5.6 — Müşteri, Ekip, Ayarlar, Abonelik

| Özellik | Durum |
|---------|--------|
| Müşteri listesi / detay / tier | ✅ |
| Ekip listesi + bekleyen davetler (`useStaff`) | ✅ |
| Personel davet / yeniden gönder / iptal / rol | ✅ |
| Ayarlar (genel, çalışma saatleri, kapora, bildirim, entegrasyon) | ✅ |
| Abonelik sayfası (iskelet / plan bilgisi) | 🔶 Sunum için yeterli; İyzico akışı Faz 7 |

---

## Faz 5 Son Cila — Düzeltilen Sorunlar

Bu oturumda tamamlanan kritik düzeltmeler (sunum öncesi):

### TypeScript ve tip tutarlılığı

- `CustomerTier`: `Regular | Gold | VIP | Blacklisted` — kural test formu ve API mapping ile hizalandı.
- `useRules.ts`: `error: unknown` + `ruleApiErrorMessage` (strict mode).
- `validateCustomConditionJson.ts`: alan karşılaştırmaları lowercase normalize.

### Auth ve ekip (stub → gerçek API)

- `ResetPasswordPage`, `VerifyEmailPage`, `InvitePage` — RHF + Zod, `AuthPageShell`.
- `hooks/useStaff.ts` + `StaffPage` / `InviteStaffDialog` entegrasyonu.

### Plan limiti (403)

- **Kök neden:** `PlanLimitService` limitleri `LimitsJson` yerine yanlış okuyordu → `maxTables` / `maxRules` = 0.
- **Düzeltme:** `FeaturesJson` öncelikli okuma (`GetPlanLimitsQueryHandler` ile aynı mantık).

### Manuel rezervasyon

- Mekan listesi (`useVenues`) + varsayılan mekan seçimi.
- Masa dropdown boş / `Invalid uuid` — Zod `onSubmit` modu, Select `undefined` değerleri.
- **405** — `POST /api/v1/reservations/evaluate` backend’de eklendi.
- **500** — `DateTime Kind=Unspecified` → `DateTimeNormalization.ToUtcReservedFor` + frontend `toISOString()`.

---

## Backend — Faz 5 ile Eklenen / Güncellenen API

| Endpoint | Açıklama |
|----------|----------|
| `POST /api/v1/rules/test-draft` | Kaydedilmemiş kuralı test |
| `POST /api/v1/reservations/evaluate` | Manuel rezervasyon öncesi slot + kural değerlendirme |
| `GET/POST /api/v1/staff/*` | Ekip ve davetler (mevcut Faz 2) — panel bağlandı |
| `GET /api/v1/invite/{token}` | Davet önizleme |
| `POST /api/v1/invite/{token}/accept` | Davet kabul + JWT |

**Dosyalar (örnek):**

- `EvaluateManualReservationCommandHandler.cs`
- `DateTimeNormalization.cs`
- `PlanLimitService.cs` (limit okuma düzeltmesi)
- `RuleController` — `test-draft`
- `ReservationController` — `evaluate`

---

## Önemli Dosya Konumları

```
frontend/admin-panel/
  src/features/
    auth/          Login, Register, Reset, Verify, Invite
    dashboard/
    tables/
    rules/         Builder, test, custom JSON, group composition
    reservations/  Timeline, ManualReservationDialog
    customers/
    staff/
    settings/
    subscription/
  src/hooks/       useRules, useTables, useReservations, useStaff, useVenues, ...
  src/types/api.ts

src/Tablewise.Application/
  Features/Reservation/Commands/EvaluateManualReservation*
  Common/DateTimeNormalization.cs
  Services/PlanLimitService.cs

src/Tablewise.Api/Controllers/
  ReservationController.cs
  RuleController.cs
```

---

## Demo Ortamı (Sunum)

| Alan | Değer |
|------|--------|
| API | `http://localhost:5086` (veya `launchSettings.json`) |
| Admin panel | `http://localhost:3000` (Vite) |
| Demo giriş | `ahmet@demo-restoran.com` / `Demo123!` |
| Demo mekan ID | `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb` |
| Seed masalar | T1–T5 (+ kullanıcı oluşturduğu masalar) |

**Not:** Dev Login gerçek JWT üretmez; masa/kural/rezervasyon demo için **gerçek login** kullanın.

**API yeniden başlatma:** Eski `Tablewise.Api` süreci DLL kilidi verir — önce durdurun:

```powershell
Stop-Process -Name Tablewise.Api -Force -ErrorAction SilentlyContinue
cd D:\Projects\TableWise\src\Tablewise.Api
dotnet run
```

---

## Bilinçli Sınırlar (Refactor / Sonraki Fazlar)

| Konu | Not |
|------|-----|
| Booking UI (`frontend/booking-ui`) | Faz 6 — grup kompozisyonu için yalnızca iskelet bileşen |
| Abonelik / İyzico checkout | Faz 7 |
| Email “tekrar doğrulama gönder” | Backend endpoint yok; Verify sayfasında bilgilendirme |
| Rezervasyon “Tüm mekanlar” timeline | Tek mekan seçimi; çoklu mekan aggregate ileride |
| `error: any` | Bazı hook’larda (`useTables`, `useReservations` vb.) — Faz 5 sonrası refactor |
| SubscriptionPage | Tam ödeme akışı değil |
| SignalR canlı güncelleme | Pro+ plan — henüz panelde tam bağlı değil |

---

## Sunum Akışı Önerisi (5–10 dk)

1. **Giriş** — demo hesap, dashboard metrikleri.  
2. **Masalar** — liste, yeni masa, kombinasyon.  
3. **Kurallar** — şablondan kural, grup dengesi veya özel kural, **kural testi**.  
4. **Rezervasyonlar** — timeline, **manuel rezervasyon**, **kuralları kontrol et** (engel/izin mesajı).  
5. **Ekip** — davet gönder (opsiyonel).  
6. **Kapanış** — Faz 6: public booking UI, Faz 7: kapora/abonelik.

---

## Faz 6’ya Geçiş

Plan dokümanına göre sıradaki faz:

- `frontend/booking-ui` — `tablewise.com.tr/rezervasyon/[slug]`
- Public reserve, slot seçimi, grup kompozisyonu alanları, kural evaluate (booking slug)
- Landing ayrı (Faz 11)

Bu dosya Faz 5 kapanış özetidir; Faz 6 başladığında `FAZ_6_*.md` eklenebilir.

---

## İlgili Dokümanlar

- [Faz 1.8 — Personel Daveti](./FAZ_1.8_PERSONEL_DAVETI.md)
- [Faz 2.1a Tamamlandı](./FAZ_2.1a_TAMAMLANDI.md)
- [Faz 2.1b Tamamlandı](./FAZ_2.1b_TAMAMLANDI.md)
- [Faz 2.1c Tamamlandı](./FAZ_2.1c_TAMAMLANDI.md)
- [Cursor Prompt Planı v1.1](./tablewise-cursor-prompts-v1.1.md)
