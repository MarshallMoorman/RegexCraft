# Changelog

All notable changes to RegexCraft are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

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
