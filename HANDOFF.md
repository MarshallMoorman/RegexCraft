# RegexCraft – HANDOFF.md

**Current Version**: 0.7.0 (Phase 6 complete — multi-flavor expansion)  
**Date**: 2026-07-11  
**Next**: Toward **1.0** (Debug, Compare, packaging, website — do not rush 1.0)

---

## What Was Completed in Phase 6

1. **Theme persistence fixed**  
   - Theme (Light / Dark / System) saved in `settings.json` and restored on startup  
   - Root cause: `SelectedFlavor` assignment during VM init triggered `PersistSettings` and overwrote theme with the default **before** `ApplyThemeFromSettings`  
   - Fix: suppress settings saves for the entire settings-load block; re-apply theme on window open  

2. **Generate tab auto-run**  
   - C# (default) generates immediately on startup and when Generate is selected  
   - Regenerates on pattern / options / flavor / language / operation changes  
   - Generate editor document synced on open and property change (including force refresh)  

3. **Default Library**  
   - ~20 built-in patterns (email, URL, IPv4/IPv6, phones, dates, time, hex color, UUID, credit card, password, HTML tags, whitespace, log levels, semver, slug, …)  
   - Merged into `library.json` on load; **Built-in** badge in UI; not deletable; favorites preserved  

4. **Multi-flavor expansion**  
   - Engines: **.NET**, **PCRE2**, **JavaScript (Jint)**  
   - Flavors: .NET, PCRE2, JavaScript, TypeScript, Python, Java, PHP, Ruby, Go, Rust, Perl, Kotlin, Swift  
   - `TestingFidelity` + banner + status labels  
   - Codegen expanded (TypeScript, Ruby, Perl, Kotlin, Swift + existing languages)  

- Version **0.7.0**; NUnit **195** tests (all green)

## Versioning recommendation

Stay on **0.7.x** / **0.8.x** for depth features. Ship **1.0.0** only after:

1. At least one of Debug or Compare feels production-ready  
2. Packaging / install story is documented  
3. Website (regexcraft.com) has a real landing page + download path  
4. A short public beta or RC cycle with no P0 layout/engine bugs  

Optional tags: `0.8.0` (Debug), `0.9.0` (Compare/export), `1.0.0-rc1`.

---

## Recommended Post-0.7 Roadmap

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

10. **Packaging** — documented `dotnet publish` per OS; optional installers  
11. **Website (regexcraft.com)** — feature list, screenshots, download, docs  
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

## How to Continue in a New Conversation

1. Open latest `main` at **v0.7.0**.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Pick a roadmap item (recommend **Debug** or **Compare**).  
4. Author `docs/development/PHASE-7-REQUIREMENTS.md` if doing a full phase.  
5. Do not re-build Phase 0–6 foundations unless blocked.  
6. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating the baseline.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.Core/Flavors/` | Flavor registry + fidelity |
| `src/RegexCraft.Engines/JavaScript/` | Jint engine |
| `src/RegexCraft.Core/Library/BuiltInLibrary.cs` | Default patterns |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Settings suppress, generate, flavors |
| `src/RegexCraft.App/Views/MainWindow.axaml` | Fidelity banner, library badges |
| `docs/user/flavors.md` | User-facing fidelity guide |
| `Directory.Build.props` | Version 0.7.0 |
