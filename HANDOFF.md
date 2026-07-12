# RegexCraft – HANDOFF.md

**Current Version**: **1.0.0-rc1** (Phase 9 complete — Compare + GitHub Actions + polish)  
**Date**: 2026-07-11  
**Next**: Ship final **1.0.0** after RC validation (website, feedback, optional polish)

---

## What Was Completed in Phase 9

1. **Compare panel**  
   - Mode tab + toolbar + **Ctrl+6**  
   - Select **2–4** flavors; live re-run on pattern/subject/options  
   - Cards: engine, fidelity badge, validity, match samples/groups, timing, key notes  
   - Cross-flavor difference list + **Copy summary**  
   - Core service: `IRegexCompareService` / `RegexCompareService` (parallel Match)

2. **GitHub Actions**  
   - `.github/workflows/ci.yml` — push/PR: restore, Debug + Release build, full tests, TRX, optional screenshots  
   - `.github/workflows/publish.yml` — manual or tag `v*`: publish win-x64 / linux-x64 / osx-x64 / osx-arm64; artifacts; Release on tag  

3. **Packaging & docs**  
   - `docs/development/packaging.md`  
   - User: `docs/user/comparing.md`  
   - README 1.0-rc ready (CI badge, Compare, packaging links)  
   - CHANGELOG **1.0.0-rc1** entry  

4. **Tests**  
   - `Category=Compare` service + ViewModel tests  
   - Headless Compare mode test; screenshot `main-compare.png` capture  

5. **Version**  
   - `Directory.Build.props` → **1.0.0-rc1**  

---

## Path from 1.0.0-rc1 → final 1.0.0

Work these in order unless a P0 bug forces a hot patch on the RC.

### 1. Validate the RC (required)

- [ ] Confirm **GitHub Actions CI is green** on `main` after the Phase 9 commit/push  
- [ ] Smoke-test Compare on macOS / Windows / Linux if possible  
- [ ] Regenerate docs screenshots if UI polish lands after RC (`dotnet test --filter Category=Screenshots`)  
- [ ] Fix any P0 layout, engine crash, or CI flakiness  

Optional RC tags: `1.0.0-rc2` only if needed; otherwise jump to `1.0.0`.

### 2. Website (regexcraft.com) — strong 1.0 expectation

- [ ] Landing page: value prop, feature list, screenshots from `docs/screenshots/`  
- [ ] Download path: link GitHub Releases (artifacts from Publish workflow)  
- [ ] Link to user docs (`docs/user/` or published mirror)  
- [ ] Domain DNS / HTTPS already assumed; flesh out content  

### 3. Public release packaging

- [ ] Final version bump: `Directory.Build.props` → **1.0.0**  
- [ ] CHANGELOG **1.0.0** section (promote RC notes + any fixes)  
- [ ] Tag `v1.0.0` and push → Publish workflow creates GitHub Release  
- [ ] Attach / verify win-x64, linux-x64, osx-x64, osx-arm64 archives  
- [ ] Optional: short release blog / social post  

### 4. Optional product depth (not blocking 1.0 if RC is solid)

These improve 1.0 *quality* but Phase 9 already met the “one major track (Compare)” bar:

| Track | Notes |
|-------|--------|
| **Debug / step-through** | Match position, captures, next match; start with .NET |
| **Export** | Matches/groups → CSV/JSON; GREP export; library import/export |
| **Editor polish** | Find-in-pattern, word-wrap toggle, error underlines, F3 match nav |
| **GREP UX** | Open hit externally, multi-select replace, optional `.gitignore` |
| **Layout persistence** | Column widths / analysis height in settings |
| **Installers** | MSI / DMG / AppImage — packaging.md already documents portable zips |

### 5. Engine fidelity (optional post-1.0)

- True Python / Java interop only with a clear packaging story  
- True RE2 if a maintained .NET wrapper appears  
- Virtualization for huge match lists / GREP; cancel in-flight Test  

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

## How to Continue in a New Conversation

1. Open latest `main` at **v1.0.0-rc1**.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Prefer **RC validation + website + 1.0.0 tag** over new features unless blocked.  
4. Do not re-build Phase 0–9 foundations unless fixing bugs.  
5. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating a baseline.  
6. Keep CI green: `dotnet test` and watch Actions on push.  
7. Publish: see `docs/development/packaging.md`.  
8. Flavor/engine/compare tests:  
   `dotnet test --filter "Category=Engines|Category=Flavors|Category=Compare"`  

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.Core/Compare/` | Compare service + models |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Compare mode + all modes |
| `src/RegexCraft.App/Views/MainWindow.axaml` | Compare panel UI |
| `.github/workflows/` | CI + Publish |
| `docs/development/packaging.md` | Release / publish how-to |
| `docs/user/comparing.md` | User-facing Compare guide |
| `Directory.Build.props` | Version **1.0.0-rc1** |
| `docs/CHANGELOG.md` | RC entry |
