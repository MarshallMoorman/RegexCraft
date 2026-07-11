# Changelog

All notable changes to RegexCraft are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [0.1.0] — 2026-07-11

### Added

- Solution foundation: `RegexCraft.Core`, `RegexCraft.Engines`, `RegexCraft.App`, `RegexCraft.Tests`
- `IRegexEngine` abstraction with `Match` and `Replace`
- Consistent result models: `MatchCollectionResult`, `MatchResult`, `GroupResult`, `ReplaceResult` (including named groups and highlight-friendly ranges)
- `DotNetRegexEngine` using `System.Text.RegularExpressions`
- `PcreRegexEngine` using PCRE.NET (PCRE2)
- Flavor system: `FlavorDefinition`, `IFlavorService` / `FlavorService` with hard-coded .NET and PCRE2 flavors
- Cross-engine options via `RegexOptionsEx` (IgnoreCase, Multiline, Singleline, ExplicitCapture, IgnorePatternWhitespace)
- Variable-driven professional blue light/dark theme (`Themes/Colors.axaml`)
- Serilog file logging with 7-day rolling retention, configured via `appsettings.json`
- Minimal Avalonia 12 shell: flavor selector, Match/Replace test area, theme cycle, version display
- NUnit test suite covering both engines, flavors, and result models
- Documentation skeleton: user getting-started, architecture, this changelog
- Versioning via `Directory.Build.props` (`0.1.0`)
- Central package management (`Directory.Packages.props`)
- MIT license, `.gitignore`, `AGENTS.md`, `HANDOFF.md`

### Notes

- Phase 0 deliberately omits AvaloniaEdit, token palette, analysis tree, library/history, GREP, and code generation (Phase 1+).
