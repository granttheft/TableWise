import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Kurallar', () => {
  adminTest('kural listesi yükleniyor (seed kurallar)', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    await expect(page.getByText(/erken|vip|büyük grup|kapora|turnover/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('kural aktif/pasif toggle çalışıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    const toggle = page.getByRole('switch').first()
    await toggle.waitFor({ timeout: 10_000 })
    const initialChecked = await toggle.isChecked()
    await toggle.click()
    await expect(toggle).toHaveAttribute('aria-checked', String(!initialChecked), { timeout: 6_000 })
    await toggle.click()
  })

  adminTest('yeni kural oluşturma modalı açılıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    await page.getByRole('button', { name: /yeni kural|kural ekle|ekle/i }).click()
    const dialog = page.locator('[role="dialog"]')
    await expect(dialog).toBeVisible({ timeout: 6_000 })
    // Modal açıldı ve içerik yüklendi
    await expect(dialog.locator('button, input, [role="combobox"]').first()).toBeVisible({ timeout: 5_000 })
  })

  adminTest('kural istatistikleri (TimesTriggered) görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    await expect(page.getByText(/tetiklen|kez|istatistik|triggered|\d+/i).first()).toBeVisible({ timeout: 10_000 })
  })
})
