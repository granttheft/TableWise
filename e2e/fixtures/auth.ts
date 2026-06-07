import { test as base, type Page } from '@playwright/test'
import { SEED } from './seed'

// Admin panel login helper
async function loginAdmin(page: Page) {
  await page.goto('/login')
  await page.getByLabel(/e-?posta|email/i).fill(SEED.admin.email)
  await page.getByLabel(/şifre|parola|password/i).fill(SEED.admin.password)
  await page.getByRole('button', { name: /giriş|login/i }).click()
  await page.waitForURL('**/dashboard', { timeout: 10_000 })
}

// Super admin login helper
async function loginSuperAdmin(page: Page) {
  await page.goto('/login')
  await page.getByLabel(/e-?posta|email/i).fill(SEED.superAdmin.email)
  await page.getByLabel(/şifre|parola|password/i).fill(SEED.superAdmin.password)
  await page.getByRole('button', { name: /giriş|login/i }).click()
  await page.waitForURL('**/dashboard', { timeout: 10_000 })
}

// Admin panel test fixture
export const adminTest = base.extend<{ authenticatedPage: Page }>({
  authenticatedPage: async ({ page }, use) => {
    await loginAdmin(page)
    await use(page)
  },
})

// Super admin test fixture
export const superAdminTest = base.extend<{ authenticatedPage: Page }>({
  authenticatedPage: async ({ page }, use) => {
    await loginSuperAdmin(page)
    await use(page)
  },
})

export { loginAdmin, loginSuperAdmin }
