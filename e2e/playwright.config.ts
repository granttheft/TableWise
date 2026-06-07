import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: '.',
  timeout: 30_000,
  retries: 1,
  workers: 1,
  reporter: [['html', { outputFolder: 'playwright-report' }], ['list']],
  use: {
    headless: true,
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'admin-panel',
      testDir: './admin-panel',
      use: {
        baseURL: 'http://localhost:3000',
        ...devices['Desktop Chrome'],
      },
    },
    {
      name: 'booking-ui',
      testDir: './booking-ui',
      use: {
        baseURL: 'http://localhost:5174',
        ...devices['Desktop Chrome'],
      },
    },
    {
      name: 'super-admin',
      testDir: './super-admin',
      use: {
        baseURL: 'http://localhost:3001',
        ...devices['Desktop Chrome'],
      },
    },
  ],
})
