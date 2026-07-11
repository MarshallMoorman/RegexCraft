# RegexCraft – HANDOFF.md

**Current Version**: 0.2.0 (Phase 1 complete)  
**Date**: 2026-07-11  
**Next Phase**: Phase 2  

---

## What Was Completed in Phase 1

- Multi-panel professional UI (toolbar, tokens, editor, analysis, test/replace, status)
- AvaloniaEdit pattern editor with blue regex syntax highlighting, line numbers, current line
- Text-only searchable Token palette (categories, tooltips, insert at caret/selection)
- Live Analysis Tree (debounced; incomplete patterns handled without crashes)
- Test panel: subject editor + match/group highlighting for **both** .NET and PCRE2
- Expandable match list with numbered and named groups (index/length/value)
- Basic Replace panel with preview and replacement count
- Flavor switch re-tests automatically; status bar shows engine + matches + timing
- Core helpers: `TokenCatalog`, `TokenInsertion`, `RegexAnalysisService`, `MatchHighlightBuilder`
- NUnit: 87 tests (engines + tokens + analysis + highlighting + ViewModels)
- Docs: testing guide, updated getting-started/architecture/CHANGELOG
- Phase 0 planning files moved to `docs/development/`
- Version **0.2.0**

## Exact Next Steps for Phase 2

Suggested Phase 2 focus (confirm with a formal `PHASE-2-REQUIREMENTS.md` before implementing):

1. **Library** — save/load named patterns (local storage), categories, open into editor.
2. **History** — recent patterns/subjects with one-click restore (persistence + UI beyond placeholders).
3. **Split panel** — implement Split using `IRegexEngine` (add `Split` to the interface if needed) with both engines.
4. **Options polish** — richer options UI; persist last-used flavor/options/theme.
5. **Editor upgrades** — find/replace in pattern, multi-line pattern comfort, optional word-wrap toggle, better incomplete-parse messaging.
6. **Analysis depth** — richer explanations, error underlines in the editor, click tree node → select pattern range.
7. **Test UX** — click match/group → select range in subject; export matches; compare .NET vs PCRE2 side-by-side mode.
8. **Performance** — cancel in-flight tests on huge subjects; match limits; UI virtualization for large match lists.
9. Expand tests and user docs for Library/History/Split.
10. When green → bump to **0.3.0**, update AGENTS.md + this file, CHANGELOG, commit.

Optional later (not Phase 2 unless specified): GREP, code generation, debug stepping, more engines.

## Known Issues / TODOs from Phase 1

- Analysis tree is structural/heuristic — not a full flavor-faithful AST; some exotic constructs are “Special group” or partial.
- Token insert with View attached uses editor caret; unit tests use pure `TokenInsertion` when no event subscriber.
- Replace tab MultiBinding for count is simple; fine for Phase 1.
- Split is disabled stub only.
- Library/History are non-functional placeholders.
- Avalonia.AvaloniaEdit is 12.0.0 while Avalonia is 12.1.0 (compatible; watch for package updates).
- Debounced live test uses `Task.Run` + UI dispatcher; fine for desktop, not stress-tested on multi-MB subjects.
- Group highlight color set includes a soft violet (`GroupHighlight3`) for distinguishability — chrome remains blue-only.

## How to Continue in a New Conversation

1. Open latest `main` at v0.2.0.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Read `docs/development/architecture.md` and `docs/development/PHASE-1-REQUIREMENTS.md` for history.  
4. Author or load `docs/development/PHASE-2-REQUIREMENTS.md`, then implement.  
5. Do not re-build Phase 0/1 foundations unless blocked.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.App/Views/MainWindow.axaml` | Multi-panel layout |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Live test orchestration |
| `src/RegexCraft.App/Highlighting/` | Syntax + match transformers |
| `src/RegexCraft.Core/Tokens/` | Palette catalog |
| `src/RegexCraft.Core/Analysis/` | Analysis tree |
| `src/RegexCraft.Core/Highlighting/` | Highlight span builder |
| `Directory.Build.props` | Version 0.2.0 |
| `docs/user/testing-regexes.md` | User-facing Test docs |
