# Tablewise — Cursor Promptları: Faz 7, 8.5, 14

> Bu dosya mevcut **tablewise-cursor-prompts-v1.1.md** dosyasının devamıdır.
> Her prompttan önce mutlaka **[GENEL BAĞLAMI YAPISTIR]** bloğunu ekle.
> Çelişki olursa ürün dökümanı (.docx) kazanır.

---

## 🎯 Model Seçim Hatırlatması

| Görev | Model |
|-------|-------|
| Entity, DTO, controller, basit UI, CRUD | **Sonnet 4.5** |
| İyzico akış mimarisi, webhook güvenliği, idempotency, refund mantığı | **Opus 4.5** |
| WhatsApp bildirim bağlantıları (Faz 7.5) — minimal entegrasyon | **Sonnet 4.5** |
| Otomatik tamamlama, tekrarlı kod | **Cursor Tab** |

**Kural:** Yeni chat = temiz context. Her prompttan önce genel bağlamı yapıştır. Üst üste değişiklik biriktirme.

---

# FAZ 7 — İYZİCO ENTEGRASYONU

> Hedef: Hem SaaS abonelik ödemeleri (restoran → Tablewise) hem de kapora ödemeleri (müşteri → restoran, Alt Üye İşyeri modeli).
> Karar: **Sadece İyzico.** Stripe yok, hibrit yok. Kapora isteyen restoran İyzico kullanmak zorunda.

---

## Prompt 7.1 — İyzico Altyapısı + Abonelik Ödemeleri

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.1 — İyzico ödeme altyapısı ve SaaS abonelik ödemeleri.

1. IyzicoConfig (Infrastructure/Payment/):
   - ApiKey, SecretKey, BaseUrl (sandbox + production)
   - appsettings'ten okunur, secrets ile yönetilir
   - Sandbox/Production toggle

2. IPaymentService interface (Core/Interfaces/):
   - CreateSubscriptionAsync(tenant, plan, card) → SubscriptionResult
   - CancelSubscriptionAsync(subscriptionRef)
   - UpdateCardAsync(tenant, newCard)
   - GetSubscriptionStatusAsync(subscriptionRef)

3. IyzicoPaymentService implementasyonu:
   - İyzico .NET SDK kullan (iyzipay-dotnet NuGet paketi)
   - Kart bilgisini ASLA veritabanında saklama
   - Sadece İyzico'dan dönen cardUserKey ve cardToken sakla
   - Tüm istek/yanıtları logla (kart bilgisi hariç)

4. Subscription entity (Core/Entities/):
   - TenantId, PlanType, Status (Active/Cancelled/PastDue/Trial)
   - IyzicoSubscriptionRef, CardUserKey
   - CurrentPeriodStart, CurrentPeriodEnd
   - CancelAtPeriodEnd (bool)
   - Soft delete + audit alanları

5. Abonelik akışı (Application katmanı — CQRS):
   - CreateSubscriptionCommand: plan seç + kart bilgisi → İyzico'da abonelik oluştur
   - Plan limitleri Subscription'a yansır
   - Trial'dan paid'e geçiş mantığı
   - Plan downgrade/upgrade (proration dahil)

6. Idempotency:
   - Her ödeme isteğine unique idempotencyKey ekle
   - Aynı istek iki kez gelirse tek işlem yap
   - Redis'te idempotency key cache'le (24 saat TTL)

7. Hata yönetimi:
   - İyzico hata kodlarını anlamlı Türkçe mesajlara çevir
   - Kart reddi, yetersiz bakiye, geçersiz kart vb.
   - Retry mantığı (geçici hatalar için)

Tüm kodu Clean Architecture'a uygun yaz. Kritik adımları logla.
```

---

## Prompt 7.2 — Alt Üye İşyeri (Sub-merchant) + Kapora Altyapısı

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.2 — İyzico Alt Üye İşyeri (sub-merchant) ve kapora ödeme altyapısı.

KONSEPT: Tablewise = Ana Üye İşyeri (Platform).
Her restoran = Alt Üye İşyeri. Kapora parası direkt restorana gider,
İyzico komisyonu keser, Tablewise araya girmez.

1. SubMerchant entity (Core/Entities/):
   - TenantId (1:1)
   - IyzicoSubMerchantKey
   - Status (Pending/Approved/Rejected)
   - SubMerchantType (Personal/PrivateCompany/LimitedOrJointStock)
   - Iban, TaxOffice, TaxNumber, LegalCompanyTitle
   - ContactName, ContactSurname, Email, Phone
   - RejectReason (nullable)
   - SubmittedAt, ApprovedAt
   - Soft delete + audit

2. SubMerchant API metotları (IyzicoPaymentService genişlet):
   - CreateSubMerchantAsync(subMerchantData) → İyzico'ya başvuru
   - UpdateSubMerchantAsync(...)
   - GetSubMerchantAsync(subMerchantKey)

3. Kapora akışı (CQRS):
   - CreateDepositPaymentCommand:
     * Reservation + amount + subMerchantKey
     * İyzico'ya marketplace ödeme isteği (paymentItems içinde subMerchantKey)
     * Ödeme başarılı → Reservation.Status = Confirmed
     * Ödeme başarısız → Reservation.Status = PaymentPending
   - DepositPayment entity:
     * ReservationId, Amount, Status
     * IyzicoPaymentId, IyzicoPaymentTransactionId
     * RefundedAmount, RefundStatus
     * Soft delete + audit

4. Kapora aktifleştirme kontrolü:
   - Restoran SubMerchant.Status != Approved ise kapora alınamaz
   - Kural motorundaki "Deposit" kuralları SubMerchant onaylı değilse skip
   - Net hata mesajı: "Kapora için İyzico başvurunuz onaylanmalı"

5. Kural motoru entegrasyonu:
   - DepositRequiredRule tetiklenince ödeme adımı zorunlu olur
   - Ödeme tamamlanmadan rezervasyon Confirmed olmaz

Tüm kodu yaz. Marketplace ödeme akışını İyzico dökümanına göre doğru kur.
```

---

## Prompt 7.3 — Webhook İşleme + İade (Refund) Akışı

**Model:** Opus 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.3 — İyzico webhook handler ve iade (refund) akışı.

1. IyzicoWebhookController (API):
   POST /api/webhooks/iyzico
   - İmza doğrulama (İyzico'nun gönderdiği signature'ı verify et) — KRİTİK
   - Doğrulanmamış istekleri reddet (401)
   - Event tiplerini handle et:
     * Abonelik ödemesi başarılı → Subscription.Status = Active, period güncelle
     * Abonelik ödemesi başarısız → Status = PastDue, restorana email
     * Abonelik iptal → Status = Cancelled
     * SubMerchant onaylandı → Status = Approved, restorana email "Kapora aktif"
     * SubMerchant reddedildi → Status = Rejected, sebep ile email
   - Idempotency: aynı webhook iki kez gelirse tek işlem
   - Tüm webhook'ları logla (audit)

2. İade akışı (CQRS — RefundDepositCommand):
   - Rezervasyon iptal edilince tetiklenir
   - İptal zamanına göre iade tutarı hesapla:
     * Restoran Settings > Kapora politikasından oku
     * Örn: 24+ saat → tam iade, 12-24 saat → %50, 12 saat altı → iade yok
   - İyzico refund API çağrısı (subMerchant'tan iade)
   - DepositPayment.RefundedAmount + RefundStatus güncelle
   - Müşteriye iade bilgilendirme emaili

3. İade hesaplama servisi (RefundCalculator):
   - Politika + iptal zamanı → iade tutarı
   - Edge case'ler: tam iade, kısmi iade, iade yok
   - Birim testleri yaz (farklı senaryolar)

4. Başarısız ödeme yönetimi:
   - Abonelik PastDue olursa: 3 gün grace period
   - Grace period sonunda hesap Suspended
   - Otomatik retry (İyzico smart retry veya kendi job'un)

Tüm kodu yaz. Webhook imza doğrulamasını ATLAMA — güvenlik kritik.
```

---

## Prompt 7.4 — Frontend Ödeme Ekranları

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.4 — Ödeme arayüzleri (admin-panel + booking-ui).

ADMIN PANEL (frontend/admin-panel):

1. Abonelik sayfası (/features/subscription/):
   - Mevcut plan kartı + kullanım (X/Y rezervasyon, X/Y mekan)
   - Plan karşılaştırma tablosu (Starter/Pro/Business)
   - "Yükselt" / "Düşür" butonları
   - İyzico kart formu (iframe veya İyzico checkout)
   - Kayıtlı kart gösterimi (son 4 hane) + kart değiştir
   - Fatura geçmişi listesi + PDF indir

2. Kapora kurulum sihirbazı (/features/settings/deposit-setup/):
   Adım adım wizard:
   - Adım 1: "Kapora nedir?" bilgilendirme
   - Adım 2: İşletme tipi seç (Şahıs/Limited/A.Ş.)
   - Adım 3: Bilgi formu (IBAN, vergi dairesi, vergi no, ünvan, iletişim)
   - Adım 4: Özet + "Başvuruyu Gönder"
   - Adım 5: "Başvurunuz alındı, 2-5 iş günü içinde aktif olacak"
   - Durum göstergesi: Bekliyor → İnceleniyor → Aktif/Reddedildi
   - Reddedilirse sebep göster + "Tekrar Başvur"

BOOKING UI (frontend/booking-ui):

3. Kapora ödeme adımı (booking wizard'a ekle):
   - Kural motoru kapora gerektiriyorsa ödeme adımı çıkar
   - Kapora tutarı net göster
   - İyzico ödeme formu (checkout veya iframe)
   - Ödeme başarılı → onay ekranı + "Rezervasyonunuz onaylandı"
   - Ödeme başarısız → tekrar dene
   - İade politikasını ödeme öncesi göster (şeffaflık)

4. React Query hooks:
   - useSubscription(), useUpgradePlan(), useInvoices()
   - useDepositSetup(), useSubmitSubMerchant()
   - useCreateDepositPayment()

Tüm kodu yaz. Ödeme hatalarında kullanıcı dostu mesajlar göster.
```

---

# FAZ 7.5 — WHATSAPP + ÖDEME KÖPRÜSÜ

> Hedef: Faz 7'deki İyzico ödeme event'lerini Faz 6.5'teki WhatsApp
> altyapısına bağlamak. Ödeme Booking UI üzerinden yapılır; WhatsApp
> sadece sonucu bildirir.
>
> Bağımlılıklar: Faz 6.5 (WhatsApp altyapısı) + Faz 7.1-7.4 (İyzico) tamamlanmış olmalı.

---

## Prompt 7.5 — WhatsApp Ödeme Bildirimlerinin Bağlanması

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 7.5 — İyzico ödeme event'lerinde WhatsApp bildirimlerinin tetiklenmesi.

BAĞLAM:
- Faz 6.5'te WhatsApp altyapısı (IMessagingChannel) kuruldu.
  Şablonlar: ReservationReceived, ReservationConfirmed, Reminder, Cancellation
- Faz 7'de İyzico webhook'ları kuruldu (ödeme başarılı, başarısız, iade vb.)
- Bu fazda sadece o webhook event'lerine WhatsApp bildirimlerini bağlıyoruz.

YAPILACAKLAR:

1. Ödeme başarılı (İyzico webhook → PaymentSuccess):
   MEVCUT: Reservation.Status = Confirmed, email gönderiliyor
   EKLE: WhatsApp ReservationConfirmed şablonu tetikle
   → "✅ Ödemeniz alındı, rezervasyonunuz onaylandı!"

2. Ödeme başarısız (İyzico webhook → PaymentFailed):
   MEVCUT: Reservation.Status = PaymentPending kalıyor, email gönderiliyor
   YENİ WhatsApp şablonu ekle: PaymentFailed
   → "{{mekan}} rezervasyonu için ödeme alınamadı.
      Tekrar denemek için rezervasyon sayfanızı ziyaret edin: {{bookingLink}}"
   → Hem WhatsApp hem email gönder

3. İade gerçekleşti (RefundDepositCommand tamamlanınca):
   MEVCUT: Email gönderiliyor
   EKLE: WhatsApp Cancellation şablonuna iade bilgisini ekle
   → İade varsa: "{{tutar}} TL iade 3-5 iş günü içinde kartınıza yansır."
   → İade yoksa: "Kapora politikası gereği iade yapılamamaktadır."

4. SubMerchant onaylandı (İyzico webhook):
   MEVCUT: Restorana email gönderiliyor
   EKLE: Restoranın kayıtlı telefonu varsa WhatsApp bildirimi
   → "✅ İyzico başvurunuz onaylandı! Kapora özelliği artık aktif."

5. Yeni MessageTemplate enum değerleri (Faz 6.5'teki enum'a ekle):
   - PaymentFailed

6. WhatsApp hatası hiçbir zaman ödeme akışını bloklamasın:
   - Try/catch ile sardır
   - Hata logla, email fallback'i zaten var
   - WhatsApp başarısız olsa bile İyzico işlemi tamamlanmış sayılır

Değişiklikleri mevcut webhook handler'larına ve command handler'larına
minimal diff olarak ekle. Yeni dosya açma, mevcut yapıya entegre et.
```

---

# FAZ 8.5 — SUPER ADMIN PANEL

> Hedef: Tablewise ekibinin (sen + pazarlamacı + finansçı) tüm platformu yönettiği panel.
> Yeni frontend projesi: `frontend/super-admin/`
> Faz 8 (CRM altyapısı) bittikten sonra yapılmalı.

---

## Prompt 8.5.1 — Platform Backend API + Rol Sistemi

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 8.5.1 — Super Admin (Platform) backend altyapısı.

ÖNEMLİ: Bu, restoranların kullandığı admin panelden TAMAMEN ayrı bir
yetki düzlemi. Platform çalışanları (Tablewise ekibi) tüm tenant'ları görür.

1. PlatformUser entity (Core/Entities/):
   - Email, PasswordHash, FullName
   - PlatformRole enum: SuperAdmin, Marketing, Finance
   - IsActive, LastLoginAt
   - Soft delete + audit

2. Platform auth:
   - Tenant auth'tan AYRI JWT (farklı audience/issuer claim)
   - PlatformUser login endpoint: /api/platform/auth/login
   - PlatformAuthorize attribute — sadece PlatformUser erişebilir
   - Rol bazlı yetki: [PlatformAuthorize(PlatformRole.Finance)]

3. Rol erişim matrisi (policy olarak tanımla):
   | Modül              | SuperAdmin | Marketing | Finance |
   |--------------------|-----------|-----------|---------|
   | Dashboard          | ✅        | ✅        | ✅      |
   | Müşteri görüntüle  | ✅        | ✅        | ✅      |
   | Plan değiştir      | ✅        | ❌        | ✅      |
   | Fiyatlandırma      | ✅        | ❌        | ✅      |
   | İskonto/Kupon      | ✅        | ✅        | ✅      |
   | Ödeme takibi       | ✅        | ❌        | ✅      |
   | Cihaz yönetimi     | ✅        | ❌        | ❌      |
   | Ekip yönetimi      | ✅        | ❌        | ❌      |

4. Platform controller'ları:
   GET  /api/platform/stats              → dashboard metrikleri
   GET  /api/platform/tenants            → tüm tenant listesi (filtre+arama+sayfalama)
   GET  /api/platform/tenants/{id}       → tenant detay
   PUT  /api/platform/tenants/{id}/plan  → plan değiştir
   PUT  /api/platform/tenants/{id}/suspend
   POST /api/platform/tenants/{id}/notes → iç not ekle
   GET  /api/platform/subscriptions      → ödeme/abonelik durumları
   GET  /api/platform/submerchants       → İyzico başvuru durumları

5. Dashboard stats hesaplama:
   - Toplam tenant, aktif/trial/suspended dağılımı
   - MRR (aylık tekrarlayan gelir)
   - Bu ay yeni kayıt / churn
   - Plan dağılımı

6. Audit: tüm platform aksiyonları loglanır (kim, ne zaman, ne yaptı).

GÜVENLİK: Platform endpoint'leri ASLA tenant token ile erişilemez olmalı.
Tüm kodu yaz.
```

---

## Prompt 8.5.2 — Super Admin Frontend İskelet + Dashboard

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 8.5.2 — Super Admin Panel frontend iskeleti ve dashboard.

Yeni proje: frontend/super-admin/
Stack: React 18 + Vite + TailwindCSS + shadcn/ui + Zustand + React Query.
Admin-panel ile aynı stack ama AYRI proje. Görsel olarak farklı tema
(daha koyu/kurumsal — bu bir iç araç).

1. Proje kurulumu + Router:
   - /login (platform login)
   - /dashboard, /tenants, /tenants/:id, /pricing, /coupons,
     /payments, /devices, /team
   - ProtectedRoute (platform token) + rol bazlı route guard

2. AppLayout:
   - Sidebar: Dashboard, Müşteriler, Fiyatlandırma, Kuponlar,
     Ödemeler, Cihazlar, Ekip
   - Rol'e göre menü öğeleri gizlenir (Finance cihazları görmez vb.)
   - Top bar: kullanıcı + rol badge + çıkış

3. Dashboard sayfası:
   - Üst: 4-6 stat kartı (toplam müşteri, MRR, aktif/trial,
     bu ay yeni, churn, aktif cihaz)
   - Grafik: aylık gelir trendi (Recharts)
   - Grafik: plan dağılımı (pie/donut)
   - Liste: son kayıtlar + dikkat gerektirenler (gecikmiş ödeme,
     bekleyen İyzico başvurusu)

4. React Query hooks:
   - usePlatformStats(), usePlatformAuth()

5. .env.example:
   VITE_PLATFORM_API_URL=http://localhost:5000

Tüm kodu yaz. Responsive olsun.
```

---

## Prompt 8.5.3 — Müşteri Yönetimi + Fiyatlandırma + İskonto

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 8.5.3 — Müşteri yönetimi, fiyatlandırma ve iskonto modülleri.

1. TenantsPage (/tenants):
   - Tablo: restoran adı, plan, durum, kayıt tarihi, MRR katkısı,
     son aktivite
   - Filtre: plan, durum (aktif/trial/suspended)
   - Arama: isim/email
   - Satıra tıkla → detay

2. TenantDetailPage (/tenants/:id):
   - Üst: temel bilgiler (iletişim, plan, durum)
   - Sekmeler:
     * Genel: mekan sayısı, kural sayısı, aylık rezervasyon
     * Abonelik: mevcut plan, ödeme geçmişi
     * Aksiyonlar: plan değiştir, askıya al, iskonto uygula
     * Notlar: iç notlar (kim ekledi, ne zaman)
   - Plan değiştir modal (SuperAdmin + Finance)
   - Askıya al/aktif et toggle

3. PricingPage (/pricing) — SuperAdmin + Finance:
   - Plan fiyatları düzenlenebilir (DB'den, hardcode DEĞİL)
   - Starter/Pro/Business fiyat + özellik limitleri
   - "Değişiklik ne zaman geçerli?" → anında / sonraki dönem
   - Geçmiş fiyat değişiklikleri logu

   BACKEND NOT: Plan fiyatları PricingPlan entity'sinden gelmeli.
   Eğer Faz 7'de hardcode edildiyse, burada DB'ye taşı (migration).

4. İskonto modülü:
   a) Müşteri bazlı (TenantDetail içinde):
      - Tip: yüzde/sabit tutar
      - Süre: X ay / süresiz
      - Sebep (iç not)
   b) CouponsPage (/coupons) — SuperAdmin + Marketing:
      - Kupon listesi: kod, indirim, kullanım/limit, durum
      - Yeni kupon: kod, indirim tipi+miktarı, kullanım limiti,
        geçerlilik süresi, hangi planlara uygulanır
      - Kupon devre dışı bırak

5. Backend (önceki prompt'tan eksikse ekle):
   - Discount entity (tenant bazlı)
   - Coupon entity + CouponRedemption
   - PricingPlan entity

Tüm kodu yaz. Rol bazlı buton/sayfa gizleme uygula.
```

---

## Prompt 8.5.4 — Ödeme Takibi + Cihaz Yönetimi + Ekip

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 8.5.4 — Ödeme takibi, cihaz yönetimi ve ekip yönetimi.

1. PaymentsPage (/payments) — SuperAdmin + Finance:
   - Sekmeler: Abonelikler | İyzico Başvuruları
   - Abonelikler:
     * Tablo: restoran, plan, durum (aktif/gecikmiş/iptal),
       son ödeme, sonraki ödeme
     * Filtre: gecikmiş ödemeler
     * Detay: ödeme geçmişi
   - İyzico Başvuruları (SubMerchant):
     * Bekleyen/onaylanan/reddedilen başvurular
     * Başvuru detayı (IBAN, vergi no vb.)
     * Manuel durum güncelleme (İyzico panelinden takip notu)

2. DevicesPage (/devices) — SuperAdmin only:
   - Tüm cihazlar tablosu: deviceId, hangi tenant, hangi masa,
     online/offline, batarya, son görülme, firmware versiyonu
   - Filtre: tenant, durum (online/offline), düşük batarya
   - Cihaz detay: bağlantı geçmişi, atama geçmişi
   - OTA firmware güncelleme:
     * Firmware versiyonu yükle/seç
     * Hedef seç (tüm cihazlar / belirli tenant / seçili cihazlar)
     * "Güncellemeyi Başlat" → MQTT push
     * Güncelleme durumu takibi (başarılı/başarısız/devam ediyor)
   - Kira takibi: hangi tenant kaç cihaz, kira durumu

   BACKEND NOT: Device entity Faz 13'te oluşturulacak. Eğer Faz 13
   henüz yapılmadıysa, bu sayfayı feature flag arkasına al veya
   placeholder olarak bırak. Cihaz altyapısı Faz 13'e bağımlı.

3. TeamPage (/team) — SuperAdmin only:
   - Platform kullanıcıları listesi: isim, email, rol, son giriş
   - Yeni üye davet: email + rol (SuperAdmin/Marketing/Finance)
   - Rol değiştir, devre dışı bırak
   - Son SuperAdmin silinemez güvenliği

4. React Query hooks:
   - usePayments(), useSubMerchantApplications()
   - useDevices(), useFirmwareUpdate()
   - usePlatformTeam(), useInvitePlatformUser()

Tüm kodu yaz. Rol bazlı erişimi her sayfada uygula.
```

---

# FAZ 14 — MUHASEBE & E-FATURA

> Hedef: Otomatik fatura üretimi ve e-Fatura/muhasebe yazılımı entegrasyonu.
> Öncelik: Düşük (ileride). İlk müşterilerde manuel veya basit CSV yeterli.
> Parasut veya Logo entegrasyonu hedeflenir.

---

## Prompt 14.1 — Fatura Altyapısı

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 14.1 — Fatura üretimi ve yönetimi altyapısı.

1. Invoice entity (Core/Entities/):
   - TenantId, InvoiceNumber (sıralı, formatlı: TW-2026-00001)
   - PlanType, Amount, TaxAmount, TotalAmount
   - DiscountApplied (varsa)
   - PeriodStart, PeriodEnd
   - Status: Draft/Issued/Paid/Cancelled
   - IssuedAt, PaidAt
   - Soft delete + audit

2. Fatura üretimi (CQRS):
   - Abonelik ödemesi başarılı olunca otomatik fatura oluştur
   - Fatura numarası sıralı ve benzersiz (race condition'a dikkat)
   - KDV hesaplama (%20 veya güncel oran, config'ten)
   - İskonto fatura üzerinde gösterilir

3. PDF fatura üretimi:
   - Profesyonel fatura şablonu (Tablewise logo, kurumsal)
   - Müşteri bilgileri, plan, dönem, tutar, KDV, toplam
   - QuestPDF veya benzeri .NET kütüphanesi
   - R2/MinIO'ya kaydet, indirme linki üret

4. Endpoint'ler:
   - GET /api/v1/invoices (tenant kendi faturaları)
   - GET /api/v1/invoices/{id}/pdf
   - GET /api/platform/invoices (super admin — tüm faturalar)

5. Frontend:
   - Admin panel: abonelik sayfasında fatura geçmişi + PDF indir
   - Super admin: tüm faturalar, filtreleme, toplu export

Tüm kodu yaz.
```

---

## Prompt 14.2 — e-Fatura / Muhasebe Entegrasyonu

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 14.2 — e-Fatura ve muhasebe yazılımı entegrasyonu.

NOT: Entegrasyon hedefi olarak Parasut API (en yaygın, iyi dökümante).
Logo veya e-Fatura özel entegratör API'leri alternatif.

1. IAccountingService interface (Core/Interfaces/):
   - CreateInvoiceAsync(invoice) → harici sistemde fatura oluştur
   - SendEInvoiceAsync(invoiceId) → e-fatura/e-arşiv gönder
   - GetInvoiceStatusAsync(externalId)

2. ParasutAccountingService implementasyonu:
   - OAuth2 auth flow (Parasut API)
   - Müşteri (contact) oluştur/eşleştir
   - Satış faturası oluştur
   - e-Fatura/e-Arşiv olarak resmileştir
   - Hata yönetimi + retry

3. Entegrasyon ayarları (Super Admin):
   - AccountingSettingsPage (/settings/accounting):
     * Sağlayıcı seç (Parasut/Logo/Manuel)
     * API kimlik bilgileri (şifreli sakla)
     * Otomatik e-fatura gönderimi toggle
     * Test bağlantısı butonu

4. Otomatik akış:
   - Invoice "Paid" olunca → muhasebe sistemine gönder (job)
   - Başarısız olursa → super admin'e bildirim + manuel retry
   - Senkronizasyon durumu Invoice'ta tutulur (SyncStatus)

5. Gelir raporu export:
   - Aylık gelir/KDV raporu CSV/Excel export
   - Muhasebeci için hazır format

Tüm kodu yaz. API kimlik bilgilerini ASLA loglama veya plain text saklama.
```

---

## Genel Hatırlatmalar

1. **Her prompttan önce genel bağlam bloğunu yapıştır.**
2. **Faz 7 ödeme = Opus.** Para işleri kritik, ucuz modelle riske girme.
3. **Faz 7.5 = Sonnet.** Sadece bildirim bağlantısı, yeni mimari yok.
4. **Webhook imza doğrulamasını asla atlama** (İyzico + WhatsApp/Twilio).
5. **Kart bilgisi asla DB'de saklanmaz** — sadece İyzico token'ları.
6. **Super Admin token'ı tenant token'ından tamamen ayrı olmalı.**
7. **Faz 8.5.4 cihaz kısmı Faz 13'e bağımlı** — sıralamaya dikkat.
8. **Faz 7.5, Faz 6.5 + Faz 7.1-7.4 bitmeden yapılmaz.**
9. **WhatsApp sadece bildirim** — ödeme asla WhatsApp üzerinden değil.
10. Bir fazı bitirmeden diğerine geçme; demo baskısıyla atlama (bu sefer öyle olmasın 🙂).
