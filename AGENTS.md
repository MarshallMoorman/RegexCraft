# RegexCraft – AGENTS.md

**Last updated**: 2026-07-11 — Phase 1 complete (v0.2.0)  
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

## Architecture Quick Reference

| Project | Role |
|---------|------|
| `RegexCraft.Core` | `IRegexEngine`, models, flavors, tokens, analysis, highlight builder, token insertion |
| `RegexCraft.Engines` | `DotNetRegexEngine`, `PcreRegexEngine`, `EngineFactory` |
| `RegexCraft.App` | Avalonia UI, AvaloniaEdit, theme, Serilog, ViewModels |
| `RegexCraft.Tests` | NUnit (engines, tokens, analysis, highlighting, VMs) |

### UI map (Phase 1)

- Left: Token palette (search + categories) + Library/History placeholders  
- Center: Pattern editor (AvaloniaEdit) + Analysis Tree  
- Right: Test (subject + highlights + groups) / Replace preview  
- Toolbar: Flavor, Match, Replace, Options, Theme  
- Status: flavor/engine, match count, timing  

### Key Phase 1 types

- `TokenCatalog` / `TokenInsertion`  
- `RegexAnalysisService` → `AnalysisNode` tree  
- `MatchHighlightBuilder` → `HighlightSpan`  
- `RegexHighlightingDefinition` + `MatchHighlightTransformer`  
- `MainWindowViewModel` (debounce live test)  

## Current Engines

| Id | Display | Full Testing | Notes |
|----|---------|--------------|-------|
| `dotnet` | .NET | Yes | `System.Text.RegularExpressions` |
| `pcre2` | PCRE2 | Yes | PCRE.NET |

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
```

Logs: `logs/` (gitignored).

## Key Paths

- Requirements: `docs/development/PHASE-*-REQUIREMENTS.md`  
- Shell: `src/RegexCraft.App/Views/MainWindow.axaml`  
- VM: `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs`  
- Tokens/Analysis/Highlight: `src/RegexCraft.Core/`  
