# Tablewise — Cursor Geliştirme Master Rehberi v1.1

> Bu döküman **Tablewise_Urun_Dokumani_v1.1.docx** ile senkronizedir.
> Tüm kararlar oradaki kaynağa bağlıdır. Çelişki olursa .docx kazanır.
>
> Hedef: Ticari kalitede ürün. MVP değil.
> Toplam tahmini: **80-110 Cursor chat session**, **6-9 ay** kalendar süre.

---

## 📑 İÇİNDEKİLER

1. [Cursor Verimlilik Rehberi](#bölüm-1--cursor-verimlilik-rehberi) (ÖNCE OKU)
2. [Genel Bağlam Bloğu](#bölüm-2--genel-bağlam-bloğu)
3. [Faz 1 — Backend Çekirdeği](#faz-1--backend-çekirdeği)
4. [Faz 2 — API Katmanı](#faz-2--api-katmanı)
5. [Faz 3 — Kural Motoru](#faz-3--kural-motoru)
6. [Faz 4 — Email & Dosya Depolama](#faz-4--email--dosya-depolama)
7. [Faz 5 — Admin Panel](#faz-5--admin-panel)
8. [Faz 6 — Booking UI](#faz-6--booking-ui)
9. [Faz 7 — Ticari Katman + Kapora](#faz-7--ticari-katman--kapora)
10. [Faz 8 — CRM + Raporlama](#faz-8--crm--raporlama)
11. [Faz 9 — Güvenlik + Monitoring](#faz-9--güvenlik--monitoring)
12. [Faz 10 — Deployment](#faz-10--deployment)
13. [Faz 11 — Landing Page](#faz-11--landing-page)
14. [Faz 12 — Faz 4 Ölçek Özellikleri](#faz-12--ölçek-özellikleri)

---

# BÖLÜM 1 — CURSOR VERİMLİLİK REHBERİ

## 🎯 Model Seçim Matrisi

Cursor'da kullanabileceğin modeller ve hangi görev için uygun olduğu:

| Model | Görev Tipi | Kullanım Oranı | Notlar |
|-------|------------|----------------|--------|
| **Claude Sonnet 4.5** (varsayılan) | CRUD, entity, DTO, controller, basit UI, boilerplate | **%70** | Hız + kalite dengesi. Günlük çalışmanın büyük kısmı. |
| **Claude Opus 4.5** | Kural motoru tasarımı, idempotency, auth mimarisi, karmaşık debug, refactoring | **%15** | Pahalı ama gerektiğinde kritik. |
| **Cursor Tab / Composer** | Otomatik tamamlama, tekrarlı kod, küçük refactoring | **%10** | Ücretsiz / düşük maliyet, çok değerli. |
| **GPT-5 / Gemini 2.5 Pro** | İkinci fikir, kod review, alternatif bakış açısı | **%5** | Aynı koda farklı model baktırınca bug yakalarsın. |

### Pratik Kurallar

1. **Varsayılan Sonnet.** Her chat'e Sonnet ile başla. Opus'a sadece "bu gerçekten karmaşık" diye düşündüğünde geç.
2. **Karmaşık görev = tek Opus session.** Opus'u açtığında görevi sonlandırana kadar orada kal. Sonnet'e dönme.
3. **Boilerplate için asla Opus kullanma.** Entity, DTO, basit CRUD için Opus parayı havaya atmaktır.
4. **Cursor Tab her zaman açık.** Yazma hızını 3x artırır, pahalı model kullanımını azaltır.

### Kural Motoru İçin Özel Not

Custom IRuleEvaluator pipeline'ını yazarken **Opus kullan.** NRules kullanmama kararı aldığımız için tüm kural değerlendirme mantığını sen (ve Cursor) yazacaksın. Bu projenin teknik kalbidir, pahalı modele değer.

---

## 💰 Token Tasarruf Stratejileri

### 1. Context Window Yönetimi

Cursor her mesajla geçmişi tekrar gönderir. Uzun bir chat = her mesaj pahalı.

**Kural:** 15-20 mesaja ulaşınca yeni chat aç. Genel Bağlam bloğunu yapıştır, devam et.

### 2. `@` Referansları Kullan (Agent mode)

```
❌ @codebase                  → Tüm kodu tarar, PAHALI
✅ @src/Tablewise.Core/Entities/Tenant.cs  → Sadece o dosya, UCUZ
✅ @folder:src/Tablewise.Core → Sadece bir klasör
```

`@codebase` sadece "hiçbir fikrim yok nerede ne var" durumunda kullan.

### 3. Dosya Ekleyerek Değil, Yapıştırarak Paylaş

Küçük dosyalar için (< 100 satır) içeriği doğrudan chat'e yapıştır. Cursor'un `@` indexer'ı bunun için fazla token harcar.

### 4. "Uygula" vs "Yaz" Farkı

- **Composer / Agent mode** = otomatik uygular, daha pahalı
- **Chat mode (Cmd+L)** = sadece kod üretir, sen kopyalarsın

Basit değişiklikler için **Chat mode** yeterli ve daha ucuz.

### 5. Diff İste, Tam Dosya İsteme

```
❌ "ReservationController'ı güncelle ve tam halini yaz"
✅ "ReservationController.cs'e sadece Cancel endpoint'ini ekle. Diff olarak ver."
```

Tam dosya = 500 satır token. Diff = 30 satır token.

### 6. Prompt'u Bir Kez Yaz, İyi Yaz

Uzun ve detaylı bir prompt > 5 düzeltme mesajı. Bu rehberdeki her prompt bunun için tasarlandı.

### 7. Cursor Pro Ayarları

`Settings → Rules for AI` kısmına şunu ekle (her chat'te tekrar etmezsin):

```
- .NET 8, Clean Architecture, CQRS/MediatR, FluentValidation kullanıyorum.
- Tüm yeni kod XML doc comment içermeli.
- PostgreSQL + EF Core. Global Query Filter TenantId ile zorunlu.
- Soft delete (IsDeleted + DeletedAt) her entity'de standart.
- API endpoint'leri /api/v1/ prefix'iyle başlar.
- Kural motoru için NRules KULLANMA — Custom IRuleEvaluator pipeline.
- Türkçe açıklama, İngilizce kod.
- Gereksiz using eklemelerinden kaçın.
- Diff olarak ver, tam dosya yazma (aksini söylemediğim sürece).
```

Bu blok her mesajında context'e otomatik eklenir → her seferinde yazmazsın → token tasarrufu.

---

## 🔀 MES Projesiyle Paralel Çalışma

MES projende çoğu zaman Opus gerekir (endüstriyel mantık karmaşık). Tablewise'ın çoğu Sonnet ile halledilebilir.

### Günlük İş Bölümü

| Zaman Dilimi | Proje | Model |
|--------------|-------|-------|
| Sabah (zihin taze) | MES - karmaşık mantık | Opus |
| Öğleden sonra | Tablewise - CRUD, UI | Sonnet |
| Akşam (yorgun) | Tablewise - boilerplate | Cursor Tab + Sonnet |

### Workspace Ayrımı

Cursor'da iki ayrı pencere aç:
- `~/projects/mes` (MES projesi)
- `~/projects/tablewise` (Tablewise projesi)

Context karışmasın. Her pencere için Rules for AI ayrı.

### Git Branch Stratejisi

MES projende main'e commitlerken Tablewise'da `develop` branch'inde çalış. Ay sonunda develop → main PR. MES kritik üretim ortamıysa Tablewise rahat hareket alanı sağlasın.

---

## 📏 Her Faz İçin Çalışma Formatı

```
1. Yeni Cursor chat aç (Cmd+N)
2. Genel Bağlam bloğunu yapıştır (Bölüm 2)
3. O faza ait prompt'u yapıştır
4. Üretilen kodu incele
5. Gerekirse 2-3 düzeltme mesajı
6. Kod çalışıyor mu? Unit test yaz / çalıştır
7. Git commit (her faz sonunda!)
8. Chat'i kapat, bir sonrakine geç
```

**Chat'i kapatma kuralı:** Her faz sonunda, başarılı olsa bile kapat. Yeni faz = yeni chat. Context temizliği kritik.

---

# BÖLÜM 2 — GENEL BAĞLAM BLOĞU

> Her yeni Cursor chat'inde önce bunu yapıştır. `[MEVCUT FAZ]` satırını o an üzerinde çalıştığın faz ile güncelle.

```
Sen kıdemli bir full-stack yazılım mimarısın. Ticari kalitede, production-ready
kod yazıyorsun. Clean Architecture, SOLID, KVKK/GDPR uyumu konularında titizsin.

PROJE: "Tablewise" — Kural Tabanlı SaaS Rezervasyon ve Masa Yönetim Platformu
Kaynak doküman: docs/Tablewise_Urun_Dokumani_v1.1.docx
Hedef kitle: Premium restoranlar, lounge, beach club, pub & etkinlik mekanları.
Domain: tablewise.com.tr (birincil)
Alt domain yapısı:
  - tablewise.com.tr              → Landing page
  - app.tablewise.com.tr          → Admin panel
  - tablewise.com.tr/rezervasyon/[slug] → Booking UI

TEKNOLOJİ STACK (kesin karar):
- Backend:       .NET 8 Web API, Clean Architecture, CQRS (MediatR)
- Kural Motoru:  Custom IRuleEvaluator Pipeline (NRules KULLANMA)
- ORM:           EF Core 8 + Npgsql, Global Query Filter (TenantId)
- Cache:         Redis (StackExchange.Redis) — slot uygunluk, idempotency
- Gerçek Zamanlı: SignalR (Pro+ plan için)
- Auth:          JWT + refresh token (BCrypt), staff davet sistemi
- Dosya:         Cloudflare R2 (S3-compat) — VPS diski KULLANMA
- Email:         SendGrid
- SMS:           Netgsm (Pro+ plan)
- Ödeme:         İyzico (abonelik + kapora)
- Loglama:       Serilog → Seq/Elasticsearch
- Hata:          Sentry SDK
- Monitoring:    HealthChecks + Prometheus metrics
- Frontend:      React 18 + Vite + TailwindCSS + shadcn/ui
- State/Data:    Zustand + React Query v5
- Form:          React Hook Form + Zod
- Deploy:        Docker Compose → Ubuntu 22.04 VPS → Nginx + Certbot
- CI/CD:         GitHub Actions

MULTI-TENANT YAPISI:
- Tenant → Venue → Table hiyerarşisi (Business değil, TENANT!)
- Her tablo TenantId (UUID) içerir, indexed
- EF Core Global Query Filter TenantId'yi otomatik uygular
- Enterprise planda opsiyonel DB-per-tenant ilerde eklenecek

KRİTİK ZORUNLULUKLAR (İlk günden bunlar olmalı):
1. Her tabloda TenantId (indexed)
2. Her entity'de Soft Delete (IsDeleted + DeletedAt)
3. Conditions/Actions JSON'larında "version" alanı (şema migration için)
4. API endpoint'leri /api/v1/ prefix'i
5. POST /reserve için Idempotency-Key header zorunlu (Redis + DB)
6. ConfirmCode = kriptografik güvenli random, 8 karakter alphanumeric
7. PII log'lara yazılmaz (Serilog destructuring policy)
8. Plans tablosu DB'de, feature flag JSON (deploy gerektirmeden plan değiştirilir)
9. İyzico webhook signature doğrulanmadan işlem yapılmaz
10. VenueClosures olmadan slot hesaplama YAPMA

PLAN YAPISI:
- Starter    → ₺490/ay  → 1 mekan, 3 masa, 5 kural, 100 rez/ay, Email
- Pro        → ₺990/ay  → 1 mekan, sınırsız, SMS, CRM tier, Kapora modülü
- Business   → ₺1990/ay → 3 mekan, API, öncelikli destek
- Enterprise → Teklif   → Sınırsız, white-label, SLA

PROJE KLASÖR YAPISI:
/
├── src/
│   ├── Tablewise.Api/              → Controller, middleware, Program.cs
│   ├── Tablewise.Application/      → CQRS, validator, use cases, DTO
│   ├── Tablewise.Domain/           → Entity, value object, interface
│   ├── Tablewise.Infrastructure/   → EF Core, Redis, İyzico, SendGrid, R2
│   └── Tablewise.RuleEngine/       → IRuleEvaluator, şablonlar, pipeline
├── tests/
│   ├── Tablewise.UnitTests/
│   └── Tablewise.IntegrationTests/
├── frontend/
│   ├── admin-panel/                → app.tablewise.com.tr
│   ├── booking-ui/                 → tablewise.com.tr/rezervasyon/[slug]
│   └── landing/                    → tablewise.com.tr
├── docker/
│   ├── docker-compose.yml
│   ├── docker-compose.prod.yml
│   └── nginx/
├── scripts/
└── docs/
    └── Tablewise_Urun_Dokumani_v1.1.docx

YAZIM TALİMATLARI:
- Diff olarak ver (aksini söylemediysem). Tam dosya yazma.
- XML doc comment her public metoda zorunlu (Türkçe açıklama).
- Kod isimleri İngilizce, yorumlar Türkçe.
- Exception handling eksiksiz, swallow etme.
- LINQ sorguları `AsNoTracking()` kullan (read-only'de).
- `ConfigureAwait(false)` library code'da.
- Magic number yok — const veya config.

MEVCUT FAZ: [Burayı doldur: örn "Faz 1.3 — Entity tanımları"]
Dokunmam gereken dosyalar: [Varsa listele]
```

---

# FAZ 1 — BACKEND ÇEKİRDEĞİ

**Süre:** 2-3 hafta | **Chat sayısı:** ~8 | **Model:** Çoğunlukla Sonnet

## Prompt 1.1 — Solution ve Proje İskeleti

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.1 — Solution ve proje iskeleti.

Aşağıdaki komutları çalıştır ve şu dosyaları oluştur:

1. Solution:
   dotnet new sln -n Tablewise

2. Projeler:
   cd src
   dotnet new webapi    -n Tablewise.Api            --use-minimal-apis false
   dotnet new classlib  -n Tablewise.Application
   dotnet new classlib  -n Tablewise.Domain
   dotnet new classlib  -n Tablewise.Infrastructure
   dotnet new classlib  -n Tablewise.RuleEngine
   cd ../tests
   dotnet new xunit     -n Tablewise.UnitTests
   dotnet new xunit     -n Tablewise.IntegrationTests

3. Referanslar (Clean Architecture kuralları):
   - Api           → Application, Infrastructure
   - Application   → Domain
   - Infrastructure → Application (Domain implicit)
   - RuleEngine    → Domain
   - Api'de RuleEngine referansı VAR (DI için)
   - UnitTests     → Application, Domain, RuleEngine
   - IntegrationTests → Api

4. NuGet paketleri (versiyon belirt):
   Api:
     - Serilog.AspNetCore 8.*
     - Serilog.Sinks.Seq
     - Serilog.Sinks.Console
     - Sentry.AspNetCore
     - Swashbuckle.AspNetCore 6.*
     - FluentValidation.AspNetCore 11.*
     - Microsoft.AspNetCore.Authentication.JwtBearer
     - Microsoft.AspNetCore.SignalR
     - AspNetCoreRateLimit 5.*

   Application:
     - MediatR 12.*
     - FluentValidation 11.*
     - AutoMapper 13.*

   Infrastructure:
     - Microsoft.EntityFrameworkCore 8.*
     - Npgsql.EntityFrameworkCore.PostgreSQL 8.*
     - StackExchange.Redis 2.*
     - SendGrid 9.*
     - Iyzipay 2.*
     - AWSSDK.S3 3.* (R2 için S3-compat)
     - BCrypt.Net-Next 4.*

   Domain:
     - (bağımsız, paket yok)

   RuleEngine:
     - (sadece Domain referansı)

5. .gitignore ekle (.NET standart + .env + *.User)

6. docs/ klasörü oluştur, v1.1 docx'i oraya koyacağım (placeholder README ekle)

7. Directory.Build.props (root'ta):
   - <TargetFramework>net8.0</TargetFramework>
   - <Nullable>enable</Nullable>
   - <ImplicitUsings>enable</ImplicitUsings>
   - <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
   - <GenerateDocumentationFile>true</GenerateDocumentationFile>
   - <NoWarn>CS1591</NoWarn> (XML doc eksik uyarısı)

8. README.md — proje açıklaması, klasör yapısı, nasıl çalıştırılır.

Tüm çıktıyı kabuk komutu + dosya içerik şeklinde ver.
Diff değil, ilk kurulum olduğu için tam dosya iste.
```

---

## Prompt 1.2 — BaseEntity ve Ortak Value Object'ler

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat (kısa)

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.2 — Domain temel yapıları.

Tablewise.Domain projesinde aşağıdaki dosyaları oluştur:

1. Common/BaseEntity.cs:
   public abstract class BaseEntity
   {
       public Guid Id { get; set; } = Guid.NewGuid();
       public DateTime CreatedAt { get; set; }
       public DateTime? UpdatedAt { get; set; }
       public bool IsDeleted { get; set; }
       public DateTime? DeletedAt { get; set; }
   }

2. Common/TenantScopedEntity.cs : BaseEntity
   - public Guid TenantId { get; set; }

3. Enums/ klasörü altında şunlar:
   - UserRole: SuperAdmin, Owner, Staff
   - PlanTier: Starter, Pro, Business, Enterprise
   - PlanStatus: Trial, Active, PastDue, Suspended, Cancelled
   - ReservationStatus: Pending, Confirmed, Completed, Cancelled, NoShow
   - ReservationSource: BookingUI, ManualAdmin, ManualStaff, Api, Whatsapp
   - CustomerTier: Regular, Gold, VIP, Blacklisted
   - TableLocation: Indoor, Outdoor, Balcony, Bar, Private, Terrace, Garden
   - NotificationChannel: Email, Sms, Push
   - NotificationType: Confirm, Reminder, Cancel, NoShow, Welcome, PasswordReset
   - RuleActionType: Allow, Block, Warn, Suggest, Discount, Deposit, Redirect
   - RuleTrigger: OnReservationCreate, OnSeatAssign, OnCancel
   - DepositStatus: NotRequired, Pending, Paid, Refunded, Forfeited, Failed
   - DepositRefundPolicy: FullRefund, PartialRefund, NoRefund
   - CustomFieldType: Text, Number, Boolean, Select, Date
   - FieldType enumları için Flags değil, basit enum.

4. Exceptions/ klasörü:
   - DomainException (base)
   - NotFoundException (entity, id)
   - ValidationException (field errors dictionary)
   - UnauthorizedException
   - ForbiddenException
   - PlanLimitExceededException (currentLimit, upgradeUrl)
   - BusinessRuleException
   - TenantIsolationException (kritik güvenlik)

5. Common/Result.cs:
   Generic Result<T> pattern:
     - IsSuccess, Value, Error, ErrorCode
     - Success(value), Failure(error, code)
   CQRS handler'lar bunu döndürecek.

6. Interfaces/ICurrentUser.cs:
   - Guid? TenantId
   - Guid? UserId
   - UserRole? Role
   - string? Email

7. Interfaces/ITenantContext.cs:
   - Guid TenantId (nullable değil — HER zaman set edilmiş olmalı, değilse exception)
   - void SetTenant(Guid tenantId)

Her dosya için XML doc comment ekle. Tam dosya halinde ver.
```

---

## Prompt 1.3 — Ana Entity'ler (Tenant, Venue, Table ve ilişkiler)

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.3 — Ana entity'ler (v1.1 dokümanı Bölüm 8'e tam uyum).

Tablewise.Domain/Entities/ altında şu dosyaları oluştur:

1. Tenant.cs : BaseEntity
   Alanlar:
     - Name (string, 200)
     - Slug (string, 100, unique)
     - Email (string, 200, unique)
     - PasswordHash (string)
     - PlanId (Guid, FK → Plans)
     - PlanStatus (enum)
     - TrialEndsAt (DateTime)
     - PlanRenewsAt (DateTime?)
     - IsEmailVerified (bool)
     - EmailVerificationToken (string?)
     - PasswordResetToken (string?)
     - PasswordResetExpiry (DateTime?)
     - Settings (string, jsonb) — JSON metadata
     - ReservationCountThisMonth (int)
     - IsActive (bool, default true)
   Navigasyon:
     - List<User> Users
     - List<Venue> Venues
     - List<Subscription> Subscriptions

2. User.cs : TenantScopedEntity
   - Email, PasswordHash, FirstName, LastName
   - Role (UserRole enum)
   - IsActive
   - InvitedAt (DateTime?)
   - LastLoginAt (DateTime?)
   - PhoneNumber (string?)

3. UserInvitation.cs : TenantScopedEntity
   - Email, Role
   - Token (unique)
   - ExpiresAt
   - AcceptedAt (DateTime?)
   - InvitedByUserId (Guid)

4. Venue.cs : TenantScopedEntity
   - Name, Address (string?)
   - TimeZone (string, default "Europe/Istanbul")
   - OpeningTime, ClosingTime (TimeSpan)
   - SlotDurationMinutes (int, default 90)
   - WorkingHours (string, jsonb) — haftalık yapılandırma
   - LogoUrl (string?) — R2 URL
   - DepositEnabled (bool, default false) — KAPORA MODÜLÜ TOGGLE
   - DepositAmount (decimal?)
   - DepositPerPerson (bool, default false)
   - DepositRefundPolicy (enum)
   - DepositRefundHours (int?, min saat öncesi iade için)
   - DepositPartialPercent (decimal?)
   - PhoneNumber, Description (string?)
   Navigasyon:
     - List<Table> Tables
     - List<Rule> Rules
     - List<Reservation> Reservations
     - List<VenueClosure> Closures
     - List<VenueCustomField> CustomFields
     - List<TableCombination> TableCombinations

5. VenueClosure.cs : TenantScopedEntity
   - VenueId (FK)
   - Date (DateOnly) veya DateTime tarih bazlı
   - IsFullDay (bool)
   - OpenTime, CloseTime (TimeSpan?) — kısmi gün için
   - Reason (string?)
   NOT: Aynı VenueId + Date unique olmalı (index).

6. VenueCustomField.cs : TenantScopedEntity
   - VenueId (FK)
   - Label (string, 200)
   - FieldType (enum: Text, Number, Boolean, Select, Date)
   - IsRequired (bool)
   - SortOrder (int)
   - Options (string, jsonb) — Select için seçenekler

7. Table.cs : TenantScopedEntity
   - VenueId (FK)
   - Name (string, 100)
   - Capacity (int)
   - Location (TableLocation enum)
   - Description (string?)
   - SortOrder (int, default 0)
   - IsActive (bool, default true)

8. TableCombination.cs : TenantScopedEntity
   - VenueId (FK)
   - Name (string) — "Masa 3+4 birleşik"
   - TableIds (string, jsonb) — List<Guid>
   - CombinedCapacity (int)
   - IsActive (bool)

9. Customer.cs : TenantScopedEntity
   - FullName, Phone (string), Email (string?)
   - Tier (CustomerTier enum, default Regular)
   - IsBlacklisted (bool)
   - BlacklistReason (string?) — zorunlu field eğer blacklisted ise
   - Notes (string?)
   - TotalVisits (int)
   - LastReservationAt (DateTime?)
   NOT: Phone + TenantId composite unique index.

10. Reservation.cs : TenantScopedEntity
    - VenueId, TableId (FK)
    - TableCombinationId (Guid?, FK — masa birleşimi kullanıldıysa)
    - CustomerId (Guid?, FK)
    - GuestName, GuestEmail, GuestPhone
    - PartySize (int)
    - ReservedFor (DateTime) — tarih + saat
    - EndTime (DateTime) — ReservedFor + SlotDurationMinutes
    - Status (ReservationStatus)
    - Source (ReservationSource)
    - ConfirmCode (string, 8, unique)
    - SpecialRequests (string?)
    - InternalNotes (string?)
    - DiscountPercent (decimal?)
    - AppliedRulesSnapshot (string, jsonb) — runtime uygulanan kurallar
    - CustomFieldAnswers (string, jsonb) — VenueCustomField yanıtları
    - DepositStatus (DepositStatus enum)
    - DepositAmount (decimal?)
    - DepositPaymentRef (string?) — İyzico referans
    - DepositPaidAt (DateTime?)
    - DepositRefundedAt (DateTime?)
    - CancellationReason (string?)
    - CancelledAt (DateTime?)
    - ReminderSentAt (DateTime?)
    - ModifiedFromReservationId (Guid?) — eğer değiştirme ile oluştuysa

11. AppliedRule.cs (BaseEntity — TenantScoped DEĞİL, Reservation üzerinden gelir):
    - ReservationId, RuleId (FK)
    - ActionType (RuleActionType enum)
    - ActionPayload (string, jsonb)
    - EvaluatedAt

12. ReservationStatusLog.cs (BaseEntity):
    - ReservationId (FK)
    - FromStatus, ToStatus
    - ChangedByUserId (Guid?)
    - ChangedBy (string?) — "Sistem" veya email
    - Reason (string?)
    NOT: BaseEntity yeter, status log TenantId'yi Reservation'dan alır.

13. Rule.cs : TenantScopedEntity
    - VenueId (Guid?, FK — null = tenant geneli)
    - Name, Description (string?)
    - RuleType (string) — "EarlyBooking", "VIPPriority", "CustomCondition" vb.
    - ConditionsJson (string, jsonb) — version alanı içerir
    - ActionsJson (string, jsonb) — version alanı içerir
    - Priority (int, 1 = en yüksek)
    - TriggerType (RuleTrigger enum)
    - IsActive (bool)
    - ApplicableTimeSlots (string?, jsonb) — hangi saatler
    - TimesTriggered (int) — istatistik

14. Plan.cs : BaseEntity (TenantScoped DEĞİL — sistem geneli)
    - Name, Description
    - Tier (PlanTier enum)
    - MonthlyPriceTry (decimal)
    - YearlyPriceTry (decimal)
    - FeaturesJson (string, jsonb) — feature flag'ler
    - LimitsJson (string, jsonb) — maxRules, maxTables, maxReservations
    - IsVisible (bool)

15. Subscription.cs : TenantScopedEntity
    - PlanId (FK)
    - Status (enum)
    - PeriodStart, PeriodEnd
    - Amount (decimal), Currency ("TRY")
    - IyzicoSubscriptionId, IyzicoCustomerId (string?)
    - NextBillingDate (DateTime?)
    - CancelledAt (DateTime?)

16. NotificationLog.cs : TenantScopedEntity
    - ReservationId (Guid?, FK)
    - Channel (enum), Type (enum)
    - Recipient (string) — masked olabilir
    - Status (Sent, Failed, Pending)
    - ErrorMessage (string?)
    - SentAt (DateTime?)

17. AuditLog.cs : TenantScopedEntity
    - UserId (Guid?), PerformedBy (string)
    - Action (string) — "RULE_CREATED" vb.
    - EntityType, EntityId (string?)
    - OldValue, NewValue (string?, jsonb)
    - IpAddress (string?)

18. IdempotencyKey.cs : TenantScopedEntity
    - Key (string, unique) — client tarafından üretilen
    - ResponseJson (string) — cache'lenen response
    - ExpiresAt (DateTime)

Tüm entity'ler için navigation property'ler eklenmeli.
Public setter yerine mümkünse private setter + factory method kullan
(ama EF Core düşünerek makul ölçüde — aşırıya kaçma).

Bu prompt'u çok büyük bulursan sadece 1-8 arası ver, kalanı ayrı mesajda alırım.
```

**İpucu:** Bu prompt çok uzun. Cursor "çok uzun" derse ikiye böl: önce Tenant/User/Venue/VenueClosure/VenueCustomField/Table/TableCombination, sonra Customer/Reservation/diğerleri.

---

## Prompt 1.4 — EF Core DbContext + Global Query Filter

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.4 — EF Core yapılandırması.

Infrastructure/Persistence/ altında:

1. TablewiseDbContext.cs : DbContext
   - Tüm entity'ler için DbSet
   - Constructor ITenantContext inject edilecek
   
2. OnModelCreating:
   - Assembly'den tüm IEntityTypeConfiguration<T> uygula
   - GLOBAL QUERY FILTER (kritik):
     * TenantScopedEntity'den türeyen her entity için:
       modelBuilder.Entity<T>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId && !x.IsDeleted)
     * BaseEntity için sadece !x.IsDeleted filter'ı
     * Plan ve SuperAdmin entity'leri filter'sız olsun (tenant geneli)
   - Tüm jsonb alanlarına .HasColumnType("jsonb")
   - Unique index'ler:
     * Tenant.Slug, Tenant.Email
     * User.Email + TenantId composite
     * Venue.Name + TenantId composite (opsiyonel)
     * Reservation.ConfirmCode (global unique)
     * Customer.Phone + TenantId composite
     * VenueClosure.VenueId + Date composite
     * IdempotencyKey.Key (global unique)
   - Decimal precision: HasPrecision(10, 2) tüm para alanları
   - DateTime UTC converter — PostgreSQL timestamp with time zone

3. SaveChangesAsync override:
   - CreatedAt, UpdatedAt otomatik set
   - TenantScopedEntity için TenantId otomatik set (eğer boşsa)
   - Soft delete: Remove() çağrıldığında IsDeleted=true, DeletedAt=now yap
     (EF Core interceptor veya Entry().State kontrolü)
   - Her değişiklik için AuditLog üret (ama sadece audit entity'si değilse — recursion önle)

4. Configurations/ klasörü, her entity için ayrı IEntityTypeConfiguration:
   - TenantConfiguration
   - UserConfiguration
   - VenueConfiguration
   - ReservationConfiguration (en karmaşık, ilişkiler çok)
   - RuleConfiguration
   - ... diğerleri
   
   Her configuration:
     - Property length'leri (Name varsa 200 vb)
     - Required kolonlar
     - Index'ler
     - Relationships (HasOne, HasMany, WithMany, OnDelete davranışı)
     - Onemli: Cascade delete SADECE Tenant→Venue→Table için.
       Tenant→Reservation: Restrict (iş kuralı)
       Venue→Rule: Cascade OK
       Venue→Reservation: Restrict

5. Interceptors/AuditSaveChangesInterceptor.cs:
   - ISaveChangesInterceptor implement
   - BeforeSaveChanges: CreatedAt/UpdatedAt set
   - AfterSaveChanges: AuditLog kaydet (change tracker'dan)

6. Interceptors/SoftDeleteInterceptor.cs:
   - EntityState.Deleted → Modified + IsDeleted/DeletedAt set
   - Audit entity'leri hariç tut

7. Infrastructure/DependencyInjection.cs:
   AddInfrastructure(IConfiguration config):
     - DbContext Npgsql, EnableRetryOnFailure(3), interceptor'lar
     - Repository/UnitOfWork kayıtları (sonraki prompt'ta)
     - ITenantContext, ICurrentUser (HttpContextAccessor bazlı) — sonraki prompt

8. İlk migration:
   dotnet ef migrations add InitialCreate -p src/Tablewise.Infrastructure -s src/Tablewise.Api

Tüm dosyaları tam halde ver. Migration komutu output'unu da yaz
(migration dosyasının kendisini değil — migration başarısız olursa
düzeltme için).
```

---

## Prompt 1.5 — Repository, UnitOfWork, Seed Data

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.5 — Repository pattern ve seed data.

1. Infrastructure/Persistence/Repositories/GenericRepository.cs : IRepository<T>
   where T : BaseEntity
   
   Metodlar:
     - Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
     - Task<IReadOnlyList<T>> GetAsync(
         Expression<Func<T, bool>>? predicate = null,
         Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
         string? includeProperties = null,
         int? skip = null,
         int? take = null,
         bool asNoTracking = true,
         CancellationToken ct = default)
     - Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, ...)
     - Task AddAsync(T entity, ct)
     - void Update(T entity)
     - void Remove(T entity) — soft delete
     - Task<bool> ExistsAsync(Expression predicate, ct)
     - Task<int> CountAsync(Expression? predicate = null, ct)
     - IQueryable<T> Query() — karmaşık sorgular için (dikkatli kullan)

2. IUnitOfWork ve UnitOfWork implementation:
   - IRepository<Tenant> Tenants { get; }
   - IRepository<User> Users { get; }
   - ... tüm entity'ler
   - Task<int> SaveChangesAsync(CancellationToken ct = default)
   - Transaction başlatma: BeginTransactionAsync

3. Infrastructure/Services/TenantContext.cs : ITenantContext
   - HttpContext üzerinden TenantId çöz:
     * JWT claim'den (authenticated istekler için)
     * URL path'den: /rezervasyon/{slug} → Tenant.Slug → TenantId
     * Subdomain'den (app.tablewise.com.tr → claim'den tenant)
   - TenantId set edilmediyse ValidateTenant() exception fırlat

4. Infrastructure/Services/CurrentUserService.cs : ICurrentUser
   - HttpContext.User claim'lerini oku
   - TenantId, UserId, Role, Email döndür

5. Infrastructure/Persistence/DbSeeder.cs:
   Sadece Development ortamında çalışır (Program.cs'de kontrol).
   
   Seed data:
   - 4 Plan kaydı (Starter/Pro/Business/Enterprise, gerçek fiyatlar,
     FeaturesJson ve LimitsJson dolu)
   - 1 SuperAdmin user (TenantId null)
   - Demo Tenant: "Demo Restoran", slug "demo-restoran", plan Pro, trial bitmiş
   - Demo Venue: "Ana Salon", Europe/Istanbul, 12:00-23:00, slot 90dk
   - 5 Table (değişik kapasiteler: 2, 2, 4, 6, 8)
   - 1 TableCombination: Masa 3+4 birleştirilmiş (capacity 10)
   - 2 VenueClosure: Bayram günü tam kapalı, özel gün 21:00 kapanır
   - 3 VenueCustomField: "Doğum günü mü?" (Boolean), "Menü tercihi" (Select), "Özel istek" (Text)
   - 5 Rule (hazır şablonlardan):
     * Erken Rezervasyon (+7 gün, %10 indirim)
     * VIP Önceliği
     * Büyük Grup Yönlendirmesi (>6 kişi)
     * Kapora Zorunluluğu (hafta sonu akşam için — DEPOSIT action)
     * Masa Çevrim Süresi (30 dk arayla)
   - 15 Customer (çeşitli tier'larda)
   - 30 Reservation (son 30 gün dağılmış, değişik status'larda)

   Her seed entity için Id sabit (Guid.Parse) — testlerde tutarlılık için.
   Idempotent olsun: eğer Tenant varsa seed'i atla.

Tüm kodu tam olarak ver. Seed data büyüyecek, Constants/ klasörü kullan.
```

---

## Prompt 1.6 — Cloudflare R2 Dosya Depolama

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat (kısa)

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.6 — Cloudflare R2 dosya depolama servisi.

R2 S3-uyumlu olduğu için AWSSDK.S3 paketi kullan.

1. Application/Interfaces/IFileStorageService.cs:
   - Task<string> GeneratePresignedUploadUrlAsync(string key, string contentType, TimeSpan expiry)
   - Task<string> GetPresignedDownloadUrlAsync(string key, TimeSpan expiry)
   - Task DeleteAsync(string key)
   - Task<bool> ExistsAsync(string key)
   - string BuildTenantKey(Guid tenantId, string folder, string filename) — "tenants/{tid}/logos/{filename}"

2. Infrastructure/Storage/R2FileStorageService.cs : IFileStorageService
   - IAmazonS3 inject (AmazonS3Client, R2 endpoint'e point eder)
   - Presigned URL üretimi (GetPreSignedURL)
   - Max dosya boyutu: logo 2MB, genel 10MB (config'te)
   - İzin verilen content-type'lar: image/jpeg, image/png, image/webp
   - Bucket: "tablewise-files" (config'te)

3. appsettings.json'a:
   "R2": {
     "AccountId": "...",
     "AccessKey": "...",
     "SecretKey": "...",
     "BucketName": "tablewise-files",
     "PublicUrlBase": "https://cdn.tablewise.com.tr"
   }
   
   User-secrets'ta gerçek değerler. Boş bırak appsettings'te.

4. DI kaydı:
   - IAmazonS3 singleton (R2 endpoint'e point eden AmazonS3Config ile)
   - IFileStorageService scoped

5. Unit test (Tablewise.UnitTests/Infrastructure/):
   - Moq ile IAmazonS3 mock'la
   - GeneratePresignedUploadUrlAsync doğru parametreler gönderiyor mu?
   - BuildTenantKey doğru path üretiyor mu?

NOT: Gerçek R2 hesabı ilk aşamada şart değil, MinIO ile lokal test edilebilir.
docker-compose'a MinIO servisi ekle (sonraki faz). Şimdilik sadece kod.
```

---

## Prompt 1.7 — JWT Auth ve Middleware Stack

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat | **Neden Opus?** Güvenlik kritik

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.7 — Auth sistemi ve middleware (güvenlik kritik — dikkatli ol).

1. Application/Services/AuthService implementation:

   RegisterTenantAsync(RegisterDto dto):
     - Email global unique kontrolü (Tenant + User'larda)
     - Slug generator: name'den URL-friendly slug üret
       * Türkçe karakter → ASCII (İ→i, Ş→s vb)
       * Lowercase, spaces → tire
       * Uniqueness: mevcutsa -2, -3 ekle
     - Password: BCrypt WorkFactor 12 ile hash
     - Starter plan'a trial (14 gün) ile aç
     - EmailVerificationToken üret (Guid)
     - HoşgeldinEmail kuyruğa ekle
     - Tenant owner'ı User olarak da kaydet (Role: Owner)
     - JWT access token (60 dk) + refresh token (30 gün) döndür
     - AuditLog: "TENANT_REGISTERED"

   LoginAsync(email, password):
     - Brute-force koruması: Redis'te "login_fail:{ip}:{email}" counter
       * 5 fail → 15 dk kilit (throw BusinessRuleException)
     - Password BCrypt.Verify
     - Email verified mi? (verified değilse özel hata)
     - Tenant.PlanStatus kontrol (Suspended ise özel hata + support email)
     - LastLoginAt güncelle
     - Access + Refresh token döndür
     - Refresh token DB'ye kaydet (RevocableRefreshToken entity)
       — Başka prompt'ta bu entity'yi ekleyeceğiz

   RefreshTokenAsync(refreshToken):
     - DB'de bul, revoked değilse
     - Rotation: eski token revoke, yenisi oluştur
     - Yeni access + refresh ver

   LogoutAsync(refreshToken):
     - Refresh token revoke

   VerifyEmailAsync(token), ForgotPasswordAsync(email),
   ResetPasswordAsync(token, newPassword) — v1.1 dokümanında belirtilen akışlar.

2. Api/Controllers/AuthController:
   - POST /api/v1/auth/register
   - POST /api/v1/auth/login
   - POST /api/v1/auth/refresh
   - POST /api/v1/auth/logout (authenticated)
   - POST /api/v1/auth/verify-email
   - POST /api/v1/auth/forgot-password
   - POST /api/v1/auth/reset-password

   Her endpoint:
     - FluentValidation
     - Rate limit: auth endpoint'leri için 10 req/dakika
     - ProblemDetails hata formatı

3. Infrastructure/Auth/JwtTokenService.cs:
   - GenerateAccessToken(User, Tenant): 
     Claims: sub (userId), tenant_id, email, role, plan_tier, exp, iat
   - GenerateRefreshToken(): 64-byte random → base64
   - ValidateToken metodu
   - Symmetric key başlangıçta (HS256), Faz 9'da RS256'ya geç

4. Api/Middleware stack (Program.cs sırası önemli):
   1. UseExceptionHandler (GlobalExceptionHandler)
   2. UseSerilogRequestLogging
   3. UseHttpsRedirection
   4. UseCors (izinli origin'ler)
   5. UseRateLimiter
   6. UseAuthentication
   7. UseAuthorization
   8. UseMiddleware<TenantResolverMiddleware>
   9. UseMiddleware<IdempotencyMiddleware> — sonraki faz
   10. MapControllers
   11. MapHealthChecks

5. TenantResolverMiddleware:
   - Authenticated istek → JWT'den tenant_id çöz → ITenantContext.SetTenant
   - Booking UI istekleri (/rezervasyon/{slug}) → slug'dan Tenant çöz → set
   - Süper admin endpoint'leri → set etme, by-pass
   - Tenant bulunamazsa 404

6. GlobalExceptionHandler:
   - NotFound → 404 ProblemDetails
   - Validation → 422 + fields
   - PlanLimitExceeded → 403 + upgradeUrl
   - TenantIsolation → 403 (güvenlik event, Sentry'ye log)
   - Unauthorized → 401
   - Forbidden → 403
   - Diğer → 500 + Sentry + correlation ID

7. Rate limiting yapılandırma:
   - Booking endpoint'leri: IP bazlı, 30 req/dakika
   - Authenticated: user bazlı, 200 req/dakika
   - Auth endpoint'leri: 10 req/dakika
   - /reserve: 5 req/dakika IP bazlı (brute-force)

KAYNAKLAR İÇİN:
- v1.1 dokümanı Bölüm 9 "Hassas Veri Yönetimi" — tüm kurallar yerine getirilmeli.

Tüm kodu ver, middleware sırası kritik — Program.cs'i tam haliyle göster.
```

---

## Prompt 1.8 — Staff Davet Akışı

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 1.8 — Personel davet sistemi (v1.1 dokümanı Akış: Personel).

1. Application/Features/Staff/ altında CQRS:
   - InviteStaffCommand { Email, Role }
   - InviteStaffCommandHandler:
     * Sadece Owner çağırabilir (yetki kontrolü)
     * Email zaten bu tenant'ta user mı? → hata
     * Aktif davet varsa (ExpiresAt > now, AcceptedAt null) → hata
     * UserInvitation oluştur (Token = Guid, ExpiresAt = 7 gün)
     * Email kuyruğa: "Davetlisin, {link} ile katıl"
     * Link: app.tablewise.com.tr/invite/{token}
     * AuditLog
   
   - AcceptInvitationCommand { Token, FirstName, LastName, Password, PhoneNumber }
   - AcceptInvitationCommandHandler:
     * Invitation bul, expire mi / accepted mi kontrolü
     * User oluştur (bağlı Tenant'a), password hash, email verified=true
     * Invitation.AcceptedAt = now
     * JWT döndür (direkt giriş yapmış sayılsın)
     * AuditLog: "STAFF_JOINED"

   - CancelInvitationCommand { InvitationId } — Owner tarafından
   - ResendInvitationCommand { InvitationId }
   - ListInvitationsQuery → aktif + bekleyen davetler
   - ListStaffQuery → tenant'ın tüm kullanıcıları

2. Controllers:
   Api/Controllers/StaffController (Owner-only):
   - GET    /api/v1/staff                → liste
   - GET    /api/v1/staff/invitations    → bekleyen davetler
   - POST   /api/v1/staff/invitations    → davet gönder
   - POST   /api/v1/staff/invitations/{id}/resend
   - DELETE /api/v1/staff/invitations/{id}
   - PUT    /api/v1/staff/{id}/role      → rol değiştir (Staff ↔ Owner riski: owner sayısı min 1)
   - DELETE /api/v1/staff/{id}           → kullanıcı sil (soft)

   Api/Controllers/InviteController (Public):
   - GET    /api/v1/invite/{token}        → davet geçerli mi, email göster
   - POST   /api/v1/invite/{token}/accept → bilgileri ile kabul et

3. Permission Filter (Authorization):
   - [Authorize(Roles = "Owner")] attribute
   - [Authorize(Roles = "Owner,Staff")] attribute
   - Custom [RequireOwner] attribute yaz (daha okunabilir)

4. Email şablonu: StaffInvitationEmail.html
   - "Tablewise'a Davet Edildin"
   - Davet eden kişi adı, tenant adı, rol
   - "Daveti Kabul Et" butonu (link)
   - 7 gün geçerlilik

5. Rate limiting:
   - InviteStaff: tenant başına 10 req/saat (spam önle)
   - AcceptInvitation: token başına 5 deneme / 1 saat

6. Unit test:
   - Owner olmayan davet gönderemez
   - Aynı email'e aktif davet varken yeni davet hata verir
   - Expire olmuş token accept edilmez
   - Son Owner'ı Staff'a düşürme engellenir

Tüm kodu ver.
```

---

# FAZ 2 — API KATMANI

**Süre:** 1.5-2 hafta | **Chat sayısı:** ~5 | **Model:** Sonnet (kural motoru hariç)

## Prompt 2.1 — Tenant ve Venue Yönetimi API

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 2.1 — Tenant ve Venue yönetimi endpoint'leri (Owner authenticated).

CQRS/MediatR pattern. Her endpoint için Command/Query + Handler + Validator.

1. TenantController (JWT Owner):
   GET    /api/v1/tenant/me              → Profil + plan + trial durumu + limitler
   PUT    /api/v1/tenant/me              → Güncelle (name, vb — slug değiştirilemez)
   PUT    /api/v1/tenant/me/logo         → R2 presigned URL döndür, client direct upload
   POST   /api/v1/tenant/me/logo/confirm → Upload tamamlandı, Tenant.LogoUrl set
   GET    /api/v1/tenant/me/usage        → Bu ay kaç rez, kural, masa (plan limitine göre)
   GET    /api/v1/tenant/me/audit-logs   → Sayfalı

2. VenueController (JWT Owner):
   GET    /api/v1/venues                 → Tenant'ın mekanları
   GET    /api/v1/venues/{id}
   POST   /api/v1/venues                 → Plan limit: Starter 1, Pro 1, Business 3
   PUT    /api/v1/venues/{id}
   DELETE /api/v1/venues/{id}            → Aktif rezervasyonu varsa engelle
   PUT    /api/v1/venues/{id}/working-hours → WorkingHours JSON + SlotDuration
   PUT    /api/v1/venues/{id}/deposit    → DepositEnabled + tüm deposit ayarları

3. VenueClosureController:
   GET    /api/v1/venues/{id}/closures    → Yıllık liste
   POST   /api/v1/venues/{id}/closures    → Kapalı gün ekle
   PUT    /api/v1/venues/{id}/closures/{cid}
   DELETE /api/v1/venues/{id}/closures/{cid}
   POST   /api/v1/venues/{id}/closures/bulk → Birden çok gün tek seferde

4. VenueCustomFieldController:
   GET    /api/v1/venues/{id}/custom-fields
   POST   /api/v1/venues/{id}/custom-fields
   PUT    /api/v1/venues/{id}/custom-fields/{cfid}
   DELETE /api/v1/venues/{id}/custom-fields/{cfid}
   PUT    /api/v1/venues/{id}/custom-fields/reorder → List<{id, sortOrder}>

5. TableController:
   GET    /api/v1/venues/{id}/tables      → Sıralı
   POST   /api/v1/venues/{id}/tables      → Plan limit kontrolü (Starter 3 masa)
   PUT    /api/v1/venues/{id}/tables/{tid}
   DELETE /api/v1/venues/{id}/tables/{tid}
   PUT    /api/v1/venues/{id}/tables/reorder
   PUT    /api/v1/venues/{id}/tables/{tid}/toggle

6. TableCombinationController:
   GET    /api/v1/venues/{id}/combinations
   POST   /api/v1/venues/{id}/combinations   → {name, tableIds, combinedCapacity}
   PUT    /api/v1/venues/{id}/combinations/{cid}
   DELETE /api/v1/venues/{id}/combinations/{cid}

Her controller için:
- FluentValidation validator
- CQRS handler
- [Authorize(Roles = "Owner")]
- Swagger XML doc
- Error response'lar ProblemDetails formatında

Her Command/Query için:
- Tenant izolasyonu (handler'da ICurrentUser'dan TenantId al, 
  ama repository zaten global filter ile zorlayacak — doğrulama)
- Plan limit kontrolü (gerekirse PlanLimitService inject)

PlanLimitService (Application/Services/):
- CheckRuleLimitAsync(tenantId) → Plans.LimitsJson'dan oku
- CheckVenueLimitAsync, CheckTableLimitAsync
- CheckReservationLimitAsync
- Limit aşıldıysa PlanLimitExceededException

Tüm kodu diff olarak verebilirsin — sadece yeni dosyalar tam.
```

---

## Prompt 2.2 — Rezervasyon Motoru (Idempotency Dahil)

**Model:** Opus 4.5 | **Tahmini:** 2 chat | **Neden Opus?** Race condition, idempotency, slot overlap hepsi kritik

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 2.2 — Rezervasyon motoru. Bu projenin en kritik parçası.
3 ana konu: (A) Idempotency, (B) Slot çakışma önleme, (C) Rezervasyon değiştirme.

=== KISIM A: IDEMPOTENCY ===

1. Api/Middleware/IdempotencyMiddleware:
   - Sadece POST endpoint'lerinde aktif
   - Header "Idempotency-Key" var mı kontrol
   - Yoksa (booking endpoint'lerinde) → 400 BadRequest, mesaj: "Idempotency-Key header required"
   - Varsa: Redis'te "idem:{tenantId}:{key}" var mı
     * Varsa → cached response'u döndür, handler'a hiç girme
     * Yoksa → request'i işle, response'u Redis'e yaz (TTL 60 sn)
     * Ayrıca DB'ye IdempotencyKey olarak kaydet (TTL 24 saat)
   - Redis down olursa DB fallback

2. IdempotencyService (Application/Services/):
   - Task<CachedResponse?> GetAsync(tenantId, key)
   - Task SaveAsync(tenantId, key, response, ttl)
   - TTL 60 sn Redis, 24 saat DB

=== KISIM B: SLOT ÇAKIŞMA ÖNLEME ===

3. Application/Services/SlotAvailabilityService:
   GetAvailableSlotsAsync(venueId, date, partySize, tableId? = null):
   Algoritma:
   - Venue'nin çalışma saatleri al (WorkingHoursJson)
   - VenueClosures kontrolü: o gün tamamen kapalıysa [] döndür
   - Kısmi kapalıysa başlangıç/bitiş override et
   - SlotDurationMinutes'e göre 00:00'dan başlayarak aralıklar üret
   - Her slot için: aktif masalar + hedef masa capacity ≥ partySize
   - Var olan rezervasyonlar (o gün, o venue, Status in Confirmed/Pending):
     * Her masa için [start, end] aralıkları topla
     * Yeni slot ile çakışma var mı?
     * TableCombination kullanılıyorsa dahil masalar da dolu sayılsın
   - Redis cache: "avail:{venueId}:{date}" TTL 5 dk
     * Herhangi bir rezervasyon create/cancel/update → cache invalidate

4. ReserveCommand.Handler (en kritik):
   PostgreSQL advisory lock + transaction kullan (race condition önemli):
   
   ```
   using var tx = await _db.BeginTransactionAsync(IsolationLevel.RepeatableRead);
   await _db.ExecuteAsync("SELECT pg_advisory_xact_lock(@key)", 
       new { key = HashVenueDate(venueId, date) });
   
   // slot dolu mu tekrar kontrol (transaction içinde)
   // dolu değilse create
   // commit
   ```
   
   Veya Redis distributed lock (SETNX) tercih ederseniz.
   
   Flow:
   a. Müsaitlik kontrolü (yukarıdaki service)
   b. Kural motoru evaluate (Faz 3'te gelecek — şimdilik stub: RuleResult.Allow())
   c. Eğer RuleResult.IsBlocked → 422 + mesaj
   d. Customer bul/oluştur (email veya phone ile)
   e. DepositEnabled + kural DEPOSIT action var → Deposit akışı başlat (Faz 7)
   f. Reservation oluştur, Status = Confirmed (deposit yoksa) / Pending (deposit bekleniyor)
   g. ConfirmCode üret: RandomNumberGenerator ile 8 karakter alphanumeric
      * Çakışma olasılığı düşük ama kontrol et (retry 3 kez)
   h. AppliedRulesSnapshot JSON olarak kaydet
   i. Email kuyruğa at (onay)
   j. Redis cache invalidate
   k. Tenant.ReservationCountThisMonth Redis INCR (atomic)
   l. AuditLog
   m. Reservation döndür (ConfirmCode'la)

=== KISIM C: REZERVASYON DEĞİŞTİRME ===

5. ModifyReservationCommand { ConfirmCode, NewDateTime?, NewTableId?, NewPartySize? }
   Handler:
   - Reservation bul (ConfirmCode ile — public endpoint olabilir)
   - ReservedFor - Now >= 24 saat mi? Değilse hata
   - Status Confirmed mi?
   - Yeni değerlerle yukarıdaki "reserve" akışını çalıştır
     (idempotency dahil, slot lock dahil, kural motoru dahil)
   - Eski reservation'a Status = Modified (yeni enum değeri eklemen gerekebilir)
     VEYA aynı record güncelle + ReservationStatusLog
   - Kapora ödendiyse yeni rezervasyona transfer (ref kopyala)
   - Değişiklik email bildirimi

=== ENDPOINT'LER ===

6. Api/Controllers/BookingController (PUBLIC — slug bazlı):
   GET  /api/v1/book/{slug}/config               → Venue config (UI için)
   GET  /api/v1/book/{slug}/availability          → ?date=2026-05-10&partySize=4
   POST /api/v1/book/{slug}/evaluate              → Kural preview (kayıt yok)
   POST /api/v1/book/{slug}/reserve               → Rezervasyon oluştur (Idempotency-Key zorunlu)
   GET  /api/v1/book/confirm/{code}               → Public: rezervasyon detay
   POST /api/v1/book/confirm/{code}/cancel        → Public (24 saat öncesine kadar)
   POST /api/v1/book/confirm/{code}/modify        → Public modify

7. Api/Controllers/ReservationController (JWT authenticated):
   GET    /api/v1/reservations              → Staff/Owner list, filter
   GET    /api/v1/reservations/{id}
   POST   /api/v1/reservations              → Manuel rezervasyon
   PUT    /api/v1/reservations/{id}/status  → Completed / NoShow işaretleme
   PUT    /api/v1/reservations/{id}/cancel  → İşletme tarafından iptal
   POST   /api/v1/reservations/{id}/notes   → Internal not ekle
   GET    /api/v1/reservations/export       → CSV bu ay

Tüm kodu tam halde ver (bu faz kritik, özet verme).
Unit test zorunlu:
- 100 eş zamanlı istek aynı slota → 1 başarılı, 99 hata
- Idempotency-Key aynı → aynı response
- 23 saat sonra modify → hata
- 25 saat sonra modify → başarılı
```

---

## Prompt 2.3 — Rule Management API

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 2.3 — Kural yönetimi endpoint'leri (henüz kural motoru bağlanmamış, sadece CRUD).

1. RuleController:
   GET    /api/v1/rules                     → Tenant'ın tüm kuralları
   GET    /api/v1/rules/{id}
   POST   /api/v1/rules                     → Plan limit (Starter 5 kural)
   PUT    /api/v1/rules/{id}
   DELETE /api/v1/rules/{id}
   PUT    /api/v1/rules/{id}/toggle
   PUT    /api/v1/rules/reorder             → Priority sırasını topluca güncelle
   POST   /api/v1/rules/{id}/test           → Örnek context ile test (Faz 3'te motor gelecek)
   GET    /api/v1/rules/templates           → Hazır şablonlar (sabit liste)
   GET    /api/v1/rules/stats               → Her kural kaç kez tetiklendi

2. Rule Templates (sabit, kodda — JSON olarak RuleTemplatesProvider.cs):
   Her template {
     id, name, description, icon,
     defaultConditionsJson, defaultActionsJson,
     paramsSchema (UI form oluşturmak için)
   }
   
   Template'ler (v1.1 doküman bölüm 6):
   - early_booking
   - vip_priority
   - large_group
   - deposit_required (NEW — kapora modülüne bağlı)
   - peak_hour
   - min_duration
   - blacklist
   - special_day
   - table_cooldown
   - min_spend
   - custom_condition

3. RuleValidation:
   ConditionsJson schema validator:
   - version alanı zorunlu
   - Her condition type için gerekli parametreler kontrol
   - Invalid JSON → 422

4. RuleTestService (Faz 3 motor hazır olunca dolacak):
   Şimdilik stub:
   - Input: rule + sample context
   - Output: "Kural motoru henüz implement edilmedi (Faz 3)"

Tüm kodu diff olarak ver.
```

---

# FAZ 3 — KURAL MOTORU (CUSTOM PIPELINE)

**Süre:** 2 hafta | **Chat sayısı:** ~4 | **Model:** ÇOĞUNLUKLA OPUS (teknik kalp)

## Prompt 3.1 — IRuleEvaluator Pipeline Temeli

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 3.1 — Custom Kural Motoru (NRules KULLANMIYORUZ — v1.1 kararı).

Tablewise.RuleEngine projesinde:

1. Facts/ReservationContext.cs:
   Kural motoruna girecek tüm bilgi:
   - Tenant (snapshot, readonly)
   - Venue
   - Reservation (draft — henüz kaydedilmedi)
   - Table / TableCombination
   - List<GuestProfile> Guests (v1.1 dokümanda fiili bir yapı yok, dahil edip edilmeyeceği tartışmalı — biz şimdilik opsiyonel tutalım)
   - Customer (varsa, yoksa null)
   - CurrentOccupancyRate (double, bu slot için)
   - DaysInAdvance (int)
   - IsEarlyBooking, IsPeakHour (computed)
   - EvaluatedAt (DateTime)

2. Results/RuleEvaluationResult.cs:
   - IsBlocked (bool)
   - List<RuleOutcome> Outcomes:
     * RuleId, RuleName, ActionType, Message, Payload (JSON)
   - PreferredPosition (string?)
   - TotalDiscountPercent (decimal)
   - RequiresDeposit (bool)
   - DepositAmount (decimal?)
   - List<Table> SuggestedAlternatives
   - List<string> Warnings
   - List<string> Infos
   - AppliedRulesSnapshotJson (string) — Reservation'a kaydedilecek

3. Core interfaces:
   
   IRuleEvaluator:
     - string RuleType { get; }
     - Task<RuleOutcome?> EvaluateAsync(Rule rule, ReservationContext ctx, CancellationToken ct)
     - Null dönerse kural tetiklenmedi demektir.
   
   IRuleEnginePipeline:
     - Task<RuleEvaluationResult> ExecuteAsync(ReservationContext ctx, CancellationToken ct)

4. RuleEnginePipeline implementation (Services/RuleEnginePipeline.cs):
   
   Algoritma:
   a. DB'den aktif kuralları çek (tenant + venue scope, priority desc)
   b. Her kural için RuleType'a göre doğru IRuleEvaluator'ı bul (DI registry)
   c. Evaluator.EvaluateAsync çağır
   d. Outcome null ise skip
   e. Outcome.ActionType = Block → pipeline'ı durdur, IsBlocked=true döndür
   f. Diğer Action'lar için Result'a ekle, devam
   g. Sonuç döndür
   
   Performance: Paralel değil — Priority önemli, sırayla.
   Cache: Rule list'i Redis "rules:{tenantId}:{venueId}" TTL 5 dk
     * Rule CRUD → invalidate

5. RuleEvaluator Registry:
   - RuleType → IRuleEvaluator mapping
   - DI: services.AddScoped<IRuleEvaluator, EarlyBookingRuleEvaluator>()
     services.AddScoped<IRuleEvaluator, VipPriorityRuleEvaluator>()
     ... vb
   - Factory pattern: IRuleEvaluatorFactory.GetFor(ruleType)

6. Her evaluator için base class: RuleEvaluatorBase
   - Protected: ParseConditions<T>() — ConditionsJson'ı typed DTO'ya parse et
   - Protected: ParseActions<T>()
   - Version kontrolü: schemaVersion mismatch ise warning log

7. Tablewise.Api/DependencyInjection içine AddRuleEngine() extension:
   - Tüm IRuleEvaluator'ları otomatik register et (assembly scan)

8. Unit test (UnitTests/RuleEngine/):
   - Mock Rule + Context ile pipeline çalışıyor mu
   - Block encountered → kesiliyor mu
   - Priority sırası doğru mu
   - Evaluator bulunamıyor → log warning, skip

Tüm kodu tam halde ver. Bu temel — üzerine 10+ evaluator inşa edeceğiz.
```

---

## Prompt 3.2 — 6 Temel Kural Evaluator Implementasyonu

**Model:** Opus 4.5 (ilk 2) + Sonnet (kalan 4) | **Tahmini:** 2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 3.2 — Hazır kural şablonları için 6 evaluator.

Her evaluator Tablewise.RuleEngine/Evaluators/ altında.

1. EarlyBookingRuleEvaluator:
   RuleType = "early_booking"
   Condition schema:
     { version: 1, minDaysInAdvance: int, maxDaysInAdvance?: int }
   Action schema:
     { version: 1, discountPercent?: decimal, priorityBoost?: bool, label?: string }
   
   Logic:
     if ctx.DaysInAdvance >= minDays [and <= maxDays if set]:
       outcome Discount dönmek istiyorsa → ActionType = Discount
       Payload = { discountPercent }
   
2. VipPriorityRuleEvaluator:
   RuleType = "vip_priority"
   Condition: { version: 1, minTier: "VIP" | "Gold" }
   Action: { version: 1, suggestBestTable: bool, allowOverbook: bool }
   
   Logic:
     if ctx.Customer?.Tier >= minTier:
       → Suggest en iyi (SortOrder=1) müsait masa
       → Warnings'e "VIP misafiriniz için özel masa ayrıldı"

3. LargeGroupRuleEvaluator:
   RuleType = "large_group"
   Condition: { version: 1, minPartySize: int }
   Action: { version: 1, requireCombination: bool, warn: bool, message: string }
   
   Logic:
     if ctx.Reservation.PartySize >= minPartySize:
       Seçilen masa kapasitesi yetersizse:
         TableCombination ara, uygun var mı
         Yoksa Block + mesaj
         Varsa Suggest

4. DepositRequiredRuleEvaluator:
   RuleType = "deposit_required"
   Condition: { 
     version: 1, 
     scopes?: { days?: string[], times?: string[], tableIds?: Guid[], minPartySize?: int }
   }
   Action: { 
     version: 1, 
     amount?: decimal, 
     perPerson?: bool,
     useVenueDefault?: bool
   }
   
   Logic:
     Venue.DepositEnabled kontrolü — değilse skip
     Scope match ediyorsa:
       → RequiresDeposit = true
       → DepositAmount hesapla (useVenueDefault ise venue config'ten)
       → ActionType = Deposit

5. PeakHourRuleEvaluator:
   RuleType = "peak_hour"
   Condition: { 
     version: 1, 
     startTime: "19:00", endTime: "22:00",
     minOccupancyPercent: 80,
     days?: string[]
   }
   Action: { version: 1, block?: bool, warn?: bool, message: string }

6. TableCooldownRuleEvaluator:
   RuleType = "table_cooldown"
   Condition: { version: 1, cooldownMinutes: int }
   Action: { version: 1, block: true, message: string }
   
   Logic:
     Aynı masada önceki rezervasyon bitişi + cooldown > new start → Block

Her evaluator için:
- Unit test en az 3 senaryo (trigger eder, etmez, edge case)
- XML doc comment

İstersen bu prompt'u ikiye böl: 1-2 bir chat, 3-6 ikinci chat.
```

---

## Prompt 3.3 — Custom Condition Evaluator + Test Mode

**Model:** Opus 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 3.3 — Özel kural (custom_condition) ve test modu.

1. CustomConditionRuleEvaluator:
   RuleType = "custom_condition"
   İşletme "kendi kuralı"nı tanımlayabilsin.
   
   Condition schema:
   {
     version: 1,
     operator: "and" | "or",
     conditions: [
       { field: "partySize", op: ">=", value: 6 },
       { field: "customer.tier", op: "==", value: "VIP" },
       { field: "reservation.reservedFor.dayOfWeek", op: "in", value: ["Friday","Saturday"] }
     ]
   }
   
   Safe expression evaluator (DANGER: asla eval kullanma):
   - Whitelist field'lar: 
     "partySize", "daysInAdvance", "customer.tier", "customer.totalVisits",
     "reservation.reservedFor.hour", "reservation.reservedFor.dayOfWeek",
     "venue.currentOccupancy", "table.capacity", "table.location"
   - Operators: ==, !=, <, <=, >, >=, in, contains
   - ReservationContext'ten reflection ile oku
   - Hata durumunda false döndür, Sentry'ye log
   
   Action schema: diğer kurallardakine benzer (block/warn/discount/suggest)

2. RuleTestService implementation:
   TestRuleAsync(Rule rule, SampleContext sample):
   - Sample context'ten ReservationContext üret (in-memory, DB'ye dokunma)
   - Sadece bu tek kuralı evaluate et (pipeline değil)
   - Outcome döndür + tetiklendi mi bilgisi
   
   SampleContext DTO'su:
   {
     partySize: int,
     daysInAdvance: int,
     customerTier: string,
     dayOfWeek: string,
     hour: int,
     venueOccupancy: double,
     tableCapacity: int
   }

3. Test endpoint'i (daha önceki stub'u doldur):
   POST /api/v1/rules/{id}/test
   Body: SampleContextDto
   Response: { triggered: bool, outcome: {...}, executionMs: int }

4. RuleSchemaValidator:
   - Condition JSON'u parse et, version kontrol et
   - Unknown field'lar → error
   - Bu validator Rule CRUD'da da kullanılıyor (Faz 2.3'te ekledik)
   - Kapsamlı test et

Tüm kodu ver. Güvenlik kritik — custom expression'da eval veya reflection injection olmasın.
```

---

## Prompt 3.4 — Rule Engine Entegrasyonu

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 3.4 — Rule engine'i rezervasyon akışına bağla.

Faz 2.2'de ReserveCommandHandler'a "kural motoru stub" koymuştuk:
// var ruleResult = RuleResult.Allow(); // TODO Faz 3

Şimdi gerçek bağlantıyı yap:

1. ReserveCommandHandler güncelle:
   - IRuleEnginePipeline inject
   - Müsaitlik kontrolünden sonra, kayıt öncesi:
     ReservationContext context = BuildContext(dto, tenant, venue, table, customer);
     RuleEvaluationResult result = await _pipeline.ExecuteAsync(context, ct);
     
     if (result.IsBlocked) return Result.Failure(...);
     
     reservation.DiscountPercent = result.TotalDiscountPercent > 0 ? result.TotalDiscountPercent : null;
     reservation.AppliedRulesSnapshot = result.AppliedRulesSnapshotJson;
     
     if (result.RequiresDeposit) {
       reservation.DepositStatus = DepositStatus.Pending;
       reservation.DepositAmount = result.DepositAmount;
       // Deposit ödeme akışı Faz 7'de tamamlanacak
     }

2. EvaluateCommandHandler (sadece preview, kayıt yok):
   Aynı context build + pipeline execute, sonucu döndür.

3. ModifyReservationCommand handler de aynı kural motoru akışından geçsin.

4. ManuelReservation (işletme tarafından) için özel flag:
   - [Authorize] authenticated manuel rezervasyonda kural motoru OPSİYONEL
   - Request body'de "overrideRules: true" varsa pipeline bypass
   - Owner/Staff için izinli, audit log'a "RULES_OVERRIDDEN" yaz

5. Rule TimesTriggered güncellemesi:
   - Her outcome dönen kural için TimesTriggered++
   - Performans için batch update (UnitOfWork sonunda)
   - Veya Redis INCR + periyodik DB flush

6. Integration test:
   - 5 kural seed'ten, bir rezervasyon yap
   - AppliedRulesSnapshot dolmuş mu
   - Discount doğru uygulanmış mı
   - Block rule ile engelli → 422 mi

Diff olarak ver.
```

---

# FAZ 4 — EMAIL & DOSYA DEPOLAMA

**Süre:** 1 hafta | **Chat sayısı:** ~2 | **Model:** Sonnet

## Prompt 4.1 — SendGrid Email Servisi + Şablonlar

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 4.1 — Email servisi (SendGrid) ve tüm HTML şablonlar.

1. Infrastructure/Email/SendGridEmailService.cs : IEmailService
   - SendAsync(to, subject, html, plainText?)
   - SendTemplatedAsync(to, EmailTemplate enum, Dictionary<string,object> data)
   - Retry policy: Polly ile 3 deneme, exponential backoff
   - Hata durumunda NotificationLog kaydı (Status = Failed, ErrorMessage)

2. Email Queue (Redis):
   Infrastructure/Email/EmailQueueService.cs:
   - EnqueueAsync(emailRequest): Redis list LPUSH
   - Emails handler'larda doğrudan queue'ya yaz, anında gönderme
   - Background worker queue'dan tüketir

3. Infrastructure/HostedServices/EmailWorkerService.cs : BackgroundService
   - Redis queue'dan BRPOP ile blocking al
   - SendGridEmailService'e forward
   - 3 retry
   - NotificationLog kaydet

4. Email Templates — Infrastructure/Email/Templates/ altında HTML dosyaları:
   
   Tüm şablonlar:
   - Responsive (inline CSS, max 600px)
   - Koyu lacivert + altın sarısı
   - Türkçe
   - Plain text versiyonu otomatik üret (HtmlAgilityPack ile strip)
   - {{placeholder}} syntax
   
   Şablonlar:
   
   a) welcome.html:
      "Tablewise'a Hoş Geldiniz! 🎉"
      - Tenant adı
      - Email doğrulama butonu ({{verificationUrl}})
      - 14 günlük trial bilgisi
      - "Dashboard'a Git" CTA
      - Destek emaili: destek@tablewise.com.tr
   
   b) email-verification.html: Basit — doğrulama linki
   
   c) password-reset.html:
      - Sıfırlama linki (1 saat geçerli)
      - Güvenlik uyarısı
   
   d) reservation-confirm.html:
      - "Rezervasyonunuz Onaylandı ✓"
      - Tarih, saat, mekan, masa, kişi sayısı
      - {{confirmCode}} (büyük, belirgin)
      - İptal linki: tablewise.com.tr/rezervasyon/iptal/{{code}}
      - Değiştirme linki: tablewise.com.tr/rezervasyon/degistir/{{code}}
      - Mekan iletişim
      - Uygulanan indirim varsa göster
      - Kapora bilgisi (ödendiyse)
   
   e) reservation-modified.html: Değişiklik bildirimi
   
   f) reservation-cancelled.html: İptal bildirimi
   
   g) reservation-reminder.html:
      "Yarın rezervasyonunuz var 🍽"
      - Detaylar + Google Maps linki
   
   h) new-reservation-owner.html (işletmeye):
      - Yeni rezervasyon detayları
      - Admin panel linki
   
   i) no-show-notification.html
   
   j) staff-invitation.html:
      - Davet eden kişi, tenant adı, rol
      - Kabul linki ({{inviteUrl}})
      - 7 gün geçerlilik
   
   k) trial-expiry-reminder.html (3 gün kala):
      - Upgrade CTA
   
   l) plan-upgraded.html, plan-payment-failed.html
   
   m) deposit-paid.html, deposit-refunded.html

5. EmailTemplateRenderer:
   - HTML dosyayı embedded resource olarak oku
   - Placeholder replace (Regex)
   - Plain text üret (HtmlAgilityPack)

6. EmailTemplate enum:
   Welcome, EmailVerification, PasswordReset, ReservationConfirm,
   ReservationModified, ReservationCancelled, ReservationReminder,
   NewReservationOwner, NoShowNotification, StaffInvitation,
   TrialExpiryReminder, PlanUpgraded, PlanPaymentFailed,
   DepositPaid, DepositRefunded

7. DI kayıtları (Infrastructure DependencyInjection).

8. appsettings.json:
   "SendGrid": {
     "ApiKey": "",  // user-secrets'ta
     "FromEmail": "noreply@tablewise.com.tr",
     "FromName": "Tablewise",
     "ReplyTo": "destek@tablewise.com.tr"
   }

Tüm şablon dosyalarını tam halde ver — uzun olacak ama kritik.
Cursor'a "şablonları sadeleştirmeden tam HTML ver" de.
```

---

# FAZ 5 — ADMIN PANEL

**Süre:** 3-4 hafta | **Chat sayısı:** ~7 | **Model:** Sonnet

## Prompt 5.1 — Admin Panel İskeleti ve Auth

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 5.1 — React Admin Panel iskeleti.

/frontend/admin-panel altında Vite React projesi.

1. Kurulum:
   npm create vite@latest admin-panel -- --template react-ts
   npm install:
     @tanstack/react-query@5 axios react-hook-form zod @hookform/resolvers
     zustand react-router-dom@6 recharts lucide-react date-fns
     @radix-ui/react-dialog @radix-ui/react-dropdown-menu 
     @radix-ui/react-select @radix-ui/react-tabs @radix-ui/react-toast
     @radix-ui/react-slider @radix-ui/react-switch
     clsx tailwind-merge class-variance-authority
     framer-motion sonner (toast)
     @microsoft/signalr (gerçek zamanlı için Faz 7)
     @dnd-kit/core @dnd-kit/sortable (drag drop)

2. Tailwind config + shadcn/ui setup:
   npx shadcn@latest init
   Komponentleri kur: button, input, dialog, dropdown, card, 
     select, tabs, toast, switch, slider, badge, avatar, skeleton

3. Renk paleti (tailwind.config.ts):
   primary: #0f172a (slate-900)
   accent: #f59e0b (amber-500)
   Dark mode default.
   Font: Inter

4. Klasör yapısı:
   src/
     lib/
       api.ts          → axios instance + interceptors
       auth.ts         → auth hooks
       cn.ts
     stores/
       authStore.ts    → zustand
       uiStore.ts
     hooks/
       useAuth.ts
       useCurrentTenant.ts
     features/
       auth/
         LoginPage.tsx
         RegisterPage.tsx
         ForgotPasswordPage.tsx
         VerifyEmailPage.tsx
       dashboard/
       tables/
       rules/
       reservations/
       customers/
       staff/
       settings/
       subscription/
     components/
       ui/              → shadcn generated
       layout/
         AppLayout.tsx
         Sidebar.tsx
         TopBar.tsx
       common/
     router.tsx
     App.tsx
     main.tsx

5. lib/api.ts:
   - axios.create({ baseURL: import.meta.env.VITE_API_URL })
   - Request interceptor: Bearer token from authStore
   - Request interceptor: Idempotency-Key for POST (UUID generate)
   - Response interceptor:
     * 401 → refresh token attempt
     * 401 sonrası fail → logout + redirect /login
     * 403 + body.errorCode === "PLAN_LIMIT_EXCEEDED" → 
       toast "Plan limitinize ulaştınız" + upgrade linki
     * 5xx → toast "Sistem hatası"
     * network error → toast "Bağlantı hatası"

6. stores/authStore.ts (Zustand + persist):
   - user, accessToken, refreshToken
   - login(credentials), logout(), refresh()
   - localStorage persist

7. Router (react-router-dom):
   - /login, /register, /forgot-password, /reset-password/:token, /verify-email/:token
     → Public
   - /invite/:token → public, davet kabul
   - Diğer tüm route'lar → authenticated (ProtectedRoute wrapper)
   - /dashboard, /tables, /rules, /reservations, /customers, 
     /staff, /settings, /subscription, /onboarding

8. AppLayout:
   - Sol sidebar (collapsible, w-64 / w-16)
   - Top bar: breadcrumb + notifications + user menu
   - Main content: max-w-screen-2xl, padding

9. Sidebar içerik:
   - Tablewise logo + adı
   - Nav: Dashboard 📊, Rezervasyonlar 📅, Masalar 🪑, Kurallar ⚙️,
     Müşteriler 👥, Ekip 🧑‍💼, Ayarlar 🔧
   - Alt: Plan badge + "Yükselt" (eğer Pro+ değilse)
   - Alt: Kullanıcı avatar + çıkış

10. Login sayfası:
    - Email + password form (RHF + Zod)
    - "Şifremi unuttum" linki
    - "Hesabın yok mu? Kayıt ol"
    - Koyu gradient arka plan, centered card
    - Form hata mesajları (yanlış şifre, hesap askıda vb)

11. Register sayfası:
    - İşletme adı, email, şifre (min 8, büyük+küçük+rakam)
    - Şifre tekrarı
    - KVKK onay checkbox'ı
    - Başarılı kayıt → "Email doğrulama" ekranı (sonra giriş)

12. ProtectedRoute:
    - Token yoksa → /login (redirect after login için query param)
    - Token var ama TrialExpired → banner göster
    - PlanStatus = Suspended → /subscription'a zorla

13. .env.example:
    VITE_API_URL=http://localhost:5000
    VITE_BOOKING_BASE_URL=http://localhost:5173/rezervasyon

Tüm kodu tam halde ver. Mobilde de düzgün çalışsın (responsive).
```

---

## Prompt 5.2 — Dashboard

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 5.2 — Admin Panel Dashboard sayfası.

/frontend/admin-panel/src/features/dashboard/ altında.

1. DashboardPage.tsx:
   
   Layout (grid):
   
   Üst satır — 4 stat kartı:
   - "Bugünkü Rezervasyonlar" (sayı + dün karşılaştırma: +3, ↑)
   - "Bu Hafta Doluluk" (% + geçen hafta karşılaştırma)
   - "Bu Ay Rezervasyon" ("87 / 500" + progress bar, plan limiti varsa)
   - "Aktif Kurallar" (sayı)
   
   Orta satır:
   Sol (7 gün grafiği, col-span-2):
     - Recharts BarChart
     - Son 7 gün rezervasyon sayısı + doluluk % ikincil axis
     - Hover tooltip
   
   Sağ (Bugünün rezervasyonları):
     - Saat bazlı liste (12:00, 13:00... )
     - Her rezervasyon: müşteri adı, kişi sayısı, masa, status badge
     - Tıklanınca detay drawer açılır
   
   Alt satır:
   - Son aktiviteler feed (AuditLog son 10)
   - En sık tetiklenen kurallar top 5 (TimesTriggered desc)

2. React Query hooks:
   - useDashboardStats() → /api/v1/tenant/me/stats
   - useTodayReservations() → /api/v1/reservations?date=today
   - useAuditLogRecent() → /api/v1/tenant/me/audit-logs?limit=10
   - useRuleStats() → /api/v1/rules/stats
   
   Auto-refresh: 30 sn (refetchInterval)

3. Trial banner (ayrı component, AppLayout'ta):
   if tenant.planStatus === "Trial":
     Top of page banner:
     "Deneme süreniz {daysLeft} gün sonra bitiyor. Planınızı yükseltin → "
     Styled sarı/amber.

4. Skeleton loading states her kart için.

5. Empty states:
   - Hiç rezervasyon yoksa: "Henüz rezervasyon yok. Booking linkinizi paylaşın!"
   - Booking URL'yi göster, kopyala butonu.

Tüm kodu ver.
```

---

## Prompt 5.3 — Masa ve Yerleşim Yönetimi

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 5.3 — Masa yönetimi ekranı.

/features/tables/ altında.

1. TablesPage.tsx:
   
   Üst: Venue seçici (tek venue'lu hesaplarda gizli)
        + "Yeni Masa" butonu
        + "Masa Birleşimi" butonu (ikincil)
   
   Grid: 3 kolon (responsive → 2 → 1)
   Her masa kartı:
     - Drag handle (@dnd-kit)
     - Masa adı (büyük)
     - Kapasite rozeti (4 kişi)
     - Konum ikonu + label (Indoor, Terrace vb)
     - Açıklama (truncated 2 satır)
     - Aktif/Pasif switch
     - Düzenle, Sil butonları
   
   Drag-drop sıralama:
     - @dnd-kit/sortable
     - onDragEnd → /api/v1/venues/{id}/tables/reorder
     - Optimistic update
   
   Masa Birleşimleri ayrı tab:
     - Mevcut birleşimler listesi
     - "Yeni Birleşim" → modal: isim + birden fazla masa seç + kapasite

2. TableFormDialog.tsx (ekle/düzenle modal):
   Fields:
   - İsim (text, required)
   - Kapasite (number 1-50)
   - Konum (select: Indoor/Outdoor/Balcony/Bar/Private/Terrace/Garden)
   - Açıklama (textarea, opsiyonel)
   - Aktif (switch)
   Zod validation.

3. TableCombinationDialog.tsx:
   - İsim
   - Masa seçici (multi-select, mevcut masalardan)
   - Birleşik kapasite (auto-hesapla ama editable)

4. Plan limit kontrolü:
   - Masa sayısı plan limitine yaklaşınca (80% üzerinde) uyarı
   - Limit'teyken "Yeni Masa" butonu disabled + upgrade CTA

5. Empty state: "Henüz masanız yok. İlk masanızı ekleyin."

Tüm kodu ver.
```

---

## Prompt 5.4 — Kural Builder (No-Code Editör)

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat | **Neden Opus?** En karmaşık UI

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 5.4 — No-code kural builder — admin panel en karmaşık ekranı.

/features/rules/ altında.

1. RulesPage.tsx:
   Üst:
   - "Yeni Kural" butonu → Şablon galerisi
   - "Özel Kural" butonu → Boş builder
   - Plan limit banner (Starter için: "5 kural hakkınız kaldı")
   
   Liste (priority sıralı):
   Her kural satırı:
   - Drag handle (priority sıralama)
   - Priority badge (renk: 10=kırmızı, 5=sarı, 1=gri)
   - İkon + isim + kısa açıklama
   - Trigger etiketi (Rezervasyonda / Oturmada)
   - TimesTriggered sayısı
   - Aktif/Pasif switch
   - Test, Düzenle, Sil butonları

2. RuleTemplateGalleryDialog.tsx:
   - 10+ şablon kart (grid)
   - Her kart: ikon (büyük), başlık, açıklama, "Bu şablonu kullan" buton
   - "Custom Condition" en sondaki kart: "İleri seviye - kendi mantığınızı yazın"

3. RuleBuilderDialog.tsx — Multi-step wizard:
   
   ADIM 1 — Temel:
   - İsim
   - Açıklama
   - Priority slider (1-10, canlı açıklama)
   - Trigger radio cards
   - (Şablon seçilmişse bu adımlar önceden doldurulmuş)
   
   ADIM 2 — Koşul:
   - Şablondan geliyorsa: şablon parametreleri formu
     (örn EarlyBooking: "Kaç gün öncesi?" number input)
   - Custom ise: Condition Builder UI
     * Field selector (whitelist'ten)
     * Operator selector
     * Value input (field type'a göre)
     * AND/OR gruplama (+Ekle)
   - Advanced: "JSON olarak düzenle" toggle (sadece uzman için)
   
   ADIM 3 — Aksiyon:
   - Action type cards:
     Block, Warn, Suggest, Discount, Deposit, Redirect
   - Seçilen action için parametre formu:
     * Block: message
     * Warn: message
     * Suggest: message (opsiyonel masa seçici)
     * Discount: percent slider (1-50)
     * Deposit: amount, perPerson, useVenueDefault
     * Redirect: URL
   
   ADIM 4 — Önizleme:
   - Tüm ayarları özet
   - "Kural Özeti: Pazartesi-Perşembe arası rezervasyonlarda, 7+ gün öncesinde yapıldıysa %10 indirim uygula"
   - Otomatik "human-readable" çevirici (kural mantığını Türkçe açıkla)
   - "Test Et" butonu → RuleTestDialog aç
   
   Submit: POST /api/v1/rules (veya PUT edit ise)

4. RuleTestDialog.tsx:
   - SampleContext form:
     * Kişi sayısı
     * Kaç gün öncesi
     * Müşteri tier (select)
     * Hafta günü
     * Saat
     * Venue doluluk %
   - "Test Et" → /api/v1/rules/{id}/test
   - Sonuç:
     * ✓ Tetiklendi / ✗ Tetiklenmedi
     * Outcome detay (action, message, payload)
     * Execution süresi

5. RuleHumanReadable util:
   - Rule conditions + actions → Türkçe açıklama üretici
   - "Rezervasyon 7+ gün öncesinden yapılırsa %10 indirim"
   - Template-based, her rule type için switch

6. Advanced JSON mode:
   - Monaco editor
   - Schema validation (zod veya JSON schema)
   - "Kaydet" disabled if invalid

Tüm kodu ver. Bu ekran projenin farklılaştırıcı UI'sı — özenli ol.
```

---

## Prompt 5.5 — Rezervasyon Yönetimi (Timeline View)

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 5.5 — Rezervasyon yönetimi ekranı.

/features/reservations/ altında.

1. ReservationsPage.tsx:
   Üst bar:
   - Tarih navigasyonu: ← Dün | Bugün | Yarın → + date picker
   - Filtreler: Venue (multi-venue hesaplarda), Masa, Status
   - "Yeni Rezervasyon" butonu
   - "CSV İndir" butonu
   
   Ana görünüm — Timeline/Gantt tablosu:
   - Y ekseni: Masalar (sıralı)
   - X ekseni: Saat dilimleri (her 30 dk)
   - Her rezervasyon: blok kutu
     * Genişlik: slot süresi
     * Renk: status'a göre
       Confirmed: emerald
       Pending: amber
       Completed: slate
       Cancelled: muted gray (opacity 50%)
       NoShow: rose
     * İçerik: müşteri adı, kişi sayısı
     * Hover: tam detay tooltip
     * Click: detail drawer
   - Boş slotlar: açık renk, tıklanınca manuel rezervasyon modal'ı
   
   Responsive: Mobilde liste görünümü (timeline yerine)

2. ReservationDetailDrawer.tsx:
   Sağdan açılan drawer (shadcn Sheet):
   - Müşteri bilgileri (isim, email, telefon, tier badge)
   - Tarih, saat, masa
   - Kişi sayısı
   - Special requests
   - Internal notes (editable)
   - Uygulanan kurallar (chip list)
   - İndirim (varsa)
   - Kapora durumu (ödendi/bekleniyor/iade edildi)
   - Confirm code (kopyala)
   - Aksiyon butonları: Tamamlandı, Gelmedi, İptal, Düzenle
   - StatusLog timeline

3. ManualReservationDialog.tsx:
   - Masa + saat seçimi (eğer boş slot'tan geldiyse preset)
   - Kişi sayısı
   - Müşteri: existing seç veya yeni oluştur
     * Email/phone ile ara (typeahead)
     * Bulunamazsa "Yeni müşteri ekle" form
   - Misafir bilgileri (guest name + özel istek)
   - VenueCustomFields: eğer tanımlıysa dinamik form alanları
   - "Kuralları Kontrol Et" → evaluate endpoint
   - Kural uyarıları göster
   - "Kuralları atla" checkbox (override, owner-only)
   - "Rezervasyon Oluştur" submit

4. CSV Export:
   - Button → GET /api/v1/reservations/export?date=...
   - Blob'u indir

5. Empty state: "Bugün için rezervasyon yok"

6. Optimistic updates: status değişimlerinde UI hemen güncellenir

Tüm kodu ver.
```

---

## Prompt 5.6 — Müşteri, Ekip, Ayarlar, Abonelik

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat (büyük ama basit)

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 5.6 — Kalan admin sayfaları: Müşteriler, Ekip, Ayarlar, Abonelik.

1. CustomersPage (/features/customers/):
   - Arama + filtre (tier, blacklisted)
   - Tablo: İsim, Email, Telefon, Tier, Toplam Ziyaret, Son Rezervasyon
   - Tier badge renkli
   - Tıklama → CustomerDetailDrawer:
     * Tüm rezervasyon geçmişi
     * Tier güncelleme (Owner-only)
     * Blacklist toggle + sebep textarea
     * Notes editor

2. StaffPage (/features/staff/):
   Tab'lar: Aktif Ekip | Bekleyen Davetler
   
   Aktif Ekip:
   - Kullanıcı listesi: avatar, isim, email, rol badge, son giriş
   - Actions: Rol değiştir, Sil (son Owner'ı silemezsiniz)
   
   Bekleyen Davetler:
   - Email, rol, gönderilme tarihi, expire
   - "Tekrar Gönder" ve "İptal" butonları
   
   "Yeni Davet" modal: email + rol (Owner/Staff)

3. SettingsPage (/features/settings/):
   Tab'lar: Genel | Çalışma Saatleri | Bildirimler | Kapora | Booking | Entegrasyon
   
   GENEL:
   - Logo upload (R2 presigned URL flow)
   - İşletme adı, adres, telefon
   - Zaman dilimi
   - Dil (şimdilik sadece TR)
   
   ÇALIŞMA SAATLERİ:
   - Günlük açılış/kapanış (her gün ayrı)
   - Slot süresi (60/90/120 dk)
   - İstisna takvim (VenueClosure CRUD):
     * Yıllık calendar view
     * Tıklanınca gün ekle/kaldır
     * "Tam kapalı" vs "Kısmi saat" seçimi
     * Bulk ekle: birden çok tarih seç + kapalı işaretle
   
   BİLDİRİMLER:
   - Email bildirim toggle'ları
   - SMS bildirim toggle'ları (Pro+ badge ile gate)
     * Ücretsiz plan ise disabled + "Pro'ya yükselt" CTA
   
   KAPORA (yeni tab — v1.1'de eklendi):
   - "Kapora modülü aktif" master switch
   - Aktif ise:
     * Kapora miktarı (sabit TRY veya kişi başı)
     * İade politikası (Tam İade X saat / Kısmi İade / İade Yok)
     * Partial ise: iade yüzdesi slider
     * Minimum saat öncesi iade
   - Bilgi: "Kapora akışı İyzico üzerinden yönetilir"
   - Disable'da: kural motorundaki Deposit kuralları skip edilir uyarısı
   
   BOOKING:
   - Özel form alanları (VenueCustomField CRUD):
     * Drag-drop sıralama
     * Field tipine göre önizleme
   - Booking URL göster + kopyala + QR kod generator
   - Embed iframe kodu kopyala
   
   ENTEGRASYON:
   - API key (show/hide, regenerate) — Business+ plan
   - Webhook URL input + secret
   - Webhook event'leri checkbox listesi

4. SubscriptionPage (/features/subscription/):
   - Mevcut plan kartı (büyük):
     * Plan adı, aylık/yıllık fiyat, yenileme tarihi
     * "Bu ay kullanım" progress bar'ları (rez, kural, mekan)
     * Özellikler listesi
   - Diğer planlar (karşılaştırma kartları):
     * Yıllık/Aylık toggle
     * Her plan: fiyat, özellikler, "Yükselt" butonu
   - Fatura geçmişi tablosu
   - "Aboneliği İptal Et" altta küçük link (confirm dialog)
   - "Yükselt" → İyzico checkout akışı (Faz 7'de implement)

5. OnboardingPage (/features/onboarding/):
   First-login ise zorunlu wizard (4 adım):
   1. "Hoş geldiniz, {name}! İlk masanızı ekleyin"
   2. "Çalışma saatlerinizi ayarlayın"
   3. "İlk kuralınızı seçin" (3 öneri + atla)
   4. "Hazırsınız! Booking linkiniz: ..."
   
   Her adım save eder, atla butonu var.
   Wizard tamamlanınca tenant.isFirstLogin = false

Tüm kodu diff olarak ver (çok dosya var).
```

---

# FAZ 6 — BOOKING UI (Müşteri Arayüzü)

**Süre:** 1.5 hafta | **Chat sayısı:** ~2 | **Model:** Sonnet

## Prompt 6.1 — Booking UI Tam Akış

**Model:** Sonnet 4.5 | **Tahmini:** 2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 6.1 — Müşteri booking arayüzü.

/frontend/booking-ui — ayrı Vite React projesi (aynı stack, hafifletilmiş).

URL: tablewise.com.tr/rezervasyon/{slug}

Tasarım:
- AÇIK tema (müşteri arayüzü, güven veren)
- Mobile-first (çoğu rezervasyon telefonda)
- Minimal, hızlı
- İşletme logosu ve rengi üstte (tenant.settings.brandColor varsa)
- shadcn/ui light mode

1. Kurulum aynı admin-panel'deki gibi ama SignalR, @dnd-kit kapsama.

2. Route yapısı:
   /rezervasyon/:slug → ana booking sayfası
   /rezervasyon/onay/:code → başarılı rezervasyon ekranı
   /rezervasyon/goruntule/:code → rezervasyon detay + değiştir/iptal
   /rezervasyon/iptal/:code
   /rezervasyon/degistir/:code

3. BookingPage.tsx — 5 adımlı wizard (progress bar üstte):
   
   ADIM 1 — Tarih:
   - İşletme config'i ilk yükle (/api/v1/book/{slug}/config)
   - Takvim (date-picker veya custom):
     * Bugünden 60 güne kadar
     * Geçmiş, çalışma saati dışı (VenueClosure) disabled
     * Closure varsa altta "Not: {reason}"
   - "Devam" butonu
   
   ADIM 2 — Kişi ve Saat:
   - Kişi sayısı seçici (büyük +/- butonlar, 1-20)
   - Availability fetch: /api/v1/book/{slug}/availability?date=...&partySize=...
   - Saat slotları chip'leri (grid, mobilde 4 kolon)
     * Dolu slotlar görünmez
     * Seçilen highlight
   - Slot yok durumu: "Bu tarih/kişi için müsait saat yok. Farklı gün?"
   
   ADIM 3 — Masa:
   - Müsait masalar kart listesi:
     * İsim, kapasite, konum ikonu
     * "Seç" butonu
   - Default: otomatik en uygun masa önerilir (öncelik sıralı)
   - "Masa seçmeden sistem karar versin" opsiyonu
   
   ADIM 4 — Bilgiler:
   - Ad Soyad (required)
   - Email (required)
   - Telefon (required, TR format)
   - Özel istek (textarea, opsiyonel)
   - VenueCustomFields varsa dinamik form:
     * Her field type'a göre input
     * Required olanları zorla
   - "Kuralları Kontrol Et" butonu → /api/v1/book/{slug}/evaluate
   - Kural sonuçları göster:
     * WARN: sarı alert, "Devam et" butonu
     * BLOCK: kırmızı alert, "Geri dön" zorunlu
     * SUGGEST: mavi alert + "Önerilen masaya geç" butonu
     * DISCOUNT: yeşil "🎉 %10 erken rezervasyon indirimi!"
     * DEPOSIT: mavi "Bu rezervasyon için {X}₺ kapora gerekmektedir"
   - KVKK onay checkbox (required)
   
   ADIM 5 — Onay / Kapora:
   
   Kapora yoksa:
     - Rezervasyon özeti (büyük)
     - "Rezervasyon Yap" butonu → POST /api/v1/book/{slug}/reserve
       * Idempotency-Key header (UUID)
     - Success → /rezervasyon/onay/:code
   
   Kapora varsa:
     - "Ödeme Bilgileri" başlığı
     - İyzico iframe/checkout form yüklenir
     - Ödeme tamamlanınca server-side confirm → rezervasyon oluşur
     - (Detaylar Faz 7 — kapora ile birlikte)

4. OnayPage (/rezervasyon/onay/:code):
   - Büyük yeşil ✓ animasyonu (Framer Motion)
   - Confirm code (büyük, kopyala butonu)
   - Rezervasyon özeti
   - "Onay emaili gönderildi: {email}"
   - "Takvime Ekle" butonu (Google Calendar deep link)
   - "Yol Tarifi" (Google Maps linki)
   - "Yeni Rezervasyon" butonu
   
5. GoruntulePage (/rezervasyon/goruntule/:code):
   - Confirm code ile detay fetch
   - Rezervasyon detayı
   - Kapora durumu (ödendi/iade edildi)
   - "Değiştir" butonu (24 saatten fazlaysa)
   - "İptal Et" butonu (24 saatten fazlaysa)
   
6. DegistirModal:
   - Yeni tarih / saat / masa
   - Availability fetch
   - Confirm → PATCH /api/v1/book/confirm/{code}/modify
   
7. IptalModal:
   - İptal sebebi (opsiyonel)
   - Kapora iade uyarısı (policy'ye göre)
   - Confirm → POST /api/v1/book/confirm/{code}/cancel

8. SEO meta (react-helmet-async):
   - Dynamic title: "{venueName} - Rezervasyon"
   - OG tags

9. Mobil optimizasyon:
   - Bottom sheet modal'lar (Dialog vs Drawer)
   - Büyük tap target'lar (min 44px)
   - Native datepicker kullanımı
   - Tel keyboard phone input'ta

Tüm kodu tam halde ver. Her adım ayrı component.
```

---

# FAZ 7 — TİCARİ KATMAN + KAPORA

**Süre:** 2 hafta | **Chat sayısı:** ~4 | **Model:** Opus (ödeme = dikkatli)

## Prompt 7.1 — İyzico Abonelik Entegrasyonu

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.1 — İyzico abonelik sistemi.

İyzipay .NET SDK kullan (zaten NuGet eklendi).

1. Infrastructure/Payment/IyzicoSubscriptionService.cs : IPaymentService
   
   Metodlar:
   - Task<CheckoutResult> CreateCheckoutFormAsync(Tenant, Plan, billingType):
     * Subscription customer oluştur (varsa mevcut)
     * Subscription product (plan'a map et İyzico product)
     * Pricing plan (aylık/yıllık)
     * Checkout form token dön
   
   - Task<SubscriptionStatus> HandleWebhookAsync(payload, signature):
     * HMAC signature doğrula (secret key'le)
     * Event type'a göre işle:
       - SUBSCRIPTION_ORDER_CREATED → Subscription.Status = Active, Plan güncelle
       - SUBSCRIPTION_ORDER_RENEWED → NextBillingDate update
       - SUBSCRIPTION_ORDER_CANCELLED → Cancelled, Plan = Starter'a düşür
       - SUBSCRIPTION_ORDER_PAYMENT_FAILED → PastDue, email gönder, 3 deneme sonra Suspended
     * İdempotency: Event ID cache (aynı webhook 2 kez gelirse skip)
   
   - Task CancelSubscriptionAsync(tenantId): iptal başlat, dönem sonunda etkili
   
   - Task<PaymentResult> CreateOneTimePaymentAsync(amount, buyer, description):
     * Kapora için kullanılacak (Faz 7.2)
     * Return: paymentId, status
   
   - Task<RefundResult> RefundPaymentAsync(paymentId, amount):
     * Kapora iadesi için

2. Api/Controllers/SubscriptionController (JWT Owner):
   GET    /api/v1/subscription            → Mevcut abonelik + faturalar
   GET    /api/v1/subscription/plans      → Plan listesi (DB'den)
   POST   /api/v1/subscription/checkout   → Checkout token al
   POST   /api/v1/subscription/cancel     → İptal
   GET    /api/v1/subscription/invoices   → Fatura listesi

3. Api/Controllers/WebhookController (PUBLIC — signature ile korunur):
   POST /api/v1/webhooks/iyzico
   - Raw body'i oku (JSON)
   - Signature header doğrula
   - Service'e forward
   - 200 OK hızlı dön (İyzico timeout olmasın)
   - Async processing: hosted service ile işle (ileride)

4. PlanUpgradeHandler:
   - Webhook'tan gelen Plan değişikliğini işle:
     * Tenant.PlanId güncelle
     * Feature flag'ler anında yeni plana göre
     * Email gönder (plan-upgraded.html)
     * AuditLog

5. Invoice.cs (yeni entity):
   - TenantId, SubscriptionId
   - InvoiceNumber, Amount, Tax, Total
   - Status (Pending, Paid, Failed)
   - IyzicoInvoiceRef
   - InvoicePdfUrl (R2'ye kaydet, müşteri indirebilsin)
   - IssuedAt, PaidAt

6. Billing calculations:
   - KDV %20 (Türkiye)
   - Yıllık plan: aylık * 12 * 0.80 (yani %20 indirim)
   - PriceCalculator service

7. Unit test:
   - Webhook signature invalid → 401
   - Webhook event handling
   - Plan upgrade flow

ÖNEMLİ:
- Sandbox ile test. appsettings'te "Iyzico.Environment": "Sandbox" / "Production"
- Production key'leri user-secrets'ta
- Hata durumlarında detaylı log (ama PII masked)

Tüm kodu ver. Ödeme kritik — titiz ol.
```

---

## Prompt 7.2 — Kapora Modülü (v1.1'in İmza Özelliği)

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat | **Neden Opus?** Para akışı + iade mantığı

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.2 — Kapora / Ön Ödeme modülü (v1.1 dokümanı Modül L).

KRİTİK: Kapora işletme bazında aktif/pasif. DepositEnabled = false ise tüm
akış by-pass. İlgili kural motoru Deposit action'ı da skip olur.

1. Application/Features/Deposit/ altında:

   CalculateDepositCommand:
   - Input: ReservationDraft + Venue
   - Output: DepositCalculation { Amount, Breakdown, RefundPolicy }
   
   Logic:
   - Venue.DepositEnabled false → DepositCalculation.Amount = 0
   - PerPerson true → Amount * PartySize
   - Kural motoru Deposit action geldi → onun amount'unu kullan (override)
   - Min/Max limit kontrolü (güvenlik)

2. InitiateDepositPaymentCommand:
   - ReservationId
   - Reservation.Status = Pending_Payment (yeni enum değeri)
   - IPaymentService.CreateOneTimePaymentAsync çağır
   - Payment URL (İyzico checkout) döndür
   - DepositStatus = Pending
   - 15 dk TTL (ödeme bu sürede olmazsa rezervasyon iptal)

3. DepositPaymentWebhookHandler:
   - İyzico webhook'unda payment success
   - Reservation bul (paymentRef ile)
   - DepositStatus = Paid, DepositPaidAt = now
   - Reservation.Status = Confirmed
   - Email gönder (deposit-paid.html + confirm)
   - Slot availability cache invalidate

4. RefundDepositCommand:
   - ReservationId, RefundReason
   - Policy kontrolü:
     * NoRefund → 0 iade
     * FullRefund + saatler OK → full
     * PartialRefund → PartialPercent
   - IPaymentService.RefundPaymentAsync çağır
   - DepositStatus = Refunded, DepositRefundedAt
   - Email (deposit-refunded.html)
   - AuditLog

5. CancelReservationCommand güncelle:
   - Varsa kapora iade policy'sini uygula
   - DepositStatus = Forfeited (iade yoksa)

6. Background timeout service:
   DepositTimeoutService : BackgroundService
   - Her 1 dakika: Pending_Payment + 15dk geçmiş rezervasyonları bul
   - Reservation.Status = Cancelled (timeout)
   - Slot cache invalidate

7. ReserveCommandHandler update (Faz 2.2'yi güncelle):
   - RuleEngine sonucu RequiresDeposit = true ise:
     * Reservation oluştur ama Status = Pending_Payment
     * DepositStatus = Pending
     * InitiateDepositPayment çağır
     * Response: paymentUrl + confirmCode
   - Değilse: doğrudan Confirmed

8. Booking UI güncelleme (Faz 6 zaten bunu anlatmıştı):
   - Adım 5'te kapora varsa İyzico checkout göster
   - Ödeme tamamlanınca onay sayfasına yönlendir
   - Ödeme başarısız olursa geri dön + retry

9. Admin Panel güncelleme:
   - Reservation detail'de DepositStatus göster
   - "Manuel İade" butonu (Owner-only, confirm ile)
   - Reports: Aylık kapora geliri, iade edilen, forfeited

10. Email bildirimleri:
    - deposit-paid.html
    - deposit-refunded.html

11. KVKK:
    - Kapora iade politikası rezervasyon akışında AÇIK göster
    - Müşteri onaylamadan kapora istenmez
    - İade süreçleri loglanır

Tüm kodu tam halde ver. Test senaryoları mutlaka:
- Ödeme başarılı → Confirmed
- Ödeme başarısız → Cancelled
- 15 dk timeout → otomatik iptal
- Full refund / partial / no refund senaryoları
- Kapora disabled iken akış normal (skip)
```

---

## Prompt 7.3 — SMS (Netgsm) + SignalR Bildirimler

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.3 — Netgsm SMS ve SignalR gerçek zamanlı bildirimler.

1. Infrastructure/Sms/NetgsmSmsService.cs : ISmsService
   - SendAsync(phone, message)
   - Netgsm HTTP API entegrasyon
   - TR format normalization (05XX → 905XX)
   - Max 160 karakter (otomatik split uyarısı)
   - NotificationLog kaydet
   - Plan kontrolü: Sadece Pro+ SMS gönderebilir

2. SMS şablonları (Türkçe, kısa):
   const SmsTemplates = {
     ReservationConfirm: (venueName, date, time, code) =>
       `Tablewise: ${venueName} rezervasyonunuz onaylandı. ${date} ${time}, Kod: ${code}`,
     Reminder: (venueName, time) =>
       `Hatırlatma: ${venueName} rezervasyonunuz yarın ${time}. İyi eğlenceler!`,
     Cancel: (venueName) =>
       `Tablewise: ${venueName} rezervasyonunuz iptal edildi.`,
     NewReservationOwner: (guestName, date, time, partySize, tableName) =>
       `Yeni rezervasyon: ${guestName}, ${date} ${time}, ${partySize} kişi, ${tableName}`,
     DepositRequired: (venueName, amount, url) =>
       `${venueName} için ${amount}₺ kapora gereklidir: ${url}`
   };

3. Background Services:
   
   ReminderSchedulerService : BackgroundService
   - Her gece 20:00'da (Timer)
   - Ertesi günün rezervasyonlarını bul (Confirmed, ReminderSentAt null)
   - Email + SMS (Pro+ ise) hatırlatıcı
   - ReminderSentAt update
   
   MonthlyResetService : BackgroundService
   - Her ayın 1'i 00:01
   - Tüm Tenant.ReservationCountThisMonth = 0 sıfırla
   - Redis key'leri temizle
   
   TrialExpiryService : BackgroundService
   - Her gün 09:00
   - Trial 3 gün kalanlara email
   - Trial bitenlere PlanStatus = Suspended, email

4. SignalR Hub:
   Api/Hubs/AdminNotificationHub.cs : Hub
   - [Authorize] (Owner/Staff only)
   - Groups: "tenant_{tenantId}"
   - OnConnectedAsync: tenant group'a katıl
   
   Events (server → client):
   - "ReservationCreated" → { reservation details }
   - "ReservationModified"
   - "ReservationCancelled"
   - "DepositPaid"
   - "NewStaffJoined"

5. Service'lerde event publish:
   ReserveCommandHandler:
   - Reservation kaydedildikten sonra:
     await _hubContext.Clients.Group($"tenant_{tenantId}")
       .SendAsync("ReservationCreated", reservationDto);

6. Admin Panel frontend SignalR:
   /frontend/admin-panel/src/lib/signalr.ts:
   - Connection kurulumu (@microsoft/signalr)
   - Access token'la auth
   - Event dinleyicileri:
     * ReservationCreated → toast + refetch listesi
     * ReservationCancelled → toast
   - Reconnect policy

7. Admin Panel Toast:
   "Yeni rezervasyon geldi: Ali Bey, 4 kişi, 21:00"
   Tıklanınca detay drawer'a git.

8. Plan bazlı SMS kontrolü:
   SmsService.SendAsync'te:
   - Tenant'ın plan feature flag'i "sms" true mu
   - Değilse: log warning, skip (hata değil, sessiz atla)
   - Email fallback otomatik gönderilir zaten

9. SMS kredi tracking (opsiyonel):
   - Her SMS maliyet tahmini (~0.10 TRY)
   - Aylık SMS sayısı tenant tarafında göster
   - Limit aşımında email uyarısı

Tüm kodu ver.
```

---

## Prompt 7.4 — Landing Page Abonelik Bağlantısı

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.4 — Frontend: Admin panel abonelik sayfası tamamla + landing pricing.

1. Admin Panel /features/subscription/ tamamla (Faz 5.6'da iskelet vardı):
   
   PlanUpgradeFlow:
   - "Yükselt" tıklandı → loading overlay
   - POST /api/v1/subscription/checkout → İyzico token alınır
   - İyzico checkout form modal'da açılır (iframe)
   - Success callback → backend webhook'u zaten handle ediyor
   - Polling: her 2 saniyede plan status kontrol → güncelleme algılandığında
     "Ödemeniz alındı! Plan güncellendi." toast
   - Başarısız → hata mesajı + retry
   
   Fatura listesi:
   - /api/v1/subscription/invoices
   - Tarih, tutar, status, PDF indir butonu

2. Cancel flow:
   - "Aboneliği İptal Et" → 2 adımlı confirm
   - Adım 1: "Neden iptal ediyorsunuz?" survey (tek seçim)
   - Adım 2: "Bu işlem dönem sonunda aktif olacak. Onaylıyor musunuz?"
   - Success → "Plan {date} tarihinde Starter'a düşürülecek"

3. Landing page (Faz 11'de detaylı gelecek — şimdi sadece stub):
   /frontend/landing/src/pages/PricingPage.tsx:
   - 4 plan kartı (fiyatlar DB'den fetch, /api/v1/subscription/plans public)
   - "Ücretsiz Dene" → /register'a yönlendir
   - Feature karşılaştırma tablosu
   - FAQ

Tüm kodu ver.
```

---

# FAZ 8 — CRM + RAPORLAMA

**Süre:** 1.5 hafta | **Chat sayısı:** ~3 | **Model:** Sonnet

## Prompt 8.1 — CRM Modülü ve Customer Feedback

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 8.1 — CRM derinleştirme + müşteri feedback sistemi.

1. Customer entity genişlet (zaten var, ek alanlar):
   - PreferredLanguage (default "tr")
   - Birthday (DateOnly?) — doğum günü hatırlatma
   - Allergies (string?)
   - DietaryRestrictions (string?)
   - Tags (string, jsonb) — esnek tag sistemi
   - AverageSpend (decimal?) — Faz 9+ POS entegrasyonuyla dolar

2. Customer auto-merge logic:
   - Rezervasyon oluşturmada:
     * Email ve phone ile existing customer ara
     * Birden fazla eşleşme varsa: duplicate candidate işaretle (admin'e göster)
     * Tek eşleşme: link et, TotalVisits++
     * Yok: yeni oluştur

3. CustomerController genişlet:
   GET    /api/v1/customers              → sayfalı, search, filter
   GET    /api/v1/customers/{id}
   GET    /api/v1/customers/{id}/reservations  → geçmiş
   PUT    /api/v1/customers/{id}         → Owner-only
   PUT    /api/v1/customers/{id}/tier    → tier değiştir
   POST   /api/v1/customers/{id}/blacklist → blacklist/unblacklist
   POST   /api/v1/customers/merge        → iki müşteriyi birleştir
                                          body: { keepId, deleteId }
   GET    /api/v1/customers/duplicates   → olası duplicate liste

4. CustomerFeedback entity (yeni):
   - ReservationId (FK, unique)
   - Rating (int, 1-5)
   - Comment (string?)
   - WouldRecommend (bool?)
   - SubmittedAt
   - ShareToPublic (bool, default false)

5. FeedbackEmailSender (Background):
   - Her gün 10:00
   - 1 gün önce Completed olan rezervasyonlara feedback email
   - Link: tablewise.com.tr/feedback/{token}
   - 1 kez gönder (FeedbackEmailSentAt flag)

6. FeedbackController (Public, token bazlı):
   GET  /api/v1/feedback/{token}         → rezervasyon bilgisi
   POST /api/v1/feedback/{token}         → rating + comment submit
   - Token Reservation.FeedbackToken (HMAC)
   - Bir kere submit edilir

7. Admin panel:
   /features/customers/:
   - Customer detail'e feedback geçmişi ekle
   - Ortalama rating göster
   - "Birleştir" butonu duplicate önerilerinde

   /features/reports/feedback:
   - Haftalık/aylık ortalama rating grafiği
   - En son yorumlar feed
   - Low-rating rezervasyonlar (1-2 yıldız)

Tüm kodu ver.
```

---

## Prompt 8.2 — Reports Dashboard

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 8.2 — İşletme raporlama ekranı.

1. Backend reports service:
   
   GET /api/v1/reports/overview?from=...&to=...
   - Toplam rezervasyon, completed, cancelled, noShow sayıları
   - Doluluk oranı (%)
   - Ortalama party size
   - Toplam gelir (kapora üzerinden — POS entegrasyonu yok)
   - Cancellation rate
   - No-show rate
   
   GET /api/v1/reports/reservations-by-day?from=...&to=...
   - Günlük rezervasyon sayısı array
   
   GET /api/v1/reports/reservations-by-hour?from=...&to=...
   - Saatlik dağılım
   
   GET /api/v1/reports/top-customers?limit=10
   - En çok ziyaret eden müşteriler
   
   GET /api/v1/reports/rule-performance
   - Her kural: tetiklenme sayısı, etkilediği rezervasyon, dönüştürdüğü discount
   
   GET /api/v1/reports/table-utilization
   - Her masa: doluluk %, ortalama party size
   
   GET /api/v1/reports/deposit-summary
   - Toplanan kapora, iade edilen, forfeited
   
   GET /api/v1/reports/customer-segments
   - Tier dağılımı pie chart data
   - Yeni vs tekrarlayan müşteri oranı
   
   GET /api/v1/reports/no-show-analysis
   - Kim, ne sıklıkla, hangi saatlerde
   
2. Admin Panel /features/reports/:
   Ana sayfa — tab'lı:
   - Genel
   - Rezervasyon
   - Müşteri
   - Masa
   - Kapora
   - Kural Performansı
   
   Her tab Recharts grafikleri + özet kartlar.
   
   Filtreler (her sayfada):
   - Tarih aralığı (preset: Son 7 gün / 30 gün / 90 gün / Bu ay / Custom)
   - Venue (multi-venue)
   
   "CSV İndir" her raporun sağ üstünde.

3. PDF export (opsiyonel ama değerli):
   - iText veya QuestPDF ile
   - Tenant logosu + aylık özet
   - "Aylık Rapor İndir" butonu

4. Data caching:
   - Rapor query'leri ağır → Redis cache 10 dk
   - Realtime olmayan veriler için makul

Tüm kodu diff olarak ver (çok grafik var, önce backend sonra frontend).
```

---

## Prompt 8.3 — Audit Log UI ve Aktivite Feed

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat (kısa)

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 8.3 — Audit log görüntüleme ve aktivite feed'i.

1. Admin Panel /features/settings/audit-logs:
   - Tablo: tarih, kullanıcı, aksiyon, entity, değişiklik özeti
   - Filter: aksiyon türü, tarih, kullanıcı
   - Pagination (50/sayfa)
   - Satıra tıklama → detay drawer (OldValue vs NewValue diff)

2. Backend: /api/v1/tenant/me/audit-logs query genişlet
   - Search, filter, pagination
   - Only Owner accessible

3. Activity feed (Dashboard'a entegre):
   - Son 10 aktivite widget
   - İkon + mesaj + zaman
   - "X masa ekledi", "Y rezervasyon iptal edildi" vb

4. AuditLog action mapper:
   - Enum yerine string'ler kullanıyoruz, readable mesajlara çevir:
     * "RULE_CREATED" → "{user} '{ruleName}' kuralını oluşturdu"
     * "PLAN_UPGRADED" → "{user} planı Pro'ya yükseltti"
     * ...
   - Localization-ready (şimdilik sadece TR)

Tüm kodu diff olarak ver.
```

---

# FAZ 9 — GÜVENLİK + MONITORING

**Süre:** 1 hafta | **Chat sayısı:** ~2 | **Model:** Opus (güvenlik)

## Prompt 9.1 — Security Hardening

**Model:** Opus 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 9.1 — Production güvenlik sertleştirme.

1. SecurityHeadersMiddleware:
   - X-Content-Type-Options: nosniff
   - X-Frame-Options: DENY (booking UI için SAMEORIGIN — embed edilebilir)
   - X-XSS-Protection: 1; mode=block
   - Strict-Transport-Security: max-age=31536000; includeSubDomains
   - Content-Security-Policy: default-src 'self'; script-src 'self' iyzico;
     frame-src iyzico; img-src * data:;
   - Referrer-Policy: strict-origin-when-cross-origin
   - Permissions-Policy: geolocation=(), camera=(), microphone=()

2. JWT Güvenlik Upgrade:
   - HS256 → RS256 (RSA key pair)
   - Key rotation desteği (kid claim)
   - Key pair'leri /keys klasöründe .pem dosyaları (config'te path)
   - Refresh token DB'de tutuluyor (RevocableRefreshToken entity)
     * Logout → bu token revoke
     * 30 günlük rotation

3. KVKK hassas alan şifreleme:
   - Customer.Phone, Customer.Email → EncryptedString (AES-256)
   - EF Core Value Converter
   - Anahtar: "encryption:key" config'te (rotation için versionlama)
   - Arama yapmak için: hash version de sakla (SHA256 hash, indexable)
     * Customer.PhoneHash, Customer.EmailHash
     * Query: hash ile, return: decrypted value
   - Düşünce: Bu büyük bir değişiklik. Eğer çok karmaşıksa:
     Alternatif: PostgreSQL-level pgcrypto (uygulama basit kalır)
     Cursor'a her iki yolu değerlendirtir, tercih belirt.

4. Data retention policy:
   Background service: DataRetentionService
   - Her gün 03:00
   - 2 yıldan eski Completed/Cancelled Reservation → anonymize
     * GuestEmail, GuestPhone → "REDACTED_{id}"
     * GuestName → "Geçmiş Müşteri"
     * Customer'daki PII de benzer (son rez 2+ yıl önce ise)
   - AuditLog 5 yıl sonra silinir (hukuki saklama süresi)

5. Brute-force koruması genişlet:
   - Login: 5 fail / 15 dk per IP + email
   - Password reset: 3/saat per email
   - Booking /reserve: 5 / saat per IP (zaten var)
   - Confirm code check: 10 / dakika per code (code enumeration önle)

6. ConfirmCode güvenlik:
   - Cryptographically secure random (RandomNumberGenerator.GetBytes)
   - 8 karakter, Base32 alphabet (O, 0, I, 1 hariç — okunabilirlik)
   - Collision retry 3 kez

7. PII masking in logs:
   - Serilog destructuring policy:
     * Email → "***@example.com" (domain görünür, user kısmı masked)
     * Phone → "+90555***1234" (ilk ve son 4)
     * Password → "[REDACTED]"
     * PasswordHash → "[REDACTED]"
     * Token → "[REDACTED]"

8. API Key Authentication (Business+ plan için API erişim):
   - ApiKey entity: TenantId, Key (hash), Name, LastUsedAt, IsActive
   - Middleware: X-API-Key header kontrol
   - Rate limit: key başına 1000 req/saat

9. Webhook signature generation (dışarı giden):
   - Tenant.WebhookUrl'e POST gönderildiğinde
   - Header: X-Tablewise-Signature: hmac-sha256={hex}
   - Body'nin HMAC'i tenant WebhookSecret ile

10. CORS sıkılaştırma:
    - AllowedOrigins: prod'da sadece tablewise.com.tr ve app.tablewise.com.tr
    - Credentials: true (JWT için)
    - Booking UI embedding için bir white-list mekanizması (ileride)

11. OWASP Top 10 checklist — kod review:
    - SQL injection: EF Core parametrik, güvende
    - XSS: CSP + input validation + React auto-escape
    - CSRF: JWT zaten koruyor (stateless), SameSite cookie gerekmez
    - SSRF: Webhook URL'ler internal IP'lere çıkmasın (kara liste)
    - XXE: XML parsing yok (JSON only)
    - Deserialization: System.Text.Json sadece, type restriction

Tüm kodu ver. Her middleware ayrı dosya, Program.cs'de sıralı kayıt.
```

---

## Prompt 9.2 — Monitoring ve Loglama

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 9.2 — Serilog, Sentry, Prometheus, HealthChecks.

1. Serilog tam yapılandırma:
   - Sinks:
     * Console (pretty, development)
     * File (rolling daily, /logs/tablewise-{date}.log)
     * Seq (development, http://seq:5341)
     * Elasticsearch (production, opsiyonel)
   - Enrichers:
     * FromLogContext
     * WithMachineName
     * WithEnvironmentName
     * WithThreadId
     * Custom: WithTenantId (ITenantContext'ten)
     * Custom: WithUserId
     * WithCorrelationId (middleware'den)
   - Minimum level: Debug dev, Information prod
   - Override Microsoft.* → Warning
   - PII destructuring policy (Faz 9.1'de ekledik)

2. Sentry SDK:
   - Program.cs: UseSentry({ options =>
       options.Dsn = config["Sentry:Dsn"];
       options.Environment = env.EnvironmentName;
       options.TracesSampleRate = 0.2;  // %20 trace
       options.AttachStacktrace = true;
       options.SendDefaultPii = false;  // önemli
     });
   - Custom: Reservation işlemlerinde transaction trace

3. HealthChecks:
   GET /health → detaylı (admin only)
   GET /health/live → just app alive (k8s liveness, public)
   GET /health/ready → DB + Redis hazır (k8s readiness)
   
   Checks:
   - PostgreSQL (AddNpgSql)
   - Redis (AddRedis)
   - SendGrid (dummy request yapmadan, sadece config var mı)
   - İyzico config check
   - Disk space (min 10% free)
   - Memory (warning at 80%)

4. Prometheus metrics (/metrics endpoint):
   - Paket: prometheus-net.AspNetCore
   - HTTP metrics default
   - Custom counters:
     * tablewise_reservations_created_total (labels: tenant, status, source)
     * tablewise_rules_triggered_total (labels: rule_type)
     * tablewise_deposits_paid_total (labels: tenant)
     * tablewise_deposits_refunded_total
     * tablewise_active_tenants (gauge)
     * tablewise_subscriptions_active (gauge, labels: plan)
   - /metrics Nginx'te korunur (internal only)

5. AuditLog servisi genişlet:
   - IAuditService interface
   - LogAsync(action, entityType, entityId, oldValue, newValue)
   - Kritik aksiyonlar otomatik log:
     * Rule CRUD
     * Plan changes
     * Subscription events
     * Password changes
     * Staff invitations / role changes
     * Reservation status changes
     * Deposit operations

6. Request/Response logging middleware:
   - Her isteği logla: method, path, status, duration
   - Yavaş istekleri (>2000ms) Warning
   - Hassas path'lerde body loglama (/auth/login, /auth/reset-password)

7. Correlation ID middleware:
   - Incoming header'da var mı, yoksa üret (Guid)
   - HttpContext.Items["CorrelationId"]
   - Response header'a ekle: X-Correlation-ID
   - Serilog context'e ekle
   - Sentry'ye tag olarak

8. Structured logging örnekleri (docs):
   - Bug report'ta correlation ID olsun
   - "rezervasyon X oluşturuldu (tenant=Y, user=Z, correlation=ABC)"

Tüm kodu ver.
```

---

# FAZ 10 — DEPLOYMENT

**Süre:** 1 hafta | **Chat sayısı:** ~2

## Prompt 10.1 — Docker, Nginx, CI/CD

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 10.1 — Docker ve deployment yapılandırması.

1. Backend Dockerfile (src/Tablewise.Api/Dockerfile):
   Stage 1 (build): mcr.microsoft.com/dotnet/sdk:8.0
   - Tüm .csproj kopyala → restore
   - Source kopyala → publish Release
   
   Stage 2 (runtime): mcr.microsoft.com/dotnet/aspnet:8.0
   - Non-root user: useradd appuser
   - PORT=8080
   - HEALTHCHECK: curl /health/live
   - dotnet "Tablewise.Api.dll"

2. Frontend Dockerfile'ları (admin-panel, booking-ui, landing):
   Stage 1: node:20-alpine
   - npm ci → npm run build
   
   Stage 2: nginx:alpine
   - nginx.conf: SPA routing (try_files $uri /index.html)
   - Cache headers: static assets 1 yıl, html no-cache
   - Gzip + Brotli

3. docker-compose.yml (development):
   services:
     postgres:
       image: postgres:16-alpine
       env: POSTGRES_DB, USER, PASSWORD
       volumes: postgres_data
     
     redis:
       image: redis:7-alpine
       volumes: redis_data
     
     seq:
       image: datalust/seq:latest
       env: ACCEPT_EULA=Y
       ports: "5341:80"
     
     minio:
       image: minio/minio
       env: root user/pass
       ports: "9000:9000", "9001:9001"
       command: server /data --console-address ":9001"
     
     mailhog:  # email testing
       image: mailhog/mailhog
       ports: "1025:1025", "8025:8025"
     
     api:
       build: src/Tablewise.Api
       depends_on: postgres, redis, minio
       env: .env
     
     admin-panel:
       build: frontend/admin-panel
       ports: "3000:80"
     
     booking-ui:
       build: frontend/booking-ui
       ports: "3001:80"
     
     landing:
       build: frontend/landing
       ports: "3002:80"
     
     pgadmin:  # opsiyonel
       image: dpage/pgadmin4

   networks: internal, public

4. docker-compose.prod.yml (overrides):
   - restart: always her servise
   - Memory limits
   - Logging: json-file, max-size 50m
   - No MinIO, MailHog, pgAdmin (production values)
   - Real R2 + SendGrid + Netgsm + İyzico

5. .env.example:
   Tüm env var'lar listeli (gizliler boş):
   DB_HOST, DB_PORT, DB_USER, DB_PASSWORD, DB_NAME
   REDIS_CONNECTION
   JWT_PRIVATE_KEY_PATH, JWT_PUBLIC_KEY_PATH
   SENDGRID_API_KEY
   NETGSM_USERCODE, PASSWORD, HEADER
   IYZICO_API_KEY, SECRET_KEY, BASE_URL
   R2_ACCOUNT_ID, ACCESS_KEY, SECRET_KEY, BUCKET
   SENTRY_DSN
   API_URL, BOOKING_BASE_URL, APP_URL

6. nginx/nginx.conf (reverse proxy):
   ```nginx
   # /etc/nginx/nginx.conf
   http {
     upstream api { server api:8080; }
     upstream admin { server admin-panel:80; }
     upstream booking { server booking-ui:80; }
     upstream landing { server landing:80; }
     
     limit_req_zone $binary_remote_addr zone=booking:10m rate=30r/m;
     limit_req_zone $binary_remote_addr zone=auth:10m rate=10r/m;
     
     # tablewise.com.tr → landing + booking /rezervasyon/
     server {
       listen 443 ssl http2;
       server_name tablewise.com.tr;
       ssl_certificate /etc/letsencrypt/live/tablewise.com.tr/fullchain.pem;
       ssl_certificate_key /etc/letsencrypt/live/tablewise.com.tr/privkey.pem;
       
       location /rezervasyon/ {
         limit_req zone=booking;
         proxy_pass http://booking/;
       }
       location / {
         proxy_pass http://landing;
       }
     }
     
     # app.tablewise.com.tr → admin panel
     server {
       listen 443 ssl http2;
       server_name app.tablewise.com.tr;
       ssl_certificate ...;
       location / { proxy_pass http://admin; }
     }
     
     # api.tablewise.com.tr → API
     server {
       listen 443 ssl http2;
       server_name api.tablewise.com.tr;
       ssl_certificate ...;
       
       location /api/v1/auth/ {
         limit_req zone=auth burst=3;
         proxy_pass http://api;
       }
       location /api/v1/book/ {
         limit_req zone=booking burst=5;
         proxy_pass http://api;
       }
       location / {
         proxy_pass http://api;
       }
     }
     
     # HTTP → HTTPS redirect
     server { listen 80; return 301 https://$host$request_uri; }
   }
   ```
   Security headers her server block'ta.
   Gzip compression.

7. scripts/deploy.sh:
   ```bash
   #!/bin/bash
   set -e
   
   cd /opt/tablewise
   git pull origin main
   docker compose -f docker-compose.yml -f docker-compose.prod.yml build
   docker compose up -d --no-deps api
   docker compose exec -T api dotnet ef database update
   
   # health check wait
   for i in {1..30}; do
     if curl -f http://localhost:8080/health/live; then break; fi
     sleep 2
   done
   
   docker compose up -d --no-deps admin-panel booking-ui landing
   ```

8. scripts/backup.sh:
   - pg_dump günlük
   - R2'ye upload
   - 30 gün tut
   - Başarısızlık → Sentry + email

9. GitHub Actions (.github/workflows/):
   
   ci.yml (her PR):
   - dotnet test
   - frontend lint + build test
   - Docker build kontrolü (push etme)
   
   deploy-staging.yml (develop branch):
   - Build + push docker images (ghcr.io)
   - SSH to staging VPS → deploy
   
   deploy-production.yml (main branch, manuel onay):
   - Production deploy

10. scripts/server-setup.sh (Ubuntu 22.04):
    - Docker + compose install
    - UFW firewall (22, 80, 443)
    - Certbot + auto-renew
    - Swap dosyası (2GB)
    - Auto-security updates

Tüm dosyaları tam halde ver.
```

---

# FAZ 11 — LANDING PAGE

**Süre:** 1.5 hafta | **Chat sayısı:** ~2 | **Model:** Sonnet

## Prompt 11.1 — Landing Page

**Model:** Sonnet 4.5 | **Tahmini:** 2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 11.1 — Tablewise.com.tr landing page.

/frontend/landing — Vite React (SSR opsiyonel, SPA yeterli başta).

Tasarım:
- Koyu lacivert (#0f172a) + altın (#f59e0b)
- Türkiye premium restoran/mekan estetiği
- Modern, güven veren, profesyonel
- Heavy hero, bol beyaz alan, tipografi güçlü

Sayfalar:

1. / (Ana sayfa):
   
   HERO:
   - Başlık: "Restoranınızın Rezervasyon Sistemini Akıllı Hale Getirin"
   - Alt: "Cinsiyet dengesi, VIP yerleşimi, erken rezervasyon, kapora — 
     kendi kurallarınızı tanımlayın, sistem sizin yerinize karar versin."
   - CTA: "14 Gün Ücretsiz Deneyin" (app.tablewise.com.tr/register)
   - Sağda: admin panel ekran görüntüsü mockup
   - Alt: "İstanbul'un 50+ prestijli mekanı güveniyor" (placeholder)
   
   ÖZELLİKLER (3-column):
   - 🎯 Kural Motoru
   - 📊 Anlık Takip  
   - 📱 Kolay Paylaşım
   
   NASIL ÇALIŞIR (3 adım):
   1. Kaydolun
   2. Kurallarınızı belirleyin
   3. Booking linkinizi paylaşın
   
   DEMO BÖLÜMÜ:
   - Animasyonlu rezervasyon akışı (video veya Lottie)
   - "1 dakikada nasıl çalışır görün"
   
   FİYATLANDIRMA ÖZETİ:
   - 3 plan kartı (Starter/Pro/Business)
   - Pro "En Popüler" badge
   - "Detaylı fiyatlandırma →" link
   
   TESTIMONIALS (3 placeholder):
   - Quote, isim, pozisyon
   
   FAQ (accordion, 8 soru):
   - Kural motoru nasıl çalışır?
   - Kapora sistemi nasıl çalışır?
   - Verilerim güvende mi? (KVKK)
   - Minimum sözleşme süresi var mı?
   - SMS dahil mi?
   - Kaç mekan ekleyebilirim?
   - Enterprise için?
   - Ücretsiz trial nasıl?
   
   CTA BANNER:
   - "Hemen başlayın"
   - Dual CTA: "Ücretsiz Dene" + "Demo İste"
   
   FOOTER:
   - Logo + slogan
   - Linkler (4 kolon): Ürün, Şirket, Destek, Yasal
   - İletişim: info@tablewise.com.tr
   - Sosyal medya
   - © 2026 Tablewise. Tüm hakları saklıdır.
   - KVKK, Gizlilik, Kullanım Koşulları linkleri

2. /ozellikler:
   - Her ana modül detaylı açıklama
   - Ekran görüntüleri
   - Video/GIF demolar

3. /fiyatlandirma:
   - Aylık/Yıllık toggle (yıllıkta %15-20 indirim)
   - 4 plan kartı detaylı
   - Karşılaştırma tablosu (50+ özellik)
   - SSS

4. /blog:
   - Placeholder: 2 örnek yazı
     * "Restoranlar için rezervasyon rehberi"
     * "No-show nasıl önlenir?"
   - SEO-friendly slug'lar

5. /iletisim:
   - Form: isim, email, mekan, mesaj
   - Haritada İstanbul ofis (placeholder koordinat)
   - enterprise@tablewise.com.tr adresine yönlendir

6. /hakkimizda (opsiyonel):
   - Vizyon/misyon
   - Ekip (varsa)

7. /kvkk:
   - Tam KVKK metni (Türkçe, hukuki)
   - Veri sorumlusu, işleme amaçları, saklama, haklarınız
   - İletişim: kvkk@tablewise.com.tr

8. /gizlilik-politikasi:
   - Full gizlilik politikası

9. /kullanim-kosullari:
   - Kullanım koşulları, abonelik, iptal

Teknik:
- React Helmet Async (SEO)
- Google Analytics 4 entegrasyonu (placeholder)
- Smooth scroll
- Framer Motion animations (hero entrance, scroll reveal)
- Responsive mobile-first
- Lighthouse Performance >90

Tüm sayfaların kodunu ver. Hukuki metinler için gerçek Türk mevzuatına göre
ciddi draft'lar ver (avukat doğrulayacak olsa da).
```

---

# FAZ 12 — ÖLÇEK ÖZELLİKLERİ (İlerleyen Fazlar)

Bu faz opsiyonel — ilk canlı alım sonrası müşteri geri bildirimine göre öncelikle.

## Prompt 12.1 — Çoklu Mekan (Business Plan)

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 12.1 — Multi-venue özellikleri (Business+ plan).

1. Admin Panel:
   - Top bar'a venue switcher (dropdown)
   - Tüm sayfalar seçili venue ile filtrelenir
   - Tenant-wide görünümler (örn Reports) da var

2. Onboarding:
   - Business plana yükselttikten sonra "Yeni mekan ekle" CTA

3. Venue CRUD zaten var, sadece plan gate ekle:
   - Starter: 1 venue max
   - Pro: 1
   - Business: 3
   - Enterprise: unlimited

4. Booking URL'ler venue bazlı:
   - tablewise.com.tr/rezervasyon/{tenant-slug}/{venue-slug}
   - Tek venue ise: /rezervasyon/{tenant-slug} (default venue)

5. Rule scope:
   - Kurallar venue-specific veya tenant-wide olabilir
   - Rule.VenueId null = tüm venue'lar
   - Kural builder UI'da scope seçici

6. Reports:
   - "Tüm mekanlar" vs "Şu mekan" filter

Diff olarak ver.
```

---

## Prompt 12.2 — White-label (Enterprise)

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 12.2 — White-label altyapısı.

1. Custom Domain:
   - Tenant'a CustomDomain alanı
   - DNS: CNAME kayıt müşteri tarafında
   - Let's Encrypt otomatik SSL (Traefik veya Caddy'e geçmeyi düşün — 
     Nginx manual, dinamik subdomain/domain zor)
   - Veya tablewise'ın Cloudflare'i üzerinden
   
2. Branding:
   - Tenant.BrandColor (hex)
   - Tenant.LogoUrl (zaten var)
   - Tenant.FavIconUrl
   - Booking UI config'te bu değerleri yükle, CSS variable'a inject
   - Email şablonlarında logo göster

3. Custom Email From:
   - Tenant.CustomFromEmail + CustomFromName
   - SendGrid domain auth gerekiyor → onboarding guide
   - Fallback: noreply@tablewise.com.tr

4. Removed "Tablewise" branding (Enterprise):
   - Footer'da "Powered by Tablewise" → kapat
   - Email'lerde "Tablewise" mention'ları kaldır

Diff olarak ver.
```

---

# 🎯 ÖZET TABLO

| Faz | Konu | Chat | Süre | Model Ağırlıklı |
|-----|------|------|------|-----------------|
| 1 | Backend Çekirdeği | 8 | 2-3 hafta | Sonnet + Opus(auth) |
| 2 | API Katmanı | 5 | 1.5-2 hafta | Sonnet + Opus(rez) |
| 3 | Kural Motoru | 4 | 2 hafta | **Opus** |
| 4 | Email & R2 | 2 | 1 hafta | Sonnet |
| 5 | Admin Panel | 7 | 3-4 hafta | Sonnet + Opus(rule builder) |
| 6 | Booking UI | 2 | 1.5 hafta | Sonnet |
| 7 | Ticari + Kapora | 4 | 2 hafta | **Opus** |
| 8 | CRM + Raporlama | 3 | 1.5 hafta | Sonnet |
| 9 | Güvenlik + Monitor | 2 | 1 hafta | Opus + Sonnet |
| 10 | Deployment | 2 | 1 hafta | Sonnet |
| 11 | Landing Page | 2 | 1.5 hafta | Sonnet |
| 12 | Ölçek Özellikleri | 2+ | 2 hafta | Sonnet |

**Toplam:** ~45 ana chat (+10-15 debug/düzeltme)
**Takvim:** ~6-9 ay (solo + MES projesiyle paralel)

---

# 🧠 SON TAKTIKLER

## 1. Günlük Ritüel

**Sabah (45 dk)** — Planlama:
- Dün nereye kaldım? Git log'a bak.
- Bugünün hedefi: Hangi prompt?
- 2 saatlik blok ayır.

**Öğleden Sonra (2-4 saat)** — Asıl çalışma:
- Cursor yeni chat
- Genel Bağlam + o günkü prompt
- Çalış, commit et.
- 15 mesaj sonra chat yenile.

**Akşam (30 dk)** — Review:
- Bugün yazılan kodu gözden geçir
- Unit test eksikse ekle
- `git push`

## 2. Stuck Olduğunda

**15 dakika kural:** Bir hataya 15 dk takılırsan dur. Yaklaşım değiştir:
- Cursor'a farklı formüle et
- Aynı soruyu Opus'a sor
- GPT-5'e "Cursor bu hatayı çözemedi, sen ne düşünürsün?" de
- Stack Overflow / GitHub issues

## 3. MES Projesinin Yanında

**Tablewise'ı "rahat zihin" projesi yap.**
- MES stresliyse Tablewise'a geç, CRUD yazarak dinlen
- MES büyük karara ihtiyaç duyuyorsa Tablewise'da debug yap (farklı beyin kısmı)
- İki proje benzer teknoloji — bir projede çözdüğün pattern diğerine transfer

## 4. Git Stratejisi

```
main
 └── develop (Tablewise çalışması)
      ├── feat/phase-1
      ├── feat/phase-2
      └── feat/kapora-module
```

- Her faz bitince develop'a merge
- Her 4 faz'da bir develop → main (deploy checkpoint'i)
- MES acil patch gerekirse direkt main'e hotfix

## 5. Unutma

- v1.1 dokümanı **tek gerçek kaynak**. Promptlarla çelişirse doküman kazanır.
- Her faz sonunda git commit. Bir hafta çalış, commit etmeden uyuma.
- Cursor Auto mode yerine manuel model seçimi = daha öngörülebilir
- Yorgunsan Sonnet. Kritik karar varsa Opus. Boilerplate ise Tab.

---

**Başarılar Efe. Soru çıktığında buraya döneriz.**
**— v1.1, Nisan 2026**
