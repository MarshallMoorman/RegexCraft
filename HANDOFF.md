# RegexCraft – HANDOFF.md

**Current Version**: **1.2.0** (Phase 13)  
**Date**: 2026-07-12  
**Next**: Marshall completes [commercial.md](docs/development/commercial.md) checklist (dist repo, token, payment, tag, incognito, make private, re-verify)

---

## What Shipped in 1.2.0 (Phase 13)

1. **Commercial model (no keys)**  
   - Product **EULA** (root `LICENSE`) — free personal / paid business, honor system  
   - About dialog messaging + optional business-license checkbox  
   - Site: pricing, EULA, download, landing without MIT/open-source claims  

2. **Monorepo + Actions public surface**  
   - Tag `v*` → build/test/publish RID zips → **public dist repo** Releases (`DIST_REPO_TOKEN`)  
   - Site build = `website/` + `docs/user/` only → dist **`gh-pages`**  
   - Never publishes `docs/development/` or source  

3. **Export**  
   - Test panel: **CSV**, **JSON**, **Copy JSON**  
   - `MatchExportService` + `Category=Export` tests  

4. **Docs**  
   - `docs/development/commercial.md` (ordered human checklist)  
   - Updated packaging.md, website.md, user guides (+ exporting.md)  
   - CHANGELOG / README / AGENTS  

**Do not re-litigate dual-repo day-to-day development.** Work stays on main; Actions own the public side.

---

## Marshall must do next (ordered)

Full detail: **`docs/development/commercial.md`**.

1. Create public **`RegexCraft-Releases`**  
2. Fine-scoped PAT → secret **`DIST_REPO_TOKEN`** on main  
3. Payment product URL into site-config / CommercialLinks / pricing  
4. Tag **`v1.2.0`** → verify Publish + site Actions  
5. Incognito download test  
6. Verify website (Pages on dist `gh-pages` + DNS)  
7. Make **main** private  
8. Re-verify downloads + site  

---

## Post–1.2.0 product roadmap

| Track | Notes |
|-------|--------|
| Payment URL live | Gumroad / Lemon / Stripe — no app keys |
| Debug expansion | PCRE2 / JS educational steppers |
| GREP export / library import-export | Nice-to-haves from Export track |
| Editor polish | Find-in-pattern, wrap toggle, F3 match nav |
| Installers | MSI / DMG / AppImage — portable zips remain supported |
| Higher engine fidelity | Only with clear packaging story |

---

## Known Issues / Limitations (carry-forward)

- Analysis tree is structural/heuristic — not a full flavor-faithful AST.  
- Debug is educational (Match + analysis overlay), not cycle-accurate .NET NFA.  
- Debug not available for PCRE2 / JavaScript yet.  
- Approximate flavors intentionally use closest engines; banners + token matrices communicate gaps.  
- Go/Rust testing still runs on .NET (may accept patterns real RE2 rejects).  
- GREP does not parse `.gitignore`; use exclude globs.  
- Public site / dist releases require human setup of dist repo + `DIST_REPO_TOKEN` (see commercial.md).  
- Buy/checkout URL may still be a placeholder until Marshall creates the payment product.  

## How to Continue in a New Conversation

1. Open latest `main` (1.2.0 / Phase 13).  
2. Read this `HANDOFF.md`, `AGENTS.md`, and `docs/development/commercial.md` if ops-related.  
3. Prefer payment URL polish, Debug for PCRE2/JS, or installers — do not re-open dual-repo product development.  
4. Releases: tag on main only after dist secret is ready.  
5. Keep CI green: `dotnet test`.  
6. Export tests: `dotnet test --filter Category=Export`.  

## Key Files for Phase 13+

| Path | Why |
|------|-----|
| `docs/development/commercial.md` | Human go-live checklist |
| `docs/development/packaging.md` | Tags + publish to dist |
| `docs/development/website.md` | Site deploy on dist gh-pages |
| `.github/workflows/publish.yml` | Dist binaries |
| `.github/workflows/pages.yml` | Public site |
| `scripts/build-site.sh` | website + user docs only |
| `src/RegexCraft.Core/Export/` | Match CSV/JSON |
| `src/RegexCraft.Core/Commercial/` | Public URLs / license summary |
| `LICENSE` | Product EULA |
| `website/pricing.html` / `eula.html` | Commercial pages |
