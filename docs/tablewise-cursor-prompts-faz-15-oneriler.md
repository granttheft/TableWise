# Tablewise — Faz 15: Gelecek Öneriler

> Bu dosya ileriki aşamalarda değerlendirilebilecek fikirleri içerir.
> Şu an için geliştirme yapılmayacak — sadece unutmamak için burada duruyor.
> Öncelik sırası ve faz ataması ilerleyen dönemde belirlenecek.

---

## 💡 Öneri 1 — No-Show Geri Bildirim Mesajı

**Fikir:**
Restoran bir müşteriyi "no-show" (gelmedi) olarak işaretlediğinde,
müşteriye otomatik WhatsApp mesajı gönderilsin:

```
"Merhaba {{ad}}, bugün {{mekan}} rezervasyonunuzda
sizi göremedik. Bir sorun mu yaşandı?
Bize bildirirseniz memnuniyet duyarız."
```

**Değeri:**
- Müşteri ilişkisi korunur
- CRM'e no-show verisi işlenir
- Tekrar eden no-show'lar otomatik kara listeye alınabilir

**Bağımlılık:** Faz 6.5 (WhatsApp), Faz 8 (CRM)

---

## 💡 Öneri 2 — Rezervasyon Sonrası Değerlendirme

**Fikir:**
Rezervasyon tamamlandıktan X saat sonra müşteriye WhatsApp mesajı:

```
"{{mekan}} deneyiminiz nasıldı?
⭐ Kötü  ⭐⭐ Orta  ⭐⭐⭐ İyi  ⭐⭐⭐⭐ Çok İyi  ⭐⭐⭐⭐⭐ Mükemmel"
```

**Değeri:**
- Restoran Admin Panel'den puanları görür
- CRM'de müşteri bazlı puan geçmişi tutulur
- İleride Google Reviews entegrasyonu düşünülebilir
- Kötü puan gelince restorana otomatik bildirim

**Bağımlılık:** Faz 6.5 (WhatsApp), Faz 8 (CRM)

---

## 💡 Öneri 3 — Bekleme Listesi (Waitlist)

**Fikir:**
Seçilen saat/tarih doluysa müşteri bekleme listesine girebilsin.
Yer açılınca (iptal veya no-show) otomatik WhatsApp:

```
"{{mekan}}'de bu akşam 20:00 için yer açıldı!
Rezerve etmek için: {{link}}
Bu teklif 30 dakika geçerlidir."
```

**Değeri:**
- Doluluk oranı artar
- İptal kayıpları minimize edilir
- Popüler mekanlarda çok kullanılan bir özellik

**Bağımlılık:** Faz 6.5 (WhatsApp), Faz 6 (Booking UI)

---

## 💡 Öneri 4 — Restorana Anlık Rezervasyon Bildirimi

**Fikir:**
Yeni rezervasyon geldiğinde restoran yöneticisinin telefonuna WhatsApp:

```
"📋 Yeni Rezervasyon
👤 Ahmet Yılmaz · 📞 0532 XXX XX XX
📅 Bu akşam 20:00 · 👥 4 kişi · Teras
Admin Panel: {{link}}"
```

**Değeri:**
- Yönetici Admin Panel'i sürekli açık tutmak zorunda kalmaz
- Anlık aksiyon alabilir (onayla, reddet, not ekle)
- Özellikle küçük restoranlar için çok pratik

**Bağımlılık:** Faz 6.5 (WhatsApp)

---

## 💡 Öneri 5 — Google Reviews Entegrasyonu

**Fikir:**
Değerlendirme puanı 4-5 olan müşterilere otomatik:

```
"Memnuniyetinizi Google'da paylaşır mısınız?
{{googleReviewLink}}"
```

Düşük puan alanlara ise restoran sessizce müdahale edebilir.

**Değeri:**
- Organik Google puanı artar
- Kötü yorumlar önceden yakalanır

**Bağımlılık:** Öneri 2 (Değerlendirme sistemi) tamamlanmış olmalı

---

## 💡 Öneri 6 — Muhasebe & e-Fatura Entegrasyonu

> Faz 14 olarak zaten planlandı. Buraya taşındı mı kontrol et.

---

## 💡 Öneri 7 — Çok Dil Desteği (i18n)

**Fikir:**
Tüm frontend'lerde (Booking UI, Admin Panel) dil seçeneği sunulsun.
Başlangıç: Türkçe + İngilizce. İleride Arapça, Almanca vb. eklenebilir.

**Öncelik Notu:**
Bu önerinin bir kısmı **şimdiden doğru kurulmak zorunda.**
Sonradan eklemek tüm metinleri tek tek bulmak demek — çok maliyetli.

Şimdiden yapılması gereken (Faz 5-6 içinde):
- Frontend'de `i18next` veya `react-intl` kütüphanesi kur
- Tüm sabit metinleri kod içine yazmak yerine `t('key')` ile çağır
- Türkçe dil dosyası oluştur (en azından Booking UI için)
- İngilizce dosyayı şimdilik boş bırakabilirsin

Sonraya bırakılabilecek:
- İngilizce çeviri dosyasını doldurma
- Dil seçici UI (bayrak ikonu + dropdown)
- Backend hata mesajlarının çevirisi
- RTL dil desteği (Arapça için — çok sonra)
- Otomatik dil algılama (tarayıcı dilinden)

**Booking UI için özellikle kritik:**
Yabancı turist veya expat müşteri telefondan rezervasyon yapacaksa
Türkçe görmesi kötü deneyim. Bu yüzden Booking UI'da dil desteği
Admin Panel'den önce gelir.

**Admin Panel:**
Restoranın kendi dil tercihini seçebilmesi (Settings > Genel > Dil)

**Değeri:**
- Yurtdışı açılımında sıfırdan yazmak zorunda kalmazsın
- Turistik mekanlarda (beach club, boutique otel restoranları) hemen fark yaratır
- Uluslararası zincir restoranlar için Enterprise plan satışını kolaylaştırır

**Bağımlılık:** Yok — ama ne kadar erken başlanırsa o kadar iyi

---

## 📌 Genel Not

Bu önerilerin büyük çoğunluğu birbirine bağlı:
- WhatsApp altyapısı (Faz 6.5) olmadan hiçbiri çalışmaz
- CRM (Faz 8) olmadan müşteri verisi tutulmaz
- Önce o fazlar tamamlanmalı, sonra buraya dönülmeli

Yeni bir fikir geldiğinde bu dosyaya ekle.
