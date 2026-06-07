import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Ekip/Staff', () => {
  adminTest('personel listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/staff')
    // En az mevcut owner görünmeli
    await expect(page.getByText(/ahmet|owner|ekip|personel/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('yeni personel davet formu açılıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/staff')
    await page.getByRole('button', { name: /davet|ekle|invite/i }).click()
    await expect(page.locator('[role="dialog"]')).toBeVisible({ timeout: 6_000 })
    // E-posta alanı görünmeli
    await expect(page.getByLabel(/e-?posta|email/i)).toBeVisible()
  })

  adminTest('geçersiz email ile davet hata veriyor', async ({ authenticatedPage: page }) => {
    await page.goto('/staff')
    await page.getByRole('button', { name: /davet|ekle|invite/i }).click()
    await page.locator('[role="dialog"]').getByLabel(/e-?posta|email/i).fill('gecersizemail')
    await page.locator('[role="dialog"]').getByRole('button', { name: /gönder|davet et|kaydet/i }).click()
    // Validasyon hatası
    await expect(page.getByText(/geçersiz|hatalı|invalid|email/i)).toBeVisible({ timeout: 6_000 })
  })

  adminTest('yeni personel davet ediliyor (benzersiz email)', async ({ authenticatedPage: page }) => {
    await page.goto('/staff')
    const uniqueEmail = `test${Date.now()}@example.com`

    await page.getByRole('button', { name: /davet|ekle|invite/i }).click()
    const dialog = page.locator('[role="dialog"]')
    await dialog.getByLabel(/e-?posta|email/i).fill(uniqueEmail)

    // Rol seçimi varsa
    const roleSelect = dialog.getByLabel(/rol|role/i)
    if (await roleSelect.count() > 0) {
      await roleSelect.selectOption({ index: 0 })
    }

    await dialog.getByRole('button', { name: /gönder|davet et|kaydet/i }).click()

    // Başarı mesajı veya listede görünüyor
    await expect(page.getByText(/davet|gönderildi|başarı|success/i)).toBeVisible({ timeout: 10_000 })
  })
})
