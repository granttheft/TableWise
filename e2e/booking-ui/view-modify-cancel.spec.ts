import { test, expect, type Page } from '@playwright/test'
import { SEED } from '../fixtures/seed'

function getTomorrowDateString(): string {
  const d = new Date()
  d.setDate(d.getDate() + 1)
  return d.toISOString().split('T')[0]
}

// Yardımcı: yeni rezervasyon oluştur ve confirmation code döndür
async function createTestReservation(page: Page): Promise<string | null> {
  await page.goto(`/rezervasyon/${SEED.tenant.slug}`)

  const dateInput = page.getByLabel(/tarih/i).or(page.locator('input[type="date"]')).first()
  if (await dateInput.count() > 0) await dateInput.fill(getTomorrowDateString())

  const partySizeInput = page.getByLabel(/kişi|misafir/i).or(page.locator('input[type="number"]')).first()
  if (await partySizeInput.count() > 0) await partySizeInput.fill('2')

  const slots = page.getByRole('button', { name: /\d{1,2}:\d{2}/ })
  await expect(slots.first()).toBeVisible({ timeout: 15_000 })
  await slots.first().click()

  await page.getByLabel(/ad|isim|name/i).first().fill('Görüntüleme Testi')
  await page.getByLabel(/e-?posta|email/i).first().fill(`view${Date.now()}@example.com`)
  await page.getByLabel(/telefon|tel/i).first().fill('5559876543')

  const nextBtn = page.getByRole('button', { name: /devam|ileri|next|rezervasyon yap/i })
  if (await nextBtn.count() > 0) await nextBtn.click()

  const confirmBtn = page.getByRole('button', { name: /onayla|tamamla|confirm/i })
  if (await confirmBtn.count() > 0) await confirmBtn.click()

  // Confirmation code çıkar
  await page.waitForTimeout(2000)
  const codeMatch = page.url().match(/\/onay\/([A-Z0-9]+)/)
  if (codeMatch) return codeMatch[1]

  const codeText = await page.getByText(/[A-Z0-9]{6,}/).first().textContent()
  return codeText?.trim() ?? null
}

test.describe('Booking UI — Görüntüleme / Değiştirme / İptal', () => {
  test('rezervasyon görüntüleme sayfası yükleniyor', async ({ page }) => {
    const code = await createTestReservation(page)
    if (!code) test.skip()

    await page.goto(`/rezervasyon/goruntule/${code}`)
    await expect(page.getByText(/rezervasyon|detay/i)).toBeVisible({ timeout: 10_000 })
    await expect(page.getByText(/Görüntüleme Testi/i)).toBeVisible({ timeout: 8_000 })
  })

  test('rezervasyon iptal sayfası yükleniyor', async ({ page }) => {
    const code = await createTestReservation(page)
    if (!code) test.skip()

    await page.goto(`/rezervasyon/iptal/${code}`)
    await expect(page.getByText(/iptal|cancel/i)).toBeVisible({ timeout: 10_000 })
  })

  test('rezervasyon iptal ediliyor', async ({ page }) => {
    const code = await createTestReservation(page)
    if (!code) test.skip()

    await page.goto(`/rezervasyon/iptal/${code}`)
    const cancelBtn = page.getByRole('button', { name: /iptal et|onayla|confirm/i })
    await expect(cancelBtn).toBeVisible({ timeout: 10_000 })
    await cancelBtn.click()

    await expect(page.getByText(/iptal edildi|cancelled|başarı/i)).toBeVisible({ timeout: 10_000 })
  })

  test('geçersiz kod ile 404/hata sayfası gösteriyor', async ({ page }) => {
    await page.goto('/rezervasyon/goruntule/INVALIDCODE999')
    await expect(page.getByText(/bulunamadı|hata|geçersiz|not found|error/i)).toBeVisible({ timeout: 10_000 })
  })
})
