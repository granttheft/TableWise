import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

test.describe('Super Admin — Auth', () => {
  test('geçerli credentials ile dashboard\'a yönlendiriyor', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email').fill(SEED.superAdmin.email)
    await page.locator('#password').fill(SEED.superAdmin.password)
    await page.getByRole('button', { name: /giriş yap/i }).click()
    await page.waitForURL('**/dashboard', { timeout: 15_000 })
    await expect(page).toHaveURL(/\/dashboard/)
  })

  test('yanlış şifre hata gösteriyor', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email').fill(SEED.superAdmin.email)
    await page.locator('#password').fill('YanlisParola!')
    await page.getByRole('button', { name: /giriş yap/i }).click()
    await expect(page.getByText(/geçersiz|hata|e-posta veya şifre/i)).toBeVisible({ timeout: 8_000 })
  })

  test('auth guard çalışıyor', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })

  test('admin panel credentials super admin\'e çalışmıyor', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email').fill(SEED.admin.email)
    await page.locator('#password').fill(SEED.admin.password)
    await page.getByRole('button', { name: /giriş yap/i }).click()
    await expect(page.getByText(/geçersiz|hata|e-posta veya şifre/i)).toBeVisible({ timeout: 8_000 })
  })
})
