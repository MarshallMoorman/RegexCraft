# RegexCraft Architecture

**Version**: 1.0.1

## Overview

```
┌──────────────────────────────────────────────────────────────────┐
│ RegexCraft.App (Avalonia 12 + AvaloniaEdit)                      │
│  Toolbar · Tokens/Library/History · Editor · Analysis            │
│  Test / Replace / Split / Generate / GREP / Compare (one host)   │
│  Fidelity banner · Column splitters · Status                     │
│  About RegexCraft dialog · regexcraft-icon (ICO/ICNS/PNG)        │
└───────────────────────────────┬──────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
 RegexCraft.Core          RegexCraft.Engines      Theme / Serilog
 IRegexEngine             DotNetRegexEngine       Colors.axaml
 Result models            PcreRegexEngine         appsettings.json
 FlavorService            JavaScriptRegexEngine
  + TestingFidelity       EngineFactory
 RegexCompareService
 TokenCatalog
 RegexAnalysisService
 MatchHighlightBuilder / ReplaceHighlightBuilder
 CodeGenerationService
 GrepService / FileGlobMatcher
 JsonLibraryStore (+ BuiltInLibrary) / JsonHistoryStore / JsonSettingsStore
 TokenInsertion
```

### Testing layers

| Layer | Location | Notes |
|-------|----------|--------|
| Unit | `tests/RegexCraft.Tests/**` | Core, Engines, ViewModels, services |
| Headless UI | `tests/RegexCraft.Tests/Headless/` | Avalonia.Headless.NUnit + Skia |
| Screenshots | `Category=Screenshots` | `CaptureRenderedFrame()` → `docs/screenshots/` |


## UI layout

```
Toolbar: Flavor (many) | Match | Replace | Split | Generate | GREP | Compare | Options | Theme
┌──────────┬─┬────────────────────────────┬─┬────────────────────┐
│ Tokens   │ │ Regex Editor (AvaloniaEdit)│ │ Test | Replace     │
│ Library  │S│ high-contrast light/dark   │S│ Split | Generate   │
│ History  │p│ syntax + Editor* brushes   │p│ GREP | Compare     │
│ built-in │l│ Analysis Tree (rich, live) │l│ Fidelity banner    │
│ badges   │ │ click → select in editor   │ │ Single host fills  │
└──────────┴─┴────────────────────────────┴─┴────────────────────┘
Status: Flavor (+ fidelity) | Engine | Counts | Time | Shortcuts
```

Right modes share one DockPanel last-child **Grid host**. Only one mode is visible; each mode Grid uses `rightMode` + star rows so previews/lists stretch.

**Right-panel width memory** (`LayoutDefaults` + `AppSettings`): Normal absolute width for Test/Replace/Split/Generate/GREP. On **Compare**, the center column collapses to a fixed strip (~280 px) and the right panel takes the remaining star space (~72% of the body by default) so multi-flavor cards fit. Leaving Compare restores Normal. Splitter drags update the active mode’s stored value; stale narrow Compare widths from older builds are ignored.

### Modes

| Mode | Behavior |
|------|----------|
| **Test** | Live Match, subject highlights, match/group list with Copy/Go |
| **Replace** | Live replace preview, substitution spans highlighted, backrefs |
| **Split** | Parts list, delimiter highlights on subject, remove-empty option |
| **Generate** | Auto snippet for language + operation; Copy to clipboard |
| **GREP** | Folder search/replace, globs, progress, cancel, dry-run, preview |
| **Compare** | 2–4 flavors side-by-side; parallel Match; diffs + copy summary; wider right panel |

### Live updates

`MainWindowViewModel` debounces (~200 ms) pattern/subject/option changes, then:

1. Rebuilds the analysis tree  
2. Regenerates code snippets  
3. Runs Match on the active engine (when not on GREP-only live path)  
4. Rebuilds highlight spans and match list  
5. Refreshes Replace, Split, or **Compare** when that tab is active  

GREP work is **async**, reports progress, and supports **cancellation**.  
Compare runs engine Matches in **parallel** via `RegexCompareService`.

### CI / packaging

- GitHub Actions: `.github/workflows/ci.yml`, `publish.yml`  
- Packaging guide: `docs/development/packaging.md`  

### Keyboard

- **Ctrl+Enter** — Run current mode (Search in GREP; Compare in Compare)  
- **Ctrl+1…6** — Test / Replace / Split / Generate / GREP / Compare  

### Application identity

- `Application.Name = "RegexCraft"`  
- `Window.Title` bound to mode-aware `WindowTitle`  

## Core services

| Type | Role |
|------|------|
| `FlavorDefinition` / `FlavorService` | Flavor registry, engine map, fidelity, options, token matrix, codegen lang |
| `FlavorTokenSets` | Shared unsupported-token sets (RE2, JS, Python, Java, .NET-only) |
| `TestingFidelity` | Full / High / Approximate / CodegenOnly |
| `IRegexCompareService` / `RegexCompareService` | Multi-flavor side-by-side Match + difference analysis |
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

Flavors map onto these engines with **SupportedOptions**, **UnsupportedTokenIds**, **CodegenLanguageId**, and **KnownDifferences**. Approximate flavors show a banner; see `docs/user/flavors.md`.

### Flavor → engine (summary)

| Flavor | Engine | Fidelity | Notes |
|--------|--------|----------|-------|
| .NET | dotnet | Full | Balancing groups, ExplicitCapture |
| PCRE2 | pcre2 | Full | Possessive / atomic; ExplicitCapture ~ |
| JavaScript / TypeScript | javascript | High | No free-spacing / ExplicitCapture |
| PHP | pcre2 | High | Same family as preg |
| Python, Java, Kotlin, Swift | dotnet | Approximate | Token/option matrices differ |
| Go, Rust | dotnet | Approximate | RE2 limits in token matrix |
| Ruby, Perl | pcre2 | Approximate | Onigmo / full Perl differ |

Adding a flavor: define `FlavorDefinition` in `FlavorService.BuildDefaultFlavors()` (options, tokens, codegen, differences) and ensure `EngineId` is registered in `EngineFactory`. Optional: new `IRegexEngine` implementation.

### Engine evaluation (Phase 8)

- **Python.NET**: not integrated (requires CPython embed).  
- **RE2 wrappers** (e.g. RE2.Managed): not integrated (maintenance risk); RE2 constraints modeled on Go/Rust flavors.  
- **Jint**: retained; deep tests cover lookbehind, named groups, Unicode, replace/split.

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
