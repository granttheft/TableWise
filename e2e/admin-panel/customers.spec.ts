import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'
import { SEED } from '../fixtures/seed'

adminTest.describe('Admin Panel — Müşteriler', () => {
  adminTest('seed müşteriler listeleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/customers')
    await expect(page.getByText(SEED.customers.mehmet.name)).toBeVisible({ timeout: 10_000 })
  })

  adminTest('isim araması çalışıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/customers')
    const searchInput = page.getByPlaceholder(/ara|search/i)
    await searchInput.fill('Mehmet')
    await expect(page.getByText(SEED.customers.mehmet.name)).toBeVisible({ timeout: 8_000 })
    // Ayşe görünmemeli
    await expect(page.getByText(SEED.customers.ayse.name)).not.toBeVisible()
  })

  adminTest('tier filtresi çalışıyor — VIP', async ({ authenticatedPage: page }) => {
    await page.goto('/customers')
    const tierFilter = page.getByRole('combobox').filter({ hasText: /tier|tümü/i })
    await tierFilter.click()
    await page.getByRole('option', { name: /VIP/i }).click()
    await expect(page.getByText('VIP').first()).toBeVisible({ timeout: 8_000 })
  })

  adminTest('müşteri detay drawer açılıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/customers')
    await page.getByText(SEED.customers.mehmet.name).click()
    await expect(page.locator('[role="dialog"], [class*="drawer"], [class*="sheet"]')).toBeVisible({ timeout: 6_000 })
    await expect(page.getByText(SEED.customers.mehmet.name)).toBeVisible()
  })

  adminTest('"Son Mekan" kolonu görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/customers')
    await expect(page.getByText(/son mekan/i)).toBeVisible({ timeout: 10_000 })
  })

  adminTest('blacklisted müşteri işaretli görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/customers')
    await expect(page.getByText(/blacklist/i).first()).toBeVisible({ timeout: 10_000 })
  })
})
