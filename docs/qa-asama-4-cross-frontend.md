# QA Aşama 4 — Cross-Frontend Integration Testleri

## Bağlam

Tablewise'da 4 frontend birbirine bağlı çalışıyor:

| Frontend | Port | URL |
|----------|------|-----|
| Admin Panel | 3000 | http://localhost:3000 |
| Booking UI | 5174 | http://localhost:5174 |
| Super Admin | 3001 | http://localhost:3001 |
| Landing | (Faz 11, henüz yok) | http://localhost:4000 |

Mevcut testlerin tamamı tek bir frontend'i izole test ediyor.
Bu aşamada **iki veya daha fazla frontend'in birlikte çalışmasını** doğrulayan testler yazılacak.

## Teknik Yapı

Playwright `browser.newContext()` ile tek test içinde birden fazla baseURL kullanılabilir:

```typescript
import { test, expect, Browser } from '@playwright/test'
import { SEED } from '../fixtures/seed'

test('cross-frontend örnek', async ({ browser }) => {
  // Frontend 1
  const ctx1 = await browser.newContext({ baseURL: 'http://localhost:5174' })
  const page1 = await ctx1.newPage()

  // Frontend 2
  const ctx2 = await browser.newContext({ baseURL: 'http://localhost:3000' })
  const page2 = await ctx2.newPage()

  // ... testler ...

  await ctx1.close()
  await ctx2.close()
})
```

## Görev

`e2e/` kök dizinine `cross-frontend/` klasörü oluştur ve içine aşağıdaki **3 dosyayı** ekle.

---

## Dosya 1: `booking-to-admin.spec.ts`

**Booking UI'da yapılan rezervasyon Admin Panel'e yansıyor mu?**

### Test 1: Yeni rezervasyon admin listesinde görünüyor

```
Akış:
1. Booking UI (5174) → rezervasyon oluştur (happy path)
2. Rezervasyon kodu veya müşteri adını kaydet
3. Admin Panel (3000) → giriş yap (SEED.admin credentials)
4. /reservations sayfasında az önce oluşturulan rezervasyonu ara
5. Rezervasyon listede görünüyor olmalı
```

Teknik not: Rezervasyon oluşturduktan sonra confirmation sayfasından rezervasyon adını/kodunu al (page1). Sonra admin panel'de (page2) bu değeri arama veya listede görünüp görünmediğini kontrol et.

### Test 2: Admin onayladığında booking UI'da durum güncelleniyor

```
Akış:
1. Booking UI (5174) → rezervasyon oluştur → confirmation URL'ini kaydet
2. Admin Panel (3000) → giriş yap → rezervasyonu bul → "Onayla" butonuna tıkla
3. Booking UI (5174) → confirmation sayfasına geri dön (kayıtlı URL)
4. Sayfa yenilenince durum "Onaylandı" veya "Confirmed" olarak gösteriyor olmalı
```

### Test 3: Admin iptal ettiğinde booking UI'da durum güncelleniyor

```
Akış:
1. Booking UI (5174) → rezervasyon oluştur
2. Admin Panel (3000) → rezervasyonu bul → "İptal Et" butonuna tıkla
3. Booking UI (5174) → aynı rezervasyonun görüntüleme sayfasına git
4. "İptal edildi" veya "Cancelled" durumu görünüyor olmalı
```

---

## Dosya 2: `admin-rule-to-booking.spec.ts`

**Admin Panel'de oluşturulan kural Booking UI'da uygulanıyor mu?**

### Test 1: Admin yeni kural ekler → Booking UI kural ihlalini yakalar

```
Akış:
1. Admin Panel (3000) → giriş yap → /rules sayfası
2. "Yeni Kural" → örneğin minimum kişi sayısı = 10 olan bir kural ekle (imkansız koşul)
   (Bu kural seed tenant'ına eklenmiş olacak)
3. Booking UI (5174) → normal rezervasyon akışı başlat
4. Kişi sayısı girilince veya rezervasyon tamamlanmaya çalışılınca
   kural ihlali mesajı görünmeli
5. Admin Panel (3000) → kuralı sil veya pasif yap
6. Booking UI (5174) → yeniden dene → bu sefer hata çıkmamalı
```

Teknik not: Kural oluşturma adımı admin panel'deki form yapısına göre uyarlanmalı. Kural tipi ve koşul seçimi için `page.locator('[role="combobox"]')` pattern'ı kullan. Kural kaydedildikten sonra `page.waitForResponse` ile API yanıtını bekle.

### Test 2: Admin çalışma saatlerini değiştirince Booking UI'da slotlar değişiyor

```
Akış:
1. Admin Panel (3000) → çalışma saatlerini güncelle
   (örn. Pazartesi'yi tamamen kapat → IsOpen = false)
2. Booking UI (5174) → tarih seçiminde önümüzdeki Pazartesi'yi seç
3. O gün için slot görünmemeli veya "kapalı" mesajı gösterilmeli
4. Admin Panel (3000) → Pazartesi'yi tekrar aç
5. Booking UI (5174) → slot listesi geri gelmiş olmalı
```

---

## Dosya 3: `super-admin-to-tenant.spec.ts`

**Super Admin'in tenant üzerindeki değişiklikleri diğer frontend'lere yansıyor mu?**

### Test 1: Super Admin tenant'ı pasif yapınca admin girişi engelleniyor

```
Akış:
1. Super Admin (3001) → giriş yap (SEED.superAdmin)
2. Tenant listesinden SEED.tenant'ı bul → "Pasif yap" veya status toggle
3. Admin Panel (3000) → SEED.admin credentials ile giriş yapmayı dene
4. Giriş başarısız olmalı ("hesap askıya alındı" veya 403 mesajı)
5. Super Admin (3001) → tenant'ı tekrar aktif yap
6. Admin Panel (3000) → giriş tekrar çalışıyor olmalı
```

### Test 2: Super Admin plan değiştirince booking UI özellikleri etkileniyor

```
Akış:
1. Super Admin (3001) → SEED.tenant'ın planını Starter'a indir
   (Starter'da bazı özellikler kısıtlıysa)
2. Booking UI (5174) → kısıtlı özelliğe erişmeyi dene
   (örn. plan bazlı özellik varsa feature flag kontrol et)
3. Super Admin (3001) → planı Pro'ya yükselt
4. Booking UI (5174) → özellik tekrar erişilebilir olmalı
```

Teknik not: Eğer plan bazlı özellik kısıtlaması Booking UI'da henüz implement edilmediyse bu test `test.skip()` ile işaretle ve yorum ekle: "// TODO: Plan bazlı özellik kısıtlaması Booking UI'a eklenince aktif et"

### Test 3: Super Admin kupon oluşturur → Booking UI'da uygulanıyor

```
Akış:
1. Super Admin (3001) → pricing/coupons sayfası → yeni kupon oluştur
   (kod: "E2ETEST", indirim: %10, geçerlilik: 30 gün)
2. Booking UI (5174) → rezervasyon akışında kupon kodu alanı varsa "E2ETEST" gir
3. İndirim uygulanmalı veya "kupon geçerli" mesajı görünmeli
4. Super Admin (3001) → kuponu pasif yap
5. Booking UI (5174) → aynı kodu tekrar dene → "geçersiz kupon" hatası gelmeli
```

Teknik not: Kupon kodu alanı booking UI'da henüz yoksa bu test de `test.skip()` ile işaretle.

---

## Genel Teknik Kurallar

### Context yönetimi
```typescript
// Her test sonunda context'leri kapat
test.afterEach(async ({ browser }) => {
  // context değişkenleri test scope'unda olduğu için
  // try/catch ile kapat
})

// Ya da test içinde finally kullan:
try {
  // test adımları
} finally {
  await ctx1.close()
  await ctx2.close()
}
```

### Veri aktarımı (frontend'ler arası)
```typescript
// Bir frontend'den diğerine ID/kod taşımak için değişken kullan:
let reservationCode: string

// Page 1'de oluştur:
reservationCode = await page1.locator('[data-reservation-code]').textContent() ?? ''

// Page 2'de kullan:
await page2.getByPlaceholder(/ara|search/i).fill(reservationCode)
```

### Bağımsız test verisi
- Her cross-frontend test kendi verisini oluşturmalı ve temizlemeli
- Seed verilerini değiştirme (kuralları sil/ekle gibi işlemler sonradan geri alınmalı)
- Test sonunda oluşturulan kurallar, kuponlar, rezervasyonlar temizlenmeli (`afterEach` içinde)

### Timeout'lar
- Cross-frontend testler ağ gecikmesi nedeniyle yavaş olabilir
- Genel timeout: `{ timeout: 20_000 }`
- Durum güncellemelerini beklerken `page.waitForResponse()` kullan

### Skip stratejisi
- Henüz implement edilmemiş özellikler için `test.skip()` kullan
- Yorum ekle: neden skip edildiği ve hangi faz tamamlanınca aktif edileceği

## Çalıştırma

```bash
# Sadece cross-frontend testleri
cd e2e
npx playwright test cross-frontend/ --reporter=list

# Tüm E2E suite (tüm aşamalar tamamlandıktan sonra)
npx playwright test --reporter=list
```

### Ön koşul
Cross-frontend testler çalışmadan önce tüm frontend'ler ayakta olmalı:
```bash
# Backend
cd src/Tablewise.Api && dotnet run

# Admin Panel
cd frontend/admin-panel && npm run dev

# Booking UI
cd frontend/booking-ui && npm run dev

# Super Admin
cd frontend/super-admin && npm run dev
```

## Beklenen Sonuç

Aşama 4 tamamlandığında:
- `e2e/cross-frontend/` → 3 dosya, ~8 test
- Skip edilenler dahil toplam senaryo sayısı: **~115 test**
- Gerçek uçtan uca (end-to-end) kapsam sağlanmış olur

## Önemli Not

Bu testler diğer aşamalara göre daha kırılgan (flaky) olabilir çünkü:
- Birden fazla sunucu çalışıyor olmalı
- Ağ gecikmesi var
- State'ler arası bağımlılık var

Bu nedenle CI/CD'de bu testleri ayrı bir job olarak çalıştır:
```yaml
# GitHub Actions örneği
- name: Cross-Frontend Tests
  run: npx playwright test cross-frontend/
  continue-on-error: false  # kritik — fail etmemeli
```

---

## GÜNCELLEME — Özel Limitler Cross-Frontend Testi

`super-admin-to-tenant.spec.ts` dosyasına aşağıdaki **4. testi** ekle:

### Test 4: Super Admin custom limit atar → Admin Panel dashboard'da yansıyor

```typescript
test('Super Admin özel limit atar → Admin Panel dashboard güncelleniyor', async ({ browser }) => {
  // Context 1: Super Admin (port 3001)
  const superCtx = await browser.newContext({ baseURL: 'http://localhost:3001' })
  const superPage = await superCtx.newPage()

  // Context 2: Admin Panel (port 3000)
  const adminCtx = await browser.newContext({ baseURL: 'http://localhost:3000' })
  const adminPage = await adminCtx.newPage()

  try {
    // 1. Super Admin → giriş yap
    await superPage.goto('/auth/login')
    await superPage.getByLabel(/email/i).fill(SEED.superAdmin.email)
    await superPage.getByLabel(/şifre|password/i).fill(SEED.superAdmin.password)
    await superPage.getByRole('button', { name: /giriş/i }).click()
    await superPage.waitForURL(/dashboard|tenants/, { timeout: 10_000 })

    // 2. Super Admin → Seed tenant'ın detay sayfasına git
    await superPage.goto(`/tenants`)
    await superPage.getByText(SEED.tenant.name).click()
    await superPage.waitForURL(/tenants\//, { timeout: 10_000 })

    // 3. Özel limit gir: maxVenues = 99
    const venueInput = superPage.locator('input[placeholder*="Plan"]').first()
    await venueInput.fill('99')
    await superPage.getByRole('button', { name: /limitleri kaydet/i }).click()
    await superPage.waitForResponse(
      resp => resp.url().includes('custom-limits') && resp.status() === 204,
      { timeout: 10_000 }
    )

    // 4. Admin Panel → giriş yap
    await adminPage.goto('/auth/login')
    await adminPage.getByLabel(/email/i).fill(SEED.admin.email)
    await adminPage.getByLabel(/şifre|password/i).fill(SEED.admin.password)
    await adminPage.getByRole('button', { name: /giriş/i }).click()
    await adminPage.waitForURL(/dashboard/, { timeout: 10_000 })

    // 5. Dashboard'da plan kullanım widget'ında "99" görünüyor
    await expect(
      adminPage.getByText(/99/, { exact: false })
    ).toBeVisible({ timeout: 10_000 })

    // 6. "Özel limitler uygulanıyor" badge'i görünüyor
    await expect(
      adminPage.getByText(/özel limit/i)
    ).toBeVisible({ timeout: 10_000 })

    // 7. Temizlik: Super Admin → limiti sıfırla
    await superPage.getByRole('button', { name: /tüm limitleri sıfırla/i }).click()
    await superPage.waitForResponse(
      resp => resp.url().includes('custom-limits') && resp.status() === 204,
      { timeout: 10_000 }
    )

  } finally {
    await superCtx.close()
    await adminCtx.close()
  }
})
```

---

## GÜNCELLEME — Landing Page Cross-Frontend Testi

`e2e/cross-frontend/` klasörüne `landing-to-admin.spec.ts` dosyasını ekle:

### Dosya: `landing-to-admin.spec.ts`

```typescript
import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

test.describe('Cross-Frontend — Landing → Admin Panel', () => {

  test('Landing "Giriş Yap" butonu Admin Panel login sayfasına yönlendiriyor', async ({ browser }) => {
    const landingCtx = await browser.newContext({ baseURL: 'http://localhost:4000' })
    const adminCtx   = await browser.newContext({ baseURL: 'http://localhost:3000' })
    const landingPage = await landingCtx.newPage()

    try {
      await landingPage.goto('/')
      await landingPage.waitForLoadState('networkidle')

      // "Giriş Yap" butonuna tıkla
      const [newPage] = await Promise.all([
        landingCtx.waitForEvent('page').catch(() => null),
        landingPage.getByRole('link', { name: /giriş yap/i }).first().click(),
      ])

      const targetPage = newPage ?? landingPage
      await targetPage.waitForLoadState()
      expect(targetPage.url()).toContain('localhost:3000')

    } finally {
      await landingCtx.close()
      await adminCtx.close()
    }
  })

  test('Super Admin fiyat günceller → Landing sayfasında yansıyor', async ({ browser }) => {
    // Bu test API mock ile çalışır — gerçek Super Admin akışı değil
    const landingCtx = await browser.newContext({ baseURL: 'http://localhost:4000' })
    const landingPage = await landingCtx.newPage()

    try {
      // Mock: güncellenmiş fiyat
      const MOCK_PRICE = 9876

      await landingPage.route('**/api/public/pricing', route => {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            { id: '1', name: 'Starter', tier: 'Starter',
              monthlyPriceTry: MOCK_PRICE, yearlyPriceTry: 7900,
              isVisible: true, limitsJson: '{}', featuresJson: '{}' },
            { id: '2', name: 'Pro', tier: 'Pro',
              monthlyPriceTry: 2990, yearlyPriceTry: 2392,
              isVisible: true, limitsJson: '{}', featuresJson: '{}' },
            { id: '3', name: 'Enterprise', tier: 'Enterprise',
              monthlyPriceTry: 0, yearlyPriceTry: 0,
              isVisible: true, limitsJson: '{}', featuresJson: '{}' },
          ]),
        })
      })

      await landingPage.goto('/')
      await landingPage.locator('#pricing').scrollIntoViewIfNeeded()

      // Mock fiyat görünmeli
      await expect(
        landingPage.locator('#pricing').getByText(new RegExp(String(MOCK_PRICE)))
      ).toBeVisible({ timeout: 10_000 })

    } finally {
      await landingCtx.close()
    }
  })

})
```

Aşama 4 + güncellemeler tamamlandığında cross-frontend test sayısı **~8 → ~14** olmalı.
