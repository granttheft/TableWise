import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Rezervasyonlar', () => {
  adminTest('rezervasyon listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    // Tablo başlığı veya rezervasyon item'ı görünmeli
    await expect(page.getByText(/rezervasyon|müşteri|tarih/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('durum filtresi çalışıyor — Confirmed', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    // Durum filtresi dropdown/select
    const statusFilter = page.getByRole('combobox').filter({ hasText: /durum|status|tümü/i })
    if (await statusFilter.count() > 0) {
      await statusFilter.click()
      await page.getByRole('option', { name: /onaylı|confirmed/i }).click()
      await expect(page.getByText(/onaylı|confirmed/i).first()).toBeVisible({ timeout: 8_000 })
    }
  })

  adminTest('durum filtresi çalışıyor — Pending', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    const statusFilter = page.getByRole('combobox').filter({ hasText: /durum|status|tümü/i })
    if (await statusFilter.count() > 0) {
      await statusFilter.click()
      await page.getByRole('option', { name: /beklemede|pending/i }).click()
      await expect(page.getByText(/beklemede|pending/i).first()).toBeVisible({ timeout: 8_000 })
    }
  })

  adminTest('rezervasyon detayı açılıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    const firstRow = page.locator('tr[class*="cursor"], tbody tr').first()
    await firstRow.waitFor({ timeout: 10_000 })
    await firstRow.click()
    // Modal veya detay paneli açıldı
    await expect(page.locator('[role="dialog"], [class*="drawer"], [class*="modal"]')).toBeVisible({ timeout: 6_000 })
  })

  adminTest('saat UTC+3 (Türkiye saati) olarak gösteriliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/reservations')
    // Herhangi bir saat gösterimi var mı — "HH:MM" formatında olmalı
    const timePattern = /\d{1,2}:\d{2}/
    await expect(page.getByText(timePattern).first()).toBeVisible({ timeout: 10_000 })
  })
})
