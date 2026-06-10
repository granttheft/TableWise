# Graphify + Frontend-Design Skill Kurulumu

## Bağlam

Proje: `D:\Projects\TableWise\`
Platform: Windows / PowerShell
Claude Code CLI kullanılıyor.

Bu prompt iki şeyi tek seferde kurar:
1. **Graphify** — proje bilgi grafiği, Claude Code'un dosya taramalarını azaltır
2. **frontend-design** — UI geliştirme için tasarım rehberi skill'i

---

## Adım 1 — Graphify Kurulumu

Aşağıdaki komutları sırasıyla çalıştır:

```powershell
# 1. Graphify paketini kur (uv önerilir, PATH sorunlarını önler)
uv tool install graphifyy

# uv yoksa pipx ile:
# pipx install graphifyy

# uv de pipx de yoksa pip ile (PATH ayarı gerekebilir):
# pip install graphifyy

# 2. Kurulumu doğrula
graphify --version
```

Eğer `graphify: command not found` hatası alınırsa:
```powershell
# pip ile kurulduysa Python scripts klasörünü PATH'e ekle:
python -m graphify --version
# Çalışıyorsa bundan sonra `graphify` yerine `python -m graphify` kullan
```

---

## Adım 2 — Graphify'ı Proje Skill'i Olarak Kaydet

```powershell
# D:\Projects\TableWise\ içindeyken çalıştır:
cd D:\Projects\TableWise

# Proje-scoped kurulum (.claude/skills/ altına yazar)
# PowerShell'de / ile başlama — graphify . kullan
graphify claude install --project
```

Bu komut şunları yapar:
- `.claude/skills/graphify/SKILL.md` oluşturur
- `CLAUDE.md`'ye graphify kurallarını ekler (mimari sorularda önce GRAPH_REPORT.md oku)
- Claude Code'a `PreToolUse` hook kurar (her Grep/Glob çağrısından önce grafik kontrol edilir)

---

## Adım 3 — Bilgi Grafiğini İlk Kez Oluştur

```powershell
# Tüm proje klasöründen grafik üret
# PowerShell'de graphify . kullan (/graphify . değil)
graphify update .
```

Bu işlem 1-3 dakika sürer. Tamamlandığında:
- `graphify-out/GRAPH_REPORT.md` — mimari özet (Claude Code her oturumda bunu okur)
- `graphify-out/graph.json` — tam bilgi grafiği
- `graphify-out/wiki/` — modül bazlı dokümantasyon

Obsidian entegrasyonu (vault'una ekle):
```powershell
# graphify-out klasörünü Obsidian vault'una kopyala veya sembolik link yap:
# C:\Users\hasan\Documents\Obsidian\Tablewise\graphify-out\
New-Item -ItemType Junction -Path "C:\Users\hasan\Documents\Obsidian\Tablewise\graphify-out" -Target "D:\Projects\TableWise\graphify-out"
```

---

## Adım 4 — frontend-design Skill'ini Ekle

```powershell
# .claude/skills/frontend-design/ klasörünü oluştur
New-Item -ItemType Directory -Force -Path "D:\Projects\TableWise\.claude\skills\frontend-design"
```

Aşağıdaki içeriği `.claude/skills/frontend-design/SKILL.md` olarak oluştur:

```markdown
---
name: frontend-design
description: Guidance for distinctive, intentional visual design when building new UI or reshaping an existing one. Helps with aesthetic direction, typography, and making choices that don't read as templated defaults.
---

# Frontend Design

Approach this as the design lead at a small studio known for giving every client a visual identity that could not be mistaken for anyone else's. This client has already rejected proposals that felt templated, and is paying for a distinctive point of view: make deliberate, opinionated choices about palette, typography, and layout that are specific to this brief, and take one real aesthetic risk you can justify.

## Ground it in the subject

If the brief does not pin down what the product or subject is, pin it yourself before designing: name one concrete subject, its audience, and the page's single job, and state your choice. If there's any information in your memory about the human's preferences, context about what they're building, or designs you've made before – use that as a hint. The subject's own world, its materials, instruments, artifacts, and vernacular, is where distinctive choices come from. Build with the brief's real content and subject matter throughout.

## Design principles

For web designs, the hero is a thesis. Open with the most characteristic thing in the subject's world, in whatever form makes sense for it: a headline, an image, an animation, a live demo, an interactive moment. Be deliberate with your choice: a big number with a small label, supporting stats, and a gradient accent is the template answer, only use if that's truly the best option.

Typography carries the personality of the page. Pair the display and body faces deliberately, not the same families you would reach for on any other project, and set a clear type scale with intentional weights, widths, and spacing. Make the type treatment itself a memorable part of the design, not a neutral delivery vehicle for the content.

Structure is information. Structural devices, numbering, eyebrows, dividers, labels, should encode something true about the content, not decorate it. Question if choices like numbered markers actually make sense before incorporating them.

Leverage motion deliberately. Think about where and if animation can serve the subject: a page-load sequence, a scroll-triggered reveal, hover micro-interactions, ambient atmosphere. An orchestrated moment usually lands harder than scattered effects; choose what the direction calls for.

Match complexity to the vision. Maximalist directions need elaborate execution; minimal directions need precision in spacing, type, and detail. Elegance is executing the chosen vision well.

## Process: brainstorm, explore, plan, critique, build, critique again

Work in two passes. First, brainstorm a short design plan: create a compact token system with color, type, layout, and signature.
- Color: describe the palette as 4–6 named hex values
- Type: the typefaces for 2+ roles
- Layout: a layout concept using one-sentence prose descriptions
- Signature: the single unique element this page will be remembered by

Then review that plan against the brief before building. Only after confirming the relative uniqueness of your design plan should you start writing code.

## Restraint and self-critique

Spend your boldness in one place. Let the signature element be the one memorable thing, keep everything around it quiet and disciplined. Build to a quality floor: responsive down to mobile, visible keyboard focus, reduced motion respected.

## Writing in design

Words appear in a design for one reason: to make it easier to understand and use.
- Write from the end user's side of the screen
- Use active voice: "Save changes" not "Submit"
- Keep register conversational: plain verbs, sentence case, no filler
- Treat failure and emptiness as moments for direction, not mood
```

---

## Adım 5 — CLAUDE.md Güncellemesi

`D:\Projects\TableWise\CLAUDE.md` dosyasına aşağıdaki bölümü ekle
(graphify install zaten bir bölüm eklemiş olacak, bunun ALTINA ekle):

```markdown
## Frontend Design Skill

UI/landing page/component geliştirirken `.claude/skills/frontend-design/SKILL.md`
dosyasını oku ve tasarım kararlarını bu rehbere göre al.

Tetikleyiciler:
- Landing page bileşenleri yazarken
- Yeni React component tasarlarken  
- Mevcut UI'ı yeniden şekillendirirken
- Renk, tipografi, layout kararı verirken

## Graphify — Güncelleme Politikası

Önemli kod değişikliklerinden sonra (yeni entity, yeni feature, migration):
```
graphify update .
```
Otomatik güncelleme: git commit sonrası hook kuruluysa zaten çalışır.
Büyük refactor veya branch değişimlerinde tam rebuild:
```
graphify update . --force
```
```

---

## Adım 6 — Git Hook (Opsiyonel ama Önerilir)

Her commit sonrası grafiği otomatik güncelle:

```powershell
# Commit sonrası otomatik graphify update
graphify hook install
```

---

## Adım 7 — Doğrulama

```powershell
# 1. Skill'lerin yüklendiğini doğrula
ls .claude/skills/

# Çıktı şunu göstermeli:
# graphify/
# frontend-design/

# 2. Grafik dosyalarının oluştuğunu doğrula
ls graphify-out/

# Çıktı şunu göstermeli:
# GRAPH_REPORT.md
# graph.json
# wiki/

# 3. Claude Code'da slash komutlarını kontrol et
# Claude Code'u aç, / yaz → listede /graphify görünmeli
```

---

## Kullanım

### Graphify
```
# Claude Code içinde:
/graphify query "rezervasyon oluşturma akışı nerede"
/graphify path "ReservationController" "RuleEngine"
/graphify explain "TenantService"

# Terminal'de:
graphify update .          # incremental güncelleme
graphify update . --force  # tam rebuild
```

### Frontend-Design
Landing page veya UI yaparken Claude Code'a şunu söyle:
```
frontend-design skill'ini oku ve Tablewise landing page hero section'ını yaz
```

---

## Tamamlanma Kriterleri

- [ ] `graphify --version` çalışıyor
- [ ] `.claude/skills/graphify/SKILL.md` mevcut
- [ ] `.claude/skills/frontend-design/SKILL.md` mevcut
- [ ] `graphify-out/GRAPH_REPORT.md` oluştu
- [ ] `CLAUDE.md`'de graphify kuralları var
- [ ] Claude Code'da `/graphify` slash komutu görünüyor
