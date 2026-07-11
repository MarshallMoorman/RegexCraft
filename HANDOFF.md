# RegexCraft – HANDOFF.md

**Current Version**: 0.4.0 (Phase 3 complete)  
**Date**: 2026-07-11  
**Next Phase**: Phase 4  

---

## What Was Completed in Phase 3

- **Window title fixed properly**: `Application.Name = "RegexCraft"` (macOS menu bar / system chrome no longer “Avalonia Application”), assembly `ApplicationTitle` / `Product`, window `Title` bound to mode-aware `WindowTitle`
- **GREP** fully implemented:
  - Search + replace across folders using current engine/pattern/options
  - Folder browse, recursive, include/exclude globs (`FileGlobMatcher`)
  - Async + cancellable with progress text
  - Results list + file preview with match highlighting
  - Dry-run replace and optional `.bak` backups on write
  - Both .NET and PCRE2 via `IRegexEngine`
- **Settings** persistence: theme, flavor, options, GREP paths/globs, window size/position
- Library polish: favorites, category, tags, favorite-first sort
- Generate polish: clearer language headers, C# match timeout
- Options tooltips / engine context labels refined
- Root **README.md** rewritten as a professional, timeless project README (no “Phase X” language)
- Docs: `docs/user/grepping.md`, architecture, CHANGELOG, user getting-started shortcuts
- NUnit: **143** tests including GREP, globs, settings, library favorites, VM GREP
- Version **0.4.0**

## Exact Next Steps for Phase 4

Author or load `docs/development/PHASE-4-REQUIREMENTS.md` first, then implement. Suggested focus:

1. **Debug / step-through** — step match engine (at least .NET), show current position, captures; backtrack visualization if feasible.
2. **Compare mode** — side-by-side .NET vs PCRE2 results for the same pattern/subject (diff match counts, first divergence).
3. **Editor upgrades** — find-in-pattern, word-wrap toggle, error underlines from engine parse offsets, multi-line comfort, match navigation (F3 / Shift+F3).
4. **Performance** — virtualization for huge match lists and GREP results; match limits UI; cancel in-flight Test when pattern changes mid-run.
5. **Export** — export matches/groups as CSV/JSON; export library; export GREP results.
6. **Engine expansion** (optional) — Oniguruma, RE2, or Java-flavor shim behind `IRegexEngine` with capability flags.
7. **UX polish** — GREP open-in-external-editor, multi-select replace, panel layout persistence, high-DPI refinements.
8. When green → bump version, update AGENTS.md + this file, CHANGELOG, commit.

**Out of scope until later unless Phase 4 requirements say otherwise**: cloud library sync, plugin system, advanced visual regex builder.

## Known Issues / TODOs from Phase 3

- Analysis tree is structural/heuristic — not a full flavor-faithful AST; exotic constructs may show as “Special group”.
- Go/Rust codegen notes RE2/regex crate limits (no lookbehind/backrefs) but still emits the pattern as-is.
- PCRE replacement expansion is custom (not full PCRE2 replacement grammar); covers `$n`, `${name}`, `$&`, `\n`.
- GREP does not parse `.gitignore` files; use exclude globs instead.
- GREP preview caps very large files (~200k chars) for UI responsiveness.
- Large multi-MB subjects still not stress-tested for live debounce / UI virtualization.
- Avalonia.AvaloniaEdit is 12.0.0 while Avalonia is 12.1.0 (compatible; watch for package updates).
- History records on successful non-live runs and when the pattern changes in live mode — not every keystroke.

## How to Continue in a New Conversation

1. Open latest `main` at v0.4.0.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Read `docs/development/architecture.md` and `docs/development/PHASE-3-REQUIREMENTS.md` for history.  
4. Author or load `docs/development/PHASE-4-REQUIREMENTS.md`, then implement.  
5. Do not re-build Phase 0–3 foundations unless blocked.  
6. Do not commit `docs/development/current_screenshot.png` unless intentionally updating the baseline.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.App/Views/MainWindow.axaml` | Multi-panel layout, modes including GREP |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Live test/replace/split, GREP, codegen, library, settings |
| `src/RegexCraft.Core/Grep/` | GREP service, globs, models |
| `src/RegexCraft.Core/Settings/` | Persisted preferences |
| `src/RegexCraft.Core/Analysis/RegexAnalysisService.cs` | Rich analysis tree |
| `src/RegexCraft.Core/Codegen/CodeGenerationService.cs` | Language snippets |
| `src/RegexCraft.Core/Library/` | JSON library + history |
| `src/RegexCraft.Engines/` | Match / Replace / Split both engines |
| `Directory.Build.props` | Version 0.4.0 |
| `docs/user/*.md` | User-facing guides |
