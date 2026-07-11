# RegexCraft – AGENTS.md

**Last updated**: 2026-07-11 — Phase 6 complete (v0.7.0)  
**Owner**: Marshall Moorman  

Living guide for AI agents and humans working on RegexCraft.

## Project Conventions

- **Language / Framework**: C# / .NET 10 + Avalonia 12 + AvaloniaEdit + Jint  
- **UI Pattern**: MVVM (CommunityToolkit.Mvvm)  
- **Testing**: NUnit only. All new code must have tests. `dotnet test`  
- **Logging**: Microsoft.Extensions.Logging + Serilog file sink. No `Console.WriteLine` for real logging  
- **Theme**: Named resources only from `Themes/Colors.axaml`. No hard-coded UI colors  
- **Tokens**: Text-only palette — **no icons for individual tokens**  
- **Versioning**: Only in `Directory.Build.props`  
- **Packages**: Central management in `Directory.Packages.props`  
- **Commits**: One clean commit per completed phase on `main`  
- **Planning docs**: Phase requirements live under `docs/development/`; root keeps AGENTS/HANDOFF/README only  
- **Persistence**: Library/History/Settings JSON under OS ApplicationData `RegexCraft/`  
- **Window identity**: `Application.Name` and window title must be **RegexCraft** (never leave Avalonia defaults)

## Architecture Quick Reference

| Project | Role |
|---------|------|
| `RegexCraft.Core` | `IRegexEngine`, models, **flavors + fidelity**, tokens, analysis, highlight builders, token insertion, codegen, library/history/settings, **GREP**, built-in library |
| `RegexCraft.Engines` | `DotNetRegexEngine`, `PcreRegexEngine`, **`JavaScriptRegexEngine` (Jint)**, `EngineFactory` |
| `RegexCraft.App` | Avalonia UI, AvaloniaEdit, theme, Serilog, ViewModels |
| `RegexCraft.Tests` | NUnit (engines, tokens, analysis, highlighting, codegen, library, GREP, flavors, VMs) |

### UI map (Phase 6)

- Left: Tokens / Library / History — Library shows **Built-in** badge; built-ins not deletable  
- Center: Pattern editor (AvaloniaEdit) + Analysis Tree  
- Right: **single mode host** — Test / Replace / Split / Generate / GREP  
- Toolbar: **expanded Flavor list**, modes, Options, Theme (persisted correctly)  
- Fidelity **banner** when testing is High/Approximate  
- Status: flavor (+ fidelity) / engine, counts, timing, shortcuts  
- Generate: auto-runs for default **C#** and on every pattern/options/language change  

### Still relevant from Phase 3–5

- `IGrepService` / GREP models, settings store, library favorites, resizable columns  
- `MainWindowViewModel` live test/replace/split, GREP async, settings  
- `TokenCatalog` / `TokenInsertion` / `RegexToken.SupportedEngines`  
- `RegexAnalysisService`, highlight builders, codegen service  

## Current Engines

| Id | Display | Full Testing | Replace | Split | GREP | Notes |
|----|---------|--------------|---------|-------|------|-------|
| `dotnet` | .NET | Yes | Yes | Yes | Yes | Also backs approximate Python/Java/Go/Rust/Kotlin/Swift |
| `pcre2` | PCRE2 | Yes | Yes | Yes | Yes | Also backs PHP (High) / Ruby / Perl (Approximate) |
| `javascript` | JavaScript (Jint) | Yes | Yes | Yes | Yes | JS + TypeScript flavors |

### Flavors (registry)

Defined in `FlavorService.BuildDefaultFlavors()` with `TestingFidelity` + `FidelityNote`.  
Only flavors whose `EngineId` is registered are shown.

## How to Run

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

## Theme Colors

`src/RegexCraft.App/Themes/Colors.axaml` — Light/Dark dictionaries.

Use brushes: `{DynamicResource PrimaryBlueBrush}`, `EditorForegroundBrush`, `EditorBackgroundBrush`, `SyntaxGroupBrush`, `MatchHighlightBrush`, `GroupHighlight0Brush`–`3`, etc.

**Never hard-code UI colors.**

## Settings / theme persistence

- Theme must be restored from `settings.json` on startup.  
- **Critical**: suppress settings saves while applying loaded settings in the VM constructor (setting `SelectedFlavor` must not overwrite theme with the default).  
- Re-apply theme on window open via `ReapplyThemeFromSettings()`.

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
```

Logs: `logs/` (gitignored).  
Library/History/Settings: `%AppData%/RegexCraft` (Windows) or `~/Library/Application Support/RegexCraft` (macOS) / `~/.config/RegexCraft` (Linux).

## Key Paths

- Requirements: `docs/development/PHASE-*-REQUIREMENTS.md`  
- Shell: `src/RegexCraft.App/Views/MainWindow.axaml`  
- VM: `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs`  
- Flavors: `src/RegexCraft.Core/Flavors/`  
- JS engine: `src/RegexCraft.Engines/JavaScript/JavaScriptRegexEngine.cs`  
- Built-in library: `src/RegexCraft.Core/Library/BuiltInLibrary.cs`  
- Theme: `src/RegexCraft.App/Themes/Colors.axaml`  
- User flavors doc: `docs/user/flavors.md`  
