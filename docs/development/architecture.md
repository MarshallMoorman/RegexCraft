# RegexCraft Architecture

**Version**: 0.6.0

## Overview

```
┌──────────────────────────────────────────────────────────────────┐
│ RegexCraft.App (Avalonia 12 + AvaloniaEdit)                      │
│  Toolbar · Tokens/Library/History · Editor · Analysis            │
│  Test / Replace / Split / Generate / GREP (single stretch host)  │
│  Column splitters · Status                                       │
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
 MatchHighlightBuilder / ReplaceHighlightBuilder
 CodeGenerationService
 GrepService / FileGlobMatcher
 JsonLibraryStore / JsonHistoryStore / JsonSettingsStore
 TokenInsertion
```

## UI layout

```
Toolbar: Flavor | Match | Replace | Split | Generate | GREP | Options | Theme
┌──────────┬─┬────────────────────────────┬─┬────────────────────┐
│ Tokens   │ │ Regex Editor (AvaloniaEdit)│ │ Test | Replace     │
│ Library  │S│ high-contrast light/dark   │S│ Split | Generate   │
│ History  │p│ syntax + Editor* brushes   │p│ GREP               │
│ equal-w  │l│ Analysis Tree (rich, live) │l│ Single host fills  │
│ panels   │ │ click → select in editor   │ │ all available space│
└──────────┴─┴────────────────────────────┴─┴────────────────────┘
Status: Flavor | Engine | Counts | Time | Shortcuts (Ctrl+1–5)
```

Right modes share one DockPanel last-child **Grid host**. Only one mode is visible; each mode Grid uses `rightMode` + star rows so previews/lists stretch. Do not place multiple mode panels as competing DockPanel fill children.

### Modes

| Mode | Behavior |
|------|----------|
| **Test** | Live Match, subject highlights, match/group list with Copy/Go |
| **Replace** | Live replace preview, substitution spans highlighted, backrefs |
| **Split** | Parts list, delimiter highlights on subject, remove-empty option |
| **Generate** | Language + operation → snippet; Copy to clipboard |
| **GREP** | Folder search/replace, globs, progress, cancel, dry-run, preview |

### Live updates

`MainWindowViewModel` debounces (~200 ms) pattern/subject/option changes, then:

1. Rebuilds the analysis tree  
2. Runs Match on the active engine (when not on GREP-only live path)  
3. Rebuilds highlight spans and match list  
4. Refreshes Replace or Split when that tab is active  
5. Regenerates code when pattern/options/language change  

GREP work is **async**, reports progress, and supports **cancellation**.

### Keyboard

- **Ctrl+Enter** — Run current mode (Search in GREP)  
- **Ctrl+1…5** — Test / Replace / Split / Generate / GREP  

### Application identity

- `Application.Name = "RegexCraft"` (macOS menu bar / system name; avoids “Avalonia Application”)  
- `Window.Title` bound to mode-aware `WindowTitle`  

## Core services

| Type | Role |
|------|------|
| `ITokenCatalog` / `TokenCatalog` | Text-only tokens + search + engine support hints |
| `TokenInsertion` | Pure insert/replace-selection logic |
| `IRegexAnalysisService` / `RegexAnalysisService` | Rich structural tree with offsets |
| `MatchHighlightBuilder` | Match/group → `HighlightSpan` |
| `ReplaceHighlightBuilder` | Replacement spans + split delimiters |
| `ICodeGenerationService` | Multi-language snippets |
| `IGrepService` / `GrepService` | Async file search & replace via `IRegexEngine` |
| `FileGlobMatcher` | Include/exclude globs (`*`, `?`, `**`) |
| `ILibraryStore` / `JsonLibraryStore` | Saved patterns (JSON), favorites/tags |
| `IHistoryStore` / `JsonHistoryStore` | Recent patterns (JSON, capped) |
| `ISettingsStore` / `JsonSettingsStore` | Theme, flavor, GREP paths, window bounds |
| `IRegexEngine` | Match / Replace / Split |

## Engines

| Id | Display | Match | Replace | Split | GREP |
|----|---------|-------|---------|-------|------|
| `dotnet` | .NET | Yes | Yes (+ spans) | Yes | Yes |
| `pcre2` | PCRE2 | Yes | Yes (manual `$n`/`${name}` expansion + spans) | Yes | Yes |

Both return the same result models so highlighting and group UI stay engine-agnostic.

## GREP pipeline

1. Enumerate files under root (recursive optional)  
2. Filter by include/exclude globs and max file size; skip binary-ish content  
3. For each file: `IRegexEngine.Match` (search) or `Replace` (replace)  
4. Map match offsets → line number + line text  
5. Report `IProgress<GrepProgress>`; honor `CancellationToken`  
6. Replace: dry-run computes previews; live write optional `.bak` then UTF-8 text  

## Persistence

User data directory via `AppDataPaths.GetDataDirectory()`:

- `library.json` — saved patterns (favorites, category, tags)  
- `history.json` — recent patterns (max ~40)  
- `settings.json` — UI / GREP preferences and window bounds  

## Theme

`Themes/Colors.axaml` ThemeDictionaries (Light/Dark). UI and highlight brushes use `{DynamicResource …}` only.
