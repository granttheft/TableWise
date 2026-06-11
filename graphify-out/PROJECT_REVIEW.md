# TableWise — Proje Genel Değerlendirme Raporu

**Tarih:** 2026-06-11
**Değerleme Derinliği:** Kapsamlı (3 paralel keşif ajanı + manuel doğrulama)
**Hedef:** Faz 9 (Güvenlik) öncesi mevcut durum tespiti
**Kapsanan commit:** `1aebd44`

---

## Yönetici Özeti

TableWise mimari olarak sağlam ve büyük ölçüde production-hazır. Backend Clean Architecture + CQRS pattern'i tutarlı uygulamış, multi-tenant izolasyon güçlü, soft delete universal. Asıl risk üç noktada:

1. **İyzico (Faz 7) sıfır** — Entity alanları hazır ama controller/webhook/service yok.
2. **Frontend tip ve API tutarsızlıkları** — 4 frontend arasında paylaşılan tip yok, `ReservationStatus` enum case-mismatch, booking-ui'de auth interceptor yok.
3. **Sırlar (`secrets`) ve CI/CD eksik** — JWT secret + DB password `appsettings.json`'da literal, `.github/workflows/` boş.

Faz 9'a geçmeden önce **5 kritik düzeltme** öneriliyor (rapor sonunda).

---

## 1. Mimari ve Kod Kalitesi

**Durum:** ✅ İyi (1 dikkat noktası)

### Bulgular
- **Clean Architecture katmanlama:** Domain → Application → Infrastructure → Api yönü doğru. 84 CQRS handler, 23 controller, hepsi `IMediator.Send()` üzerinden gidiyor. Controller'larda direkt `DbContext` kullanımı yok.
- **CQRS pattern:** `src/Tablewise.Application/Features/{Modül}/{Commands|Queries}/` yapısı tutarlı.
- **Multi-tenant izolasyon:** `src/Tablewise.Infrastructure/Persistence/TablewiseDbContext.cs:71-146` — 18 entity `HasQueryFilter` ile global filtreli. `TenantScopedEntity` → `e.TenantId == TenantFilterId.Value && !e.IsDeleted`. `BaseEntity` türevleri (Plan, Tenant, PlatformUser) sadece soft delete.
- **TenantResolverMiddleware:** `src/Tablewise.Api/Middleware/TenantResolverMiddleware.cs:115` — JWT'den `tenant_id` claim'i + slug'dan public booking için tenant resolve. Suspended/Cancelled tenant erişim reddi (`:168-171`).
- **Soft delete:** `BaseEntity` (`src/Tablewise.Domain/Common/BaseEntity.cs`) — `Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt` universal. 23 entity'nin tümünde mevcut. `SaveChangesAsync` override (`TablewiseDbContext.cs:151-184`) `CreatedAt/UpdatedAt/TenantId` otomatik set.

### Dikkat
- **EF Core sızıntısı (Application katmanı):** Application Handler'larda `IApplicationDbContext` üzerinden `.Include()`, `.CountAsync()`, `.IgnoreQueryFilters()` doğrudan kullanılıyor (`GetTenantUsageQueryHandler.cs:28-45`, `GetTenantPlanLimitsByIdQuery.cs:23-49`). Saf Clean Architecture için repository pattern olmalıydı, fakat bu pragmatik bir seçim — şu an refactor maliyeti faydadan yüksek.

### Öneri
- Şu an refactor gerekli değil. Repository pattern Faz 14+ retro'da konuşulabilir.

---

## 2. Güvenlik Açıkları (Faz 9 öncesi ön değerlendirme)

**Durum:** ⚠️ Dikkat (2 kritik, 3 iyileştirme)

### Bulgular

**JWT yapılandırması** (`src/Tablewise.Api/Program.cs:76-132`, `JwtTokenService.cs`):
- Algoritma: **HS256 (simetrik)** — Faz 9'da RS256'ya geçiş planlı (`JwtSettings.cs:16` yorumu)
- Access token: 60 dk; Refresh: 30 gün (normal) / 90 gün (remember me)
- Clock skew: 30s ✅
- Token rotation: var ✅
- **Çift JWT scheme:** `Default` (tenant) ve `Platform` (admin) — `tablewise-api` / `tablewise-platform` audience ayrımı.

**Korumasız endpoint denetimi** (23 Controller):
- `[AllowAnonymous]`: AuthController, PlatformAuthController, PublicController, WhatsAppWebhookController, booking public endpoint'leri — hepsi meşru, beklenen.
- `[Authorize]` + `[RequireOwner]` / `[RequireOwnerOrStaff]` / `[RequirePlatformRole]` dağılımı tutarlı.

**Rate limiting** (`Program.cs:170-278`):
- Auth: 10 req/dk (prod) ✅
- Booking: 30 req/dk; Reserve: 5 req/dk ✅
- Global fallback: 100 req/dk
- Authenticated: 200 req/dk (user bazlı)

**Brute-force koruması** (`AuthSettings.cs:14-21`): 5 deneme, 15 dk lockout, BCrypt work factor 12.

**Global exception handler** (`src/Tablewise.Api/Middleware/GlobalExceptionHandler.cs:13-348`):
- ValidationException → 422
- NotFoundException → 404
- ForbiddenException / TenantIsolationException → 403 (TenantIsolation'da Sentry alert)
- PlanLimitExceededException → 403 + upgrade URL
- ConflictException → 409
- Diğer → 500
- HTTP kodları doğru kullanılıyor ✅

**Input validation:** FluentValidation aktif (`RegisterTenantDtoValidator.cs`) — email, password (min 8, upper/lower/digit), Türkçe telefon, KVKK consent zorunlu.

**CORS** (`Program.cs:154-167`): Whitelist (`appsettings.json` → `tablewise.com.tr`, `app.tablewise.com.tr`). `AllowAnyOrigin` yok ✅. Dev'de localhost portları.

### 🔴 Kritik
- **Secrets hardcoded:** `src/Tablewise.Api/appsettings.json:15` → `"YOUR_SUPER_SECRET_KEY_MIN_32_CHARS_REPLACE_IN_PRODUCTION"`; `:3` DB password `dev_password`. Production'da environment variable / Azure KeyVault'a taşınmalı.
- **Health check endpoint yok:** `Program.cs`'de HealthChecks NuGet'leri yüklü ama `/health` endpoint'i map edilmemiş. Load balancer / Docker liveness için gerekli.

### ⚠️ İyileştirme
- HS256 → RS256 geçişi (Faz 9 planı).
- `appsettings.Local.json` doğru `.gitignore`'da, kullanıcı `.env.example` (62 satır) ile şablonlanmış ✅.

---

## 3. Tamamlanan Fazların Eksiksizliği

### Faz 5 — Admin Panel
**Durum:** ✅ Tamamlandı

- Dashboard, rezervasyonlar, masalar, kurallar, müşteri DB, ayarlar, abonelik, personel sayfaları mevcut.
- 18 E2E test geçiyor.
- Plan kullanım widget'ı yeni eklendi (Özel Limitler kapsamında).

### Faz 6 — Booking UI
**Durum:** ⚠️ Eksik var

- Public rezervasyon akışı tam (`booking-flow:7` test).
- View/modify/cancel akışları test edildi (8 test).
- **Eksik:** `frontend/booking-ui/src/lib/api.ts:32-38` — **auth token interceptor yok**. Public flow için sorun değil ama "View/Modify" oturumlarında reservation token kullanımı tutarlı mı kontrol edilmeli.

### Faz 6.5 — WhatsApp Backend
**Durum:** ✅ Tamamlandı

- `WhatsAppWebhookController` mevcut, Twilio signature doğrulama (`:131-132`).
- `WhatsAppMessage` entity + `VenueWhatsAppPreferences` migration'ları başarılı.
- Tablewise ortak Twilio numarası kararı (CLAUDE.md) uygulanmış.

### Faz 8.5 — Super Admin Panel
**Durum:** ✅ Tamamlandı

- Tenant listesi, detay, pricing, kuponlar, ekip yönetimi, ödemeler, cihazlar — tümü mevcut.
- 13 E2E test geçiyor.
- Faz 7 (İyzico) bekleyen yerlerde plan notu var (`PlatformSubscriptionsController.cs:18-23`).

### Faz 11 — Landing Page
**Durum:** ⚠️ Eksik var

- 10 section + dinamik pricing API entegrasyonu çalışıyor.
- **Eksik:** `frontend/landing/src/config.ts:1-5` — `ADMIN_PANEL_URL` ve `BOOKING_UI_URL` hardcoded `localhost`. Production build'de env değişkeniyle override edilmesi şart (`VITE_ADMIN_PANEL_URL`, `VITE_BOOKING_UI_URL` fallback olarak var, ama env yoksa localhost kalıyor).
- E2E test yok (`e2e/playwright-report/` listede ama landing spec'i yok).

### Özel Limitler (Backend + Super Admin + Admin Panel)
**Durum:** ✅ Tamamlandı

- Backend: `Tenant.CustomLimitsJson` + migration + `GET/PUT /api/platform/tenants/{id}/custom-limits` + `GET .../plan-limits`.
- Super Admin: TenantDetailPage "Özel Limitler" sekmesi.
- Admin Panel: Dashboard PlanUsageWidget + VenueSettings limit enforcement.
- **Eksik:** E2E test yok. Aşağıda detay var.

---

## 4. Frontend Tutarsızlıkları

**Durum:** ⚠️ Dikkat (4 tutarsızlık)

### Bulgular

**Tip çakışmaları:**
- **`ReservationStatus` enum case mismatch:**
  - admin-panel `src/types/api.ts:276-282`: `Pending|Confirmed|Seated|Completed|Cancelled|NoShow` (PascalCase)
  - booking-ui `src/types/api.ts:117-122`: `pending|confirmed|cancelled|completed|no_show` (snake_case, `Seated` yok)
  - Backend'in hangi case'i döndürdüğüne bağlı olarak bir frontend bozuk olabilir.
- **Guest vs Customer alan adlandırması:**
  - admin-panel: `guestName`, `guestPhone` (`:21, 293-295`)
  - booking-ui: `customerName`, `customerPhone` (`:99-101`)
- **`PlanLimits`** sadece admin-panel'de tipli (`:108-118`); super-admin custom limit'i string olarak parse ediyor.

**API çağrı pattern:**
- 3 admin frontend axios + React Query kullanıyor, base URL env-driven (`VITE_API_URL ?? ''`).
- super-admin ayrı base: `VITE_PLATFORM_API_URL` ✅ doğru.
- **booking-ui** axios interceptor'ında **auth token yok** (`src/lib/api.ts:32-38`) — sadece `Idempotency-Key` header'ı set ediliyor.

**Hata yönetimi tutarsızlığı:**
- admin-panel: toast + 403 PLAN_LIMIT_EXCEEDED özel handler ✅
- super-admin: 500+ ve network için minimal handler
- booking-ui: `handleApiError()` fonksiyonu var ama toast yok
- **Sessiz catch:** `admin-panel/src/hooks/useReservations.ts:72` — `catch {}` boş

**Loading state:** Skeleton component admin-panel + super-admin'de tanımlı, booking-ui'de yok.

**TypeScript config:** 3 admin frontend identical (`strict, noUnusedLocals, noUnusedParameters` ✅). ESLint config: booking-ui'de var, admin-panel ve super-admin'de yok.

### Öneri
- **Faz 9'da paylaşılan `@tablewise/types` paketi oluşturmak yerine** kısa vadede: Backend DTO'larından otomatik tip üreten OpenAPI/NSwag pipeline'ı kur, böylece tek kaynaktan dağıt.
- **booking-ui auth interceptor:** Reservation token (modify/cancel için) header'a otomatik eklensin.
- **ReservationStatus case standardize et:** Backend tarafında JSON serializer naming policy denetlenmeli.

---

## 5. Test Kapsamı

**Durum:** ⚠️ Dikkat (kritik path'ler test edilmiyor)

### Bulgular

**Mevcut E2E:** 67 test (15 spec dosyası, 929 satır):
- admin-panel: 18 test (auth, dashboard, customers, reservations, rules, settings, staff, tables)
- booking-ui: 24 test (booking-flow:7, validation:9, view-modify-cancel:8)
- super-admin: 13 test (auth, pricing-coupons, team, tenants)

**TEST EDİLMEYEN KRİTİK PATH'LER:**
1. **Kapora ödeme flow'u** — `DepositStatus` lifecycle (NotRequired → Pending → Paid → Refunded) hiç test edilmiyor.
2. **Plan upgrade akışı** — super-admin tenant'a plan değiştirme var ama E2E yok; admin-panel SubscriptionPage'in plan değişimi sonrası davranışı test yok.
3. **Custom Limits uygulaması** — Super Admin custom limit set → Admin Panel'de görünüm → limit dolunca buton disable: 3 katmanlı flow hiç test edilmemiş.
4. **Refund logic** — `DepositRefundPolicy.depositRefundHours`, `depositPartialPercent` parametreleri test edilmiyor.
5. **Multi-tenant izolasyon** — Bir tenant'ın diğerinin verisini görmediğine dair pozitif test yok.

**Backend unit/integration test yok** — `src/Tablewise.Tests/` veya benzeri yok. Bu durumda:
- 23 controller, 84 handler, 23 entity için **regresyon güvenliği sıfır**.
- Refactor cesareti düşük → teknik borç birikme riski.
- Özellikle multi-tenant filter logic için integration test kritik.

### Öneri
- Faz 9'da minimum: `Tablewise.IntegrationTests` projesi + multi-tenant izolasyon testleri.
- Custom Limits E2E (3 katmanlı flow) Faz 7 öncesi eklenmeli.

---

## 6. Teknik Borç

**Durum:** ⚠️ Dikkat (1 kritik TODO, hardcoded değerler var)

### Bulgular

**TODO/FIXME (22 adet):**
- 🔴 `DepositRefundByStaffCommandHandler.cs:74` — "TODO: Faz 7'de İyzico refund" — kapora iade akışı yarım.
- `RuleEnginePipeline.cs:287` — "TODO: v2'de redirect mantığı" — düşük öncelikli.
- Geri kalan 20'si kozmetik (Türkçe telefon formatı vb. açıklayıcı yorumlar).

**Hardcoded değerler:**
- `frontend/landing/src/config.ts:1-5` — `ADMIN_PANEL_URL`, `BOOKING_UI_URL` localhost fallback.
- `appsettings.json:3` — DB connection literal `dev_password`.
- `appsettings.json:15` — JWT secret literal placeholder.
- `appsettings.json:34` — Redis localhost.
- `appsettings.json:50` — Seq endpoint hardcoded.
- `Program.cs:55` — Serilog Seq fallback.

**Migration history (9 migration):** Anlamlı isimli, geri alınmış/tekrar oluşturulmuş yok ✅. Down() metodları sorgulanmadı.

**Kullanılmayan import:** `frontend/admin-panel/src/hooks/useRules.ts` bu oturumda `RuleTestResult` unused fix edildi. Geri kalan TypeScript çıktıları temiz (`tsc --noEmit` her iki frontend'de geçiyor).

### Öneri
- **Hardcoded secrets** prod build öncesi User Secrets / Azure KeyVault / .env'ye taşınmalı (Faz 9 / Faz 10).
- TODO'lar tek bir issue tracker'a (GitHub Issues) çekilmeli, kozmetikler kapatılmalı.

---

## 7. Deployment Hazırlığı (Faz 10 öncesi)

**Durum:** ⚠️ Dikkat (CI/CD ve health check eksik)

### Bulgular

**Docker:**
- ✅ Multi-stage Dockerfile (sdk → publish → aspnet:8.0)
- ✅ `docker-compose.yml`: PostgreSQL 15 Alpine, Redis 7 Alpine, Seq
- ⚠️ Dockerfile EXPOSE sadece 5000 hardcoded
- ❌ Health check yok (Docker `HEALTHCHECK` ve API `/health` her ikisi de eksik)
- ⚠️ docker-compose'da `POSTGRES_PASSWORD=dev_password` ve Redis `--requirepass dev_password` literal

**Environment yönetimi:**
- ✅ `appsettings.{Development,Local}.json` ayrımı
- ✅ `.env.example` 62 satır şablonlanmış (İyzico, Twilio, SendGrid, SMTP)
- ✅ `.gitignore` `.env` ve `appsettings.Local.json` koruyor
- ❌ Production secret vault yok

**CORS:** Whitelist aktif ✅. `AllowAnyMethod/AllowAnyHeader` üretim için kabul edilebilir.

**Logging:** Serilog + Seq + Sentry (PII disabled, KVKK uyumlu) ✅. File sink yok — Seq'e bağımlı.

**CI/CD:** ❌ `.github/workflows/` boş. Build/test/docker push otomasyonu yok.

### Öneri
- Faz 10 öncesi: Health check endpoint + GitHub Actions workflow (build → test → docker push → migration apply) + secret vault entegrasyonu.

---

## 8. İyzico Entegrasyonu Öncesi Riskler (Faz 7)

**Durum:** 🔴 Kritik (entity hazır, kod sıfır)

### Bulgular

**Hazır olan:**
- `Subscription` entity'sinde alanlar (`src/Tablewise.Domain/Entities/Subscription.cs:43-50`):
  - `IyzicoSubscriptionId`, `IyzicoCustomerId`, `NextBillingDate`, `Amount`, `Currency` ("TRY"), `CancelledAt`
- `Reservation` entity'sinde kapora alanları:
  - `DepositStatus` (enum: NotRequired, Pending, Paid, Refunded, Forfeited, Failed)
  - `DepositAmount`, `DepositPaymentRef`, `DepositPaidAt`, `DepositRefundedAt`

**Eksik:**
- ❌ `Payment` entity (fatura/makbuz için)
- ❌ `Refund` entity
- ❌ `WebhookController` (sadece WhatsApp webhook var — `WhatsAppWebhookController`)
- ❌ `PaymentController` veya `SubscriptionsController` (Public/Tenant tarafı)
- ❌ İyzico SDK / interface (Service layer): `IIyzicoService` yok, `iyzipay` NuGet referansı yok
- ❌ **Sub-merchant (alt üye işyeri)** akışı: Tenant entity'de `IyzicoMerchantId` alanı yok. Mimari karara göre (CLAUDE.md: "Kapora restoranın İyzico alt üye işyeri hesabına gider") kritik.

**Risk Seviyesi: YÜKSEK** — Faz 7 başlamadan önce minimum entity ekleme + interface tanımı + webhook iskeleti yapılmalı, yoksa Faz 7 başlangıçta plan revizyonu gerekecek.

### Öneri
- Faz 7 başında ilk adım: `Tenant.IyzicoMerchantId` + `Payment` + `Refund` entity'leri + `IPaymentService` interface + `PaymentWebhookController` iskeleti.

---

## Faz 9'a Geçmeden Önce Düzeltilmesi Gereken 5 Kritik Eksik

Öncelik sırasıyla:

### 1. 🔴 Production Secret Yönetimi
**Neden:** JWT secret + DB password `appsettings.json`'da literal placeholder. Faz 9 güvenlik fazı; başlamadan önce secret yönetimi mimari oturmalı, yoksa fazın bulguları yarım kalır.

**Aksiyon:** User Secrets (.NET) lokal için + Azure KeyVault / Docker secret production için. `appsettings.json`'dan literal değerleri çıkar, environment variable expansion kullan (`${JWT_SECRET}` pattern).

### 2. 🔴 Health Check Endpoint
**Neden:** Faz 10'da deployment'ın ön şartı. Docker `HEALTHCHECK`, K8s liveness/readiness ve load balancer için zorunlu.

**Aksiyon:** `Program.cs`'e `builder.Services.AddHealthChecks().AddNpgSql().AddRedis()` + `app.MapHealthChecks("/health")` ekle. Dockerfile'a `HEALTHCHECK CMD curl -f http://localhost:5000/health`.

### 3. 🔴 Frontend `ReservationStatus` Case Tutarsızlığı + booking-ui Auth Interceptor
**Neden:** Şu an üretimde booking-ui'nin modify/cancel akışı potansiyel olarak status string'ini yanlış parse ediyor olabilir. Backend ne döndürüyor doğrulanıp tek case'e sabitlenmeli. Auth interceptor yokluğu da reservation token akışını kırılgan bırakıyor.

**Aksiyon:** Backend `JsonSerializerOptions.PropertyNamingPolicy` denetlenmeli, status enum'u tek case'e (`PascalCase` öneririm — backend'in default'u o) sabitle. booking-ui `lib/api.ts`'ye token interceptor ekle.

### 4. 🔴 Multi-Tenant İzolasyon Integration Test
**Neden:** 18 entity için global query filter var, fakat regresyon testi yok. Faz 9'da güvenlik denetimi yaparken bir `IgnoreQueryFilters()` çağrısı yanlışlıkla kalırsa fark edilemez.

**Aksiyon:** `Tablewise.IntegrationTests` projesi oluştur, minimum 5 test: (a) tenant A user'ı tenant B venue göremez, (b) tenant A user'ı tenant B reservation güncelleyemez, (c) `IgnoreQueryFilters()` kullanılan tüm yerlerde controlled bypass olduğunu doğrula, (d) suspended tenant erişim reddi, (e) custom limits override doğru tenant'a uygulanıyor.

### 5. ⚠️ İyzico Hazırlığı (Faz 7 başlangıç bloğu)
**Neden:** Faz 9'dan önce direkt etki yok ama Faz 9 sonrası Faz 7'ye geçişte plan revizyonu olmaması için. `Tenant.IyzicoMerchantId` ve `Payment` / `Refund` entity'leri eklenmeli + migration. Bu bir öğleden sonralık iş, faz başında stres yapılmasın.

**Aksiyon:** `Tenant.IyzicoMerchantId` (nullable string) + `Payment` entity (TenantScoped) + `Refund` entity (Payment FK) + migration. Henüz logic yok, sadece iskelet.

---

## Sonuç

Proje, dokümantasyon ve commit disipliniyle ölçeklenebilir bir tabana sahip. Backend güçlü, frontend hızla büyümüş ve doğal olarak tip/pattern tutarsızlıkları birikmiş. **Faz 9 başlamadan yukarıdaki 5 maddenin tamamlanması yaklaşık 1-2 günlük iş**; faz güvenlik denetiminin değerini katlar.

Mevcut commit `1aebd44` üzerinden değerlendirme yapıldı.
