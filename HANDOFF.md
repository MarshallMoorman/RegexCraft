# RegexCraft – HANDOFF.md

**Current Version**: **1.1.0** (Phase 11 — Debug + equal-width match cards)  
**Date**: 2026-07-11  
**Next**: Website, Debug for more engines, export, fidelity, installers  

---

## What Shipped in 1.1.0 (Phase 11)

1. **Debug / step-through (.NET)**  
   - New **Debug** tab (Ctrl+7); F10 forward / F11 back; Reset / End / Refresh  
   - Hybrid **educational** session: real .NET `Match` results + Analysis Tree walk  
   - Pattern selection + subject highlight + step list + capture explanations  
   - Clear unavailable message for PCRE2 / JavaScript engines  
   - Core: `IRegexDebugService` / `RegexDebugService` under `src/RegexCraft.Core/Debug/`  
   - User doc: `docs/user/debugging.md`  

2. **Matches & Groups equal width**  
   - `ListBox.matchList` styles force stretch so cards share one consistent width  

3. **Tests**  
   - `Category=Debug` unit + ViewModel tests  
   - Headless: Debug step + match card width assertions  
   - Full suite green (653+ tests at ship)  

4. **Release**  
   - Version **1.1.0** in `Directory.Build.props`  
   - Tag `v1.1.0` → Publish workflow multi-RID zips  

### Debug approach (for future agents)

Do **not** try to re-implement the full .NET NFA. Extend `RegexDebugService` (or add engine-specific builders behind `IRegexDebugService`) so UI stays engine-agnostic. Prefer real Match overlays + structural walk-throughs over cycle-accurate simulation.

---

## Post-1.1 Roadmap

Work these as separate tracks on `main` (or short-lived branches if needed). Prefer small, shippable increments.

### Website (regexcraft.com)

- Landing: value prop, feature list, screenshots from `docs/screenshots/`  
- Download: link to GitHub Releases  
- Docs mirror or deep links into `docs/user/`  
- **GitHub Pages** is fine: `gh-pages` or `/docs`, CNAME `regexcraft.com`, registrar CNAME → Pages host  
- Not blocking for binary distribution (Releases already work)

### Debug expansion

- PCRE2 / JavaScript educational steppers (same UI, new engine builders)  
- Optional play/pause auto-step  
- Richer backtracking narratives where cheap  

### Higher engine fidelity

- True Python / Java interop only with a clear packaging story  
- True RE2 if a maintained .NET wrapper appears  
- Improve approximate-flavor notes and token matrices as dialects change  

### Product depth (any 1.x)

| Track | Notes |
|-------|--------|
| **Export** | Matches/groups → CSV/JSON; GREP export; library import/export |
| **Editor polish** | Find-in-pattern, word-wrap toggle, error underlines, F3 match nav |
| **GREP UX** | Open hit externally, multi-select replace, optional `.gitignore` |
| **Layout persistence** | Left sidebar width / analysis height (right panel already done) |
| **Virtualization** | Huge match lists / GREP; cancel in-flight Test |
| **Installers** | MSI / DMG / AppImage — packaging.md documents portable zips today |

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

## How to Continue in a New Conversation

1. Open latest `main` at **v1.1.0** (or after the Phase 11 commit).  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Prefer **website**, **export**, or **Debug for PCRE2/JS** — do not re-litigate Phases 0–11.  
4. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating a baseline.  
5. Keep CI green: `dotnet test` and watch Actions on push.  
6. Releases: see `docs/development/packaging.md`.  
7. Flavor/engine/compare/debug tests:  
   `dotnet test --filter "Category=Engines|Category=Flavors|Category=Compare|Category=Debug"`  

## Key Files for Post-1.1 Work

| Path | Why |
|------|-----|
| `HANDOFF.md` / `AGENTS.md` | Process + conventions |
| `src/RegexCraft.Core/Debug/` | Debug service — extend for more engines |
| `src/RegexCraft.Core/Compare/` | Compare service (parallel multi-flavor) |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Modes, Debug commands, panel widths |
| `src/RegexCraft.Core/Settings/` | `AppSettings`, `LayoutDefaults`, `JsonSettingsStore` |
| `src/RegexCraft.Engines/` | Real engines |
| `.github/workflows/publish.yml` | Release pipeline |
| `docs/development/packaging.md` | How to cut releases |
| `docs/user/debugging.md` | User-facing Debug guide |
| `Directory.Build.props` | Version only |
| `docs/CHANGELOG.md` | User-facing history |
