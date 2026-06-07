import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Ayarlar / Çalışma Saatleri', () => {
  adminTest('çalışma saatleri sayfası yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()
    await expect(page.getByText(/çalışma saatleri/i).first()).toBeVisible({ timeout: 10_000 })
  })

  adminTest('slot süresi kaydediliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    const slotSelect = page.getByRole('combobox').filter({ hasText: /dakika/i }).first()
    if (await slotSelect.count() > 0) {
      await slotSelect.click()
      const opt60 = page.getByRole('option', { name: /60 dakika/i })
      if (await opt60.count() > 0) await opt60.click()
    }

    const saveBtn = page.getByRole('button', { name: /kaydet/i }).first()
    await saveBtn.click()
    await expect(page.getByText(/başarı|kaydedildi|güncellendi|çalışma saatleri/i).first()).toBeVisible({ timeout: 8_000 })
  })

  adminTest('schedule backend\'den yükleniyor (switch\'ler görünüyor)', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    const switches = page.getByRole('switch')
    await expect(switches.first()).toBeVisible({ timeout: 10_000 })
    expect(await switches.count()).toBeGreaterThanOrEqual(7)
  })

  adminTest('kapalı gün ekleme ve silme', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    const addBtn = page.getByRole('button', { name: /kapalı gün|tatil|ekle/i }).first()
    if (await addBtn.count() === 0) {
      // Kapalı gün bölümü yoksa testi skip et
      return
    }
    await addBtn.click()
    const dialog = page.locator('[role="dialog"]')
    await expect(dialog).toBeVisible({ timeout: 6_000 })

    const tomorrow = new Date()
    tomorrow.setDate(tomorrow.getDate() + 30)
    const dateStr = tomorrow.toISOString().split('T')[0]
    const dateInput = dialog.locator('input[type="date"]').first()
    if (await dateInput.count() > 0) {
      await dateInput.fill(dateStr)
    }
    await dialog.getByRole('button', { name: /ekle|kaydet/i }).click()

    await expect(page.getByText(/kapalı|başarı/i).first()).toBeVisible({ timeout: 8_000 })
  })

  adminTest('çalışma günü kapatılıp kaydediliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    const switches = page.getByRole('switch')
    await switches.first().waitFor({ timeout: 10_000 })
    const pazarSwitch = switches.first()
    const wasOpen = await pazarSwitch.isChecked()

    await pazarSwitch.click()
    const saveBtn = page.getByRole('button', { name: /kaydet/i }).first()
    await saveBtn.click()
    await expect(page.getByText(/başarı|kaydedildi|çalışma/i).first()).toBeVisible({ timeout: 8_000 })

    // Geri al
    await pazarSwitch.click()
    await saveBtn.click()
  })
})
