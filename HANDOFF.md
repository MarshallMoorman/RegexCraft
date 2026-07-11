# RegexCraft – HANDOFF.md

**Current Version**: 0.1.0 (Phase 0 complete)  
**Date**: 2026-07-11  
**Next Phase**: Phase 1  

---

## What Was Completed in Phase 0

- Solution structure: `RegexCraft.Core` / `RegexCraft.Engines` / `RegexCraft.App` / `RegexCraft.Tests`
- `IRegexEngine` abstraction + consistent Match/Replace result models (groups & named groups, highlight-friendly ranges)
- Two fully working engines: `DotNetRegexEngine` + `PcreRegexEngine`
- Flavor system: `FlavorDefinition` + `FlavorService` / `IFlavorService`
- Variable-driven professional blue light/dark theme (`Themes/Colors.axaml`)
- Serilog file logging (7-day rolling, `appsettings.json` configurable)
- NUnit suite covering both engines (shared scenarios), flavors, and result models
- Minimal Avalonia shell: flavor dropdown, options, Match/Replace, results with groups, theme cycle, version
- `docs/` user + development docs and CHANGELOG for 0.1.0
- Versioning via `Directory.Build.props` (`0.1.0`) + central package management
- MIT LICENSE, `.gitignore`, README, this handoff file

## Exact Next Steps for Phase 1

Phase 1 focus: **Core Editor + Test + Highlighting** (highest priority features)

1. Add **AvaloniaEdit** for the main regex editor with blue syntax highlighting (use theme resources only).
2. Implement text-based searchable **Token palette** (categories + labels + tooltips) — **no icons for tokens**.
3. Live **Analysis Tree** (AST → hierarchical explanation). Prefer engine-agnostic or .NET-first parsing if PCRE AST is hard.
4. Full **Test panel** with excellent match highlighting and group display for **both** engines (consume `Index`/`Length` from existing result models).
5. Evolve the Phase 0 shell into the real multi-panel layout (editor | palette | test | analysis).
6. Keep both engines working; expand tests for highlighting helpers and any new services.
7. Write user docs for the new testing experience under `docs/user/`.
8. When green → bump to **0.2.0**, update AGENTS.md + this HANDOFF.md, update CHANGELOG, commit.

## Known Issues / TODOs from Phase 0

- Empty-pattern behavior differs slightly by engine (both handled without throwing; UI does not special-case it).
- PCRE2 `Replace` counts matches via a pre-pass (`Matches` then `Replace`); fine for Phase 0 scale.
- `ExplicitCapture` / `IgnorePatternWhitespace` are mapped and unit-tested for option flags but not deeply exercised in shared Match scenarios.
- Appsettings are loaded from the app output directory; running from an unusual CWD still writes `logs/` relative to CWD when Serilog expands the path — both `logs/` under CWD and output are created best-effort.
- No Avalonia headless UI tests yet (optional; engines are fully covered).
- Diagnostics package is Debug-only; not needed for production builds.

## How to Continue in a New Conversation

1. Pull/open latest `main` at v0.1.0.
2. Read this `HANDOFF.md` and `AGENTS.md`.
3. Skim `docs/development/architecture.md` and `PHASE-0-REQUIREMENTS.md` for historical context.
4. Obtain or write **Phase 1 requirements**, then implement against them.
5. Do **not** re-litigate Phase 0 architecture unless a hard blocker appears.

## Key Files to Review First

| Path | Why |
|------|-----|
| `src/RegexCraft.Core/Engines/IRegexEngine.cs` | Engine contract |
| `src/RegexCraft.Core/Models/` | Result shapes for highlighting |
| `src/RegexCraft.Engines/DotNet/` + `Pcre/` | Engine implementations |
| `src/RegexCraft.Core/Flavors/FlavorService.cs` | Flavor → engine resolution |
| `src/RegexCraft.App/Themes/Colors.axaml` | Theme tokens |
| `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs` | Current shell logic to evolve |
| `Directory.Build.props` | Current version |
| `tests/RegexCraft.Tests/Engines/` | Patterns for new engine tests |
