# Changelog

All notable changes to RegexCraft are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.0.0] — 2026-07-11

First stable **1.0.0** release.

### Added

- **Smart right-panel sizing** — Compare expands the right panel to a usable width; leaving Compare restores the previous Normal width. Both widths are persisted and respect manual splitter drags (`LayoutDefaults`, `AppSettings.RightPanelNormalWidth` / `RightPanelCompareWidth`)
- **GitHub Releases on version tags** — Publish workflow runs tests, publishes win-x64 / linux-x64 / osx-x64 / osx-arm64, attaches zip archives, and creates a Release with CHANGELOG-derived notes (`softprops/action-gh-release`)
- Full **Compare** mode (from 1.0.0-rc1): 2–4 flavors, live re-run, cards, cross-flavor notes, **Copy summary**, Ctrl+6
- **CI** on push/PR (Ubuntu, Windows, macOS) and documented packaging / release process

### Changed

- Version set to **1.0.0**
- Publish workflow: pre-publish test job, robust Windows zip via PowerShell, refuse empty releases, pre-release detection for `rc`/`beta` tags
- README, packaging docs, user Compare guide, AGENTS.md, and post-1.0 HANDOFF updated for the stable release

### Notes

- Portable self-contained zips are the supported download format for 1.0 (no MSI/DMG yet).
- Debug / step-through matching is planned for **1.1** (see `HANDOFF.md`).
- Approximate flavors still use closest real engines with fidelity banners and token matrices.

## [1.0.0-rc1] — 2026-07-11

### Added

- **Compare panel** — side-by-side multi-flavor comparison (2–4 flavors): validity, match samples/groups, engine + fidelity badges, dropped options, unsupported tokens in pattern, known differences, cross-flavor notes, and **Copy summary**
- **`IRegexCompareService` / `RegexCompareService`** — parallel per-flavor Match using existing engines and flavor matrices (`Category=Compare` tests)
- **GitHub Actions CI** (`.github/workflows/ci.yml`) — Debug + Release build and full NUnit suite on push/PR (Ubuntu, Windows, macOS); TRX + screenshot artifacts
- **Publish workflow** (`.github/workflows/publish.yml`) — `dotnet publish` for win-x64, linux-x64, osx-x64, osx-arm64; manual or tag `v*`; GitHub Release on tag
- **Packaging documentation** — `docs/development/packaging.md` (publish commands, icons, portable zips, release process)
- User guide: `docs/user/comparing.md`
- Headless UI coverage for Compare mode; screenshot capture `main-compare.png`
- Keyboard: **Ctrl+6** for Compare; Ctrl+Enter runs compare when that tab is active

### Changed

- Version bumped to **1.0.0-rc1** (release candidate)
- README updated for 1.0-rc readiness (features, CI badge, Compare, packaging links)
- App version display prefers assembly informational version (supports `rc` suffix)
- Mode shortcuts documented as Ctrl+1–6

### Notes

- Release candidate preceding final **1.0.0**.
- No new real engines; Compare reuses .NET / PCRE2 / JavaScript (Jint) via the flavor registry.

## [0.9.0] — 2026-07-11

### Added

- **Hardened multi-flavor definitions** — every selectable flavor declares supported options, unsupported token ids, preferred codegen language, and known behavioral differences
- **Flavor-aware token palette** — tokens dimmed per flavor (e.g. RE2 lookaround/backref limits for Go/Rust; JS free-spacing / possessive gaps)
- **Flavor-aware options** — unsupported option checkboxes disabled; options filtered before engine execution
- **Preferred codegen language** auto-selected when switching flavors
- **Significant automated tests** — deep engine suite (`Category=Engines`) and per-flavor mapping / fidelity / token / codegen / GREP / ViewModel coverage (`Category=Flavors`)
- Built-in library entries note recommended flavors / RE2 safety where useful

### Changed

- Fidelity banners and option context labels clearer for High and Approximate flavors
- `docs/user/flavors.md` expanded with option matrix, token awareness, and engine-evaluation notes
- README flavor section and test-filter docs updated
- Version bumped to **0.9.0**

### Notes

- Python.NET and RE2.Managed evaluated and **not** integrated (embedding / maintenance); RE2 limits modeled in flavor layer for Go/Rust
- Three real engines remain: .NET, PCRE2, JavaScript (Jint)

## [0.8.0] — 2026-07-11

### Added

- **RegexCraft application icon** — multi-resolution ICO / ICNS / PNG (blue “RC” monogram); set as window icon and `ApplicationIcon`
- **Custom About RegexCraft dialog** — version, description, copyright, links to regexcraft.com and GitHub, “Built with Avalonia” credit; native menu item **About RegexCraft** (replaces Avalonia’s default About)
- **Avalonia.Headless UI tests** — main window workflows (modes, flavor, match/replace/split, library/history, theme, generate, About)
- **Automated screenshot capture** — `Category=Screenshots` writes high-quality PNGs to `docs/screenshots/` via `CaptureRenderedFrame()` for README and docs
- Expanded NUnit coverage: engine edge cases, codegen matrix (all languages × operations), built-in library validation, token insertion, replace highlights, ViewModel theme/options/history, branding smoke tests

### Fixed

- Built-in **URL slug** sample pattern/subject now produces matches (anchors were preventing demos)
- Theme re-apply on window open no longer overwrites an already-cycled in-memory theme preference

### Changed

- NUnit bumped to **4.5.1** (required by Avalonia.Headless.NUnit 12.1)
- README includes real screenshots and instructions for running tests / regenerating captures
- Version bumped to **0.8.0**
- AGENTS.md / HANDOFF.md updated for post–Phase-7 roadmap

## [0.7.0] — 2026-07-11

### Fixed

- **Theme persistence** — Light / Dark / System now restores correctly after restart (settings save was overwriting theme with the default during ViewModel init when flavor selection ran first)
- **Generate tab** — C# (and any selected language) produces code immediately when the tab is shown and whenever pattern/options/flavor/language change; editor text stays in sync without toggling the language dropdown

### Added

- **Built-in Library** — ~20 curated patterns (email, URL, IPv4/IPv6, phones, dates, time, hex color, UUID, credit card, strong password, HTML tags, whitespace, log levels, ISO datetime, slug, semver, …) merged on first run; **Built-in** badge; non-deletable; favorites preserved
- **JavaScript engine** (Jint) for high-fidelity ECMAScript testing
- **Expanded flavors**: JavaScript, TypeScript, Python, Java, PHP, Ruby, Go, Rust, Perl, Kotlin, Swift (plus existing .NET and PCRE2)
- **Testing fidelity** metadata (`Full` / `High` / `Approximate`) with UI banner and status-bar labels
- Code generation targets: TypeScript, Ruby, Perl, Kotlin, Swift (in addition to C#, JS, Python, PHP, Java, Go, Rust)
- User guide: `docs/user/flavors.md`

### Changed

- Flavor registry filters to engines that are registered; PHP shares PCRE2; Python/Java/etc. map to closest engines with clear notes
- Generate panel copy clarifies auto-update behavior
- Version bumped to **0.7.0**
- AGENTS.md / HANDOFF.md rewritten for post–Phase-6 roadmap

## [0.6.0] — 2026-07-11

### Fixed

- **Right-hand mode panels now fully fill available space** — Test, Replace, Split, Generate, and GREP share a single stretch host so star-sized previews and lists expand correctly (no large empty wasted regions, especially on Replace)
- Panel resize: column **GridSplitters** between left sidebar, center editor, and right modes

### Changed

- Consistent right-panel chrome: shared `rightMode` / `editorFrame` / `listFrame` styles, section labels, empty states
- Replace: clearer backreference hint, live-preview footer, preview editor stretches to fill the panel
- Split / GREP empty states improved; History gains **search** filter
- Status bar spacing and automation names for key controls
- Full documentation pass: README, user guides, AGENTS.md, HANDOFF.md (post-Phase-5 roadmap)
- Version bumped to **0.6.0** (final polish of the original 5-phase plan; 1.0 planned separately)

### Notes

- Debug stepping, compare mode, export, extra engines, and website implementation remain future work

## [0.5.0] — 2026-07-11

### Fixed

- **Light-mode regex editor readability** — AvaloniaEdit now uses dedicated `Editor*` theme brushes (foreground, background, selection, caret, line numbers, current line) so pattern text is highly readable
- High-contrast **regex syntax highlighting** for light and dark (`Syntax*` resources + `RegexSyntaxPalette`)
- **Token category panels** all share the same full sidebar width (consistent `tokenCategory` expander style)

### Changed

- Full light-theme visual polish: options row, status bar, panel headers, GREP chrome, Generate helper text, Library/History empty states, Analysis Tree selection
- Expanded **token catalog** (lookarounds, groups, Unicode, common patterns, possessive quantifiers) with multi-word search coverage
- **Code generation** snippets include source-engine notes and clearer dialect/limit comments (Go RE2, Rust, JS, Python, …)
- Theme system hardened with editor + syntax semantic brushes; no hard-coded UI colors
- Version bumped to **0.5.0**
- AGENTS.md / HANDOFF.md updated for Phase 4 → Phase 5

### Notes

- Debug stepping, compare mode, export, and additional engines remain future work (Phase 5+)

## [0.4.0] — 2026-07-11

### Added

- **GREP** mode: search and replace across folders using the current engine/pattern/options
  - Folder picker, recursive scan, include/exclude globs
  - Async search with progress text and **cancellation**
  - Results list (file, line, context) + file preview with match highlighting
  - **Dry-run** replace and optional **`.bak` backups** when writing
  - Works with both **.NET** and **PCRE2** via `IRegexEngine`
- Core services: `IGrepService` / `GrepService`, `FileGlobMatcher`, GREP result models
- **Settings** persistence (`settings.json`): theme, flavor, options, GREP paths/globs, window size/position
- Library **favorites**, **category**, and **tags** (search + favorite-first sort)
- Keyboard **Ctrl+5** for GREP; status-bar shortcut hints updated
- User doc: `docs/user/grepping.md`
- NUnit coverage for globs, GREP search/replace (dry-run + write), settings, library favorites

### Fixed

- macOS / system chrome title no longer shows **“Avalonia Application”** — set `Application.Name` and assembly title to **RegexCraft**; window title binds to mode-aware `WindowTitle`

### Changed

- Root **README.md** rewritten as a timeless project README (no phase language)
- Generate snippets include clearer language headers; C# uses match timeout
- Options tooltips explain engine mapping; status/options context labels refined
- Analysis / Generate / Library / History UX polish (favorite toggle, richer library form)
- Version bumped to **0.4.0**
- AGENTS.md / HANDOFF.md updated for Phase 3 → Phase 4

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
