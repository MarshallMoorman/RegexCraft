# RegexCraft Architecture

**Version**: 0.7.0

## Overview

```
┌──────────────────────────────────────────────────────────────────┐
│ RegexCraft.App (Avalonia 12 + AvaloniaEdit)                      │
│  Toolbar · Tokens/Library/History · Editor · Analysis            │
│  Test / Replace / Split / Generate / GREP (single stretch host)  │
│  Fidelity banner · Column splitters · Status                     │
└───────────────────────────────┬──────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
 RegexCraft.Core          RegexCraft.Engines      Theme / Serilog
 IRegexEngine             DotNetRegexEngine       Colors.axaml
 Result models            PcreRegexEngine         appsettings.json
 FlavorService            JavaScriptRegexEngine
  + TestingFidelity       EngineFactory
 TokenCatalog
 RegexAnalysisService
 MatchHighlightBuilder / ReplaceHighlightBuilder
 CodeGenerationService
 GrepService / FileGlobMatcher
 JsonLibraryStore (+ BuiltInLibrary) / JsonHistoryStore / JsonSettingsStore
 TokenInsertion
```

## UI layout

```
Toolbar: Flavor (many) | Match | Replace | Split | Generate | GREP | Options | Theme
┌──────────┬─┬────────────────────────────┬─┬────────────────────┐
│ Tokens   │ │ Regex Editor (AvaloniaEdit)│ │ Test | Replace     │
│ Library  │S│ high-contrast light/dark   │S│ Split | Generate   │
│ History  │p│ syntax + Editor* brushes   │p│ GREP               │
│ built-in │l│ Analysis Tree (rich, live) │l│ Fidelity banner    │
│ badges   │ │ click → select in editor   │ │ Single host fills  │
└──────────┴─┴────────────────────────────┴─┴────────────────────┘
Status: Flavor (+ fidelity) | Engine | Counts | Time | Shortcuts
```

Right modes share one DockPanel last-child **Grid host**. Only one mode is visible; each mode Grid uses `rightMode` + star rows so previews/lists stretch.

### Modes

| Mode | Behavior |
|------|----------|
| **Test** | Live Match, subject highlights, match/group list with Copy/Go |
| **Replace** | Live replace preview, substitution spans highlighted, backrefs |
| **Split** | Parts list, delimiter highlights on subject, remove-empty option |
| **Generate** | Auto snippet for language + operation; Copy to clipboard |
| **GREP** | Folder search/replace, globs, progress, cancel, dry-run, preview |

### Live updates

`MainWindowViewModel` debounces (~200 ms) pattern/subject/option changes, then:

1. Rebuilds the analysis tree  
2. Regenerates code snippets  
3. Runs Match on the active engine (when not on GREP-only live path)  
4. Rebuilds highlight spans and match list  
5. Refreshes Replace or Split when that tab is active  

GREP work is **async**, reports progress, and supports **cancellation**.

### Keyboard

- **Ctrl+Enter** — Run current mode (Search in GREP)  
- **Ctrl+1…5** — Test / Replace / Split / Generate / GREP  

### Application identity

- `Application.Name = "RegexCraft"`  
- `Window.Title` bound to mode-aware `WindowTitle`  

## Core services

| Type | Role |
|------|------|
| `FlavorDefinition` / `FlavorService` | Flavor registry, engine map, fidelity notes |
| `TestingFidelity` | Full / High / Approximate / CodegenOnly |
| `ITokenCatalog` / `TokenCatalog` | Text-only tokens + search + engine support hints |
| `TokenInsertion` | Pure insert/replace-selection logic |
| `IRegexAnalysisService` | Rich structural tree with offsets |
| `MatchHighlightBuilder` / `ReplaceHighlightBuilder` | Highlight spans |
| `ICodeGenerationService` | Multi-language snippets (12 languages) |
| `IGrepService` / `GrepService` | Async file search & replace via `IRegexEngine` |
| `FileGlobMatcher` | Include/exclude globs |
| `BuiltInLibrary` | Shipped default patterns |
| `ILibraryStore` / `JsonLibraryStore` | Saved + built-in merge, favorites/tags |
| `IHistoryStore` / `JsonHistoryStore` | Recent patterns |
| `ISettingsStore` / `JsonSettingsStore` | Theme, flavor, GREP, window bounds |
| `IRegexEngine` | Match / Replace / Split |

## Engines

| Id | Display | Match | Replace | Split | GREP |
|----|---------|-------|---------|-------|------|
| `dotnet` | .NET | Yes | Yes (+ spans) | Yes | Yes |
| `pcre2` | PCRE2 | Yes | Yes (manual `$n`/`${name}` + spans) | Yes | Yes |
| `javascript` | JavaScript (Jint) | Yes | Yes (`${name}`→`$<name>`) | Yes | Yes |

Flavors map onto these engines. Approximate flavors show a banner; see `docs/user/flavors.md`.

### Flavor → engine (summary)

| Flavor | Engine | Fidelity |
|--------|--------|----------|
| .NET | dotnet | Full |
| PCRE2 | pcre2 | Full |
| JavaScript / TypeScript | javascript | High |
| PHP | pcre2 | High |
| Python, Java, Go, Rust, Kotlin, Swift | dotnet | Approximate |
| Ruby, Perl | pcre2 | Approximate |

Adding a flavor: define `FlavorDefinition` in `FlavorService.BuildDefaultFlavors()` and ensure `EngineId` is registered in `EngineFactory`. Optional: new `IRegexEngine` implementation.

## GREP pipeline

1. Enumerate files under root (recursive optional)  
2. Filter by include/exclude globs and max file size; skip binary-ish content  
3. For each file: `IRegexEngine.Match` (search) or `Replace` (replace)  
4. Map match offsets → line number + line text  
5. Report `IProgress<GrepProgress>`; honor `CancellationToken`  
6. Replace: dry-run computes previews; live write optional `.bak` then UTF-8 text  

## Persistence

User data directory via `AppDataPaths.GetDataDirectory()`:

- `library.json` — user + built-in patterns (built-ins merged by stable id)  
- `history.json` — recent patterns (max ~40)  
- `settings.json` — theme, flavor, options, GREP paths, window bounds  

### Theme load order (important)

1. Load `settings.json`  
2. **Suppress** settings saves  
3. Apply flavor, options, GREP fields, theme  
4. Re-enable saves  
5. On window open, `ReapplyThemeFromSettings()`  

Never set `SelectedFlavor` before suppress, or changers will persist default theme over the user’s choice.

## Library built-ins

`BuiltInLibrary.GetDefaults()` returns entries with ids `builtin-*`.  
`JsonLibraryStore` merges them on load, refreshes pattern bodies, preserves `IsFavorite`, and refuses delete of built-ins.
