# RegexCraft Architecture (Phase 0)

## Overview

```
┌─────────────────────┐
│  RegexCraft.App     │  Avalonia 12 UI (MVVM)
│  (shell, theme,     │
│   logging bootstrap)│
└──────────┬──────────┘
           │ uses
           ▼
┌─────────────────────┐     ┌──────────────────────┐
│  RegexCraft.Core    │◄────│ RegexCraft.Engines   │
│  IRegexEngine       │     │ DotNetRegexEngine    │
│  Result models      │     │ PcreRegexEngine      │
│  FlavorService      │     │ EngineFactory        │
└─────────────────────┘     └──────────────────────┘
           ▲
           │ tested by
┌─────────────────────┐
│ RegexCraft.Tests    │  NUnit
└─────────────────────┘
```

## Engines

Every engine implements `IRegexEngine`:

| Member | Purpose |
|--------|---------|
| `Id` | Stable id (`dotnet`, `pcre2`) |
| `DisplayName` | UI label |
| `SupportsFullTesting` | Tier 1 engines |
| `SupportsReplace` | Replace support flag |
| `Match(...)` | All matches + groups |
| `Replace(...)` | Substitution |

Both engines map `RegexOptionsEx` to native flags and return **the same** result models. Invalid patterns yield `Success = false` and an `ErrorMessage` instead of crashing the UI.

### Result models (highlight-ready)

- `MatchCollectionResult` — success/error, list of matches, duration, engine id
- `MatchResult` — `Index`, `Length`, `Value`, `Groups`
- `GroupResult` — number, name, index, length, value, success
- `ReplaceResult` — result string, replacement count, duration

UI layers can paint ranges from `Index`/`Length` without knowing which engine ran.

## Flavor system

`FlavorDefinition` describes a flavor and which `EngineId` implements it.  
`FlavorService` holds hard-coded Phase 0 flavors and resolves `IRegexEngine` instances.

**Adding a third flavor later:**

1. Implement `IRegexEngine` (if needed) in `RegexCraft.Engines`
2. Register it in `EngineFactory.CreateDefaultEngines`
3. Add a `FlavorDefinition` in `FlavorService` (or load from YAML/JSON later)

## Theme

`Themes/Colors.axaml` defines `ResourceDictionary.ThemeDictionaries` for **Light** and **Dark** with named colors and brushes (Microsoft-style blues). All UI chrome uses `{DynamicResource ...}`. No hard-coded colors; no purple.

Theme cycle in the shell: System → Light → Dark → System (`Application.RequestedThemeVariant`).

## Logging

- Abstractions: `Microsoft.Extensions.Logging`
- Provider: **Serilog** file sink
- Config: `appsettings.json` (+ `appsettings.Development.json`)
- Defaults: daily rolling files at `logs/regexcraft-.log`, **7-day** retention, Information minimum (Debug in Development)
- Bootstrapped in `App.OnFrameworkInitializationCompleted` via `LoggingBootstrap`

## Testing

NUnit project `RegexCraft.Tests`:

- Shared engine scenarios in `EngineTestBase` (Match, Replace, named groups, options, invalid patterns, Unicode, large input)
- Concrete fixtures for DotNet and PCRE2
- Flavor registry and result model unit tests
- Categories: `Engines`, `DotNet`, `Pcre`, `Core`, `Flavors`

```bash
dotnet test
dotnet test --filter Category=Engines
```

## Versioning

Single source of truth: `Directory.Build.props` → `<Version>0.1.0</Version>`.  
Package versions: central management in `Directory.Packages.props`.

## Out of scope (Phase 0)

AvaloniaEdit, token palette, analysis tree, library/history, GREP, code gen, debug stepping, more than two engines.
