import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Kurallar', () => {
  adminTest('kural listesi yükleniyor (seed kurallar)', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    // En az bir kural görünmeli
    await expect(page.getByText(/erken|vip|büyük grup|kapora|turnover/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('kural aktif/pasif toggle çalışıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    // İlk switch/toggle'ı bul
    const toggle = page.getByRole('switch').first()
    await toggle.waitFor({ timeout: 10_000 })
    const initialChecked = await toggle.isChecked()
    await toggle.click()
    // Değer değişti
    await expect(toggle).toHaveAttribute('aria-checked', String(!initialChecked), { timeout: 6_000 })
    // Geri al
    await toggle.click()
  })

  adminTest('yeni kural oluşturma modalı açılıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    await page.getByRole('button', { name: /yeni kural|kural ekle|ekle/i }).click()
    await expect(page.locator('[role="dialog"]')).toBeVisible({ timeout: 6_000 })
    await expect(page.getByText(/kural tipi|rule type|tür/i)).toBeVisible()
  })

  adminTest('kural istatistikleri (TimesTriggered) görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/rules')
    // Tetiklenme sayısı veya istatistik sütunu
    await expect(page.getByText(/tetiklen|kez|istatistik|triggered/i).first()).toBeVisible({ timeout: 10_000 })
  })
})
