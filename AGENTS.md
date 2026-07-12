# RegexCraft – AGENTS.md

**Last updated**: 2026-07-11 — Phase 8 complete (v0.9.0)  
**Owner**: Marshall Moorman  

Living guide for AI agents and humans working on RegexCraft.

## Project Conventions

- **Language / Framework**: C# / .NET 10 + Avalonia 12 + AvaloniaEdit + Jint  
- **UI Pattern**: MVVM (CommunityToolkit.Mvvm)  
- **Testing**: NUnit only. All new code must have tests. `dotnet test`  
  - Unit categories: Engines, Analysis, Highlighting, Tokens, Codegen, Library, Grep, ViewModels, Flavors, Branding  
  - UI / headless: `Category=UI`, `Category=Headless` (Avalonia.Headless.NUnit + Skia)  
  - Screenshots: `Category=Screenshots` → `docs/screenshots/` via `CaptureRenderedFrame()`  
  - **Phase 8 quality bar**: significant tests for every real engine (deep) and every selectable flavor (mapping + fidelity + tokens + codegen)  
- **Logging**: Microsoft.Extensions.Logging + Serilog file sink. No `Console.WriteLine` for real logging  
- **Theme**: Named resources only from `Themes/Colors.axaml`. No hard-coded UI colors  
- **Tokens**: Text-only palette — **no icons for individual tokens**; support is **flavor-aware** (not only engine-aware)  
- **Versioning**: Only in `Directory.Build.props`  
- **Packages**: Central management in `Directory.Packages.props`  
- **Commits**: One clean commit per completed phase on `main`  
- **Planning docs**: Phase requirements live under `docs/development/`; root keeps AGENTS/HANDOFF/README only  
- **Persistence**: Library/History/Settings JSON under OS ApplicationData `RegexCraft/`  
- **Window identity**: `Application.Name` and window title must be **RegexCraft** (never leave Avalonia defaults)  
- **Branding**: App icon in `src/RegexCraft.App/Assets/regexcraft-icon.*`; About is custom (`AboutWindow`), menu **About RegexCraft**

## Architecture Quick Reference

| Project | Role |
|---------|------|
| `RegexCraft.Core` | `IRegexEngine`, models, **flavors + fidelity + options/token matrices**, tokens, analysis, highlight builders, token insertion, codegen, library/history/settings, **GREP**, built-in library |
| `RegexCraft.Engines` | `DotNetRegexEngine`, `PcreRegexEngine`, **`JavaScriptRegexEngine` (Jint)**, `EngineFactory` |
| `RegexCraft.App` | Avalonia UI, AvaloniaEdit, theme, Serilog, ViewModels, **About dialog**, **app icon** |
| `RegexCraft.Tests` | NUnit unit + **Avalonia headless UI** + **screenshot capture** |

### UI map (Phase 6–8)

- Left: Tokens / Library / History — Library shows **Built-in** badge; built-ins not deletable  
- Center: Pattern editor (AvaloniaEdit) + Analysis Tree  
- Right: **single mode host** — Test / Replace / Split / Generate / GREP  
- Toolbar: **expanded Flavor list**, modes, Options, Theme (persisted correctly)  
- Fidelity **banner** when testing is High/Approximate  
- Options: flavor-aware enable/disable (e.g. JS has no ExplicitCapture / free-spacing)  
- Tokens: dimmed when unsupported for the selected flavor (engine + flavor matrices)  
- Status: flavor (+ fidelity) / engine, counts, timing, shortcuts  
- Generate: auto-runs; **preferred language follows selected flavor**  
- **Help → About RegexCraft** (native menu) opens custom About dialog  

### Still relevant from Phase 3–7

- `IGrepService` / GREP models, settings store, library favorites, resizable columns  
- `MainWindowViewModel` live test/replace/split, GREP async, settings  
- `TokenCatalog` / `TokenInsertion` / `RegexToken.SupportedEngines` + **`FlavorDefinition.IsTokenSupported`**  
- `RegexAnalysisService`, highlight builders, codegen service  
- Branding + headless UI + screenshots  

## Current Engines

| Id | Display | Full Testing | Replace | Split | GREP | Notes |
|----|---------|--------------|---------|-------|------|-------|
| `dotnet` | .NET | Yes | Yes | Yes | Yes | Also backs approximate Python/Java/Go/Rust/Kotlin/Swift |
| `pcre2` | PCRE2 | Yes | Yes | Yes | Yes | Also backs PHP (High) / Ruby / Perl (Approximate) |
| `javascript` | JavaScript (Jint) | Yes | Yes | Yes | Yes | JS + TypeScript flavors |

**Not integrated (evaluated Phase 8):** Python.NET (CPython embed), RE2.Managed (maintenance). Go/Rust RE2 limits are modeled via `UnsupportedTokenIds` + fidelity notes.

### Flavors (registry)

Defined in `FlavorService.BuildDefaultFlavors()` with:

- `TestingFidelity` + `FidelityNote`  
- `SupportedOptions` / `ApproximateOptions`  
- `UnsupportedTokenIds` (see `FlavorTokenSets`)  
- `CodegenLanguageId`  
- `KnownDifferences`  

Only flavors whose `EngineId` is registered are shown.

## How to Run

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

### Tests & screenshots

```bash
dotnet test --filter Category=Engines
dotnet test --filter Category=Flavors
dotnet test --filter "Category=Engines|Category=Flavors"
dotnet test --filter Category=UI
dotnet test --filter Category=Screenshots   # writes docs/screenshots/*.png
```

Do not commit temporary or bad screenshots; only keep final good captures under `docs/screenshots/`.

## Theme Colors

`src/RegexCraft.App/Themes/Colors.axaml` — Light/Dark dictionaries.

Use brushes: `{DynamicResource PrimaryBlueBrush}`, `EditorForegroundBrush`, `EditorBackgroundBrush`, `SyntaxGroupBrush`, `MatchHighlightBrush`, `GroupHighlight0Brush`–`3`, etc.

**Never hard-code UI colors.**

## Settings / theme persistence

- Theme must be restored from `settings.json` on startup.  
- **Critical**: suppress settings saves while applying loaded settings in the VM constructor (setting `SelectedFlavor` must not overwrite theme with the default).  
- Re-apply theme on window open via `ReapplyThemeFromSettings()` (uses in-memory `ThemeLabel`, not a disk re-read that would clobber cycles).

## After Completing a Milestone

1. All tests green  
2. Update this AGENTS.md if conventions changed  
3. Rewrite HANDOFF.md with exact next steps  
4. Bump version in `Directory.Build.props`  
5. Update `docs/CHANGELOG.md` and user/dev docs  
6. Commit on `main` with a clear message  

## Useful Commands

```bash
dotnet test --filter Category=Engines
dotnet test --filter Category=Analysis
dotnet test --filter Category=Highlighting
dotnet test --filter Category=Tokens
dotnet test --filter Category=Codegen
dotnet test --filter Category=Library
dotnet test --filter Category=Grep
dotnet test --filter Category=ViewModels
dotnet test --filter Category=Flavors
dotnet test --filter Category=UI
dotnet test --filter Category=Headless
dotnet test --filter Category=Screenshots
dotnet test --filter Category=Branding
```

Logs: `logs/` (gitignored).  
Library/History/Settings: `%AppData%/RegexCraft` (Windows) or `~/Library/Application Support/RegexCraft` (macOS) / `~/.config/RegexCraft` (Linux).

## Key Paths

- Requirements: `docs/development/PHASE-*-REQUIREMENTS.md`  
- Shell: `src/RegexCraft.App/Views/MainWindow.axaml`  
- About: `src/RegexCraft.App/Views/AboutWindow.axaml`  
- Icon: `src/RegexCraft.App/Assets/regexcraft-icon.ico` (+ `.png`, `.icns`)  
- VM: `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs`  
- Flavors: `src/RegexCraft.Core/Flavors/` (`FlavorDefinition`, `FlavorService`, `FlavorTokenSets`)  
- JS engine: `src/RegexCraft.Engines/JavaScript/JavaScriptRegexEngine.cs`  
- Built-in library: `src/RegexCraft.Core/Library/BuiltInLibrary.cs`  
- Theme: `src/RegexCraft.App/Themes/Colors.axaml`  
- Headless tests: `tests/RegexCraft.Tests/Headless/`  
- Flavor tests: `tests/RegexCraft.Tests/Flavors/`  
- Engine tests: `tests/RegexCraft.Tests/Engines/`  
- Screenshots: `docs/screenshots/`  
- User flavors doc: `docs/user/flavors.md`  
