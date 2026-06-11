# QA Aşama 1 — Booking UI E2E Testleri

## Bağlam

> ⚠️ **Kritik Fix 1 Notu:** `ReservationStatus` artık PascalCase.
> Tüm durum string karşılaştırmalarında şunu kullan:
> `'Pending'` `'Confirmed'` `'Seated'` `'Completed'` `'Cancelled'` `'NoShow'`
> `'pending'`, `'confirmed'` gibi küçük harf kullanma — backend PascalCase döndürüyor.

`e2e/booking-ui/` dizininde şu an 3 dosya var:
- `booking-flow.spec.ts` — sadece 1 happy path testi, çok yüzeysel
- `validation.spec.ts` — form validasyonları
- `view-modify-cancel.spec.ts` — görüntüleme/iptal akışları

Fixtures şu şekilde kurulu:
- `e2e/fixtures/seed.ts` → `SEED` sabitleri (tenant slug: `demo-restoran`, admin email/password vs.)
- `e2e/fixtures/auth.ts` → `adminTest` fixture (authenticated admin context için)
- Booking UI testleri `test, expect` from `@playwright/test` kullanıyor (auth fixture YOK, public akış)

## Görev

`e2e/booking-ui/` dizinine aşağıdaki **4 yeni spec dosyası** ekle.
Mevcut dosyalara dokunma, sadece yeni dosyalar oluştur.

---

## Dosya 1: `rule-enforcement.spec.ts`

Kural motorunun müşteri tarafında doğru çalıştığını test eder.

Test senaryoları:
1. **Kural ihlali toast/hata mesajı gösteriyor** — Seed'deki kural ihlali tetiklenince (örn. grup kompozisyonu hatalıysa) UI'da hata mesajı görünmeli. `/api/v1/reservations` POST'u mock etmeden, gerçek API'ye istek atılmalı; response'ta `ruleViolation` veya `kural ihlali` içeren bir mesaj bekleniyor.
2. **Blacklisted müşteri rezervasyon yapamıyor** — `SEED.customers.burak` Blacklisted tier'da. Bu müşterinin telefon numarasıyla (seed'de varsa) rezervasyon tamamlanamıyor, uygun hata gösteriliyor.
3. **Kural uyarısı (warning) ile rezervasyon tamamlanabiliyor** — Eğer kural sadece uyarı (warning) ise ve bloklamıyorsa, müşteri yine de devam edebilmeli. Akış tamamlanmalı.
4. **Kural yokken akış engelsiz devam ediyor** — Normal happy path'de hiçbir kural ihlali mesajı çıkmıyor.

Pattern: `booking-flow.spec.ts`'deki `getNextWeekday()` helper'ını bu dosyada da tanımla (tekrar et, import etme — her dosya bağımsız çalışmalı).

---

## Dosya 2: `group-composition.spec.ts`

Grup kompozisyonu adımının doğru çalıştığını test eder.

Test senaryoları:
1. **Grup kompozisyonu adımı görünüyor** — Rezervasyon akışında grup tipi seçim alanı (mixed/male/female veya benzeri) render ediliyor.
2. **Grup tipi seçimi yapılabiliyor** — Bir seçenek seçilince seçili state doğru yansıyor (button veya radio seçili görünüyor).
3. **IsRequired=false iken atlama butonu var** — "Atla" veya benzeri bir buton görünüyor ve tıklanınca bir sonraki adıma geçiyor; hata yok.
4. **Grup tipi seçilip devam edilebiliyor** — Seçim yapılıp "Devam Et" tıklayınca bir sonraki step'e geçiyor.
5. **Kişi sayısı input'u varsa minimum/maksimum validasyonu çalışıyor** — Negatif veya 0 girilince hata, geçerli sayı girilince devam ediliyor.

---

## Dosya 3: `capacity-and-availability.spec.ts`

Kapasite ve müsaitlik sınırlarını test eder.

Test senaryoları:
1. **Sadece müsait slotlar listeleniyor** — Slot listesinde görünen butonların hepsi tıklanabilir (disabled değil) veya disabled olanlar "dolu" gibi bir label içeriyor.
2. **Seçilen kapasiteye uygun masalar gösteriliyor** — Kişi sayısı seçildiğinde (örn. 6 kişi) masa listesinde kapasitesi yetersiz masalar (2 kişilik) gösterilmiyor veya disabled.
3. **Saat seçimi olmadan ilerleme engellenebiliyor** — Tarih seçilip slot seçilmeden "Devam Et" tıklanınca kullanıcı bir sonraki adıma geçemiyor ya da uyarı alıyor.
4. **Geçmiş tarih seçilemiyor** — Takvimde geçmiş günler disabled görünüyor veya seçilince slot gelmiyor.

---

## Dosya 4: `reservation-confirmation.spec.ts`

Onay sayfasının ve rezervasyon tamamlama sonrası durumun testi.

Test senaryoları:
1. **Onay sayfası doğru rezervasyon bilgilerini gösteriyor** — `/rezervasyon/onay/:code` veya benzeri URL'e ulaşınca ad, tarih, saat bilgileri görünüyor.
2. **Rezervasyon kodu/linki sayfada görünüyor** — Başarılı rezervasyon sonrası confirmation sayfasında bir kod, barkod veya "rezervasyonunuzu yönetmek için" linki var.
3. **Geçersiz rezervasyon koduyla onay sayfasına gidilince hata gösteriliyor** — `/rezervasyon/onay/GECERSIZ-KOD` → hata mesajı veya yönlendirme.
4. **Rezervasyon özeti sayfasında iptal butonu var** — Aktif rezervasyonun özet sayfasında iptal seçeneği mevcut.
5. **Rezervasyon durumu PascalCase gösteriliyor** — Confirmation sayfasında durum etiketi `'Pending'` veya `'Confirmed'` içeriyor (küçük harf `'pending'` değil):
   ```typescript
   const statusText = await page.locator('[data-status], [class*="status"]').textContent()
   expect(statusText).toMatch(/Pending|Confirmed|Onay Bekliyor|Onaylandı/i)
   // 'pending' veya 'confirmed' küçük harf görünmemeli:
   expect(statusText).not.toMatch(/^pending$|^confirmed$/)
   ```

---

## Ortak Kurallar (tüm dosyalar için)

- `import { test, expect } from '@playwright/test'` ve `import { SEED } from '../fixtures/seed'` kullan
- `adminTest` fixture'ı kullanma — bu public booking akışı
- Her test bağımsız çalışabilmeli (önceki testin state'ine bağımlı olma)
- Timeout'lar: network bekleme için `{ timeout: 15_000 }`, DOM için `{ timeout: 10_000 }`
- Opsiyonel elementler için `catch(() => false)` pattern'ını kullan (mevcut dosyalardaki gibi)
- `getNextWeekday()` helper'ını ihtiyaç duyan her dosyada tekrar tanımla
- Eğer bir UI elementi bulunamazsa (step skip gibi) test fail etmemeli — graceful skip yap
- Test isimleri Türkçe olsun (mevcut dosyalarla tutarlı)

## Çalıştırma

```bash
cd e2e
npx playwright test booking-ui/ --reporter=list
```

Tüm yeni testler mevcut 9 testle birlikte çalışmalı. Toplam booking-ui test sayısı 20+ olmalı.
