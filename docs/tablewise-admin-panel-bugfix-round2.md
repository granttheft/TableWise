# Admin Panel Bug Fix — Faz 5 Pürüz Giderme (Round 2)

> Bu promptu Claude Code'a ver. Önce KRİTİK bugları düzelt,
> sonra ORTA önceliklilere geç. DÜŞÜK öncelikliler sonraya bırak.

---

## Prompt

Aşağıdaki bugları öncelik sırasına göre düzelt.
Her kategoriyi bitirince bana haber ver.

---

### KRİTİK (Demo'yu Etkiler)

**1. Çalışma Saatleri — Rezervasyon Süresi Kaydedilemiyor**
Settings > Çalışma Saatleri sayfasında rezervasyon süresi
değiştirip kaydedince hata veriyor.
- API isteğini ve response'u incele
- Validation veya backend hatası mı tespit et
- Düzelt

**2. Çalışma Saatleri İçinde Rezervasyon Oluşturulamıyor**
Haftalık çalışma saatleri ayarlanmış olmasına rağmen
rezervasyon oluşturulurken "çalışma saati dışında" hatası geliyor.
- Backend'de çalışma saati kontrolü yapan yeri bul
- Timezone dönüşümü doğru mu kontrol et (UTC vs UTC+3)
- Düzelt

**3. Kapalı Gün Eklenemiyor**
Settings > Çalışma Saatleri sayfasında kapalı gün
eklenemiyor veya kaydedilemiyor.
- İlgili component ve API endpoint'i bul
- Hatayı tespit et ve düzelt

**4. Rezervasyon Saati Timezone Farkı**
Rezervasyonlar sayfasındaki tabloda gösterilen saat
ile gerçek rezervasyon saati farklı.
- Backend UTC kaydediyor, frontend UTC+3 göstermeli
- Tüm tarih/saat gösterimlerini kontrol et
- Türkiye saati (UTC+3) olarak göster

**5. Ekip — Davet Hatası**
Yeni ekip üyesi eklenirken "kullanıcı bilgisi bulunamadı" hatası.
- Admin panel Ekip sayfasında davet/ekleme endpoint'ini bul
- Hatanın kaynağını tespit et ve düzelt

---

### ORTA (Önemli Ama Demo'yu Bloklamaz)

**6. Dashboard — Bugünkü Rezervasyonlar 0 Gösteriyor**
Aynı tarihte rezervasyon olmasına rağmen dashboard'da
bugünkü rezervasyon sayısı 0 görünüyor.
- Backend'deki bugün filtresi timezone dönüşümünü yapıyor mu?
- UTC ile lokal saat karşılaştırması yapıyor olabilir
- Düzelt

**7. Dashboard — En Sık Tetiklenen Kurallar 0**
Dashboard'daki "En sık tetiklenen kurallar" bölümü
her zaman 0 gösteriyor.
- Backend'deki query'i kontrol et
- Kural tetiklenme sayısı doğru kaydediliyor mu?
- Düzelt

**8. Logo Sayfa Yenilenince Gidiyor**
Settings'ten yüklenen logo sayfa yenilendiğinde kayboluyor.
- Logo URL'i nerede saklanıyor kontrol et (DB mi, local state mi?)
- Yeniden yüklemede logo DB'den gelmiyor mu?
- Düzelt

**9. Müşteriler — Mekan Bilgisi Yok**
Müşteriler sayfasında hangi müşterinin hangi mekanın
müşterisi olduğu görünmüyor.
- Müşteri listesine "Mekan" kolonu ekle
- Backend query'e venue bilgisini dahil et

**10. Bu Ay Rezervasyon Sayacı**
Dashboard'da "Bu ay rezervasyon" kısmında 6/500 yazıyor.
500 rakamı Pro planın limitini gösteriyor, bu doğru davranış.
Ama gösterim şu şekilde olsun:
- "6 rezervasyon bu ay" (limit gösterme)
- Veya limit gösterilecekse "6 / Plan Limiti: 500" şeklinde

---

### DÜŞÜK (Sonraya Bırak)

Aşağıdakileri şimdilik yapma, listeyi Obsidian'a ekle:

- Rezervasyonlar sayfası tablo görünümü yeniden tasarımı
  (masa sayısı fazla işletmeler için)
- Tarih seçimi açılır pencere (date picker) ile yapılsın
- Masalar sayfasında gruplandırma
- Masalar pasife çekme özelliği
- Masa birleşim düzenleme + silme ikonu
- Masa konum bilgisi ekleme/çıkarma
- Masa birleştirmede arama özelliği
- Manuel rezervasyonda grup kompozisyonu bilgileri
- Kurallar sayfası kapsamlı inceleme (ayrı prompt verilecek)
- Entegrasyon sayfasında İyzico bölümü (Faz 7'de gelecek)
- Son aktiviteler sıfırlama butonu
- Dashboard rezervasyon butonu "Bu ay" filtresi

---

## Uygulama Sonrası

Tüm kritik buglar düzeltilince:
1. dotnet build → 0 hata
2. npm run build (admin-panel) → 0 hata
3. Obsidian'ı güncelle:
   - 04 - Açık Buglar.md → çözülenleri kaldır, DÜŞÜK önceliklileri ekle
   - 00 - Ana Sayfa.md → güncelle
