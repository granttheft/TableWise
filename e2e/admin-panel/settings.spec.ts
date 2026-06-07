import { expect } from '@playwright/test'
import { adminTest } from '../fixtures/auth'

adminTest.describe('Admin Panel — Ayarlar / Çalışma Saatleri', () => {
  adminTest('çalışma saatleri sayfası yükleniyor', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    // Çalışma saatleri sekmesi / bölümü
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()
    await expect(page.getByText(/çalışma saatleri/i)).toBeVisible({ timeout: 10_000 })
  })

  adminTest('slot süresi kaydediliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    // Slot süresi select
    const slotSelect = page.getByRole('combobox').filter({ hasText: /dakika/i })
    if (await slotSelect.count() > 0) {
      await slotSelect.click()
      await page.getByRole('option', { name: '60 dakika' }).click()
    }

    await page.getByRole('button', { name: /kaydet/i }).click()
    await expect(page.getByText(/başarı|kaydedildi|güncellendi/i)).toBeVisible({ timeout: 8_000 })
  })

  adminTest('schedule backend\'den yükleniyor (switch\'ler görünüyor)', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    // 7 günlük switch/toggle
    const switches = page.getByRole('switch')
    await expect(switches.first()).toBeVisible({ timeout: 10_000 })
    expect(await switches.count()).toBeGreaterThanOrEqual(7)
  })

  adminTest('kapalı gün ekleme ve silme', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    // "Kapalı Gün Ekle" butonu
    await page.getByRole('button', { name: /kapalı gün ekle/i }).click()
    const dialog = page.locator('[role="dialog"]')
    await expect(dialog).toBeVisible({ timeout: 6_000 })

    // Tarih seç (gelecekte bir tarih)
    const tomorrow = new Date()
    tomorrow.setDate(tomorrow.getDate() + 30)
    const dateStr = tomorrow.toISOString().split('T')[0]
    await dialog.getByLabel(/tarih/i).fill(dateStr)
    await dialog.getByRole('button', { name: /ekle|kaydet/i }).click()

    // Listede görünüyor
    await expect(page.getByText(/kapalı/i)).toBeVisible({ timeout: 8_000 })

    // Sil
    const deleteBtn = page.getByRole('button', { name: /sil/i }).last()
    await deleteBtn.click()
    await expect(page.getByText(/silindi|başarı/i)).toBeVisible({ timeout: 6_000 })
  })

  adminTest('çalışma günü kapatılıp kaydediliyor', async ({ authenticatedPage: page }) => {
    await page.goto('/settings')
    const whTab = page.getByRole('tab', { name: /çalışma saatleri/i })
    if (await whTab.count() > 0) await whTab.click()

    // Pazar (ilk switch)
    const switches = page.getByRole('switch')
    await switches.first().waitFor({ timeout: 10_000 })
    const pazarSwitch = switches.first()
    const wasOpen = await pazarSwitch.isChecked()

    // Durumu değiştir
    await pazarSwitch.click()
    await page.getByRole('button', { name: /kaydet/i }).click()
    await expect(page.getByText(/başarı|kaydedildi/i)).toBeVisible({ timeout: 8_000 })

    // Geri al
    await pazarSwitch.click()
    await page.getByRole('button', { name: /kaydet/i }).click()

    if (!wasOpen) {
      // Başlangıçta kapalıysa geri açık kalsın — test nötr
    }
  })
})
