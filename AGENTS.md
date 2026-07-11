# RegexCraft – AGENTS.md

**Last updated**: 2026-07-11 — Phase 2 complete (v0.3.0)  
**Owner**: Marshall Moorman  

Living guide for AI agents and humans working on RegexCraft.

## Project Conventions

- **Language / Framework**: C# / .NET 10 + Avalonia 12 + AvaloniaEdit  
- **UI Pattern**: MVVM (CommunityToolkit.Mvvm)  
- **Testing**: NUnit only. All new code must have tests. `dotnet test`  
- **Logging**: Microsoft.Extensions.Logging + Serilog file sink. No `Console.WriteLine` for real logging  
- **Theme**: Named resources only from `Themes/Colors.axaml`. No hard-coded UI colors  
- **Tokens**: Text-only palette — **no icons for individual tokens**  
- **Versioning**: Only in `Directory.Build.props`  
- **Packages**: Central management in `Directory.Packages.props`  
- **Commits**: One clean commit per completed phase on `main`  
- **Planning docs**: Phase requirements live under `docs/development/`; root keeps AGENTS/HANDOFF/README only  
- **Persistence**: Library/History JSON under OS ApplicationData `RegexCraft/`  

## Architecture Quick Reference

| Project | Role |
|---------|------|
| `RegexCraft.Core` | `IRegexEngine`, models, flavors, tokens, analysis, highlight builders, token insertion, codegen, library/history |
| `RegexCraft.Engines` | `DotNetRegexEngine`, `PcreRegexEngine`, `EngineFactory` |
| `RegexCraft.App` | Avalonia UI, AvaloniaEdit, theme, Serilog, ViewModels |
| `RegexCraft.Tests` | NUnit (engines, tokens, analysis, highlighting, codegen, library, VMs) |

### UI map (Phase 2)

- Left: Tokens / Library / History tabs  
- Center: Pattern editor (AvaloniaEdit) + rich Analysis Tree (click → select range)  
- Right: Test / Replace / Split / Generate  
- Toolbar: Flavor, Match/Replace/Split/Generate modes, Options, Theme  
- Status: flavor/engine, counts, timing, shortcut hints  
- Shortcuts: Ctrl+Enter run; Ctrl+1–4 modes  

### Key Phase 2 types

- `TokenCatalog` / `TokenInsertion` / `RegexToken.SupportedEngines`  
- `RegexAnalysisService` → rich `AnalysisNode` tree (offsets, descriptions)  
- `MatchHighlightBuilder` / `ReplaceHighlightBuilder`  
- `ICodeGenerationService` / `CodeGenerationService`  
- `ILibraryStore` / `JsonLibraryStore`, `IHistoryStore` / `JsonHistoryStore`  
- `IRegexEngine.Split` + `SplitResult`  
- `MainWindowViewModel` (live Test/Replace/Split, library, history, codegen)  

## Current Engines

| Id | Display | Full Testing | Replace | Split | Notes |
|----|---------|--------------|---------|-------|-------|
| `dotnet` | .NET | Yes | Yes | Yes | `System.Text.RegularExpressions` |
| `pcre2` | PCRE2 | Yes | Yes | Yes | PCRE.NET; manual backref expansion for `$1` / `${name}` |

## How to Run

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

## Theme Colors

`src/RegexCraft.App/Themes/Colors.axaml` — Light/Dark dictionaries.

Use brushes: `{DynamicResource PrimaryBlueBrush}`, `MatchHighlightBrush`, `GroupHighlight0Brush`–`3`, etc.

## After Completing a Phase

1. All tests green  
2. Update this AGENTS.md if conventions changed  
3. Rewrite HANDOFF.md with exact next steps  
4. Bump version in `Directory.Build.props`  
5. Update `docs/CHANGELOG.md` and user/dev docs  
6. Commit on `main` with a clear phase message  

## Useful Commands

```bash
dotnet test --filter Category=Engines
dotnet test --filter Category=Analysis
dotnet test --filter Category=Highlighting
dotnet test --filter Category=Tokens
dotnet test --filter Category=Codegen
dotnet test --filter Category=Library
dotnet test --filter Category=ViewModels
```

Logs: `logs/` (gitignored).  
Library/History: `%AppData%/RegexCraft` (Windows) or `~/Library/Application Support/RegexCraft` (macOS) / `~/.config/RegexCraft` (Linux).

## Key Paths

- Requirements: `docs/development/PHASE-*-REQUIREMENTS.md`  
- Shell: `src/RegexCraft.App/Views/MainWindow.axaml`  
- VM: `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs`  
- Tokens/Analysis/Highlight/Codegen/Library: `src/RegexCraft.Core/`  
