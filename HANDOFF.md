# RegexCraft – HANDOFF.md

**Current Version**: **1.1.0** (app) + **Phase 12 website** (source shipped; Pages/DNS may still need human enablement)  
**Date**: 2026-07-11  
**Next**: Human enables GitHub Pages + DNS → then product tracks (Export, Debug expansion, installers, fidelity)

---

## What Shipped in Phase 12 (Website)

1. **Static site** under `website/`  
   - Landing (hero, features, screenshots, engines table, CTA)  
   - Download, Docs, About pages  
   - Blue professional theme matching the app (`#0078D4`)  
   - Responsive; light default + `prefers-color-scheme` dark  

2. **GitHub Pages plumbing**  
   - `website/CNAME` → `regexcraft.com`  
   - Workflow `.github/workflows/pages.yml` deploys `website/` via Actions  

3. **Human setup docs**  
   - `docs/development/website.md` — exact Pages settings + DNS A/CNAME checklist  
   - `website/README.md` — structure and local preview  

4. **Repo docs**  
   - README, CHANGELOG (Unreleased/Phase 12), AGENTS.md, this HANDOFF  

**App version was not bumped** (pure website). No new app features in this phase.

### Marshall must still do (one-time)

See the full checklist in **`docs/development/website.md`**. Short form:

1. Repo **Settings → Pages → Source: GitHub Actions**  
2. Run **Deploy website** (or push) and confirm `*.github.io/RegexCraft/` loads  
3. Custom domain: `regexcraft.com` + Enforce HTTPS when ready  
4. Registrar DNS: apex **A** records to GitHub Pages IPs; **www** CNAME → `marshallmoorman.github.io`  

---

## What Shipped in 1.1.0 (Phase 11) — still current app

1. **Debug / step-through (.NET)** — Debug tab, F10/F11, hybrid educational session  
2. **Equal-width Matches & Groups cards**  
3. Tests: `Category=Debug` + headless coverage  
4. Tag `v1.1.0` → multi-RID portable zips on GitHub Releases  

Debug approach for future agents: do **not** re-implement the full .NET NFA; extend `RegexDebugService` / engine-specific builders behind `IRegexDebugService`.

---

## Post–Phase 12 Roadmap

Work these as separate tracks on `main`. Prefer small, shippable increments.

### Human / ops (immediate)

- [ ] Complete Pages + DNS so https://regexcraft.com is live  
- [ ] Optional: announce site + Releases from README / social  

### Debug expansion

- PCRE2 / JavaScript educational steppers (same UI, new engine builders)  
- Optional play/pause auto-step  
- Richer backtracking narratives where cheap  

### Product depth (any 1.x)

| Track | Notes |
|-------|--------|
| **Export** | Matches/groups → CSV/JSON; GREP export; library import/export |
| **Editor polish** | Find-in-pattern, word-wrap toggle, error underlines, F3 match nav |
| **GREP UX** | Open hit externally, multi-select replace, optional `.gitignore` |
| **Layout persistence** | Left sidebar width / analysis height (right panel already done) |
| **Virtualization** | Huge match lists / GREP; cancel in-flight Test |
| **Installers** | MSI / DMG / AppImage — packaging.md documents portable zips today |

### Higher engine fidelity

- True Python / Java interop only with a clear packaging story  
- True RE2 if a maintained .NET wrapper appears  
- Improve approximate-flavor notes and token matrices as dialects change  

### Website maintenance (ongoing)

- After major UI changes: regenerate screenshots → copy into `website/assets/screenshots/`  
- Keep download CTAs pointing at GitHub Releases  
- Do not introduce a heavy SPA framework without a strong reason  

### Release process (ongoing)

- Bump only `Directory.Build.props`  
- CHANGELOG section per version  
- Tag `vX.Y.Z` → Publish workflow creates GitHub Release  
- Keep CI green on `main`  

---

## Known Issues / Limitations (carry-forward)

- Analysis tree is structural/heuristic — not a full flavor-faithful AST.  
- Debug is educational (Match + analysis overlay), not cycle-accurate .NET NFA.  
- Debug not available for PCRE2 / JavaScript yet.  
- Approximate flavors intentionally use closest engines; banners + token matrices communicate gaps.  
- Go/Rust testing still runs on .NET (may accept patterns real RE2 rejects); palette/docs warn.  
- Go/Rust codegen notes RE2 limits but still emits the pattern as-is.  
- PCRE replacement expansion is custom (covers `$n`, `${name}`, `$&`, `\n`).  
- JS named replacements map `${name}` → `$<name>` for Jint testing.  
- GREP does not parse `.gitignore`; use exclude globs.  
- GREP preview caps very large files (~200k chars).  
- Large multi-MB subjects still not stress-tested (including Compare).  
- Avalonia.AvaloniaEdit is 12.0.0 while Avalonia is 12.1.0.  
- Free-spacing `# …` comments not syntax-highlighted (avoids `#hex` false positives).  
- Built-in library patterns refresh body on upgrade; only favorite flag is user-owned on built-ins.  
- Switching flavor auto-selects preferred codegen language (may override a manual language pick).  
- Compare token detection is insert-text heuristic (not a full parser); still useful for common constructs.  
- CI badge URL assumes GitHub repo `MarshallMoorman/RegexCraft` — adjust if the remote path differs.  
- Screenshot dark-mode pattern editor may need an extra highlight refresh if theme is forced only after first paint.  
- Right-panel Compare minimum (~480 px) can feel large on very small windows (window min width is 1000).  
- Local NuGet: solution `NuGet.config` pins nuget.org (avoids private-feed NUnit resolution issues).  
- **Website is not live until Marshall enables Pages + DNS** (source is in-repo).  

## How to Continue in a New Conversation

1. Open latest `main` (Phase 12 website commit or later).  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Prefer **Export**, **Debug for PCRE2/JS**, or **installers** — do not re-litigate Phases 0–12 unless fixing bugs.  
4. Website tweaks: edit `website/` only; follow `docs/development/website.md`.  
5. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating a baseline.  
6. Keep CI green: `dotnet test` and watch Actions on push.  
7. Releases: see `docs/development/packaging.md`.  
8. Flavor/engine/compare/debug tests:  
   `dotnet test --filter "Category=Engines|Category=Flavors|Category=Compare|Category=Debug"`  

## Key Files for Post–Phase 12 Work

| Path | Why |
|------|-----|
| `HANDOFF.md` / `AGENTS.md` | Process + conventions |
| `website/` | Public site source |
| `docs/development/website.md` | Pages + DNS for humans |
| `.github/workflows/pages.yml` | Site deploy |
| `src/RegexCraft.Core/Debug/` | Debug service — extend for more engines |
| `src/RegexCraft.Core/Compare/` | Compare service |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Modes, Debug, panel widths |
| `src/RegexCraft.Core/Settings/` | `AppSettings`, `LayoutDefaults` |
| `src/RegexCraft.Engines/` | Real engines |
| `.github/workflows/publish.yml` | Release pipeline |
| `docs/development/packaging.md` | How to cut releases |
| `Directory.Build.props` | Version only |
| `docs/CHANGELOG.md` | User-facing history |
