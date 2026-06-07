import { test, expect } from '@playwright/test'
import { SEED } from '../fixtures/seed'

test.describe('Admin Panel — Auth', () => {
  test('geçerli credentials ile dashboard\'a yönlendiriyor', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email').fill(SEED.admin.email)
    await page.locator('#password').fill(SEED.admin.password)
    await page.getByRole('button', { name: 'Giriş Yap' }).click()
    await page.waitForURL('**/dashboard', { timeout: 15_000 })
    await expect(page).toHaveURL(/\/dashboard/)
  })

  test('yanlış şifre ile hata mesajı gösteriyor', async ({ page }) => {
    await page.goto('/login')
    await page.locator('#email').fill(SEED.admin.email)
    await page.locator('#password').fill('YanlisParola123!')
    await page.getByRole('button', { name: 'Giriş Yap' }).click()
    // Sonnet toast veya hata mesajı bekleniyor
    await expect(page.getByText(/hata|geçersiz|başarısız|yanlış/i)).toBeVisible({ timeout: 8_000 })
    await expect(page).toHaveURL(/\/login/)
  })

  test('boş form submit edilince input\'ta kalıyor', async ({ page }) => {
    await page.goto('/login')
    await page.getByRole('button', { name: 'Giriş Yap' }).click()
    // Zod validasyon hatası veya HTML5 required — her iki durumda da login'de kalmalı
    await expect(page).toHaveURL(/\/login/)
  })

  test('authenticated olmadan /dashboard açmak login\'e yönlendiriyor', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })
})
