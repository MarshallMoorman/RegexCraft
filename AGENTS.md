# RegexCraft – AGENTS.md

**Last updated**: 2026-07-11 — Phase 0 complete (v0.1.0)  
**Owner**: Marshall Moorman  

This file is the living guide for any AI agent (or human) working on RegexCraft.

## Project Conventions

- **Language / Framework**: C# / .NET 10 + Avalonia 12
- **UI Pattern**: MVVM (CommunityToolkit.Mvvm)
- **Testing**: NUnit only. All new code must have tests. Run with `dotnet test`.
- **Logging**: Microsoft.Extensions.Logging abstractions + Serilog (file sink). Never use `Console.WriteLine` for real logging.
- **Theme**: All colors must come from the named resources in `Themes/Colors.axaml` ThemeDictionaries. Never hard-code colors. No purple — professional blues only.
- **Versioning**: Managed only in `Directory.Build.props`. Bump according to phase.
- **Packages**: Central Package Management via `Directory.Packages.props`.
- **Commits**: One clean commit per completed phase on `main`.

## Architecture Quick Reference

| Project | Role |
|---------|------|
| `RegexCraft.Core` | `IRegexEngine`, result models, `RegexOptionsEx`, Flavor system |
| `RegexCraft.Engines` | `DotNetRegexEngine`, `PcreRegexEngine`, `EngineFactory` |
| `RegexCraft.App` | Avalonia UI, theme, Serilog bootstrap, shell ViewModels |
| `RegexCraft.Tests` | NUnit coverage for engines + core |

Flavors map to engines via `FlavorService`. Adding a new flavor should only require a definition + optional new engine class registered in `EngineFactory`.

## Current Engines (Phase 0)

| Id | Display Name | Full Testing | Notes |
|----|--------------|--------------|-------|
| `dotnet` | .NET | Yes | `System.Text.RegularExpressions` |
| `pcre2` | PCRE2 | Yes | PCRE.NET wrapper |

## How to Run

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

## Theme Colors

Defined in `src/RegexCraft.App/Themes/Colors.axaml` with Light and Dark dictionaries.

Key resources (use as brushes with `…Brush` suffix in XAML):

| Resource | Purpose |
|----------|---------|
| `PrimaryBlue` | Brand / header / primary buttons |
| `PrimaryBlueHover` / `PrimaryBluePressed` | Interaction states |
| `AccentBlue` / `AccentBlueSoft` | Accents |
| `BackgroundPrimary` / `Secondary` / `Tertiary` | Surfaces |
| `TextPrimary` / `TextSecondary` / `TextOnPrimary` | Typography |
| `BorderSubtle` / `BorderStrong` | Borders |
| `MatchHighlight` / `GroupHighlight0–3` | Future match UI |
| `Success` / `Warning` / `Error` / `Info` | Semantic |

Always bind with `{DynamicResource PrimaryBlueBrush}` (etc.).

## Logging

- Config: root `appsettings.json` (copied to app output)
- Path: `logs/regexcraft-.log`, rolling daily, retain **7** files
- Development: `appsettings.Development.json` raises minimum level to Debug
- Set `DOTNET_ENVIRONMENT=Development` for dev overrides

## After Completing a Phase

1. All tests green (`dotnet test`)
2. Update this AGENTS.md if conventions changed
3. Update HANDOFF.md with exact next steps
4. Bump version in `Directory.Build.props`
5. Update `docs/CHANGELOG.md`
6. Commit on `main` with a clear phase message

## Useful Commands

```bash
dotnet test --filter Category=Engines
dotnet test --filter Category=DotNet
dotnet test --filter Category=Pcre
```

Logs appear in `logs/` (gitignored).

## Key Source Paths

- Interfaces / models: `src/RegexCraft.Core/`
- Engines: `src/RegexCraft.Engines/DotNet/`, `…/Pcre/`
- Theme: `src/RegexCraft.App/Themes/Colors.axaml`
- Shell VM: `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs`
- Requirements history: `PHASE-0-REQUIREMENTS.md`
