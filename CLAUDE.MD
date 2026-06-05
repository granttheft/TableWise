# Tablewise

## Stack
- Backend: .NET 8, Clean Architecture, CQRS, EF Core, PostgreSQL (port 5433)
- Frontend: React 18, Vite, TailwindCSS, shadcn/ui, Zustand, React Query
- Projeler: backend/, frontend/admin-panel/, frontend/booking-ui/

## Mimari Kurallar
- Soft delete her entity'de zorunlu (IsDeleted, DeletedAt, DeletedBy)
- Multi-tenant: Global Query Filter ile izolasyon
- CQRS: tüm işlemler Command/Query üzerinden
- Clean Architecture: Core → Application → Infrastructure → API
- Kod İngilizce, yorum Türkçe olabilir

## Şu Anki Durum
- Faz 5 (Admin Panel): ✅ Tamamlandı
- Faz 6 (Booking UI): ✅ Tamamlandı
- Faz 6.5 (WhatsApp): ✅ Tamamlandı
- Sıradaki: Faz 8.5 Super Admin Panel

## Önemli Kararlar
- Ödeme: sadece İyzico (Stripe yok)
- WhatsApp: sadece bildirim kanalı, ödeme linki gönderilmez
- Kapora: restoranın İyzico alt üye işyeri hesabına gider
- WhatsApp: Tablewise ortak Twilio numarası (her restoran ayrı değil)
- Business/Enterprise'da restoranın kendi numarası bağlanabilir (Faz 12)
- Faz 7, 7.5, 8 atlanıyor — direkt Faz 8.5'e geçildi

## Sık Yapılan Hatalar (Yapma!)
- Migration oluşturmadan entity'e kolon ekleme
- TenantId olmadan entity oluşturma
- Soft delete eklemeden silme işlemi
- Global Query Filter'a yeni entity eklemeyi unutma

## Yeni Entity Şablonu
Her yeni entity şunları içermeli:
- Guid Id
- Guid TenantId
- bool IsDeleted
- DateTime CreatedAt
- DateTime UpdatedAt
- DateTime? DeletedAt

## Migration Hatırlatması
Entity değişikliği = mutlaka migration!
cd src/Tablewise.Api
dotnet ef migrations add [İsim] --project ../Tablewise.Infrastructure
dotnet ef database update --project ../Tablewise.Infrastructure

## Obsidian Entegrasyonu

Vault: C:\Users\hasan\Documents\Obsidian\Tablewise\

## Her Oturum Başında:
Şu dosyaları oku ve bağlam olarak kullan:
- 00 - Ana Sayfa.md (güncel durum, sıradaki adım)
- 01 - Faz Haritası.md (hangi fazlar bitti)
- 03 - Alınan Kararlar.md (mimari kararlar)
- 04 - Açık Buglar.md (bilinen sorunlar)
- Fazlar/ klasöründen aktif fazın dosyası

## Her Görev Sonunda:
1. Fazlar/[aktif faz].md güncelle
2. 00 - Ana Sayfa.md güncelle
3. Yeni karar → 03 - Alınan Kararlar.md ekle
4. Yeni bug → 04 - Açık Buglar.md ekle
5. Bug çözüldü → 04'ten kaldır