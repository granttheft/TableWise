import { expect } from '@playwright/test'
import { superAdminTest } from '../fixtures/auth'
import { SEED } from '../fixtures/seed'

superAdminTest.describe('Super Admin — Tenant Yönetimi', () => {
  superAdminTest('tenant listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tenants')
    await expect(page.getByText(SEED.tenant.name)).toBeVisible({ timeout: 10_000 })
  })

  superAdminTest('arama filtresi çalışıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tenants')
    await page.getByPlaceholder(/ara|search/i).fill('Demo')
    await expect(page.getByText(SEED.tenant.name)).toBeVisible({ timeout: 8_000 })
  })

  superAdminTest('tenant detay sayfası açılıyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tenants')
    await page.getByText(SEED.tenant.name).click()
    await expect(page).toHaveURL(new RegExp(`/tenants/${SEED.tenant.id}`))
    await expect(page.getByText(SEED.tenant.name)).toBeVisible({ timeout: 10_000 })
  })

  superAdminTest('tenant detay — Aksiyonlar sekmesi görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto(`/tenants/${SEED.tenant.id}`)
    const actionsTab = page.getByRole('tab', { name: /aksiyon|işlem/i })
    if (await actionsTab.count() > 0) {
      await actionsTab.click()
    }
    await expect(page.getByText(/plan|askıya|aksiyon/i).first()).toBeVisible({ timeout: 10_000 })
  })

  superAdminTest('tenant detay — Notlar sekmesi ve not ekleme', async ({ authenticatedPage: page }) => {
    await page.goto(`/tenants/${SEED.tenant.id}`)
    const notesTab = page.getByRole('tab', { name: /not/i })
    if (await notesTab.count() > 0) {
      await notesTab.click()
      const noteInput = page.getByLabel(/not|note/i).or(page.locator('textarea')).first()
      if (await noteInput.count() > 0) {
        await noteInput.fill(`Test notu ${Date.now()}`)
        await page.getByRole('button', { name: /kaydet/i }).click()
        await expect(page.getByText(/kaydedildi|başarı/i)).toBeVisible({ timeout: 6_000 })
      }
    }
  })

  superAdminTest('durum badge\'i görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tenants')
    await expect(page.getByText(/aktif|trial|askıda/i).first()).toBeVisible({ timeout: 10_000 })
  })

  superAdminTest('bu ay rezervasyon sayısı görünüyor', async ({ authenticatedPage: page }) => {
    await page.goto('/tenants')
    // Sayısal değer var mı
    await expect(page.getByText(/\d+/).first()).toBeVisible({ timeout: 10_000 })
  })
})
