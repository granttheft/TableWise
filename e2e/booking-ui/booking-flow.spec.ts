import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

// Yarın için geçerli bir tarih üretir (çalışma saati içinde)
function getTomorrowDateString(): string {
  const d = new Date()
  d.setDate(d.getDate() + 1)
  return d.toISOString().split('T')[0]
}

test.describe('Booking UI — Rezervasyon Akışı', () => {
  test('demo-restoran sayfası yükleniyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)
    await expect(page.getByText(/demo restoran|ana salon/i)).toBeVisible({ timeout: 15_000 })
  })

  test('tarih ve kişi sayısı seçimi ile slotlar yükleniyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)

    // Tarih girişi
    const dateInput = page.getByLabel(/tarih/i).or(page.locator('input[type="date"]')).first()
    if (await dateInput.count() > 0) {
      await dateInput.fill(getTomorrowDateString())
    }

    // Kişi sayısı
    const partySizeInput = page.getByLabel(/kişi|misafir|party/i).or(page.locator('input[type="number"]')).first()
    if (await partySizeInput.count() > 0) {
      await partySizeInput.fill('2')
    }

    // Müsait slotlar yüklenmeli
    await expect(page.getByText(/saat|slot|müsait/i).first()).toBeVisible({ timeout: 15_000 })
  })

  test('tam rezervasyon happy path', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)

    // 1. Tarih seç
    const dateInput = page.getByLabel(/tarih/i).or(page.locator('input[type="date"]')).first()
    if (await dateInput.count() > 0) {
      await dateInput.fill(getTomorrowDateString())
    }

    // 2. Kişi sayısı
    const partySizeInput = page.getByLabel(/kişi|misafir/i).or(page.locator('input[type="number"]')).first()
    if (await partySizeInput.count() > 0) {
      await partySizeInput.fill('2')
    }

    // 3. İlk müsait slot'u seç
    const slots = page.getByRole('button', { name: /\d{1,2}:\d{2}/ })
    await expect(slots.first()).toBeVisible({ timeout: 15_000 })
    await slots.first().click()

    // 4. Müşteri bilgileri formu
    await page.getByLabel(/ad|isim|name/i).first().fill('Test Kişi')
    await page.getByLabel(/e-?posta|email/i).first().fill(`test${Date.now()}@example.com`)
    await page.getByLabel(/telefon|tel/i).first().fill('5551234567')

    // 5. İleri / Devam
    const nextBtn = page.getByRole('button', { name: /devam|ileri|next|rezervasyon yap/i })
    if (await nextBtn.count() > 0) {
      await nextBtn.click()
    }

    // 6. Custom field'lar varsa atla
    const skipBtn = page.getByRole('button', { name: /atla|skip|devam/i })
    if (await skipBtn.count() > 0) {
      await skipBtn.click()
    }

    // 7. Onay butonu
    const confirmBtn = page.getByRole('button', { name: /onayla|rezervasyon yap|tamamla|confirm/i })
    if (await confirmBtn.count() > 0) {
      await confirmBtn.click()
    }

    // 8. Başarı: confirmation code veya onay mesajı
    await expect(
      page.getByText(/rezervasyonunuz|onaylandı|kod|confirmation|başarı/i)
    ).toBeVisible({ timeout: 15_000 })
  })
})
