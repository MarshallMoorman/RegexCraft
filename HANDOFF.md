# RegexCraft – HANDOFF.md

**Current Version**: 0.8.0 (Phase 7 complete — branding + automated testing)  
**Date**: 2026-07-11  
**Next**: Toward **1.0** (Debug, Compare, packaging, website — do not rush 1.0)

---

## What Was Completed in Phase 7

1. **Application icon**  
   - Blue “RC” monogram assets: `regexcraft-icon.ico` / `.png` / `.icns` (+ source `regexcraft-512.png`)  
   - Wired as `Window.Icon`, `ApplicationIcon`, and About image  

2. **Custom About RegexCraft**  
   - Replaced Avalonia default About  
   - `NativeMenu` on Application + Help menu on MainWindow: **About RegexCraft**  
   - Dialog: name, version, description, copyright (Marshall Moorman), regexcraft.com + GitHub links, “Built with Avalonia”  

3. **Expanded automated testing**  
   - ~330 NUnit tests (unit + headless UI + screenshots)  
   - New unit coverage: engine edge cases, full codegen matrix, built-in library compile checks, token insertion, replace highlights, ViewModel theme/options/history, branding  
   - Avalonia.Headless.NUnit + Skia (`UseHeadlessDrawing = false`) for UI workflows  
   - Screenshot category writes `docs/screenshots/*.png` for README/docs  

4. **Bugfixes found while testing**  
   - Built-in URL slug sample now matches its subject  
   - `ReapplyThemeFromSettings` uses in-memory `ThemeLabel` (no longer clobbering cycles on open)  

- Version **0.8.0**; all tests green  

## Versioning recommendation

Stay on **0.8.x** / **0.9.x** for depth features. Ship **1.0.0** only after:

1. At least one of Debug or Compare feels production-ready  
2. Packaging / install story is documented  
3. Website (regexcraft.com) has a real landing page + download path  
4. A short public beta or RC cycle with no P0 layout/engine bugs  

Optional tags: `0.9.0` (Debug or Compare), `1.0.0-rc1`.

---

## Recommended Post-0.8 Roadmap

### Priority A — Product depth

1. **Debug / step-through**  
   - Step match engine (start with .NET): current position, captures, next match  
   - Backtrack visualization if feasible without a full custom NFA  
   - Mode must fit the right-panel host pattern  

2. **Compare mode**  
   - Side-by-side flavors/engines for the same pattern/subject  
   - Diff match counts, first divergence offset, option differences  

3. **Export**  
   - Matches/groups → CSV / JSON  
   - GREP results export  
   - Library export/import  

### Priority B — Engine fidelity

4. **Higher-fidelity Python / Java**  
   - Optional native interop or better pure-.NET approximations  
   - Keep fidelity banners honest  

5. **RE2-style engine** for Go/Rust testing (lookbehind/backref rejection or true RE2 port)  

6. **Performance**  
   - Virtualization for huge match lists and GREP results  
   - Match limits UI; cancel in-flight Test when pattern changes  
   - Stress large multi-MB subjects  

### Priority C — Editor & UX

7. **Editor upgrades** — find-in-pattern, word-wrap, error underlines, match navigation (F3)  
8. **GREP UX** — open hit externally, multi-select replace, optional `.gitignore`  
9. **Layout persistence** — column widths / analysis height  

### Priority D — 1.0 & presence

10. **Packaging** — documented `dotnet publish` per OS; optional installers; use `.icns`/`.ico` in bundles  
11. **Website (regexcraft.com)** — feature list, screenshots from `docs/screenshots/`, download, docs  
12. **1.0 release** — CHANGELOG, tag, GitHub Release assets  

**Out of scope unless requested**: cloud library sync, plugin system, full visual regex builder.

---

## Known Issues / Limitations (carry-forward)

- Analysis tree is structural/heuristic — not a full flavor-faithful AST.  
- Approximate flavors (Python, Java, Go, Rust, …) intentionally use closest engines.  
- Go/Rust codegen notes RE2 limits but still emits the pattern as-is.  
- PCRE replacement expansion is custom (covers `$n`, `${name}`, `$&`, `\n`).  
- JS named replacements map `${name}` → `$<name>` for Jint testing.  
- GREP does not parse `.gitignore`; use exclude globs.  
- GREP preview caps very large files (~200k chars).  
- Large multi-MB subjects still not stress-tested.  
- Avalonia.AvaloniaEdit is 12.0.0 while Avalonia is 12.1.0.  
- Free-spacing `# …` comments not syntax-highlighted (avoids `#hex` false positives).  
- Built-in library patterns refresh body on upgrade; only favorite flag is user-owned on built-ins.  
- Screenshot dark-mode pattern editor may need an extra highlight refresh if theme is forced only after first paint (production theme cycle is fine).  
- GitHub URL in About points at a conventional repo path; update if the public repo differs.

## How to Continue in a New Conversation

1. Open latest `main` at **v0.8.0**.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Pick a roadmap item (recommend **Debug** or **Compare**).  
4. Author `docs/development/PHASE-8-REQUIREMENTS.md` if doing a full phase.  
5. Do not re-build Phase 0–7 foundations unless blocked.  
6. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating a baseline.  
7. Regenerate docs screenshots with `dotnet test --filter Category=Screenshots` when UI changes.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.App/Views/AboutWindow.axaml` | Custom About |
| `src/RegexCraft.App/Assets/regexcraft-icon.*` | Branding |
| `tests/RegexCraft.Tests/Headless/` | UI + screenshot tests |
| `docs/screenshots/` | README/doc images |
| `src/RegexCraft.Core/Flavors/` | Flavor registry + fidelity |
| `src/RegexCraft.Engines/JavaScript/` | Jint engine |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Settings, modes, theme |
| `Directory.Build.props` | Version 0.8.0 |
