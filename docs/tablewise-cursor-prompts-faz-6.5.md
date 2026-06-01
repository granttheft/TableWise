# Tablewise — Cursor Promptları: Faz 6.5 (WhatsApp Entegrasyonu)

> Bu dosya mevcut prompt dosyalarının devamıdır.
> Her prompttan önce mutlaka **[GENEL BAĞLAMI YAPISTIR]** bloğunu ekle.
> Konum: Faz 6 (Booking UI) bitince, Faz 7 (İyzico) başlamadan önce.
> Çelişki olursa ürün dökümanı (.docx) kazanır.

---

## 🎯 Faz 6.5 Amacı

Türkiye pazarında email yerine **WhatsApp** çok daha etkili. Bu faz:
- Restoranın admin panelden WhatsApp'ı bağlamasını sağlar
- Rezervasyon onayı, ödeme onayı, hatırlatma, iptal bildirimlerini WhatsApp'tan gönderir

> Mimari karar: WhatsApp sadece **bildirim kanalıdır.** Ödeme işlemi
> tamamen Booking UI üzerinden yapılır (Faz 7). WhatsApp ödeme linki
> göndermez — sadece sonucu bildirir.
>
> Mevcut email altyapısına **paralel** bir `IMessagingChannel` soyutlaması
> olarak kurulur. İleride SMS veya başka kanal eklemek de kolay olur.

| Görev | Model |
|-------|-------|
| Messaging soyutlaması, provider, webhook güvenliği | **Opus 4.5** |
| Admin panel ayar ekranı, template yönetimi UI | **Sonnet 4.5** |

---

## Prompt 6.5.1 — WhatsApp Mesajlaşma Altyapısı (Backend)

**Model:** Opus 4.5 | **Tahmini:** 1-2 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 6.5.1 — WhatsApp mesajlaşma altyapısı (backend).

MİMARİ: Mevcut IEmailService'e paralel bir mesajlaşma kanalı soyutlaması.
Sağlayıcı olarak WhatsApp Business API (Twilio veya Meta Cloud API).
Önerilen başlangıç: Twilio (kurulumu daha hızlı, dökümante).

1. IMessagingChannel interface (Core/Interfaces/):
   - SendAsync(string toPhone, MessageTemplate template, Dictionary<string,string> data)
   - SendTextAsync(string toPhone, string body)
   - GetDeliveryStatusAsync(string messageId)
   - Kanal tipini belirten ChannelType enum: WhatsApp, Sms (ileride)

2. WhatsAppChannel implementasyonu (Infrastructure/Messaging/):
   - Twilio WhatsApp API (veya Meta Cloud API) ile gönderim
   - Telefon numarası formatlama/doğrulama (E.164: +90...)
   - Onaylı şablon (template) gönderimi — WhatsApp template mesajları
     için (oturum dışı mesajlar onaylı şablon gerektirir)
   - Hata yönetimi + retry (geçici hatalar)
   - Tüm gönderimleri logla (telefon maskelenmiş)

3. MessagingConfig:
   - Twilio AccountSid, AuthToken, WhatsApp sender numarası
   - appsettings + secrets ile yönetilir
   - Sandbox/Production toggle

4. WhatsApp şablonları (MessageTemplate enum + içerik):
   Tüm şablonlar Türkçe, kısa, net. Placeholder'lar {{degisken}} formatında.

   a) ReservationReceived (rezervasyon alındı, ödeme bekleniyor):
      "Merhaba {{ad}}, {{mekan}} rezervasyon talebiniz alındı.
       📅 {{tarih}} · 🕐 {{saat}} · 👥 {{kisi}} kişi
       Ödemeniz onaylandıktan sonra rezervasyonunuz kesinleşecektir."

   b) ReservationConfirmed (ödeme başarılı veya kaporasız onay):
      "✅ {{mekan}} rezervasyonunuz onaylandı!
       📅 {{tarih}} · 🕐 {{saat}} · 👥 {{kisi}} kişi
       Sizi bekliyoruz."

   c) Reminder (1 gün önce):
      "🔔 Hatırlatma: Yarın {{mekan}} rezervasyonunuz var.
       🕐 {{saat}} · 👥 {{kisi}} kişi
       Yol tarifi: {{harita}}"

   d) Cancellation:
      "{{mekan}} rezervasyonunuz iptal edildi.
       {{iadeBilgisi}}
       Yeni rezervasyon: {{bookingLink}}"

5. Mesaj gönderim orkestrasyonu (Application katmanı):
   - Restoranın WhatsApp'ı aktif mi kontrol et
   - Aktifse WhatsApp, değilse email'e fallback (mevcut IEmailService)
   - Booking event'lerine bağla:
     * Rezervasyon oluşturuldu (kapora bekliyor) → ReservationReceived
     * Rezervasyon onaylandı (ödeme başarılı veya kaporasız) → ReservationConfirmed
     * İptal → Cancellation
   - Hatırlatma için background job (1 gün önce)
   - NOT: Ödeme başarısız ve iade bildirimleri Faz 7.5'te eklenir

6. WhatsApp webhook (gelen mesaj + teslimat durumu):
   POST /api/webhooks/whatsapp
   - İmza/token doğrulama (Twilio signature veya Meta verify token) — KRİTİK
   - Teslimat durumu güncelleme (sent/delivered/read/failed)
   - Müşteri "İPTAL" yazarsa → rezervasyon iptal akışı (opsiyonel,
     basit keyword matching)
   - Doğrulanmamış istekleri reddet

7. WhatsAppMessage entity (log/takip):
   - ReservationId, ToPhone (maskelenmiş), TemplateType
   - ProviderMessageId, Status, SentAt, DeliveredAt
   - Soft delete + audit

GÜVENLİK: Webhook imza doğrulamasını ATLAMA. Telefon numaralarını
loglarken maskele. Tüm kodu Clean Architecture'a uygun yaz.
```

---

## Prompt 6.5.2 — Admin Panel WhatsApp Ayarları + Akış Entegrasyonu

**Model:** Sonnet 4.5 | **Tahmini:** 1 chat

```
[GENEL BAĞLAMI YAPISTIR]

FAZ 6.5.2 — Admin panel WhatsApp ayarları ve booking akışı entegrasyonu.

ADMIN PANEL (frontend/admin-panel):

1. WhatsApp ayar sekmesi (Settings > Bildirimler altına veya yeni sekme):
   - "WhatsApp Bildirimleri" master toggle
   - Bağlantı durumu göstergesi: Bağlı değil / Bağlı / Hata
   - Bağlantı kurulumu:
     * Twilio modeli: WhatsApp sender numarası + bağlantı bilgileri
       (basit tutulabilir — platform seviyesinde tek hesap da olabilir,
        karar: her restoran kendi numarası mı yoksa Tablewise ortak
        numarası mı? — MVP için Tablewise ortak numarası daha kolay)
   - Hangi bildirimler WhatsApp'tan gitsin (toggle'lar):
     * Rezervasyon alındı bildirimi
     * Rezervasyon onay bildirimi
     * Ödeme başarısız bildirimi (Faz 7.5'te aktif olur)
     * Hatırlatma
     * İptal bildirimi
     * İade bildirimi (Faz 7.5'te aktif olur)
   - "Test mesajı gönder" butonu (kendi numarana test)
   - WhatsApp aktif değilse → email'e otomatik fallback bilgisi göster

2. WhatsApp mesaj geçmişi (opsiyonel, basit):
   - Son gönderilen mesajlar: kime, hangi tip, durum (gönderildi/
     iletildi/okundu/başarısız)
   - Filtre: durum, tarih

BOOKING UI (frontend/booking-ui):

3. Telefon numarası alanı:
   - Rezervasyon formunda telefon ZORUNLU (WhatsApp için gerekli)
   - E.164 formatı doğrulama (+90...)
   - "WhatsApp'tan bildirim almak istiyorum" onayı (KVKK uyumu)

4. Akış güncellemesi:
   - Rezervasyon + ödeme tamamlanınca:
     * "Rezervasyonunuz onaylandı! WhatsApp'tan bildirim gönderildi." ekranı
   - Kapora gereken ama ödeme henüz yapılmamışsa:
     * "Ödemeniz işleniyor..." → ödeme sonucu gelince WhatsApp bildirimi
   - Booking UI ödeme akışı Faz 7'de kurulur; bu fazda telefon alanı
     ve onay ekranı hazır olsun

5. React Query hooks:
   - useWhatsAppSettings(), useUpdateWhatsAppSettings()
   - useSendTestMessage()
   - useWhatsAppMessageHistory()

KVKK NOTU: WhatsApp bildirimi için açık rıza alınmalı. Booking formunda
onay checkbox'ı + gizlilik metni linki olsun.

Tüm kodu yaz. Responsive olsun.
```

---

## 📌 Faz 7.5 Bağlantı Notu

Faz 7.5'te (WhatsApp + İyzico köprüsü) şunlar eklenecek:

1. Ödeme başarılı → `ReservationConfirmed` WhatsApp mesajı (İyzico webhook'tan tetiklenir)
2. Ödeme başarısız → `PaymentFailed` WhatsApp bildirimi
3. İade → `Cancellation` şablonuna iade bilgisi eklenir

Bu fazda WhatsApp altyapısını kur, Faz 7.5'te ödeme event'leri bağlanır.

---

## Güncel Faz Haritası

| Faz | İçerik | Durum |
|-----|--------|-------|
| Faz 1-4 | Backend, API, Kural Motoru, Email | ✅ |
| Faz 5 | Admin Panel | ⚠️ Pürüz giderme |
| Faz 6 | Booking UI | ⚠️ Pürüz giderme |
| **Faz 6.5** | **WhatsApp Entegrasyonu** | 📋 Yeni |
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

---

## Hatırlatmalar

1. **Webhook imza doğrulamasını asla atlama** (Twilio/Meta).
2. **Telefon numaralarını loglarken maskele.**
3. **KVKK:** WhatsApp bildirimi için açık rıza şart.
4. **WhatsApp sadece bildirim kanalı** — ödeme asla WhatsApp üzerinden değil.
5. **WhatsApp kapalıysa email'e fallback** — müşteri bildirimsiz kalmasın.
6. **MVP kararı:** Başlangıçta Tablewise ortak WhatsApp numarası kullanmak,
   her restoran için ayrı numara kurmaktan çok daha kolay. İleride
   white-label için restoran bazlı numara eklenebilir.
