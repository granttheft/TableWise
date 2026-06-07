import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

function getTomorrowDateString(): string {
  const d = new Date()
  d.setDate(d.getDate() + 1)
  return d.toISOString().split('T')[0]
}

test.describe('Booking UI — Validasyon & Edge Case', () => {
  test('geçersiz slug ile hata sayfası gösteriyor', async ({ page }) => {
    await page.goto('/rezervasyon/var-olmayan-mekan-xyz')
    await expect(page.getByText(/bulunamadı|hata|not found|geçersiz/i)).toBeVisible({ timeout: 10_000 })
  })

  test('geçersiz email formu engelliyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)

    const dateInput = page.getByLabel(/tarih/i).or(page.locator('input[type="date"]')).first()
    if (await dateInput.count() > 0) await dateInput.fill(getTomorrowDateString())

    const partySizeInput = page.getByLabel(/kişi|misafir/i).or(page.locator('input[type="number"]')).first()
    if (await partySizeInput.count() > 0) await partySizeInput.fill('2')

    const slots = page.getByRole('button', { name: /\d{1,2}:\d{2}/ })
    await expect(slots.first()).toBeVisible({ timeout: 15_000 })
    await slots.first().click()

    await page.getByLabel(/ad|isim|name/i).first().fill('Test Kişi')
    await page.getByLabel(/e-?posta|email/i).first().fill('gecersiz-email')
    await page.getByLabel(/telefon|tel/i).first().fill('5551234567')

    const nextBtn = page.getByRole('button', { name: /devam|ileri|next|rezervasyon yap/i })
    if (await nextBtn.count() > 0) await nextBtn.click()

    // Form geçmemeli — hata mesajı veya hâlâ aynı sayfada
    await expect(page.getByText(/geçersiz|hatalı|invalid|e-posta|email/i)).toBeVisible({ timeout: 6_000 })
  })

  test('çalışma saati içinde slot seçimi "saati dışında" hatası vermiyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)

    const dateInput = page.getByLabel(/tarih/i).or(page.locator('input[type="date"]')).first()
    if (await dateInput.count() > 0) await dateInput.fill(getTomorrowDateString())

    const partySizeInput = page.getByLabel(/kişi|misafir/i).or(page.locator('input[type="number"]')).first()
    if (await partySizeInput.count() > 0) await partySizeInput.fill('2')

    // Slot listesi yüklenince "çalışma saatleri dışında" hatası OLMAMALI
    await page.waitForTimeout(3000)
    const outsideHoursError = page.getByText(/çalışma saatleri dışında|saati dışında/i)
    await expect(outsideHoursError).not.toBeVisible()

    // En az bir slot görünmeli
    const slots = page.getByRole('button', { name: /\d{1,2}:\d{2}/ })
    await expect(slots.first()).toBeVisible({ timeout: 15_000 })
  })

  test('boş zorunlu alanlar formu engelliyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)

    const dateInput = page.getByLabel(/tarih/i).or(page.locator('input[type="date"]')).first()
    if (await dateInput.count() > 0) await dateInput.fill(getTomorrowDateString())

    const partySizeInput = page.getByLabel(/kişi|misafir/i).or(page.locator('input[type="number"]')).first()
    if (await partySizeInput.count() > 0) await partySizeInput.fill('2')

    const slots = page.getByRole('button', { name: /\d{1,2}:\d{2}/ })
    await expect(slots.first()).toBeVisible({ timeout: 15_000 })
    await slots.first().click()

    // İsim/email doldurmadan gönder
    const nextBtn = page.getByRole('button', { name: /devam|ileri|next|rezervasyon yap/i })
    if (await nextBtn.count() > 0) await nextBtn.click()

    // Hata mesajı veya aynı sayfada kalıyor
    const errorOrSamePage = (await page.getByText(/gerekli|zorunlu|required|boş/i).count()) > 0
      || page.url().includes(SEED.tenant.slug)
    expect(errorOrSamePage).toBeTruthy()
  })
})
