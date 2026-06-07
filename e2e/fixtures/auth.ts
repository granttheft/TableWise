import { test as base, type Page } from '@playwright/test'
import { SEED } from './seed'

async function loginAdmin(page: Page) {
  await page.goto('/login')
  await page.locator('#email').fill(SEED.admin.email)
  await page.locator('#password').fill(SEED.admin.password)
  await page.getByRole('button', { name: 'Giriş Yap' }).click()
  await page.waitForURL('**/dashboard', { timeout: 15_000 })
}

async function loginSuperAdmin(page: Page) {
  await page.goto('/login')
  await page.locator('#email').fill(SEED.superAdmin.email)
  await page.locator('#password').fill(SEED.superAdmin.password)
  await page.getByRole('button', { name: /giriş yap/i }).click()
  await page.waitForURL('**/dashboard', { timeout: 15_000 })
}

export const adminTest = base.extend<{ authenticatedPage: Page }>({
  authenticatedPage: async ({ page }, use) => {
    await loginAdmin(page)
    await use(page)
  },
})

export const superAdminTest = base.extend<{ authenticatedPage: Page }>({
  authenticatedPage: async ({ page }, use) => {
    await loginSuperAdmin(page)
    await use(page)
  },
})

export { loginAdmin, loginSuperAdmin }
