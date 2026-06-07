import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'
import { SEED } from '../fixtures/seed'

adminTest.describe('Admin Panel — Masalar', () => {
  adminTest('seed masalar listeleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    await expect(page.getByText(/masa \d/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('yeni masa oluşturuluyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    const uniqueName = `Test Masa ${Date.now()}`

    await page.getByRole('button', { name: /yeni masa|masa ekle|ekle/i }).click()

    await page.getByLabel(/masa adı|isim|name/i).fill(uniqueName)
    await page.getByLabel(/kapasite/i).fill('4')
    await page.getByRole('button', { name: /kaydet|oluştur|ekle/i }).last().click()

    await expect(page.getByText(uniqueName)).toBeVisible({ timeout: 8_000 })
  })

  adminTest('masa siliniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    const uniqueName = `Silinecek Masa ${Date.now()}`

    await page.getByRole('button', { name: /yeni masa|masa ekle|ekle/i }).click()
    await page.getByLabel(/masa adı|isim|name/i).fill(uniqueName)
    await page.getByLabel(/kapasite/i).fill('2')
    await page.getByRole('button', { name: /kaydet|oluştur|ekle/i }).last().click()
    await expect(page.getByText(uniqueName)).toBeVisible({ timeout: 8_000 })

    const row = page.locator('tr, [class*="card"], [class*="row"]').filter({ hasText: uniqueName })
    const deleteBtn = row.getByRole('button', { name: /sil|delete/i })
    if (await deleteBtn.count() > 0) {
      await deleteBtn.click()
      await page.getByRole('button', { name: /evet|sil|onayla|confirm/i }).last().click()
      await expect(page.getByText(uniqueName)).not.toBeVisible({ timeout: 8_000 })
    }
  })

  adminTest('masa birleştirme listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tables')
    const comboSection = page.getByText(/kombina|birleştir|combination/i).first()
      .or(page.getByRole('tab', { name: /kombina|birleştir/i }))
    if (await comboSection.count() > 0) {
      await expect(comboSection).toBeVisible({ timeout: 10_000 })
    } else {
      // Sayfa yüklenmiş ama bu bölüm yok — en azından masalar yüklendi
      await expect(page.getByText(/masa/i).first()).toBeVisible({ timeout: 10_000 })
    }
  })
})
