# Tablewise — Fable 5 Kapsamlı Değerlendirme
Tarih: 2026-06-11
Commit: 478b154
Toplam bulgu: 34 (5 kritik, 13 dikkat, 9 fırsat, 7 UI/UX)

## YÖNETİCİ ÖZETİ

Tablewise'ın temel mimarisi sağlam: Clean Architecture ihlali yok, tenant izolasyonu (Global Query Filter + TenantResolverMiddleware) doğru kurulmuş, JWT/refresh token rotasyonu hırsızlık tespitiyle birlikte eksiksiz, rate limiting kapsamlı. En kritik 3 bulgu: (1) Query handler'larda 3 adet N+1 sorgu (özellikle GetCustomersQuery ve haftalık grafikte 7 ayrı sorgu), (2) frontend'ler arası enum casing uyumsuzlukları (ReservationStatus ve RuleAction — booking-ui ile admin-panel/backend farklı case kullanıyor), (3) deployment hazırlığında ciddi açıklar (appsettings.json'da `Include Error Detail=true`, Dockerfile root kullanıcı, Nginx SSL placeholder). Faz 7 (İyzico) öncesi Tenant'ta IyzicoMerchantId ve Payment/Refund entity'leri eksik; refund akışı şu an gerçek iade yapmadan "Refunded" işaretliyor. Genel sağlık durumu iyi — production öncesi yapılacaklar net ve sınırlı.

---

## KRİTİK BULGULAR 🔴

### 1. N+1 Sorgu — GetCustomersQuery müşteri başına alt sorgu
**Dosya:** `src/Tablewise.Application/Features/Customer/Queries/GetCustomersQuery.cs:82-86`
**Sorun:** `LastVisitedVenueName` her müşteri için Reservations tablosuna ayrı subquery atıyor.
**Risk:** Müşteri listesi büyüdükçe (1000+ müşteri) sayfa açılışı saniyeler sürer, DB yükü lineer artar.
**Çözüm Promptu:**
> GetCustomersQuery.cs'de LastVisitedVenueName için satır 82-86'daki per-customer subquery'yi kaldır. Bunun yerine sayfalanmış müşteri ID listesini aldıktan sonra tek bir GroupBy sorgusuyla (Reservations.Where(r => customerIds.Contains(r.CustomerId)).GroupBy(r => r.CustomerId).Select(g => en son rezervasyonun Venue.Name'i)) toplu çek ve bellekte eşle. Mevcut testleri çalıştırarak doğrula.

### 2. N+1 Sorgu — Haftalık grafik 7 ayrı CountAsync
**Dosya:** `src/Tablewise.Application/Features/Tenant/Queries/GetTenantWeeklyChartQueryHandler.cs:42-61`
**Sorun:** 7 günlük grafik için döngüde günde bir `CountAsync` — toplam 7 round-trip. Ayrıca `GetRulesQueryHandler.cs:59-64`'te kural başına venue adı subquery'si var.
**Risk:** Dashboard her açılışta gereksiz DB yükü; rate limit'e takılan dashboard istekleri.
**Çözüm Promptu:**
> GetTenantWeeklyChartQueryHandler.cs'deki 7 iterasyonlu CountAsync döngüsünü tek sorguya çevir: tarih aralığındaki rezervasyonları `GroupBy(r => r.ReservedFor.Date)` ile çekip bellekte 7 güne dağıt. Aynı oturumda GetRulesQueryHandler.cs:59-64'teki VenueName subquery'sini `.Include(r => r.Venue)` ile değiştir.

### 3. Enum casing uyumsuzluğu — booking-ui vs backend/admin-panel
**Dosya:** `frontend/booking-ui/src/types/api.ts:87,117-122` vs `frontend/admin-panel/src/types/api.ts:133,276-282`
**Sorun:** ReservationStatus admin-panel'de `'Pending'|'Confirmed'...` (PascalCase, backend ile uyumlu), booking-ui'de `'pending'|'no_show'` (snake/lower). RuleAction admin-panel'de `'Block'|'Warn'...`, booking-ui'de `'BLOCK'|'WARN'...` ve `'Redirect'` tipi booking-ui'de hiç yok. booking-ui `.toLowerCase()` ile runtime'da düzeltiyor (BookingPage çevresinde) — kırılgan.
**Risk:** Backend yeni status/action eklediğinde booking-ui sessizce yanlış eşler; Redirect aksiyonu booking-ui'de tip hatasız şekilde düşer.
**Çözüm Promptu:**
> booking-ui'deki ReservationStatus ve RuleAction tiplerini backend C# enum'larıyla birebir PascalCase olacak şekilde yeniden tanımla (Pending, Confirmed, Seated, Completed, Cancelled, NoShow; Block, Warn, Suggest, Discount, Deposit, Redirect). toLowerCase tabanlı mapping'leri ve bookingMappers.ts'teki case dönüşümlerini kaldır, kullanan tüm component'leri güncelle. tsc --noEmit ile doğrula.

### 4. Refund stub — iade yapılmadan "Refunded" işaretleniyor
**Dosya:** `src/Tablewise.Application/Features/Reservation/Commands/CancelReservationByStaffCommandHandler.cs:74` ve `frontend/booking-ui/src/pages/BookingPage.tsx:104`
**Sorun:** `// TODO: Faz 7'de İyzico refund` — personel iptalinde kapora gerçek iade olmadan Refunded statüsüne geçiyor. Booking UI'de de ödeme yönlendirmesi TODO.
**Risk:** Kapora özelliği canlıda açılırsa müşteri parası iade edilmediği halde sistem "iade edildi" gösterir — finansal/yasal risk.
**Çözüm Promptu:**
> CancelReservationByStaffCommandHandler.cs:74'te İyzico entegrasyonu gelene kadar deposit'i Refunded yerine yeni bir "RefundPending" durumuna al (DepositStatus enum'una ekle + migration) veya kapora Paid ise iptal işlemini uyarıyla blokla. Faz 7'de gerçek refund çağrısı buraya bağlanacak.

### 5. Production config açıkları — Error Detail + dev şifreler
**Dosya:** `src/Tablewise.Api/appsettings.json:3,15`
**Sorun:** Varsayılan config'de `Password=dev_password;Include Error Detail=true` ve placeholder JWT secret var. Ayrıca `TablewiseDbContextFactory.cs:39` env yoksa dev connection string'e düşüyor.
**Risk:** Production'da env değişkeni unutulursa SQL hata detayları istemciye sızar, dev şifresiyle bağlanmaya çalışır.
**Çözüm Promptu:**
> appsettings.Production.json oluştur: ConnectionString/JWT/Redis değerleri sadece environment variable'dan gelsin, Include Error Detail kaldırılsın. Program.cs'e production'da JWT SecretKey placeholder ise startup'ta fail-fast kontrolü ekle. TablewiseDbContextFactory'deki dev fallback'i sadece Development environment'ta etkin olacak şekilde koşulla.

---

## DİKKAT GEREKTİREN BULGULAR ⚠️

### 6. Dockerfile root kullanıcıyla çalışıyor, healthcheck yok
**Dosya:** `docker/Dockerfile`
**Sorun:** Multi-stage build doğru ama `USER` direktifi ve `HEALTHCHECK` yok; docker-compose.prod.yml'de de healthcheck/resource limit yok.
**Çözüm Promptu:**
> docker/Dockerfile'a non-root kullanıcı ekle (`RUN adduser -u 1000 --disabled-password app` + `USER app`) ve `/healthz`'i çağıran HEALTHCHECK ekle. docker-compose.prod.yml'deki api/postgres/redis servislerine healthcheck ve memory limit tanımla.

### 7. Nginx SSL placeholder
**Dosya:** `docker/nginx/nginx.conf`
**Sorun:** "SSL configuration will be added by Certbot" yorumu — TLS yapılandırması yok.
**Çözüm Promptu:**
> docker/nginx/nginx.conf'a 443 server bloğu, certbot webroot challenge location'ı ve HTTP→HTTPS redirect ekle; docker-compose.prod.yml'e certbot servisi ve sertifika volume'ları tanımla.

### 8. Production'da migration stratejisi tanımsız
**Dosya:** `src/Tablewise.Api/Program.cs:364`
**Sorun:** `MigrateAsync()` sadece Development'ta çalışıyor; production için ne otomatik migration ne dokümante edilmiş manuel prosedür var. scripts/ altında backup/rollback script'i de yok.
**Çözüm Promptu:**
> Deployment için bir migration stratejisi belirle ve uygula: ya container entrypoint'inde `dotnet ef database update` koşturan ayrı bir migration job'u, ya da scripts/migrate-prod.ps1 + pg_dump tabanlı backup script'i. Tercihi docs/deployment.md'ye yaz.

### 9. Auth akışlarının testi yok
**Dosya:** `tests/` (Tablewise.UnitTests, Tablewise.IntegrationTests)
**Sorun:** Rule engine, booking concurrency ve idempotency iyi test edilmiş; register/login/refresh rotation/password reset için hiç test yok. Multi-tenant izolasyonu için de dedike test yok.
**Çözüm Promptu:**
> Tablewise.IntegrationTests'e AuthFlowTests ekle: register→verify→login→refresh→eski refresh token'ın reuse'da tüm tokenları revoke ettiği senaryo. Ayrıca TenantIsolationTests: A tenant'ının JWT'siyle B tenant'ının rezervasyonuna erişilemediğini doğrula.

### 10. booking-ui axios interceptor eksikleri
**Dosya:** `frontend/booking-ui/src/lib/api.ts:41-53,107`
**Sorun:** Hata yönetimi interceptor yerine tekil fonksiyonda; satır 107'de `api.post<any>` var. (Public akış olduğu için 401-refresh gerekmiyor; sorun hata normalizasyonu ve tip.)
**Çözüm Promptu:**
> booking-ui/src/lib/api.ts'e admin-panel'dekine benzer response interceptor ekle (5xx ve network hatası için kullanıcı dostu Türkçe mesaj), satır 107'deki `<any>`'yi gerçek response DTO tipiyle değiştir.

### 11. super-admin'de 401'de refresh denenmiyor
**Dosya:** `frontend/super-admin/src/lib/api.ts:28-32`
**Sorun:** 401'de doğrudan logout/redirect; token süresi dolan platform kullanıcısı çalışmasını kaybediyor.
**Çözüm Promptu:**
> super-admin api.ts'e admin-panel'deki 401 refresh-retry mantığını uyarla (platform refresh endpoint'i ile); refresh başarısızsa mevcut logout davranışını koru.

### 12. PlatformLoginCommand validator'sız
**Dosya:** `src/Tablewise.Application/Features/Platform/Auth/PlatformLoginCommand.cs:6`
**Sorun:** Email/Password için FluentValidation validator yok (Customer tier/blacklist/notes command'larında da eksik olabilir).
**Çözüm Promptu:**
> PlatformLoginDtoValidator ekle (NotEmpty + EmailAddress + MinLength(8)). Aynı taramada UpdateCustomerTier/Blacklist/Notes command'larının validator'larını kontrol et, eksikleri tamamla.

### 13. SubscriptionPage ve NotificationSettings'te plan hardcoded
**Dosya:** `frontend/admin-panel/src/features/subscription/SubscriptionPage.tsx:80`, `frontend/admin-panel/src/features/settings/components/NotificationSettings.tsx:20`
**Sorun:** Plan bilgisi "Starter" olarak sabit; gerçek plan auth store/API'den gelmiyor.
**Çözüm Promptu:**
> Tenant'ın gerçek plan bilgisini dönen endpoint'i (GET /api/v1/tenant/me veya subscription) SubscriptionPage ve NotificationSettings'e bağla; hardcoded "Starter" değerlerini kaldır.

### 14. Onboarding isFirstLogin senkronu eksik
**Dosya:** `frontend/admin-panel/src/features/onboarding/OnboardingPage.tsx:70`
**Sorun:** `// TODO: PATCH /api/v1/tenant/me { isFirstLogin: false }` — onboarding tamamlandı bilgisi backend'e yazılmıyor; kullanıcı her girişte onboarding görebilir.

### 15. GeneralSettings logo upload akışı tamamlanmamış
**Dosya:** `frontend/admin-panel/src/features/settings/components/GeneralSettings.tsx:51`
**Sorun:** Backend'de R2 presigned URL akışı hazır (GenerateLogoUploadUrl/ConfirmLogoUpload) ama UI bağlanmamış (TODO).
**Çözüm Promptu:**
> GeneralSettings'teki logo yükleme alanını mevcut backend akışına bağla: POST generate-upload-url → R2'ye PUT → POST confirm. 5MB/format validasyonunu client'ta da göster.

### 16. WhatsApp SandboxMode production'a taşınma riski
**Dosya:** `src/Tablewise.Api/appsettings.json` (WhatsApp bölümü)
**Sorun:** `SandboxMode: true` varsayılan config'de; production'da kapatılması manuel hatırlamaya bağlı.

### 17. Tip adlandırma tutarsızlığı — guestName vs customerName
**Dosya:** `frontend/booking-ui/src/lib/bookingMappers.ts:90`
**Sorun:** admin-panel `guestName/guestEmail/guestPhone`, booking-ui `customerName/...` kullanıyor; mapper'la çözülmüş ama bakım maliyeti yaratıyor. Düşük öncelikli, enum düzeltmesiyle (bulgu 3) birlikte ele alınabilir.

### 18. noUncheckedIndexedAccess hiçbir frontend'de açık değil
**Dosya:** 4 frontend'in `tsconfig.json` dosyaları
**Sorun:** strict açık ama dizi/index erişimleri undefined kontrolünden muaf.

---

## GELİŞTİRME FIRSATLARI ✨

### 19. MediatR pipeline behavior'ları yok
**Dosya:** `src/Tablewise.Api/Program.cs:144`
FluentValidation auto-validation çalışıyor ama ValidationBehavior/LoggingBehavior/PerformanceBehavior yok. Yavaş handler'ları otomatik loglayan bir PerformanceBehavior (ör. >500ms uyarısı) N+1 benzeri sorunları erken yakalar.

### 20. Code splitting yok
4 frontend'de de tüm route'lar statik import; React.lazy kullanılmıyor. Admin-panel'de recharts eager yükleniyor. Önce admin-panel router'ında sayfa bazlı `React.lazy` + `Suspense` uygulanmalı.

### 21. GetVenuesQueryHandler TableCount
**Dosya:** `src/Tablewise.Application/Features/Venue/Queries/GetVenuesQueryHandler.cs:57` — Count'u SQL seviyesinde projekte et.

### 22. AuditLog / NotificationLog / WhatsAppMessage sorgu endpoint'leri yok
Entity'ler dolu ama admin panelden görüntülenemiyor. Destek taleplerinde "mesaj gitti mi?" sorusuna cevap için NotificationLog listesi (venue/rezervasyon filtreli) değerli.

### 23. Health check kapsamı
`/health` PostgreSQL+Redis kontrol ediyor; R2 ve SendGrid için da degraded-status check eklenebilir.

### 24. Metrics yok
Prometheus exporter veya OpenTelemetry yok; Faz 10'da `prometheus-net.AspNetCore` ile temel HTTP/DB metrikleri eklenebilir.

### 25. Subscription endpoint stub
`PlatformSubscriptionsController` "Bu özellik Faz 7'de aktif olacak" dönüyor — Faz 7 kapsamına alınmalı (zaten planlı).

### 26. RuleEngine Redirect aksiyonu ertelenmiş
**Dosya:** `src/Tablewise.RuleEngine/Services/RuleEnginePipeline.cs:287` — `// TODO: v2'de redirect mantığı`. Admin panelde Redirect seçilebiliyorsa UI'dan gizlenmeli ya da "yakında" işaretlenmeli.

### 27. TodayReservationsList satır tıklaması
**Dosya:** `frontend/admin-panel/src/features/dashboard/components/TodayReservationsList.tsx:101` — Drawer açma TODO; mevcut ReservationDetailDrawer'a bağlanması ucuz kazanım.

---

## UI/UX ÖNERİLERİ 🎨

### Admin Panel
- **[Yüksek]** Rezervasyon timeline'ı `min-w-[1200px]` (ReservationsPage.tsx:177) — mobil/tablette yatay scroll zorunlu. Restoran personeli telefonla kullanacaksa dar ekranlar için liste/karta düşen responsive fallback ekleyin.
  > Prompt: ReservationsPage'e `md` altı breakpoint'te timeline yerine saat sıralı kart listesi render eden bir görünüm ekle; mevcut filtreler ve durum renkleri korunmalı.
- **[Orta]** Tek günde çok rezervasyonlu yoğun mekânlar için timeline'da virtualization değerlendirin (şu an gerek yok, 100+ masa olursa gündeme alın).
- **[Düşük]** Loading skeleton ve empty state'ler tutarlı ve Türkçe — mevcut hali iyi.

### Booking UI
- **[Düşük]** 5 adımlı akış, ProgressBar, Türkçe validasyon, responsive grid ve 48px dokunmatik hedefler yerinde — akış olgun. Tek iyileştirme: Step1+Step2 (tarih ve saat/kişi) tek ekranda birleştirilerek adım sayısı 4'e inebilir; dönüşüm ölçümü olmadan zorunlu değil.

### Super Admin Panel
- **[Orta]** Tenant listesinde bulk action yok (TenantsPage.tsx). Tenant sayısı 50'yi geçince toplu askıya alma/plan değişimi için checkbox + toolbar pattern'i ekleyin.
- **[Düşük]** Limit editöründeki sınırsız-checkbox pattern'i (PricingPage.tsx:105-106) doğru uygulanmış.

### Landing Page
- **[Yüksek]** OG tags, Twitter card ve schema markup yok (`frontend/landing/index.html`). Sosyal paylaşım ve SEO için kritik.
  > Prompt: landing/index.html'e og:title/og:description/og:image/og:url, twitter:card ve Organization + SoftwareApplication JSON-LD schema bloklarını ekle; og:image için 1200x630 statik görsel oluştur.
- **[Düşük]** Hero CTA above-the-fold, aylık/yıllık toggle ve %20 indirim etiketi mevcut; social proof 5 mekân adı listeliyor — gerçek logo/rakam (ör. "X rezervasyon/ay") eklendiğinde inandırıcılık artar.

---

## DEPLOYMENT KONTROLÜ 📦

- ✅ Multi-stage Docker build
- ❌ Dockerfile non-root user
- ❌ Container HEALTHCHECK / compose healthcheck'leri
- ❌ Resource limit'leri (memory/CPU)
- ✅ docker-compose.prod.yml: env-var secrets + `restart: always`
- ❌ Nginx SSL (placeholder — Certbot kurulmamış)
- ✅ Health endpoint'leri: /health, /healthz, /ready (PostgreSQL + Redis)
- ⚠️ Migration: sadece Development'ta otomatik; prod prosedürü tanımsız
- ❌ Backup/rollback script'leri
- ❌ appsettings.Production.json (Error Detail + dev şifre sorunu)
- ✅ Serilog yapılandırılmış (Seq + Sentry, enrichment'lı)
- ⚠️ Seq URL hardcoded localhost; Sentry DSN boş
- ❌ Metrics/alerting (Prometheus vb.)
- ✅ Rate limiting production profilleri tanımlı
- ⚠️ E2E testler yeşil (67/67) ama auth/izolasyon entegrasyon testleri yok

## SONRAKI ADIMLAR

1. **Production config sertleştirme** (bulgu 5, 16) — appsettings.Production.json, JWT fail-fast, Error Detail kaldırma — ~2 saat
2. **N+1 düzeltmeleri** (bulgu 1, 2, 21) — GetCustomers, WeeklyChart, GetRules — ~3 saat
3. **booking-ui enum/tip hizalama** (bulgu 3, 10, 17) — ~3 saat
4. **Refund güvenlik kilidi** (bulgu 4) — RefundPending durumu veya iptal bloğu — ~2 saat
5. **Faz 7 başlangıcı**: Tenant.IyzicoSubMerchantKey + Payment/Refund entity'leri + İyzico webhook controller iskeleti — ~1-2 gün
6. **Docker/Nginx production hazırlığı** (bulgu 6, 7, 8) — non-root, healthcheck, SSL, migration script — ~1 gün
7. **Auth + tenant izolasyon entegrasyon testleri** (bulgu 9) — ~1 gün
8. **Landing SEO** (OG + schema) — ~1 saat
9. **Admin panel mobil timeline fallback** — ~yarım gün
10. **Küçük TODO temizliği** (bulgu 13, 14, 15, 27) — ~yarım gün
