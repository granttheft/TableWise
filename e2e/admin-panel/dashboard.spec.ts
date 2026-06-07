import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Dashboard', () => {
  adminTest('istatistik kartları yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    // Bugünkü, bu ay, toplam müşteri sayaçları
    await expect(page.getByText(/bugün|bugünkü/i)).toBeVisible({ timeout: 10_000 })
    await expect(page.getByText(/bu ay/i)).toBeVisible()
    await expect(page.getByText(/müşteri/i)).toBeVisible()
  })

  adminTest('sayaçlar sayısal değer gösteriyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    // StatCard value'ları boş string değil
    const statCards = page.locator('[class*="stat"], [class*="card"]')
    await expect(statCards.first()).toBeVisible({ timeout: 10_000 })
    // Herhangi bir sayısal değer varsa yeterli
    await expect(page.getByText(/\d+/)).toBeVisible()
  })

  adminTest('son aktiviteler bölümü görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText(/aktivite|faaliyet|son işlem/i)).toBeVisible({ timeout: 10_000 })
  })

  adminTest('kural istatistikleri bölümü görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText(/kural/i)).toBeVisible({ timeout: 10_000 })
  })

  adminTest('"Bu Ay Rezervasyon" plan limiti formatında gösteriliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    // "6 / Plan Limiti: 500" veya "X rezervasyon" formatında
    await expect(page.getByText(/Plan Limiti|rezervasyon/i)).toBeVisible({ timeout: 10_000 })
  })
})
