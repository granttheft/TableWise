import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

test.describe('Super Admin — Auth', () => {
  test('geçerli credentials ile dashboard\'a yönlendiriyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel(/e-?posta|email/i).fill(SEED.superAdmin.email)
    await page.getByLabel(/şifre|parola|password/i).fill(SEED.superAdmin.password)
    await page.getByRole('button', { name: /giriş|login/i }).click()
    await page.waitForURL('**/dashboard', { timeout: 10_000 })
    await expect(page).toHaveURL(/\/dashboard/)
  })

  test('yanlış şifre hata gösteriyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel(/e-?posta|email/i).fill(SEED.superAdmin.email)
    await page.getByLabel(/şifre|parola|password/i).fill('YanlisParola!')
    await page.getByRole('button', { name: /giriş|login/i }).click()
    await expect(page.getByText(/hata|geçersiz|yanlış|invalid/i)).toBeVisible({ timeout: 8_000 })
  })

  test('auth guard çalışıyor', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })

  test('admin panel credentials super admin\'e çalışmıyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel(/e-?posta|email/i).fill(SEED.admin.email)
    await page.getByLabel(/şifre|parola|password/i).fill(SEED.admin.password)
    await page.getByRole('button', { name: /giriş|login/i }).click()
    // Dashboard'a gidememeli
    await expect(page.getByText(/hata|geçersiz|unauthorized/i)).toBeVisible({ timeout: 8_000 })
  })
})
