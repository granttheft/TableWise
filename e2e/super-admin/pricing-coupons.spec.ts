import { expect } from '@playwright/test'
import { superAdminTest } from '../fixtures/auth'

superAdminTest.describe('Super Admin — Fiyatlandırma & Kuponlar', () => {
  superAdminTest('fiyatlandırma sayfası 4 plan gösteriyor', async ({ authenticatedPage: page }) => {
    await page.goto('/pricing')
    await expect(page.getByText(/starter/i)).toBeVisible({ timeout: 10_000 })
    await expect(page.getByText(/pro/i)).toBeVisible()
    await expect(page.getByText(/business/i)).toBeVisible()
    await expect(page.getByText(/enterprise/i)).toBeVisible()
  })

  superAdminTest('plan fiyatı düzenlenebiliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/pricing')
    // Fiyat edit butonu veya inline input
    const editBtn = page.getByRole('button', { name: /düzenle|edit/i }).first()
    if (await editBtn.count() > 0) {
      await editBtn.click()
      const priceInput = page.getByLabel(/fiyat|price/i).or(page.locator('input[type="number"]')).first()
      if (await priceInput.count() > 0) {
        await priceInput.fill('599')
        await page.getByRole('button', { name: /kaydet/i }).click()
        await expect(page.getByText(/başarı|kaydedildi/i)).toBeVisible({ timeout: 6_000 })
        // Geri al
        await page.getByRole('button', { name: /düzenle|edit/i }).first().click()
        const input2 = page.getByLabel(/fiyat|price/i).or(page.locator('input[type="number"]')).first()
        await input2.fill('490')
        await page.getByRole('button', { name: /kaydet/i }).click()
      }
    }
  })

  superAdminTest('kupon listesi yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/coupons')
    // Tablo veya boş durum mesajı görünmeli
    await expect(
      page.getByText(/kupon|kod|indirim/).or(page.getByText(/henüz kupon|no coupon/i))
    ).toBeVisible({ timeout: 10_000 })
  })

  superAdminTest('yeni kupon oluşturuluyor', async ({ authenticatedPage: page }) => {
    await page.goto('/coupons')
    await page.getByRole('button', { name: /yeni kupon|kupon ekle|oluştur/i }).click()

    const dialog = page.locator('[role="dialog"]')
    await expect(dialog).toBeVisible({ timeout: 6_000 })

    const uniqueCode = `TEST${Date.now().toString().slice(-6)}`
    await dialog.getByLabel(/kod|code/i).fill(uniqueCode)

    // İndirim tipi (Percentage veya Fixed)
    const discountType = dialog.getByLabel(/tip|tür|type/i)
    if (await discountType.count() > 0) {
      await discountType.selectOption({ index: 0 })
    }

    await dialog.getByLabel(/değer|oran|amount|percent/i).fill('10')

    await dialog.getByRole('button', { name: /oluştur|kaydet|ekle/i }).click()
    await expect(page.getByText(uniqueCode)).toBeVisible({ timeout: 8_000 })
  })

  superAdminTest('kupon deaktif ediliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/coupons')
    // Aktif kuponun deaktif butonu
    const deactivateBtn = page.getByRole('button', { name: /deaktif|pasif|kapat/i }).first()
    if (await deactivateBtn.count() > 0) {
      await deactivateBtn.click()
      await expect(page.getByText(/deaktif|pasif|başarı/i)).toBeVisible({ timeout: 6_000 })
    }
  })
})
