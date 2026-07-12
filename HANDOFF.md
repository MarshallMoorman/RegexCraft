# RegexCraft – HANDOFF.md

**Current Version**: **1.0.1** (Phase 10 + layout/publish hotfixes)  
**Date**: 2026-07-12  
**Next**: Post-1.0 roadmap — **Debug / step-through for 1.1**, website, fidelity, installers  

---

## 1.0.1 hotfixes (after 1.0.0)

1. **Compare takes most of the editor area**  
   - Center column collapses to ~280 px; right panel is `*` (majority of body)  
   - Cards wrap in a multi-row grid  
   - Leaving Compare restores Normal absolute right width  
   - Stale ~520 px Compare widths from 1.0.0 are ignored  

2. **Publish workflow**  
   - win-x64: `-p:` properties (not `/p:`) so Git Bash does not mangle MSBuild args  
   - linux-x64: no `ls | head` under `pipefail`  
   - Tag **`v1.0.1`** for a full four-RID release (v1.0.0 only got osx zips)

---

## What Was Completed in Phase 10 (1.0.0)

1. **Smart right-panel sizing** (refined in 1.0.1 to majority-of-body Compare layout)  
   - Normal width (Test / Replace / Split / Generate / GREP) vs Compare layout  
   - Switch **to** Compare → expand aggressively; **away** → restore Normal  
   - Splitter drags update the active mode’s stored width  
   - Persisted in `AppSettings` via `JsonSettingsStore`  
   - Constants in `LayoutDefaults` (no magic numbers in the view)

2. **GitHub Releases**  
   - `.github/workflows/publish.yml`: test job → multi-RID publish → Release on `v*` tags  
   - Artifacts: `RegexCraft-{win-x64,linux-x64,osx-x64,osx-arm64}.zip`  
   - CHANGELOG-derived notes + auto-generated release notes  
   - Soft-fail per RID (`fail-fast: false`); refuse empty releases  

3. **Docs & polish**  
   - README 1.0-ready (download table, flavors/fidelity, screenshots, CI badge)  
   - `docs/CHANGELOG.md` **1.0.0** entry  
   - `docs/development/packaging.md` full release process  
   - User Compare guide: layout / width memory notes  
   - Version **1.0.0** in `Directory.Build.props`  

4. **Tests**  
   - `LayoutDefaults` + settings round-trip  
   - ViewModel: target width, remember/persist, `RightPanelModeChanged` on tab switch  

---

## How to ship v1.0.1 (recommended after 1.0.0 osx-only release)

```bash
git checkout main
git pull origin main
grep '<Version>' Directory.Build.props   # 1.0.1
git log -1 --oneline

git tag -a v1.0.1 -m "RegexCraft 1.0.1"
git push origin main
git push origin v1.0.1
```

Then verify on GitHub: **Actions → Publish** — all four RIDs green, **Releases → v1.0.1** has win/linux/osx zips.

Optional: leave `v1.0.0` as-is (macOS-only assets) or edit the release notes to point users at 1.0.1.

---

## Post-1.0 Roadmap

Work these as separate minor/major tracks after 1.0.0 is tagged and CI/Release are green.

### 1.1 — Debug / step-through (primary candidate)

**Goal**: Interactive match debugger for understanding *why* a pattern matches (or fails).

Suggested scope:

- Step through match engine progress (start with **.NET** engine)  
- Show current position in subject, active groups/captures, next match  
- UI mode tab or overlay (e.g. **Debug** as Ctrl+7 or under Test)  
- Clear empty/error states; do not block live Test  
- Tests: unit for step model + headless smoke  

Out of 1.1 unless easy: full PCRE/JS step engines (can show “debug available for .NET only” first).

### Website (regexcraft.com)

- Landing: value prop, feature list, screenshots from `docs/screenshots/`  
- Download: link to GitHub Releases  
- Docs mirror or deep links into `docs/user/`  
- Not blocking for binary distribution (Releases already work)

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

## How to Continue in a New Conversation

1. Open latest `main` at **v1.0.0** (or after the Phase 10 commit).  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Prefer **1.1 Debug** or website/fidelity work — do not re-litigate Phases 0–10.  
4. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating a baseline.  
5. Keep CI green: `dotnet test` and watch Actions on push.  
6. Releases: see `docs/development/packaging.md`.  
7. Flavor/engine/compare tests:  
   `dotnet test --filter "Category=Engines|Category=Flavors|Category=Compare"`  

## Key Files for Post-1.0 Work

| Path | Why |
|------|-----|
| `HANDOFF.md` / `AGENTS.md` | Process + conventions |
| `src/RegexCraft.Core/Compare/` | Compare service (model for future Debug architecture) |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Modes, settings, panel widths |
| `src/RegexCraft.Core/Settings/` | `AppSettings`, `LayoutDefaults`, `JsonSettingsStore` |
| `src/RegexCraft.Engines/` | Real engines — Debug likely starts in DotNet |
| `.github/workflows/publish.yml` | Release pipeline |
| `docs/development/packaging.md` | How to cut releases |
| `Directory.Build.props` | Version only |
| `docs/CHANGELOG.md` | User-facing history |
