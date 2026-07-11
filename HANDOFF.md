# RegexCraft – HANDOFF.md

**Current Version**: 0.6.0 (Phase 5 complete — final polish of the original plan)  
**Date**: 2026-07-11  
**Next**: Post-0.6 roadmap toward **1.0** (do not rush version 1.0)

---

## What Was Completed in Phase 5

- **Critical: right-panel layout**
  - All five modes (Test / Replace / Split / Generate / GREP) live in a **single stretch host** so star-sized content fills height/width
  - Replace preview no longer leaves a large empty region; editor frames use shared `editorFrame` / `listFrame` styles
- **Resizable body**: column GridSplitters between left sidebar, center editor, and right modes
- **Empty states & polish**: Split/GREP/Test empty frames; History **search**; section labels; status bar spacing; automation names
- **Theme**: selection still persisted via `AppSettings.Theme` (System → Light → Dark)
- Documentation: README, user guides, CHANGELOG, AGENTS, this HANDOFF
- Version **0.6.0**; NUnit **148** tests (all green)

## Versioning recommendation

Stay on **0.6.x** for the next feature increments. Ship **1.0.0** only after:

1. At least one of Debug or Compare feels production-ready  
2. Packaging / install story is documented  
3. Website (regexcraft.com) has a real landing page + download path  
4. A short public beta or RC cycle with no P0 layout/engine bugs  

Optional intermediate tags: `0.7.0` (Debug), `0.8.0` (engines/export), `1.0.0-rc1`.

---

## Recommended Post-0.6 Roadmap

### Priority A — Product depth

1. **Debug / step-through**  
   - Step match engine (at least .NET first): current position, captures, next match  
   - Backtrack visualization if feasible without a full custom NFA  
   - UI tab or mode that does not break the right-panel host pattern  

2. **Compare mode**  
   - Side-by-side .NET vs PCRE2 for the same pattern/subject  
   - Diff match counts, first divergence offset, option differences  

3. **Export**  
   - Matches/groups → CSV / JSON  
   - GREP results export  
   - Library export/import  

### Priority B — Engines & performance

4. **Engine expansion** (optional)  
   - Oniguruma, RE2, or Java-flavor shim behind `IRegexEngine` + capability flags  
   - Token catalog and options row must stay engine-aware  

5. **Performance**  
   - Virtualization for huge match lists and GREP results  
   - Match limits UI; cancel in-flight Test when pattern changes mid-run  
   - Stress large multi-MB subjects with live debounce  

### Priority C — Editor & UX

6. **Editor upgrades**  
   - Find-in-pattern, word-wrap toggle, error underlines from engine parse offsets  
   - Match navigation (F3 / Shift+F3)  
   - Free-spacing `# …` comments in syntax highlighter (careful with `#hex`)  

7. **GREP UX**  
   - Open hit in external editor  
   - Multi-select replace  
   - Optional `.gitignore` respect  

8. **Layout persistence**  
   - Remember column widths / analysis height  

### Priority D — 1.0 & presence

9. **Packaging**  
   - Documented `dotnet publish` per OS; optional installers later  

10. **Website (regexcraft.com)**  
    - Feature list, screenshots, download, docs links  
    - Keep app docs in-repo as source of truth  

11. **1.0 release**  
    - CHANGELOG, tag, GitHub Release assets, short announcement  

**Out of scope unless explicitly requested**: cloud library sync, plugin system, full visual regex builder.

---

## Known Issues / Limitations (carry-forward)

- Analysis tree is structural/heuristic — not a full flavor-faithful AST; exotic constructs may show as “Special group”.
- Go/Rust codegen notes RE2/regex crate limits (no lookbehind/backrefs) but still emits the pattern as-is.
- PCRE replacement expansion is custom (not full PCRE2 replacement grammar); covers `$n`, `${name}`, `$&`, `\n`.
- GREP does not parse `.gitignore` files; use exclude globs instead.
- GREP preview caps very large files (~200k chars) for UI responsiveness.
- Large multi-MB subjects still not stress-tested for live debounce / UI virtualization.
- Avalonia.AvaloniaEdit is 12.0.0 while Avalonia is 12.1.0 (compatible; watch for package updates).
- History records on successful non-live runs and when the pattern changes in live mode — not every keystroke.
- Free-spacing `# …` end-of-line comments are not syntax-highlighted (to avoid false positives on `#hex` patterns).

## How to Continue in a New Conversation

1. Open latest `main` at **v0.6.0**.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Pick a roadmap item (recommend **Debug** or **Compare** next).  
4. If writing a new phase, author `docs/development/PHASE-6-REQUIREMENTS.md` (or feature-specific doc).  
5. Do not re-build Phase 0–5 foundations unless blocked.  
6. Do **not** commit `docs/development/current_screenshot.png` unless intentionally updating the baseline.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.App/Views/MainWindow.axaml` | Right-mode host, splitters, all mode layouts |
| `src/RegexCraft.App/Themes/Colors.axaml` | Light/dark design tokens including editor + syntax |
| `src/RegexCraft.App/Highlighting/RegexHighlightingDefinition.cs` | Regex syntax palette |
| `src/RegexCraft.App/Views/MainWindow.axaml.cs` | Editor theming, GREP preview, selection |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Live test/replace/split, GREP, codegen, library, settings |
| `src/RegexCraft.Core/Grep/` | GREP service, globs, models |
| `src/RegexCraft.Core/Tokens/TokenCatalog.cs` | Token palette |
| `src/RegexCraft.Core/Codegen/CodeGenerationService.cs` | Language snippets |
| `Directory.Build.props` | Version 0.6.0 |
| `docs/user/*.md` | User-facing guides |
