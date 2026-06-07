import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Dashboard', () => {
  adminTest('istatistik kartları yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText(/bugün|bugünkü/i).first()).toBeVisible({ timeout: 10_000 })
    await expect(page.getByText(/bu ay/i).first()).toBeVisible()
    await expect(page.getByText(/müşteri/i).first()).toBeVisible()
  })

  adminTest('sayaçlar sayısal değer gösteriyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    const statCards = page.locator('[class*="stat"], [class*="card"]')
    await expect(statCards.first()).toBeVisible({ timeout: 10_000 })
    await expect(page.getByText(/\d+/).first()).toBeVisible()
  })

  adminTest('son aktiviteler bölümü görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText(/aktivite|faaliyet|son işlem/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('kural istatistikleri bölümü görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText(/kural/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('"Bu Ay Rezervasyon" plan limiti formatında gösteriliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText(/Plan Limiti|rezervasyon/i).first()).toBeVisible({ timeout: 10_000 })
  })
})
