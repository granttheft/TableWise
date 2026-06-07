import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

test.describe('Admin Panel — Auth', () => {
  test('geçerli credentials ile dashboard\'a yönlendiriyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel(/e-?posta|email/i).fill(SEED.admin.email)
    await page.getByLabel(/şifre|parola|password/i).fill(SEED.admin.password)
    await page.getByRole('button', { name: /giriş|login/i }).click()
    await page.waitForURL('**/dashboard')
    await expect(page).toHaveURL(/\/dashboard/)
  })

  test('yanlış şifre ile hata mesajı gösteriyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel(/e-?posta|email/i).fill(SEED.admin.email)
    await page.getByLabel(/şifre|parola|password/i).fill('YanlisParola123!')
    await page.getByRole('button', { name: /giriş|login/i }).click()
    await expect(page.getByText(/hata|geçersiz|yanlış|incorrect|invalid/i)).toBeVisible({ timeout: 8_000 })
    await expect(page).toHaveURL(/\/login/)
  })

  test('boş form ile validasyon hatası gösteriyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByRole('button', { name: /giriş|login/i }).click()
    // HTML5 validation ya da custom error mesajı görünmeli
    const hasError = await page.locator('[data-invalid], [aria-invalid], .text-destructive, [role="alert"]').count()
    expect(hasError).toBeGreaterThan(0)
  })

  test('login sonrası logout dashboard\'a gitmiyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel(/e-?posta|email/i).fill(SEED.admin.email)
    await page.getByLabel(/şifre|parola|password/i).fill(SEED.admin.password)
    await page.getByRole('button', { name: /giriş|login/i }).click()
    await page.waitForURL('**/dashboard')

    await page.getByRole('button', { name: /çıkış|logout|çık/i }).click()
    await expect(page).toHaveURL(/\/login/)
  })

  test('authenticated olmadan /dashboard açmak login\'e yönlendiriyor', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })
})
