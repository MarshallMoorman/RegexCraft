# Changelog

All notable changes to RegexCraft are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [0.3.0] — 2026-07-11

### Added

- **Split** mode: split subject on pattern matches, numbered parts, delimiter highlighting, remove-empty option (both engines)
- **Code Generation** panel: C#, JavaScript, Python, PHP, Java, Go, Rust for IsMatch / Match / Matches / Replace / Split with one-click copy
- **Library**: save/load/search/delete named patterns (JSON in user app data), including subject, replacement, options, flavor
- **History**: automatic recent patterns (persisted, de-duplicated, capped), click to restore
- Rich **Analysis Tree** with nested groups, lookarounds, quantifiers, character classes, offsets, and click-to-select in the editor
- Replace **result highlighting** for substituted spans; improved backreference support on PCRE2 (`$1`, `${name}`, `\n`)
- Match/group **Copy** and **Go** (select range in subject)
- Keyboard shortcuts: Ctrl+Enter run, Ctrl+1–4 mode switch
- Expanded token catalog (Unicode, Common patterns, more groups/quantifiers) with engine support hints
- User docs: `replacing.md`, `generating-code.md`, `library-and-history.md`

### Changed

- Window title is **RegexCraft**
- Toolbar modes (Match / Replace / Split / Generate) with clearer active state
- Stronger regex syntax highlighting in light and dark themes
- Options strip shows which engine options apply to
- `IRegexEngine` gains `Split` and `SupportsSplit`; `ReplaceResult` includes `ReplacementSpans`
- Version bumped to **0.3.0**
- AGENTS.md / HANDOFF.md rewritten for Phase 2 → Phase 3

### Notes

- GREP, debug stepping, and additional engines remain future work (Phase 3+)

## [0.2.0] — 2026-07-11

### Added

- Professional multi-panel Avalonia UI (toolbar, tokens, editor, analysis, test/replace, status bar)
- AvaloniaEdit regex editor with blue syntax highlighting, line numbers, current-line highlight
- Text-based searchable **Token palette** (no token icons) with categories, tooltips, caret insert
- Live **Analysis Tree** (debounced structural parse; graceful incomplete/invalid handling)
- **Test** panel with subject editor, match + group highlighting for **.NET and PCRE2**
- Expandable match/group list (index, length, named groups)
- Basic **Replace** panel with live/button preview and replacement count
- Flavor selector re-runs tests automatically; status bar shows engine, matches, timing
- Core services: `TokenCatalog`, `TokenInsertion`, `RegexAnalysisService`, `MatchHighlightBuilder`
- User docs: `docs/user/testing-regexes.md`; updated getting-started and architecture
- NUnit coverage for tokens, insertion, analysis, highlighting, ViewModel workflows

### Changed

- Version bumped to **0.2.0**
- Phase 0 planning files moved under `docs/development/`
- Root `AGENTS.md` / `HANDOFF.md` updated for Phase 1 → Phase 2 handoff

### Notes

- Library / History are placeholders only  
- Split is a disabled stub  
- GREP, code generation, and debug stepping remain future work  

## [0.1.0] — 2026-07-11

### Added

- Solution foundation: Core / Engines / App / Tests  
- `IRegexEngine` with Match and Replace; DotNet + PCRE2 engines  
- Flavor system, blue light/dark theme, Serilog 7-day logging  
- Minimal shell and NUnit engine tests  
