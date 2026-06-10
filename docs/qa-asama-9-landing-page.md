# QA Aşama 9 — Landing Page E2E Testleri

## Bağlam

Landing page Faz 11 kapsamında tamamlandı.
`frontend/landing/` — port 4000, React 18 + Vite + TailwindCSS.

Sayfa yapısı (yukarıdan aşağıya):
- Navbar (sticky, Giriş Yap / Ücretsiz Başla butonları)
- Hero (CTA butonları, dashboard illüstrasyonu)
- SocialProof
- ProblemSolution
- Features (id="features")
- HowItWorks
- Pricing (id="pricing", API'den dinamik fiyat + toggle)
- Testimonials
- CtaBanner
- Footer

Bağlantılar:
- "Giriş Yap" → `http://localhost:3000` (Admin Panel)
- "Ücretsiz Başla" / "Kayıt Ol" → `http://localhost:3000/register`
- Pricing API → `GET /api/public/pricing` (backend port 5086)

## Görev

`e2e/landing/` klasörü oluştur ve içine aşağıdaki **4 spec dosyasını** ekle.

Playwright config'de landing için proje tanımlı değilse `playwright.config.ts`'e ekle:
```typescript
{
  name: 'landing',
  use: { baseURL: 'http://localhost:4000' },
  testMatch: '**/landing/**/*.spec.ts',
}
```

---

## Dosya 1: `page-load.spec.ts`

Sayfanın sorunsuz yüklendiğini doğrular.

```typescript
import { test, expect } from '@playwright/test'

test.describe('Landing Page — Sayfa Yükleme', () => {

  test('ana sayfa yükleniyor ve temel elementler görünüyor', async ({ page }) => {
    await page.goto('/')
    await expect(page).toHaveTitle(/Tablewise/i)
    // Navbar logo
    await expect(page.getByText(/tablewise/i).first()).toBeVisible({ timeout: 10_000 })
    // Hero başlık
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible()
    // Footer
    await expect(page.locator('footer')).toBeVisible()
  })

  test('console hatası yok', async ({ page }) => {
    const errors: string[] = []
    page.on('pageerror', err => errors.push(err.message))
    page.on('console', msg => {
      if (msg.type() === 'error') errors.push(msg.text())
    })
    await page.goto('/')
    await page.waitForLoadState('networkidle', { timeout: 15_000 })
    expect(errors).toHaveLength(0)
  })

  test('tüm section\'lar render edilmiş', async ({ page }) => {
    await page.goto('/')
    await page.waitForLoadState('networkidle')
    // Features section
    await expect(page.locator('#features')).toBeVisible()
    // Pricing section
    await expect(page.locator('#pricing')).toBeVisible()
    // Footer
    await expect(page.locator('footer')).toBeVisible()
  })

  test('sayfa 404 vermeden yükleniyor', async ({ page }) => {
    const response = await page.goto('/')
    expect(response?.status()).toBeLessThan(400)
  })

})
```

---

## Dosya 2: `navigation.spec.ts`

Navbar linkleri ve CTA butonlarının yönlendirme testleri.

```typescript
import { test, expect } from '@playwright/test'

const ADMIN_URL = 'http://localhost:3000'

test.describe('Landing Page — Navigasyon', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    await page.waitForLoadState('networkidle')
  })

  test('"Özellikler" linki ilgili section\'a scroll yapıyor', async ({ page }) => {
    await page.getByRole('link', { name: /özellikler/i }).click()
    // #features section viewport'a girmiş olmalı
    await expect(page.locator('#features')).toBeInViewport({ timeout: 5_000 })
  })

  test('"Fiyatlandırma" linki pricing section\'a scroll yapıyor', async ({ page }) => {
    await page.getByRole('link', { name: /fiyatlandırma/i }).click()
    await expect(page.locator('#pricing')).toBeInViewport({ timeout: 5_000 })
  })

  test('"Giriş Yap" butonu admin panel\'e yönlendiriyor', async ({ page, context }) => {
    const [newPage] = await Promise.all([
      context.waitForEvent('page'),
      page.getByRole('link', { name: /giriş yap/i }).first().click(),
    ])
    // Yeni sekme açılıyorsa:
    await newPage.waitForLoadState()
    expect(newPage.url()).toContain('localhost:3000')
    await newPage.close()
  })

  test('"Ücretsiz Başla" butonu admin panel register\'a yönlendiriyor', async ({ page, context }) => {
    const [newPage] = await Promise.all([
      context.waitForEvent('page').catch(() => null),
      page.getByRole('link', { name: /ücretsiz başla/i }).first().click(),
    ])
    if (newPage) {
      await newPage.waitForLoadState()
      expect(newPage.url()).toContain('localhost:3000')
      await newPage.close()
    } else {
      // Aynı sekmede açılıyorsa
      expect(page.url()).toContain('localhost:3000')
    }
  })

  test('navbar scroll\'da sticky kalıyor', async ({ page }) => {
    // Sayfayı aşağı kaydır
    await page.evaluate(() => window.scrollTo(0, 500))
    await page.waitForTimeout(300)
    // Navbar hâlâ görünür
    await expect(page.locator('nav').first()).toBeVisible()
  })

  test('footer\'da "Giriş Yap" ve "Kayıt Ol" linkleri var', async ({ page }) => {
    const footer = page.locator('footer')
    await expect(footer.getByRole('link', { name: /giriş yap/i })).toBeVisible()
    await expect(footer.getByRole('link', { name: /kayıt ol/i })).toBeVisible()
  })

})
```

---

## Dosya 3: `pricing-section.spec.ts`

Pricing section'ın API entegrasyonu ve toggle testleri.

```typescript
import { test, expect } from '@playwright/test'

test.describe('Landing Page — Fiyatlandırma', () => {

  test('pricing section yükleniyor', async ({ page }) => {
    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()
    await expect(page.locator('#pricing')).toBeVisible()
  })

  test('plan kartları render edilmiş (API veya fallback)', async ({ page }) => {
    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()
    // En az 3 plan kartı görünmeli (Starter, Pro, Enterprise)
    await expect(page.locator('#pricing').getByText(/starter/i)).toBeVisible({ timeout: 10_000 })
    await expect(page.locator('#pricing').getByText(/pro/i)).toBeVisible()
    await expect(page.locator('#pricing').getByText(/enterprise/i)).toBeVisible()
  })

  test('fiyatlar görünüyor (₺ veya "Fiyat teklifi")', async ({ page }) => {
    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()
    // Starter ve Pro fiyatları ₺ ile gösterilmeli
    const pricingSection = page.locator('#pricing')
    const hasPrice = await pricingSection.getByText(/₺/).count()
    expect(hasPrice).toBeGreaterThanOrEqual(2)  // en az Starter + Pro
    // Enterprise "Fiyat teklifi" göstermeli
    await expect(pricingSection.getByText(/fiyat teklifi|özel teklif/i)).toBeVisible()
  })

  test('aylık/yıllık toggle çalışıyor', async ({ page }) => {
    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()

    // Mevcut fiyatı kaydet (Starter aylık)
    const pricingSection = page.locator('#pricing')
    const monthlyText = await pricingSection.getByText(/₺/).first().textContent()

    // Toggle'a tıkla (Yıllık'a geç)
    const toggle = pricingSection.getByRole('button', { name: /yıllık/i })
      .or(pricingSection.getByText(/yıllık/i).locator('..'))
    await toggle.click()

    await page.waitForTimeout(300)
    // Fiyat değişmiş olmalı
    const yearlyText = await pricingSection.getByText(/₺/).first().textContent()
    expect(yearlyText).not.toBe(monthlyText)
  })

  test('API kapalıyken fallback fiyatlar gösteriliyor', async ({ page }) => {
    // API route'u engelle
    await page.route('**/api/public/pricing', route => route.abort())

    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()

    // Fallback fiyatlar görünmeli (hardcoded değerler)
    await expect(page.locator('#pricing').getByText(/starter/i)).toBeVisible({ timeout: 10_000 })
    await expect(page.locator('#pricing').getByText(/₺/).first()).toBeVisible()
  })

  test('Pro plan "En Popüler" veya highlighted gösteriyor', async ({ page }) => {
    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()
    // Pro kartı öne çıkan stil veya badge içermeli
    const proBadge = page.locator('#pricing').getByText(/en popüler|most popular|önerilen/i)
    await expect(proBadge).toBeVisible({ timeout: 10_000 })
  })

  test('plan feature listesi görünüyor', async ({ page }) => {
    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()
    // Checkmark'lı feature listesi var
    const checkmarks = page.locator('#pricing svg').filter({ hasText: '' })
    const count = await page.locator('#pricing').locator('li, [class*="feature"]').count()
    expect(count).toBeGreaterThan(3)
  })

  test('Super Admin\'den güncellenen fiyat landing page\'de yansıyor', async ({ page }) => {
    // API'yi mock et: güncellenmiş fiyat dönsün
    await page.route('**/api/public/pricing', route => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { id: '1', name: 'Starter', tier: 'Starter', monthlyPriceTry: 9999, yearlyPriceTry: 7999, isVisible: true, limitsJson: '{}', featuresJson: '{}' },
          { id: '2', name: 'Pro',     tier: 'Pro',     monthlyPriceTry: 4999, yearlyPriceTry: 3999, isVisible: true, limitsJson: '{}', featuresJson: '{}' },
          { id: '3', name: 'Enterprise', tier: 'Enterprise', monthlyPriceTry: 0, yearlyPriceTry: 0, isVisible: true, limitsJson: '{}', featuresJson: '{}' },
        ]),
      })
    })
    await page.goto('/')
    await page.locator('#pricing').scrollIntoViewIfNeeded()
    // Mock fiyat görünmeli
    await expect(page.locator('#pricing').getByText(/9.999|9999/)).toBeVisible({ timeout: 10_000 })
  })

})
```

---

## Dosya 4: `content.spec.ts`

İçerik ve erişilebilirlik testleri.

```typescript
import { test, expect } from '@playwright/test'

test.describe('Landing Page — İçerik', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    await page.waitForLoadState('networkidle')
  })

  test('hero section başlık ve CTA görünüyor', async ({ page }) => {
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible()
    // Ana CTA butonu
    await expect(page.getByRole('link', { name: /ücretsiz/i }).first()).toBeVisible()
  })

  test('features section 6 kart içeriyor', async ({ page }) => {
    const featuresSection = page.locator('#features')
    await featuresSection.scrollIntoViewIfNeeded()
    // 6 feature kartı: Kural Motoru, Kapora, WhatsApp, Masa Takibi, Ekip, Raporlama
    await expect(featuresSection.getByText(/kural motoru/i)).toBeVisible()
    await expect(featuresSection.getByText(/whatsapp/i)).toBeVisible()
    await expect(featuresSection.getByText(/masa takibi/i)).toBeVisible()
  })

  test('kapora kartında "isteğe bağlı" badge var', async ({ page }) => {
    const featuresSection = page.locator('#features')
    await featuresSection.scrollIntoViewIfNeeded()
    // Kapora modülü isteğe bağlı olarak işaretlenmiş
    await expect(featuresSection.getByText(/isteğe bağlı/i)).toBeVisible()
  })

  test('"Nasıl Çalışır" 3 adım içeriyor', async ({ page }) => {
    // 3 adım görünmeli
    await expect(page.getByText(/mekanınızı tanımlayın/i)).toBeVisible()
    await expect(page.getByText(/kurallarınızı girin/i)).toBeVisible()
    await expect(page.getByText(/rezervasyonları alın/i)).toBeVisible()
  })

  test('testimonial section görünüyor', async ({ page }) => {
    // En az 1 testimonial quote
    const quotes = page.locator('blockquote, [class*="testimonial"], [class*="quote"]')
    const count = await quotes.count()
    if (count === 0) {
      // Alternatif: tırnak işareti içeren metin
      await expect(page.getByText(/rezervasyon.*bitti|kapora.*sistem|kısıtlama.*otomatik/i)).toBeVisible()
    } else {
      expect(count).toBeGreaterThanOrEqual(1)
    }
  })

  test('mobil görünümde sayfa düzgün render ediliyor', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 })
    await page.goto('/')
    await page.waitForLoadState('networkidle')
    // Hero görünüyor
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible()
    // Horizontal scroll yok
    const hasHorizontalScroll = await page.evaluate(
      () => document.documentElement.scrollWidth > document.documentElement.clientWidth
    )
    expect(hasHorizontalScroll).toBe(false)
  })

  test('CTA banner görünüyor ve butonu çalışıyor', async ({ page }) => {
    // CtaBanner en alta scroll et
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight))
    await page.waitForTimeout(300)
    // Banner başlığı ve butonu görünmeli
    await expect(page.getByText(/rezervasyon kaosunu/i)).toBeVisible({ timeout: 8_000 })
    await expect(page.getByRole('link', { name: /hemen başla/i })).toBeVisible()
  })

})
```

---

## Ortak Kurallar

- `import { test, expect } from '@playwright/test'` kullan (fixture gerekmez — public sayfa)
- `page.route()` ile API mock'lama testlerde kullanılabilir
- Scroll testleri için `element.scrollIntoViewIfNeeded()` kullan
- Mobile test: `page.setViewportSize({ width: 375, height: 812 })`
- Timeout: `{ timeout: 10_000 }` DOM için, `{ timeout: 15_000 }` networkidle için
- Test isimleri Türkçe

## Çalıştırma

```bash
cd e2e
npx playwright test landing/ --reporter=list
```

## Beklenen Sonuç

Landing page testleri tamamlandığında: **~18 yeni test**

Tüm QA aşamaları tamamlandığında genel toplam: **~120+ test**
