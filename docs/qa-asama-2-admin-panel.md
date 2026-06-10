# QA Aşama 2 — Admin Panel E2E Testleri Derinleştirme

## Bağlam

`e2e/admin-panel/` dizininde şu an 8 dosya var:
- `auth.spec.ts`, `customers.spec.ts`, `dashboard.spec.ts`, `reservations.spec.ts`
- `rules.spec.ts` — sadece 4 yüzeysel test (modal açılıyor, toggle çalışıyor — kural oluşturma yok)
- `settings.spec.ts`, `staff.spec.ts`, `tables.spec.ts`

Fixtures:
- `adminTest` fixture: `import { adminTest } from '../fixtures/auth'` — authenticated admin session sağlar
- `SEED`: `import { SEED } from '../fixtures/seed'` — test verileri (tenant, venue, tables, customers)

## Görev

`e2e/admin-panel/` dizinine aşağıdaki **3 yeni spec dosyası** ekle.
Aynı zamanda mevcut `rules.spec.ts` dosyasını derinleştir (üzerine yaz).

---

## Dosya 1: `venues.spec.ts` (YENİ)

Mekan (venue) yönetimi testleri.

Test senaryoları:
1. **Mekan listesi yükleniyor** — `/venues` sayfası açılınca `SEED.venue.name` ("Ana Salon") görünüyor.
2. **Yeni mekan oluşturma modalı açılıyor** — "Yeni Mekan" veya "Ekle" butonuna tıklayınca dialog açılıyor.
3. **Mekan oluşturma formu doldurulup kaydediliyor** — Modal içine mekan adı gir, kaydet. Başarı toast'u veya liste güncellenmesi bekleniyor.
4. **Mekan detayı açılıyor** — Mevcut mekan satırına/kartına tıklanınca detay sayfası veya drawer açılıyor.
5. **Mekan düzenleme çalışıyor** — Edit/düzenle butonuna tıklanınca form açılıyor, içerik düzenlenebiliyor.
6. **Kapasite bilgisi görünüyor** — Mekan listesinde veya detayında masa sayısı veya kapasite bilgisi var.

---

## Dosya 2: `working-hours.spec.ts` (YENİ)

Çalışma saatleri yönetimi testleri.

Test senaryoları:
1. **Çalışma saatleri sayfası/section'ı yükleniyor** — `/settings` veya `/venues/:id/hours` veya ilgili route'da çalışma saatleri görünüyor. Gün isimleri (Pazartesi, Salı... veya Monday, Tuesday...) render edilmiş.
2. **Gün aktif/pasif toggle çalışıyor** — Bir günün toggle'ına tıklanınca durum değişiyor (örn. Pazar kapalı yapılabiliyor).
3. **Açılış/kapanış saati düzenlenebiliyor** — Saat input'larına yeni değer girilebiliyor.
4. **Değişiklikler kaydediliyor** — Kaydet/güncelle butonuna tıklanınca başarı feedback'i geliyor (toast veya "kaydedildi" mesajı).
5. **Tüm günler görünüyor** — 7 gün listeleniyor (Pazartesi–Pazar veya Monday–Sunday).
6. **Kapalı günde saat inputları disabled/gizli** — Gün kapalıyken saat alanları etkisizleşiyor veya gizleniyor.

---

## Dosya 3: `salon-view.spec.ts` (YENİ)

Gerçek zamanlı salon/timeline görünümü testleri.

Test senaryoları:
1. **Salon görünümü sayfası yükleniyor** — `/salon` veya `/timeline` veya dashboard'daki salon sekmesi açılıyor. Masa listesi (T-01, T-02... veya seed'deki isimler) görünüyor.
2. **Doluluk legend/ikonları görünüyor** — "Dolu", "Onay bekliyor", "Boş" gibi status indicator'ları render edilmiş.
3. **Şimdiki zaman göstergesi var** — "Şimdi" veya current time line görünüyor (timeline varsa).
4. **Masa slot'larına tıklanınca detay açılıyor** — Mevcut bir rezervasyon bloğuna tıklanınca drawer/modal açılıyor ve rezervasyon bilgileri görünüyor.
5. **Doluluk oranı göstergesi var** — `%87` gibi doluluk yüzdesi veya istatistik göstergesi sayfada render edilmiş (dashboard altında veya salon view'da).
6. **Gün değiştirme çalışıyor** — İleri/geri ok butonlarıyla farklı güne geçilebiliyor, sayfa yeniden yükleniyor veya data güncelleniyor.

---

## Mevcut Dosya: `rules.spec.ts` (GÜNCELLEŞTİR)

Mevcut 4 test korunacak, şunlar **eklenecek**:

5. **Kural oluşturma — tam form doldurulup kaydediliyor**
   - "Yeni Kural" / "Kural Ekle" butonuna tıkla
   - Modal açılınca kural adı gir (örn. "Test Kuralı E2E")
   - Kural tipini seç (combobox/dropdown)
   - Bir koşul ekle (varsa "Koşul Ekle" butonu)
   - Kaydet butonuna tıkla
   - Başarı toast'u veya kural listesinde yeni kural görünüyor

6. **Kural silme çalışıyor**
   - Listeden bir kural seç (önceki testte oluşturulan veya seed'deki)
   - Sil / delete butonuna tıkla
   - Onay dialog'u çıkıyorsa onayla
   - Kural listeden kayboluyor

7. **Kural öncelik sırası (priority) görünüyor**
   - Kural listesinde priority/öncelik alanı var veya kurallar sıralı gösteriliyor

8. **Birden fazla kural varken toggle sırası bozulmuyor**
   - Birden fazla kural varken (seed kuralları) her toggle bağımsız çalışıyor
   - Bir kuralın toggle'ı diğerini etkilemiyor

---

## Ortak Kurallar (tüm dosyalar için)

- `import { adminTest } from '../fixtures/auth'` ve `import { SEED } from '../fixtures/seed'` kullan
- Her test `adminTest()` ile yazılmalı — authenticated session gerekiyor
- Route'lar uygulamaya göre değişebilir: ilk önce `/venues`, `/settings`, `/salon` dene; bulunamazsa navbar/sidebar linklerinden navigate et
- Form submit sonrası network isteği bekleme: `await page.waitForResponse(resp => resp.url().includes('/api/') && resp.status() < 400, { timeout: 10_000 })`
- Modal kapandıktan sonra veya liste yenilendikten sonra assert yap (hemen değil)
- Timeout'lar: `{ timeout: 10_000 }` DOM için, `{ timeout: 15_000 }` network için
- Test isimleri Türkçe olsun (mevcut dosyalarla tutarlı)
- `describe` block ismi: `adminTest.describe('Admin Panel — [Modül Adı]', ...)`

## Çalıştırma

```bash
cd e2e
npx playwright test admin-panel/ --reporter=list
```

Aşama 2 tamamlandığında admin-panel test sayısı 34'ten 55+'e çıkmalı.

---

## GÜNCELLEME — Özel Limitler Özelliği (Faz 11 Sonrası)

Özel limitler özelliği tamamlandıktan sonra aşağıdaki **yeni dosyayı** da ekle.

### Yeni Dosya: `plan-usage.spec.ts`

Dashboard'daki plan kullanım widget'ı ve limit enforcement testleri.

Test senaryoları:

1. **Plan kullanım widget'ı dashboard'da görünüyor**
   - Dashboard açılınca "Plan Kullanımı" başlıklı kart render edilmiş
   - Mekan, Masa, Kural, Aylık Rezervasyon satırları görünüyor

2. **Mevcut kullanım sayıları doğru gösteriliyor**
   - Her satırda "X / Y" formatında sayı var (örn. "2 / 3")
   - Veya sınırsızsa "X / ∞" formatında

3. **Progress bar'lar render ediliyor**
   - Her limit satırı için bir progress bar mevcut
   - Renk: normal → primary, %80+ → amber, dolu → kırmızı (class veya style kontrol et)

4. **Sınırsız limitlerde progress bar gösterilmiyor**
   - `maxVenues: null` (sınırsız) olan limitin satırında progress bar yok veya boş

5. **Custom limit uygulandığında badge görünüyor**
   - Super Admin'den tenant'a özel limit atanmışsa "Özel limitler uygulanıyor" badge'i görünüyor
   - `GET /api/v1/tenant/me/plan-limits` response'u mock'la: `hasCustomLimits: true`

6. **Venue create butonu limit dolunca disabled oluyor**
   - `GET /api/v1/tenant/me/plan-limits` → `maxVenues: 1, currentVenueCount: 1` mock'la
   - Venue oluşturma butonuna tıkla → disabled veya tıklanamıyor olmalı
   - Tooltip veya uyarı mesajı görünüyor

7. **Kural create butonu limit dolunca disabled oluyor**
   - `maxRules: 2, currentRuleCount: 2` mock'la
   - Kural ekleme butonu disabled olmalı

8. **Plan kullanım API'si çağrılıyor**
   - Dashboard yüklenince `GET /api/v1/tenant/me/plan-limits` isteği atılmış olmalı
   - `page.waitForRequest('**/plan-limits')` ile doğrula

Teknik not: API mock için `page.route()` kullan:
```typescript
await page.route('**/api/v1/tenant/me/plan-limits', route => {
  route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({
      maxVenues: 1, currentVenueCount: 1,
      maxTables: 50, currentTableCount: 12,
      maxRules: 5, currentRuleCount: 4,
      maxReservationsPerMonth: 200, currentReservationCount: 160,
      hasCustomLimits: false,
    }),
  })
})
```

Aşama 2 + güncelleme tamamlandığında admin-panel test sayısı **55 → 63+** olmalı.
