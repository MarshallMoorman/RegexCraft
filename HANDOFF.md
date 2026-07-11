# RegexCraft – HANDOFF.md

**Current Version**: 0.3.0 (Phase 2 complete)  
**Date**: 2026-07-11  
**Next Phase**: Phase 3  

---

## What Was Completed in Phase 2

- Window title fixed to **RegexCraft** (XAML + code-behind + App startup)
- **Rich Analysis Tree**: hierarchical AST-style breakdown with human descriptions, start/length offsets, auto-expanded nodes, click-to-select in the pattern editor
- **Replace** fully functional: live/preview, replacement-span highlighting in result editor, `$1` / `${name}` / `\n` backreferences for **both** .NET and PCRE2, replacement count
- **Split** fully functional: numbered parts list, delimiter highlighting in subject, remove-empty-entries option, both engines
- **Code Generation**: C#, JavaScript, Python, PHP, Java, Go, Rust — IsMatch / Match / Matches / Replace / Split, options-aware, one-click copy
- **Library**: save/load/search/delete patterns (JSON under user app data), includes subject/replacement/options/flavor
- **History**: automatic recent patterns (capped, de-duplicated, persisted), click to restore
- Token palette expanded (Unicode, Common patterns, more groups/quantifiers); engine-specific opacity hint; reliable insert + focus restore
- Stronger regex **syntax highlighting** (groups, named groups, lookarounds, escapes, quantifiers)
- Matches & Groups: clearer badges, Copy / Go (select in subject) per match and group
- Toolbar mode buttons (Match/Replace/Split/Generate) with active state; Options context label; polish spacing/empty states
- Keyboard: **Ctrl+Enter** run; **Ctrl+1–4** modes
- Engines: `IRegexEngine.Split`, `ReplaceResult.ReplacementSpans`, PCRE manual replacement expansion
- NUnit: **124** tests (engines, analysis, codegen, library/history, VMs, tokens, highlighting)
- Docs: replacing, generating-code, library-and-history; updated testing/getting-started/architecture/CHANGELOG
- Version **0.3.0**

## Exact Next Steps for Phase 3

Author or load `docs/development/PHASE-3-REQUIREMENTS.md` first, then implement. Suggested focus:

1. **GREP / file search** — search folders/files with the current pattern; results list with line context; open-in-subject or external editor hooks.
2. **Debug / step-through** — step match engine (at least .NET), show current position, captures, and backtrack visualization if feasible.
3. **Engine expansion** (optional) — additional flavors (e.g. Oniguruma, RE2) behind `IRegexEngine` with clear capability flags.
4. **Compare mode** — side-by-side .NET vs PCRE2 results for the same pattern/subject.
5. **Editor upgrades** — find-in-pattern, word-wrap toggle, error underlines from engine parse offsets, multi-line comfort.
6. **Performance** — cancel in-flight tests, match limits, virtualization for huge match lists / GREP results.
7. **Export** — export matches/groups as CSV/JSON; export library.
8. **Settings persistence** — last flavor, options, theme, window size, recent paths for GREP.
9. When green → bump to **0.4.0** (or next agreed version), update AGENTS.md + this file, CHANGELOG, commit.

**Out of scope until later unless Phase 3 requirements say otherwise**: cloud library sync, plugin system, advanced token wizards.

## Known Issues / TODOs from Phase 2

- Analysis tree is structural/heuristic — not a full flavor-faithful AST; exotic constructs may show as “Special group”.
- Go/Rust codegen notes RE2/regex crate limits (no lookbehind/backrefs) but still emits the pattern as-is.
- PCRE replacement expansion is custom (not full PCRE2 replacement grammar); covers `$n`, `${name}`, `$&`, `\n`.
- TreeView auto-expand uses a global `TreeViewItem` style; very deep trees may still need manual collapse for focus.
- History records on successful non-live runs and when the pattern changes in live mode — not every keystroke.
- Avalonia.AvaloniaEdit is 12.0.0 while Avalonia is 12.1.0 (compatible; watch for package updates).
- Large multi-MB subjects not stress-tested for live debounce / UI virtualization.

## How to Continue in a New Conversation

1. Open latest `main` at v0.3.0.  
2. Read this `HANDOFF.md` and `AGENTS.md`.  
3. Read `docs/development/architecture.md` and `docs/development/PHASE-2-REQUIREMENTS.md` for history.  
4. Author or load `docs/development/PHASE-3-REQUIREMENTS.md`, then implement.  
5. Do not re-build Phase 0–2 foundations unless blocked.  
6. Do not commit `docs/development/current_screenshot.png` unless intentionally updating the baseline.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.App/Views/MainWindow.axaml` | Multi-panel layout, modes, library/history |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Live test/replace/split, codegen, library |
| `src/RegexCraft.Core/Analysis/RegexAnalysisService.cs` | Rich analysis tree |
| `src/RegexCraft.Core/Codegen/CodeGenerationService.cs` | Language snippets |
| `src/RegexCraft.Core/Library/` | JSON library + history |
| `src/RegexCraft.Engines/` | Match / Replace / Split both engines |
| `Directory.Build.props` | Version 0.3.0 |
| `docs/user/*.md` | User-facing guides |
