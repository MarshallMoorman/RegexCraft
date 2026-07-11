# RegexCraft Architecture

**Version**: 0.2.0 (Phase 1)

## Overview

```
┌──────────────────────────────────────────────────────────────────┐
│ RegexCraft.App (Avalonia 12 + AvaloniaEdit)                      │
│  Toolbar · Tokens · Editor · Analysis · Test/Replace · Status    │
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
 TokenInsertion
```

## UI layout (Phase 1)

```
Toolbar: Flavor | Match | Replace | Split(stub) | Options | Theme
┌──────────┬────────────────────────────┬────────────────────┐
│ Tokens   │ Regex Editor (AvaloniaEdit)│ Test | Replace     │
│ search   │ blue syntax highlighting   │ Subject + HL       │
│ text list│ ─────────────────────────  │ Matches + Groups   │
│ Library* │ Analysis Tree (live)       │ Replace preview    │
│ History* │                            │                    │
└──────────┴────────────────────────────┴────────────────────┘
Status: Flavor | Engine | Matches | Time
* placeholders only
```

### Editor

- **AvaloniaEdit** `TextEditor` for pattern and subject  
- Pattern: custom `RegexHighlightingDefinition` (groups, classes, quantifiers, escapes, anchors)  
- Subject: `MatchHighlightTransformer` paints `HighlightSpan` ranges from engine results  
- Token insert targets caret/selection via `EditorBinding` + `TokenInsertion`  

### Live updates

`MainWindowViewModel` debounces (~200 ms) pattern/subject/option changes, then:

1. Rebuilds the analysis tree  
2. Runs Match on the active engine  
3. Rebuilds highlight spans and match list  
4. Optionally refreshes Replace preview when that tab is active  

## Core services (Phase 1)

| Type | Role |
|------|------|
| `ITokenCatalog` / `TokenCatalog` | Text-only token definitions + search |
| `TokenInsertion` | Pure insert/replace-selection logic |
| `IRegexAnalysisService` / `RegexAnalysisService` | Structural analysis tree (engine-agnostic) |
| `MatchHighlightBuilder` | `MatchCollectionResult` → `HighlightSpan` list |
| `IRegexEngine` | Unchanged from Phase 0 (Match / Replace) |

## Engines

| Id | Display | Implementation |
|----|---------|----------------|
| `dotnet` | .NET | `System.Text.RegularExpressions` |
| `pcre2` | PCRE2 | PCRE.NET |

Both return the same result models so highlighting and group UI stay engine-agnostic.

## Theme

`Themes/Colors.axaml` ThemeDictionaries (Light/Dark). UI and highlight brushes use `{DynamicResource …}` only.

Key highlight keys: `MatchHighlight`, `GroupHighlight0`–`3`, plus brand blues.

## Logging

Serilog file sink, 7-day rolling, `appsettings.json`. Logs flavor selection, tests, errors.

## Testing

NUnit covers engines (Phase 0), tokens, insertion, analysis, highlight builder, ViewModel workflows (engine switch, invalid pattern, replace, token insert).

```bash
dotnet test
dotnet test --filter Category=Analysis
dotnet test --filter Category=Highlighting
```

## Versioning

`Directory.Build.props` → `0.2.0`  
Central packages: `Directory.Packages.props` (includes `Avalonia.AvaloniaEdit`)

## Out of scope (still)

GREP, Library/History persistence, code gen, debug stepping, Split panel, additional engines.
