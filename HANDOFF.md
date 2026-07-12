# RegexCraft – HANDOFF.md

**Current Version**: 0.9.0 (Phase 8 complete — multi-flavor hardening + significant per-flavor tests)  
**Date**: 2026-07-11  
**Next**: Path to **1.0** (Debug, Compare, packaging, website — do not rush 1.0)

---

## What Was Completed in Phase 8

1. **Hardened multi-flavor definitions**  
   - Every flavor: engine map, fidelity, `SupportedOptions`, `UnsupportedTokenIds`, `CodegenLanguageId`, `KnownDifferences`  
   - Shared matrices in `FlavorTokenSets` (RE2, JS, Python, Java, .NET-only)

2. **Token & option awareness**  
   - Palette dims unsupported tokens per flavor (not only per engine)  
   - Option checkboxes disabled when unsupported; `BuildOptions()` filters via `FilterOptions`  
   - Switching flavor selects preferred Generate language  

3. **Significant automated tests**  
   - Deep engine suite: `EngineDeepTests` + existing base/edge cases (`Category=Engines`)  
   - Per-flavor: completeness, mapping/execution, tokens/options, codegen, behavioral differences, ViewModel/GREP (`Category=Flavors`)  
   - Headless UI: fidelity banners, JS option disable, Go lookbehind dimming  

4. **Engine evaluation (honest)**  
   - **Jint** kept; tests strengthen confidence  
   - **Python.NET** not integrated (CPython embed)  
   - **RE2.Managed** not integrated (maintenance); Go/Rust RE2 limits modeled in flavor layer  

5. **Library & docs**  
   - Built-in patterns note recommended flavors / RE2 safety  
   - `docs/user/flavors.md`, README, architecture, CHANGELOG updated  

- Version **0.9.0**; all tests green  

## Versioning recommendation

Stay on **0.9.x** for remaining depth features. Ship **1.0.0** only after:

1. At least one of **Debug** or **Compare** feels production-ready  
2. Packaging / install story is documented  
3. Website (regexcraft.com) has a real landing page + download path  
4. A short public beta or RC cycle with no P0 layout/engine bugs  

Optional tags: `0.9.x` patches, `1.0.0-rc1`, then `1.0.0`.

---

## Path to 1.0

### Priority A — Product depth (choose one major track)

1. **Debug / step-through**  
   - Step match engine (start with .NET): current position, captures, next match  
   - Backtrack visualization if feasible without a full custom NFA  
   - Mode must fit the right-panel host pattern  

2. **Compare mode**  
   - Side-by-side flavors/engines for the same pattern/subject  
   - Diff match counts, first divergence offset, option differences  
   - Builds naturally on Phase 8 flavor/engine test confidence  

3. **Export**  
   - Matches/groups → CSV / JSON  
   - GREP results export  
   - Library export/import  

### Priority B — Engine fidelity (optional)

4. True Python / Java interop only if packaging story is clear  
5. True RE2 if a maintained wrapper appears for modern .NET  
6. Performance: virtualization for huge match lists / GREP; cancel in-flight Test  

### Priority C — Editor & UX

7. Editor upgrades — find-in-pattern, word-wrap, error underlines, match navigation (F3)  
8. GREP UX — open hit externally, multi-select replace, optional `.gitignore`  
9. Layout persistence — column widths / analysis height  

### Priority D — 1.0 & presence

10. **Packaging** — documented `dotnet publish` per OS; optional installers; use `.icns`/`.ico` in bundles  
11. **Website (regexcraft.com)** — feature list, screenshots from `docs/screenshots/`, download, docs  
12. **1.0 release** — CHANGELOG, tag, GitHub Release assets, RC cycle  

**Out of scope unless requested**: cloud library sync, plugin system, full visual regex builder, perfect RegexBuddy parity on every obscure dialect.

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
- Large multi-MB subjects still not stress-tested.  
- Avalonia.AvaloniaEdit is 12.0.0 while Avalonia is 12.1.0.  
- Free-spacing `# …` comments not syntax-highlighted (avoids `#hex` false positives).  
- Built-in library patterns refresh body on upgrade; only favorite flag is user-owned on built-ins.  
- Switching flavor auto-selects preferred codegen language (may override a manual language pick).  
- Screenshot dark-mode pattern editor may need an extra highlight refresh if theme is forced only after first paint.  

## How to Continue in a New Conversation

1. Open latest `main` at **v0.9.0**.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Pick a 1.0 track (recommend **Debug** or **Compare**).  
4. Author `docs/development/PHASE-9-REQUIREMENTS.md` if doing a full phase.  
5. Do not re-build Phase 0–8 foundations unless blocked.  
6. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating a baseline.  
7. Regenerate docs screenshots with `dotnet test --filter Category=Screenshots` when UI changes.  
8. Keep flavor/engine tests green: `dotnet test --filter "Category=Engines|Category=Flavors"`.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.Core/Flavors/` | Definitions, token sets, fidelity |
| `tests/RegexCraft.Tests/Flavors/` | Per-flavor test suites |
| `tests/RegexCraft.Tests/Engines/` | Deep engine tests |
| `docs/user/flavors.md` | User-facing fidelity / options / tokens |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Flavor UI, options, codegen language |
| `src/RegexCraft.Engines/JavaScript/` | Jint engine |
| `Directory.Build.props` | Version 0.9.0 |
