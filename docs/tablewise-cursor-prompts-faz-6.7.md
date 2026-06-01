# Tablewise — Cursor Promptları: Faz 6.7 (Çok Dil Desteği / i18n)

> Her prompttan önce mutlaka **[GENEL BAĞLAMI YAPISTIR]** bloğunu ekle.
> Konum: Faz 6.5 (WhatsApp) bitince, Faz 7 (İyzico) başlamadan önce.
> Çelişki olursa ürün dökümanı (.docx) kazanır.

---

## 🎯 Faz 6.7 Amacı

Tüm frontend'lere (Booking UI + Admin Panel) çok dil desteği eklemek.
Turistik mekanlarda yabancı müşteri deneyimi için kritik.
**Şu an eklemek en kolay zaman** — ileriye bırakılırsa yüzlerce dosyada
metin aramak gerekir.

---

## 🌍 Desteklenecek Diller

| Kod | Dil | Öncelik | Not |
|-----|-----|---------|-----|
| `tr` | Türkçe | ✅ Birincil | Tam çeviri |
| `en` | İngilizce | ✅ Birincil | Tam çeviri |
| `de` | Almanca | 🟡 Önemli | Antalya/turizm |
| `ru` | Rusça | 🟡 Önemli | Antalya/turizm |
| `ar` | Arapça | 🟡 Önemli | Körfez/İstanbul — RTL |
| `fr` | Fransızca | 🟠 Orta | Turizm |
| `es` | İspanyolca | 🟠 Orta | Turizm |
| `it` | İtalyanca | 🟠 Orta | Turizm |
| `uk` | Ukraynaca | 🟠 Orta | Türkiye'deki nüfus |
| `zh` | Çince | 🔵 İleride | Büyüyen turizm |
| `ja` | Japonca | 🔵 İleride | Niş ama değerli |
| `nl` | Hollandaca | 🔵 İleride | Turizm |

**MVP için tam çeviri:** Türkçe + İngilizce
**Diğerleri:** Dil dosyası oluşturulur, eksik anahtarlar Türkçe'ye fallback

---

## Prompt 6.7.1 — i18n Altyapısı + Dil Dosyaları

**Model:** Sonnet 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 6.7.1 — Çok dil desteği altyapısı (i18n).

Her iki frontend'e de uygulanacak:
- frontend/booking-ui
- frontend/admin-panel

KÜTÜPHANELer:
- i18next
- react-i18next
- i18next-browser-languagedetector (tarayıcı dili algılama)
- i18next-http-backend (dil dosyalarını lazy load)

1. Kurulum (her iki proje için):
   npm install i18next react-i18next i18next-browser-languagedetector
   npm install i18next-http-backend

2. Klasör yapısı (her iki projede de):
   public/
   └── locales/
       ├── tr/
       │   └── translation.json   ← Tam dolu
       ├── en/
       │   └── translation.json   ← Tam dolu
       ├── de/
       │   └── translation.json   ← Kısmi (eksikler TR fallback)
       ├── ru/
       │   └── translation.json   ← Kısmi
       ├── ar/
       │   └── translation.json   ← Kısmi (RTL)
       ├── fr/
       │   └── translation.json   ← Kısmi
       ├── es/
       │   └── translation.json   ← Kısmi
       ├── it/
       │   └── translation.json   ← Kısmi
       └── uk/
           └── translation.json   ← Kısmi

3. i18n konfigürasyonu (src/i18n.ts her iki projede):
   - Varsayılan dil: 'tr'
   - Fallback dil: 'tr' (eksik anahtar varsa Türkçe göster)
   - Tarayıcı dili otomatik algıla (i18next-browser-languagedetector)
   - Algılama sırası: localStorage → navigator.language → 'tr'
   - Lazy loading: dil dosyaları ihtiyaç olunca yüklensin

4. BOOKING UI — Türkçe dil dosyası (tam):
   Tüm sabit metinleri kapsamalı:
   {
     "common": {
       "next": "Devam Et",
       "back": "Geri",
       "confirm": "Onayla",
       "cancel": "İptal",
       "save": "Kaydet",
       "close": "Kapat",
       "loading": "Yükleniyor...",
       "error": "Bir hata oluştu",
       "required": "Bu alan zorunludur"
     },
     "booking": {
       "title": "Rezervasyon Yap",
       "selectDate": "Tarih Seçin",
       "selectTime": "Saat Seçin",
       "guestCount": "Kişi Sayısı",
       "guestInfo": "Misafir Bilgileri",
       "name": "Ad Soyad",
       "phone": "Telefon",
       "email": "E-posta",
       "notes": "Özel İstek",
       "summary": "Rezervasyon Özeti",
       "confirmed": "Rezervasyonunuz Onaylandı!",
       "confirmCode": "Onay Kodunuz",
       "whatsappConsent": "WhatsApp bildirimleri almak istiyorum",
       "privacyPolicy": "Gizlilik Politikası",
       "noSlots": "Bu tarih için uygun saat bulunmamaktadır",
       "ruleRejected": "Bu rezervasyon mekan kurallarına uymamaktadır",
       "depositRequired": "Bu rezervasyon için kapora gerekmektedir",
       "depositAmount": "Kapora Tutarı",
       "payNow": "Ödeme Yap"
     },
     "errors": {
       "networkError": "Bağlantı hatası, lütfen tekrar deneyin",
       "sessionExpired": "Oturumunuz sona erdi",
       "slotTaken": "Bu saat başkası tarafından alındı, lütfen başka saat seçin"
     }
   }

5. BOOKING UI — İngilizce dil dosyası (tam çeviri):
   Yukarıdakinin tam İngilizce karşılığı.

6. BOOKING UI — Diğer diller (kısmi — sadece booking namespace):
   Almanca, Rusça, Arapça, Fransızca, İspanyolca, İtalyanca, Ukraynaca
   için temel booking metinlerini çevir.
   Eksik anahtarlar otomatik Türkçe'ye fallback yapar.

7. ADMIN PANEL — Türkçe + İngilizce dil dosyaları:
   Admin panel metinleri (dashboard, rezervasyonlar, masalar, kurallar,
   müşteriler, ayarlar vb.) için TR + EN tam çeviri.
   Admin panel için diğer diller şimdilik gerekli değil.

8. Mevcut kodda değişiklik:
   Tüm hardcode Türkçe metinleri t('key') ile değiştir.
   Örnek:
   ÖNCE: <button>Devam Et</button>
   SONRA: <button>{t('common.next')}</button>

   Bu değişikliği önce Booking UI'a uygula (öncelikli),
   sonra Admin Panel'e.

TypeScript tipleri için i18next-resources-to-backend veya
declare module ile tip güvenliği sağla.
Tüm kodu yaz.
```

---

## Prompt 6.7.2 — Dil Seçici UI + RTL + Admin Panel Ayarı

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 6.7.2 — Dil seçici arayüzü, RTL desteği ve Admin Panel dil ayarı.

BOOKING UI:

1. Dil seçici component (LanguageSwitcher):
   - Konum: sayfanın sağ üst köşesi, her adımda görünür
   - Görünüm: bayrak ikonu + dil kodu (TR, EN, DE, RU, AR...)
   - Dropdown açılınca tüm desteklenen diller listelenir:
     🇹🇷 Türkçe
     🇬🇧 English
     🇩🇪 Deutsch
     🇷🇺 Русский
     🇸🇦 العربية
     🇫🇷 Français
     🇪🇸 Español
     🇮🇹 Italiano
     🇺🇦 Українська
   - Seçim localStorage'a kaydedilir
   - Sayfa yenilenmeden dil anında değişir

2. RTL desteği (Arapça için):
   - Dil 'ar' seçilince <html dir="rtl"> olarak ayarla
   - TailwindCSS RTL plugin'i ekle (tailwindcss-rtl veya Tailwind v3 RTL)
   - Kritik layout'lar RTL'de test edilsin:
     * Booking wizard adım göstergesi
     * Form alanları
     * Butonlar (Geri/İleri yerleri değişir)
   - Diğer diller için <html dir="ltr"> kalsın

3. Otomatik dil algılama:
   - İlk açılışta tarayıcı dili kontrol et
   - Desteklenen dil ise otomatik seç
   - Desteklenmiyorsa Türkçe göster
   - Kullanıcı değiştirirse localStorage'a kaydet,
     bir daha otomatik algılama yapma

ADMIN PANEL:

4. Mekan dil ayarı (Settings > Genel > Booking Dili):
   - "Rezervasyon sayfası varsayılan dili" dropdown
   - Tüm diller listesi
   - Bu ayar Booking UI'ın ilk açılışında kullanılır
   - Örn: beach club Rusça seçmişse müşteri sayfayı Rusça açar
   - Müşteri hala dil değiştirebilir (override)

5. Admin Panel kendi dil seçici:
   - Sağ üst köşe, kullanıcı menüsü yanında
   - Sadece TR + EN (admin panel için yeterli)
   - Tercih kullanıcı profiline kaydedilir

GÜVENLİK NOTU:
Dil dosyaları public klasöründe, hassas veri içermemeli.
Kullanıcı girdileri asla dil dosyasına yazılmaz.

Tüm kodu yaz. RTL layout'ları test et.
```

---

## 📌 WhatsApp + Email Şablonları İçin Not

WhatsApp (Faz 6.5) ve Email (Faz 4) şablonları da çok dilli olabilir.
Müşterinin seçtiği dil rezervasyona kaydedilsin,
bildirimler o dilde gitsin.

Bu entegrasyon Faz 7.5 veya Faz 12'de eklenebilir.
Şimdilik sadece not olarak kalsın:

```
Reservation entity'e PreferredLanguage alanı ekle (string, nullable)
Booking UI'da dil seçimi yapılınca bu alana kaydet
Email/WhatsApp gönderiminde bu dili kullan
```

---

## Güncel Faz Haritası

| Faz | İçerik | Durum |
|-----|--------|-------|
| Faz 1-4 | Backend, API, Kural Motoru, Email | ✅ |
| Faz 5 | Admin Panel | ⚠️ Pürüz giderme |
| Faz 6 | Booking UI | ⚠️ Pürüz giderme |
| Faz 6.5 | WhatsApp Entegrasyonu | 📋 |
| **Faz 6.7** | **Çok Dil Desteği (i18n)** | 📋 Yeni |
| Faz 7 | İyzico Entegrasyonu | 📋 |
| Faz 7.5 | WhatsApp + İyzico Köprüsü | 📋 |
| Faz 8 | CRM + Raporlama | 📋 |
| Faz 8.5 | Super Admin Panel | 📋 |
| Faz 9 | Güvenlik + Monitoring | 📋 |
| Faz 10 | Deployment | 📋 |
| Faz 11 | Landing Page | 📋 |
| Faz 12 | Ölçek Özellikleri | 📋 |
| Faz 13 | Fiziksel Masa Ekranları | 📋 |
| Faz 14 | Muhasebe & e-Fatura | 🔮 |
| Faz 15 | Gelecek Öneriler | 🔮 |

---

## Hatırlatmalar

1. **Booking UI önce, Admin Panel sonra** — turistik etki için.
2. **TR + EN tam, diğerleri kısmi** — fallback zaten var.
3. **Arapça RTL** — layout'ları mutlaka test et.
4. **Metin hardcode etme** — t('key') kullan, alışkanlık haline getir.
5. **Çeviri dosyaları versiyon kontrolünde** — Git'e ekle.
6. **İleride profesyonel çevirmen** kullanılabilir — dosya yapısı buna hazır.
