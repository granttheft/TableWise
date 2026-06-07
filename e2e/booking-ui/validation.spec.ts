import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

function getNextWeekday(): Date {
  const d = new Date()
  d.setDate(d.getDate() + 1)
  while (d.getDay() === 0 || d.getDay() === 6) d.setDate(d.getDate() + 1)
  return d
}

test.describe('Booking UI — Validasyon & Edge Case', () => {
  test('geçersiz slug ile hata sayfası gösteriyor', async ({ page }) => {
    await page.goto('/rezervasyon/var-olmayan-mekan-xyz')
    await expect(page.getByText(/bulunamadı|hata|not found|geçersiz/i)).toBeVisible({ timeout: 10_000 })
  })

  test('çalışma saati içinde slot seçimi "saati dışında" hatası vermiyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)
    await expect(page.getByText(/tarih seçin/i)).toBeVisible({ timeout: 15_000 })

    const nextDay = getNextWeekday()
    const dayBtn = page.locator('button').filter({ hasText: new RegExp(`^${nextDay.getDate()}$`) }).first()
    await expect(dayBtn).toBeVisible({ timeout: 10_000 })
    await dayBtn.click()
    await page.getByRole('button', { name: 'Devam Et' }).click()

    // "çalışma saatleri dışında" hatası OLMAMALI
    await page.waitForTimeout(3_000)
    await expect(page.getByText(/çalışma saatleri dışında|saati dışında/i)).not.toBeVisible()

    // En az bir slot görünmeli
    const slots = page.locator('button').filter({ hasText: /^\d{1,2}:\d{2}$/ })
    await expect(slots.first()).toBeVisible({ timeout: 15_000 })
  })

  test('geçersiz telefon numarası formu engelliyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)
    await expect(page.getByText(/tarih seçin/i)).toBeVisible({ timeout: 15_000 })

    const nextDay = getNextWeekday()
    const dayBtn = page.locator('button').filter({ hasText: new RegExp(`^${nextDay.getDate()}$`) }).first()
    await dayBtn.click()
    await page.getByRole('button', { name: 'Devam Et' }).click()

    const slots = page.locator('button').filter({ hasText: /^\d{1,2}:\d{2}$/ })
    await expect(slots.first()).toBeVisible({ timeout: 15_000 })
    await slots.first().click()
    await page.getByRole('button', { name: 'Devam Et' }).click()

    // Step 3 skip
    const skipBtn = page.getByRole('button', { name: /atla|masasız devam/i })
    if (await skipBtn.isVisible({ timeout: 2_000 }).catch(() => false)) {
      await skipBtn.click()
    } else {
      const continueBtn3 = page.getByRole('button', { name: 'Devam Et' })
      if (await continueBtn3.isVisible({ timeout: 2_000 }).catch(() => false)) await continueBtn3.click()
    }

    // Bilgi formu
    await expect(page.locator('#customerName')).toBeVisible({ timeout: 10_000 })
    await page.locator('#customerName').fill('Test Kişi')
    await page.locator('#customerEmail').fill('test@example.com')
    await page.locator('#customerPhone').fill('12345') // geçersiz

    const kvkk = page.getByRole('checkbox').first()
    if (await kvkk.isVisible()) await kvkk.check()

    await page.getByRole('button', { name: 'Devam Et' }).click()

    // Validasyon hatası
    await expect(page.getByText(/geçerli bir türkiye/i).first()).toBeVisible({ timeout: 6_000 })
  })

  test('KVKK onayı olmadan form submit engelliyor', async ({ page }) => {
    await page.goto(`/rezervasyon/${SEED.tenant.slug}`)
    await expect(page.getByText(/tarih seçin/i)).toBeVisible({ timeout: 15_000 })

    const nextDay = getNextWeekday()
    const dayBtn = page.locator('button').filter({ hasText: new RegExp(`^${nextDay.getDate()}$`) }).first()
    await dayBtn.click()
    await page.getByRole('button', { name: 'Devam Et' }).click()

    const slots = page.locator('button').filter({ hasText: /^\d{1,2}:\d{2}$/ })
    await expect(slots.first()).toBeVisible({ timeout: 15_000 })
    await slots.first().click()
    await page.getByRole('button', { name: 'Devam Et' }).click()

    const skipBtn2 = page.getByRole('button', { name: /atla|masasız devam/i })
    if (await skipBtn2.isVisible({ timeout: 2_000 }).catch(() => false)) {
      await skipBtn2.click()
    } else {
      const continueBtn3b = page.getByRole('button', { name: 'Devam Et' })
      if (await continueBtn3b.isVisible({ timeout: 2_000 }).catch(() => false)) await continueBtn3b.click()
    }

    await expect(page.locator('#customerName')).toBeVisible({ timeout: 10_000 })
    await page.locator('#customerName').fill('Test Kişi')
    await page.locator('#customerEmail').fill('test@example.com')
    await page.locator('#customerPhone').fill('05551234567')
    // KVKK işaretlenmedi

    await page.getByRole('button', { name: 'Devam Et' }).click()
    await expect(page.getByText(/kvkk|onay zorunlu/i)).toBeVisible({ timeout: 6_000 })
  })
})
