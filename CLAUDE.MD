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
- Faz 5 (Admin Panel): çalışıyor, küçük pürüzler var
- Faz 6 (Booking UI): çalışıyor, küçük pürüzler var
- Sıradaki: Faz 6.5 WhatsApp entegrasyonu

## Önemli Kararlar
- Ödeme: sadece İyzico (Stripe yok)
- WhatsApp: sadece bildirim, ödeme linki yok
- Kapora: restoranın İyzico alt üye işyeri hesabına gider