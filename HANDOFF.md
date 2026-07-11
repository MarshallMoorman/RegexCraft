# RegexCraft – HANDOFF.md

**Current Version**: 0.5.0 (Phase 4 complete)  
**Date**: 2026-07-11  
**Next Phase**: Phase 5  

---

## What Was Completed in Phase 4

- **Critical: light-mode regex editor readability**
  - Full editor theming (background, foreground, selection, caret, line numbers, current line)
  - High-contrast `RegexSyntaxPalette` for light and dark
  - Theme resources: `Editor*`, `Syntax*` brushes in `Colors.axaml`
  - `ApplyEditorTheme` reapplies on every theme change
- **Critical: equal-width token category panels**
  - Shared `tokenCategory` Expander style + stretch layout
  - Consistent borders, padding, hover
- **Full light-theme polish**
  - Options row spacing, status bar contrast, panel headers
  - GREP: grouped globs/options chrome, clearer empty state, progress labels
  - Generate: helper text + engine notes in snippets
  - Library/History empty states and token row density
  - Analysis tree selection / spacing
- **Token catalog expansion**
  - More lookarounds, groups, Unicode, quantifiers, common patterns
  - Multi-word search documented/tested
- **Codegen**: engine-source notes, dialect warnings (Go RE2 / Rust / JS / etc.)
- Version **0.5.0**; docs + AGENTS/HANDOFF updated
- NUnit: **147** tests (all green)

## Exact Next Steps for Phase 5

Author or load `docs/development/PHASE-5-REQUIREMENTS.md` first, then implement. Suggested focus (product-facing features deferred from polish phases):

1. **Debug / step-through** — step match engine (at least .NET), show current position, captures; backtrack visualization if feasible.
2. **Compare mode** — side-by-side .NET vs PCRE2 results for the same pattern/subject (diff match counts, first divergence).
3. **Editor upgrades** — find-in-pattern, word-wrap toggle, error underlines from engine parse offsets, multi-line comfort, match navigation (F3 / Shift+F3).
4. **Performance** — virtualization for huge match lists and GREP results; match limits UI; cancel in-flight Test when pattern changes mid-run.
5. **Export** — export matches/groups as CSV/JSON; export library; export GREP results.
6. **Engine expansion** (optional) — Oniguruma, RE2, or Java-flavor shim behind `IRegexEngine` with capability flags.
7. **UX polish** — GREP open-in-external-editor, multi-select replace, panel layout persistence, high-DPI refinements.
8. When green → bump version, update AGENTS.md + this file, CHANGELOG, commit.

**Out of scope until later unless Phase 5 requirements say otherwise**: cloud library sync, plugin system, advanced visual regex builder.

## Known Issues / TODOs from Phase 4

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

1. Open latest `main` at v0.5.0.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Read `docs/development/architecture.md` and `docs/development/PHASE-4-REQUIREMENTS.md` for history.  
4. Author or load `docs/development/PHASE-5-REQUIREMENTS.md`, then implement.  
5. Do not re-build Phase 0–4 foundations unless blocked.  
6. Do not commit `docs/development/current_screenshot.png` unless intentionally updating the baseline.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.App/Themes/Colors.axaml` | Light/dark design tokens including editor + syntax |
| `src/RegexCraft.App/Highlighting/RegexHighlightingDefinition.cs` | Regex syntax palette |
| `src/RegexCraft.App/Views/MainWindow.axaml` | Multi-panel layout, token widths, modes |
| `src/RegexCraft.App/Views/MainWindow.axaml.cs` | Editor theming, GREP preview, selection |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Live test/replace/split, GREP, codegen, library, settings |
| `src/RegexCraft.Core/Grep/` | GREP service, globs, models |
| `src/RegexCraft.Core/Tokens/TokenCatalog.cs` | Token palette |
| `src/RegexCraft.Core/Codegen/CodeGenerationService.cs` | Language snippets |
| `Directory.Build.props` | Version 0.5.0 |
| `docs/user/*.md` | User-facing guides |
