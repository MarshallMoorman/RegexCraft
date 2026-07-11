# RegexCraft Architecture

**Version**: 0.3.0 (Phase 2)

## Overview

```
┌──────────────────────────────────────────────────────────────────┐
│ RegexCraft.App (Avalonia 12 + AvaloniaEdit)                      │
│  Toolbar · Tokens/Library/History · Editor · Analysis            │
│  Test / Replace / Split / Generate · Status                      │
└───────────────────────────────┬──────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
 RegexCraft.Core          RegexCraft.Engines      Theme / Serilog
 IRegexEngine             DotNetRegexEngine       Colors.axaml
 Result models            PcreRegexEngine         appsettings.json
 FlavorService            EngineFactory
 TokenCatalog
 RegexAnalysisService
 MatchHighlightBuilder
 ReplaceHighlightBuilder
 CodeGenerationService
 JsonLibraryStore / JsonHistoryStore
 TokenInsertion
```

## UI layout (Phase 2)

```
Toolbar: Flavor | Match | Replace | Split | Generate | Options | Theme
┌──────────┬────────────────────────────┬────────────────────┐
│ Tokens   │ Regex Editor (AvaloniaEdit)│ Test | Replace     │
│ Library  │ blue syntax highlighting   │ Split | Generate   │
│ History  │ ─────────────────────────  │ Subject + HL       │
│          │ Analysis Tree (rich, live) │ Matches + Groups   │
│          │ click → select in editor   │ Replace preview HL │
└──────────┴────────────────────────────┴────────────────────┘
Status: Flavor | Engine | Counts | Time | Shortcuts
```

### Modes

| Mode | Behavior |
|------|----------|
| **Test** | Live Match, subject highlights, match/group list with Copy/Go |
| **Replace** | Live replace preview, substitution spans highlighted, backrefs |
| **Split** | Parts list, delimiter highlights on subject, remove-empty option |
| **Generate** | Language + operation → snippet; Copy to clipboard |

### Live updates

`MainWindowViewModel` debounces (~200 ms) pattern/subject/option changes, then:

1. Rebuilds the analysis tree  
2. Runs Match on the active engine  
3. Rebuilds highlight spans and match list  
4. Refreshes Replace or Split when that tab is active  
5. Regenerates code when pattern/options/language change  

### Keyboard

- **Ctrl+Enter** — Run current mode  
- **Ctrl+1…4** — Test / Replace / Split / Generate  

## Core services (Phase 2)

| Type | Role |
|------|------|
| `ITokenCatalog` / `TokenCatalog` | Text-only tokens + search + engine support hints |
| `TokenInsertion` | Pure insert/replace-selection logic |
| `IRegexAnalysisService` / `RegexAnalysisService` | Rich structural tree with offsets |
| `MatchHighlightBuilder` | Match/group → `HighlightSpan` |
| `ReplaceHighlightBuilder` | Replacement spans + split delimiters |
| `ICodeGenerationService` | Multi-language snippets |
| `ILibraryStore` / `JsonLibraryStore` | Saved patterns (JSON) |
| `IHistoryStore` / `JsonHistoryStore` | Recent patterns (JSON, capped) |
| `IRegexEngine` | Match / Replace / Split |

## Engines

| Id | Display | Match | Replace | Split |
|----|---------|-------|---------|-------|
| `dotnet` | .NET | Yes | Yes (+ spans) | Yes |
| `pcre2` | PCRE2 | Yes | Yes (manual `$n`/`${name}` expansion + spans) | Yes |

Both return the same result models so highlighting and group UI stay engine-agnostic.

## Persistence

User data directory via `AppDataPaths.GetDataDirectory()`:

- `library.json` — saved patterns  
- `history.json` — recent patterns (max ~40)  

## Theme

`Themes/Colors.axaml` ThemeDictionaries (Light/Dark). UI and highlight brushes use `{DynamicResource …}` only.

Key highlight keys: `MatchHighlight`, `GroupHighlight0`–`3`, plus brand blues.

## Logging

Serilog file sink, 7-day rolling, `appsettings.json`. Logs flavor selection, tests, replace/split, library saves, codegen copy, errors.

## Testing

NUnit covers engines (Match/Replace/Split/backrefs), tokens, insertion, analysis, highlight builders, codegen, library/history stores, ViewModel workflows.

```bash
dotnet test
dotnet test --filter Category=Analysis
dotnet test --filter Category=Codegen
dotnet test --filter Category=Library
```

## Versioning

`Directory.Build.props` → `0.3.0`  
Central packages: `Directory.Packages.props` (includes `Avalonia.AvaloniaEdit`)

## Out of scope (still)

- GREP / multi-file search  
- Debug step-through  
- Engines beyond .NET + PCRE2  
- Cloud library sync / plugins  
