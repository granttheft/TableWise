import { expect } from '@playwright/test'
import { superAdminTest } from '../fixtures/auth'
import { SEED } from '../fixtures/seed'

superAdminTest.describe('Super Admin — Ekip Yönetimi', () => {
  superAdminTest('ekip listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/team')
    // Mevcut admin görünmeli
    await expect(page.getByText(SEED.superAdmin.email)).toBeVisible({ timeout: 10_000 })
  })

  superAdminTest('süper admin rolü görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/team')
    await expect(page.getByText(/superadmin|super admin/i)).toBeVisible({ timeout: 10_000 })
  })

  superAdminTest('yeni ekip üyesi ekleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/team')
    await page.getByRole('button', { name: /yeni üye|üye ekle|davet/i }).click()

    const dialog = page.locator('[role="dialog"]')
    await expect(dialog).toBeVisible({ timeout: 6_000 })

    const uniqueEmail = `team${Date.now()}@tablewise.com`
    await dialog.getByLabel(/e-?posta|email/i).fill(uniqueEmail)
    await dialog.getByLabel(/ad|isim|name/i).fill('Test Üye')

    // Rol seçimi
    const roleSelect = dialog.getByLabel(/rol|role/i)
    if (await roleSelect.count() > 0) {
      await roleSelect.selectOption('Marketing')
    }

    // Şifre
    const passwordInput = dialog.getByLabel(/şifre|password/i).first()
    if (await passwordInput.count() > 0) {
      await passwordInput.fill('TestPass123!')
    }

    await dialog.getByRole('button', { name: /ekle|davet|kaydet/i }).click()
    await expect(page.getByText(uniqueEmail)).toBeVisible({ timeout: 8_000 })
  })

  superAdminTest('kendi hesabını deaktif etme butonu disabled/gizli', async ({ authenticatedPage: page }) => {
    await page.goto('/team')
    // Kendi satırı için toggle pasif veya yok olmalı
    const ownRow = page.locator('tr, [class*="row"]').filter({ hasText: SEED.superAdmin.email })
    await expect(ownRow).toBeVisible({ timeout: 10_000 })

    // Toggle disabled veya "Kendinizi deaktif edemezsiniz" uyarısı
    const toggle = ownRow.getByRole('switch')
    if (await toggle.count() > 0) {
      expect(await toggle.isDisabled()).toBeTruthy()
    }
  })

  superAdminTest('rol değiştirme çalışıyor (yeni üye)', async ({ authenticatedPage: page }) => {
    await page.goto('/team')

    // Önce yeni üye oluştur
    await page.getByRole('button', { name: /yeni üye|üye ekle|davet/i }).click()
    const dialog = page.locator('[role="dialog"]')
    const uniqueEmail = `roletest${Date.now()}@tablewise.com`
    await dialog.getByLabel(/e-?posta|email/i).fill(uniqueEmail)
    await dialog.getByLabel(/ad|isim|name/i).fill('Rol Test Üye')
    const roleSelect = dialog.getByLabel(/rol|role/i)
    if (await roleSelect.count() > 0) await roleSelect.selectOption('Marketing')
    const passwordInput = dialog.getByLabel(/şifre|password/i).first()
    if (await passwordInput.count() > 0) await passwordInput.fill('TestPass123!')
    await dialog.getByRole('button', { name: /ekle|davet|kaydet/i }).click()
    await expect(page.getByText(uniqueEmail)).toBeVisible({ timeout: 8_000 })

    // Rolü değiştir
    const newRow = page.locator('tr, [class*="row"]').filter({ hasText: uniqueEmail })
    const roleDropdown = newRow.getByRole('combobox')
    if (await roleDropdown.count() > 0) {
      await roleDropdown.selectOption('Finance')
      await expect(page.getByText(/kaydedildi|güncellendi|başarı/i)).toBeVisible({ timeout: 6_000 })
    }
  })
})
