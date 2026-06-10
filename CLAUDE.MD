# Tablewise

## Stack
- Backend: .NET 8, Clean Architecture, CQRS, EF Core, PostgreSQL (port 5433)
- Frontend: React 18, Vite, TailwindCSS, shadcn/ui, Zustand, React Query
- Projeler: backend/, frontend/admin-panel/, frontend/booking-ui/, frontend/super-admin/

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
- Faz 8.5 (Super Admin): ✅ Tamamlandı
- E2E Testler (Playwright): ✅ 67/67 geçiyor
- Sıradaki: Faz 7 (İyzico) / Faz 9 (Güvenlik) / Faz 10 (Deployment)

## Önemli Kararlar
- Ödeme: sadece İyzico (Stripe yok)
- WhatsApp: sadece bildirim kanalı, ödeme linki gönderilmez
- Kapora: restoranın İyzico alt üye işyeri hesabına gider
- WhatsApp: Tablewise ortak Twilio numarası (her restoran ayrı değil)
- Business/Enterprise'da restoranın kendi numarası bağlanabilir (Faz 12)
- Faz 7, 7.5, 8 atlanmıştı — direkt Faz 8.5'e geçildi; şimdi Faz 7'ye dönülecek

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
- Docs/ klasöründen ilgili faz prompt dosyası:
  - Faz 6.5 için: Docs/tablewise-cursor-prompts-faz-6.5.md
  - Faz 6.7 için: Docs/tablewise-cursor-prompts-faz-6.7.md
  - Faz 7, 8.5, 14 için: Docs/tablewise-cursor-prompts-faz-7-8.5-14.md
  - Faz 15 için: Docs/tablewise-cursor-prompts-faz-15-oneriler.md
  - Genel bağlam için: Docs/tablewise-cursor-prompts-v1.1.md

## Her Görev Sonunda:
1. Fazlar/[aktif faz].md güncelle
2. 00 - Ana Sayfa.md güncelle
3. Yeni karar → 03 - Alınan Kararlar.md ekle
4. Yeni bug → 04 - Açık Buglar.md ekle
5. Bug çözüldü → 04'ten kaldır

## Frontend Design Skill

UI/landing page/component geliştirirken `.claude/skills/frontend-design/SKILL.md`
dosyasını oku ve tasarım kararlarını bu rehbere göre al.

Tetikleyiciler:
- Landing page bileşenleri yazarken
- Yeni React component tasarlarken
- Mevcut UI'ı yeniden şekillendirirken
- Renk, tipografi, layout kararı verirken

## Ek Skill'ler

- ui-ux-pro-max: UI/UX geliştirme, erişilebilirlik denetimi, renk/tipografi kararlarında kullan
- seo: Landing page meta etiketleri, şema markup, teknik SEO denetiminde kullan
- ECC: Güvenlik taraması için kullan — Faz 9 öncesi npx ecc-agentshield scan çalıştır
- superpowers: Yeni faz başlarken brainstorm → plan → subagent uygulama akışı için kullan
- gstack: Slash komutları — /plan-ceo-review (ürün kararı), /plan-eng-review (teknik karar), /review (kod incelemesi), /ship (deployment), /retro (faz sonrası)
- marketingskills: Landing page CRO, conversion copy, email sequence için kullan
- financial-services: İyzico entegrasyon fazında (Faz 7) finansal akış kararlarında kullan (finance-billing-ops skill olarak kurulu)

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
