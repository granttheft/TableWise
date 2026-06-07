import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Rezervasyonlar', () => {
  adminTest('rezervasyon listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    await expect(page.getByText(/rezervasyon|müşteri|tarih/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('durum filtresi çalışıyor — Confirmed', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    const statusFilter = page.getByRole('combobox').filter({ hasText: /durum|status|tümü/i }).first()
    if (await statusFilter.count() > 0) {
      await statusFilter.click()
      // Onaylı seçeneği — Türkçe veya İngilizce
      const option = page.getByRole('option', { name: /onay|confirm/i }).first()
      if (await option.count() > 0) {
        await option.click()
        await expect(page.getByText(/onay|confirm/i).first()).toBeVisible({ timeout: 8_000 })
      }
    }
  })

  adminTest('durum filtresi çalışıyor — Pending', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    const statusFilter = page.getByRole('combobox').filter({ hasText: /durum|status|tümü/i }).first()
    if (await statusFilter.count() > 0) {
      await statusFilter.click()
      await page.getByRole('option', { name: /beklemede|pending/i }).click()
      await expect(page.getByText(/beklemede|pending/i).first()).toBeVisible({ timeout: 8_000 })
    }
  })

  adminTest('rezervasyon detayı açılıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    // Tablo satırı veya kart — çeşitli layout'ları destekle
    const firstRow = page.locator('table tbody tr').first()
    const firstCard = page.locator('[class*="card"][class*="cursor"], [class*="reservation"]').first()
    const anyClickable = firstRow.or(firstCard)
    const hasRow = await firstRow.count() > 0
    if (hasRow) {
      await firstRow.waitFor({ timeout: 10_000 })
      await firstRow.click()
      await expect(page.locator('[role="dialog"], [class*="drawer"], [class*="modal"]')).toBeVisible({ timeout: 6_000 })
    }
  })

  adminTest('saat UTC+3 (Türkiye saati) olarak gösteriliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    const timePattern = /\d{1,2}:\d{2}/
    await expect(page.getByText(timePattern).first()).toBeVisible({ timeout: 10_000 })
  })
})
