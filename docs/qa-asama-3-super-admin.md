# QA Aşama 3 — Super Admin E2E Testleri Tamamlama

## Bağlam

`e2e/super-admin/` dizininde şu an 4 dosya var:
- `auth.spec.ts` — giriş/çıkış
- `pricing-coupons.spec.ts` — fiyatlandırma ve kupon yönetimi
- `team.spec.ts` — ekip yönetimi
- `tenants.spec.ts` — tenant listesi/yönetimi

Fixtures:
- `adminTest` fixture: `import { adminTest } from '../fixtures/auth'` → bu Super Admin için de çalışmalı
- `SEED.superAdmin` = `{ email: 'admin@tablewise.com.tr', password: 'Admin123!' }`
- Super Admin portu: `3001` (playwright config'de ayrı project olarak tanımlı olabilir)

## Görev

`e2e/super-admin/` dizinine aşağıdaki **2 yeni spec dosyası** ekle.
Aynı zamanda mevcut `tenants.spec.ts` dosyasına ek testler ekle.

---

## Dosya 1: `tenant-lifecycle.spec.ts` (YENİ)

Tenant'ın tam yaşam döngüsü testleri — oluşturma, plan değiştirme, durum yönetimi.

Test senaryoları:
1. **Yeni tenant oluşturma formu açılıyor** — "Yeni Tenant" / "Ekle" butonuna tıklayınca form/modal açılıyor.
2. **Tenant oluşturma formu doldurulup kaydediliyor** — Restoran adı, slug, yetkili email gir, kaydet. Başarı feedback'i ve listede yeni tenant görünüyor.
3. **Tenant aktif/pasif yapılabiliyor** — Tenant satırındaki status toggle'ı veya dropdown'dan "Pasif yap" seçilince durum değişiyor. Tekrar aktif yapılabiliyor.
4. **Tenant planı değiştirilebiliyor** — Tenant detayında plan dropdown'u var ve değiştirilebiliyor (Starter → Pro gibi). Kaydet sonrası plan güncelleniyor.
5. **Tenant detay sayfası açılıyor** — Tenant satırına tıklanınca detay sayfası açılıyor; tenant adı, plan bilgisi, oluşturulma tarihi görünüyor.
6. **Tenant arama/filtreleme çalışıyor** — Arama kutusuna tenant adı yazılınca liste filtreleniyor. "Demo Restoran" arandığında seed tenant'ı çıkıyor.
7. **Silinen (soft delete) tenant listede görünmüyor** — Aktif tenant listesinde IsDeleted=true olanlar gösterilmiyor (UI'da "Aktif" filtresi varsayılan).

---

## Dosya 2: `payments-devices.spec.ts` (YENİ)

Ödemeler ve cihazlar sayfalarının temel render testleri (placeholder aşamasında olsalar bile).

**Ödemeler bölümü:**
1. **Ödemeler sayfası yükleniyor** — `/payments` veya sidebar'daki "Ödemeler" linki açılıyor; sayfa 200 dönüyor, bir başlık veya "henüz ödeme yok" mesajı görünüyor.
2. **Ödemeler tablosu veya placeholder render edilmiş** — Tablo varsa sütun başlıkları (tarih, tutar, tenant, durum gibi) görünüyor; yoksa "yakında" veya boş durum mesajı görünüyor.
3. **Sayfa çökmüyor** — Ödemeler sayfasında console error yok (page.on('console') ile logla, `error` seviyesindekiler test'i fail etmeli).

**Cihazlar bölümü:**
4. **Cihazlar sayfası yükleniyor** — `/devices` veya sidebar'daki "Cihazlar" linki açılıyor; sayfa render ediliyor.
5. **Cihaz listesi veya placeholder görünüyor** — Kayıtlı cihaz varsa liste; yoksa boş durum mesajı render edilmiş.
6. **Sayfa çökmüyor** — Console error yok.

---

## Mevcut Dosya: `tenants.spec.ts` (EKLENTİ)

Mevcut testler korunacak, şunlar **eklenecek**:

- **Tenant plan bilgisi görünüyor** — Tenant listesinde veya detayında plan adı (Starter/Pro/Enterprise) gösteriliyor.
- **Tenant oluşturma tarihi görünüyor** — Listede veya detayda `createdAt` tarihi render edilmiş.
- **Toplu işlem yapılabiliyor (varsa)** — Checkbox ile birden fazla tenant seçilip bulk action uygulanabiliyor (varsa bu özellik; yoksa test skip).

---

## Ek: Tenant İzolasyon Testi

`e2e/` kök dizinine `isolation.spec.ts` adında bir dosya oluştur (super-admin veya admin-panel altında değil, kök e2e klasöründe).

Test senaryoları:
1. **Admin A, Tenant B'nin datasını göremez** — Tenant A admin'i olarak giriş yap, Tenant B'nin reservation/table ID'leriyle API'ye istek at (`/api/v1/reservations/{tenantB_reservationId}`). Response 403 veya 404 olmalı, 200 olmamalı.
2. **Super Admin tüm tenant'ları görebilir** — `SEED.superAdmin` ile giriş yapılınca hem Tenant A hem diğer tenantlar listeleniyor.

Bu test için Playwright'ın `request` context'ini kullan (browser UI yerine doğrudan API çağrısı):
```typescript
import { test, expect } from '@playwright/test'
const { request } = test

test('tenant izolasyonu — cross-tenant erişim engelleniyor', async ({ request }) => {
  // 1. Tenant A admin token al
  // 2. Tenant B'nin resource ID'siyle istek at
  // 3. 403 veya 404 bekleniyor
})
```

---

## Ortak Kurallar

- Super Admin testleri `SEED.superAdmin` credential'larını kullanmalı
- Super Admin için ayrı fixture yoksa `test.beforeEach` içinde login yap (`/auth/login` formunu doldur)
- Playwright config'de super-admin için ayrı `project` tanımlıysa (`baseURL: http://localhost:3001`) o project altında çalıştır
- Placeholder sayfaları için "sayfa yüklendi ve çökmedi" yeterli assertion — içerik henüz tam olmayabilir
- Console error testi: `page.on('pageerror', err => errors.push(err))` ile yakala, test sonunda `expect(errors).toHaveLength(0)` yap
- Test isimleri Türkçe, describe isimleri tutarlı

## Çalıştırma

```bash
# Sadece super-admin testleri
cd e2e
npx playwright test super-admin/ --reporter=list

# İzolasyon testi
npx playwright test isolation.spec.ts --reporter=list

# Tüm E2E suite
npx playwright test --reporter=list
```

Aşama 3 tamamlandığında:
- Super Admin: 24 → 40+ test
- İzolasyon: 2 yeni test
- **Genel toplam: 67 → 100+ test**

---

## GÜNCELLEME — Özel Limitler Özelliği (Faz 11 Sonrası)

`tenant-lifecycle.spec.ts` dosyasına aşağıdaki testleri **ekle**:

8. **Tenant detay sayfasında "Özel Limitler" kartı görünüyor**
   - Tenant detay sayfası açılınca "Özel Limitler" başlıklı kart render edilmiş
   - 5 limit input'u var: Maks. Mekan, Maks. Masa, Maks. Kural, Aylık Maks. Rezervasyon, Maks. Personel

9. **Placeholder'larda plan limitleri gösteriliyor**
   - Boş input'ların placeholder'ında "Plan: X" formatında varsayılan limit yazıyor
   - Örn. Starter planındaki tenant için "Plan: 1" (maxVenues)

10. **Özel limit girilebiliyor ve kaydediliyor**
    - Maks. Mekan input'una "10" gir
    - "Limitleri Kaydet" butonuna tıkla
    - `PUT /api/platform/tenants/{id}/custom-limits` isteği gönderilmiş olmalı
    - Başarı toast'u görünüyor
    - "Özel limit aktif" badge'i beliriyor

11. **-1 girilince "Sınırsız" label'ı görünüyor**
    - Bir input'a "-1" gir
    - "Sınırsız" etiketi o input'un altında görünüyor

12. **X butonuyla tekil limit sıfırlanıyor**
    - Değer girilmiş input'un yanındaki X butonuna tıkla
    - Input boşalıyor, değer null'a dönüyor

13. **"Tüm Limitleri Sıfırla" butonu çalışıyor**
    - Butona tıkla → onay gerekiyorsa onayla
    - `PUT /api/platform/tenants/{id}/custom-limits` tüm null ile gönderiliyor
    - Başarı toast'u: "Tüm özel limitler kaldırıldı"
    - "Özel limit aktif" badge'i kayboluyor

14. **Özel limit olmayan tenant'ta badge görünmüyor**
    - Hiç custom limit atanmamış tenant detayında "Özel limit aktif" badge'i yok

Aşama 3 + güncelleme tamamlandığında super-admin test sayısı **40 → 47+** olmalı.
