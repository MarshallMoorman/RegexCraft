# Getting Started with RegexCraft

## What is RegexCraft?

RegexCraft is a modern, cross-platform regular expression tool built for **multiple regex flavors**. Test the same pattern under **.NET** and **PCRE2** with consistent results, professional highlighting, and a live analysis tree.

**Current version**: 0.2.0 (Phase 1)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- macOS, Windows, or Linux (Avalonia desktop)

## Build and run

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

## First five minutes

1. Launch the app — a sample email pattern and subject are pre-filled.
2. Watch matches highlight in the **Subject** editor and expand groups on the right.
3. Switch the flavor from **.NET** to **PCRE2** and confirm results still update.
4. Search the **Tokens** list for `named` and click to insert a named group.
5. Edit the pattern and watch the **Analysis Tree** update live.
6. Open **Replace**, set a replacement like `[$1]`, and preview the output.
7. Cycle **Theme** (System → Light → Dark) to verify the blue theme.

## Learn more

- [Testing regular expressions](testing-regexes.md) — Test panel, highlighting, groups, engines  
- [Architecture](../development/architecture.md) — engines, flavors, UI structure  

## Logs

Rolling logs live under `logs/` (for example `logs/regexcraft-20260711.log`). Retention defaults to **7 days** via `appsettings.json`.
