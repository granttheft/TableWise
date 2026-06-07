import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'
import { SEED } from '../fixtures/seed'

adminTest.describe('Admin Panel — Masalar', () => {
  adminTest('seed masalar listeleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    await expect(page.getByText(SEED.tables.table1.name)).toBeVisible({ timeout: 10_000 })
    await expect(page.getByText(SEED.tables.table3.name)).toBeVisible()
    await expect(page.getByText(SEED.tables.table5.name)).toBeVisible()
  })

  adminTest('yeni masa oluşturuluyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    const uniqueName = `Test Masa ${Date.now()}`

    // "Yeni Masa" veya "Masa Ekle" butonu
    await page.getByRole('button', { name: /yeni masa|masa ekle|ekle/i }).click()

    // Modal/form açıldı
    await page.getByLabel(/masa adı|isim|name/i).fill(uniqueName)
    await page.getByLabel(/kapasite/i).fill('4')
    await page.getByRole('button', { name: /kaydet|oluştur|ekle/i }).last().click()

    // Listede görünüyor
    await expect(page.getByText(uniqueName)).toBeVisible({ timeout: 8_000 })
  })

  adminTest('masa siliniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    // Silinebilir bir test masası bul (seed masaları sak, yeni oluşturulan silinebilir)
    const uniqueName = `Silinecek Masa ${Date.now()}`

    await page.getByRole('button', { name: /yeni masa|masa ekle|ekle/i }).click()
    await page.getByLabel(/masa adı|isim|name/i).fill(uniqueName)
    await page.getByLabel(/kapasite/i).fill('2')
    await page.getByRole('button', { name: /kaydet|oluştur|ekle/i }).last().click()
    await expect(page.getByText(uniqueName)).toBeVisible({ timeout: 8_000 })

    // Sil
    const row = page.locator('tr, [class*="card"], [class*="row"]').filter({ hasText: uniqueName })
    await row.getByRole('button', { name: /sil|delete/i }).click()

    // Onay modalı
    await page.getByRole('button', { name: /evet|sil|onayla|confirm/i }).last().click()

    await expect(page.getByText(uniqueName)).not.toBeVisible({ timeout: 8_000 })
  })

  adminTest('masa birleştirme listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    // Kombinasyon sekme veya bölümü
    const comboSection = page.getByText(/kombina|birleştir|combination/i)
    await expect(comboSection).toBeVisible({ timeout: 10_000 })
  })
})
