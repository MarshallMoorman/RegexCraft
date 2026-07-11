# Getting Started with RegexCraft

## What is RegexCraft?

RegexCraft is a modern, cross-platform regular expression workbench built for **multiple regex flavors**. Test the same pattern under **.NET** and **PCRE2** with consistent results, professional highlighting, replace/split previews, **GREP across files**, code generation, and a live analysis tree.

**Current version**: 0.4.0

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

1. Launch the app — the window title is **RegexCraft**, with a sample email pattern pre-filled.
2. Watch matches highlight in the **Subject** editor and expand groups on the right.
3. Expand the **Analysis Tree** — named groups and sequence parts should appear nested; click a node to select it in the editor.
4. Switch the flavor from **.NET** to **PCRE2** and confirm results still update.
5. Search **Tokens** for `named` and click to insert a named group.
6. Open **Replace**, set a replacement like `[$1]` or `[${user}]`, and preview highlighted substitutions.
7. Try **Split** with a pattern like `,\s*` on a comma-separated subject.
8. Open **Generate**, pick a language, and **Copy code**.
9. Open **GREP**, pick a project folder, set include globs (e.g. `*.cs`), and **Search**.
10. Save the pattern under **Library** (optionally as a favorite with category/tags), or restore it later from **History**.
11. Cycle **Theme** (System → Light → Dark) to verify the blue theme.

## Keyboard shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+Enter (⌘+Enter on macOS) | Run current mode (Test / Replace / Split / GREP Search) |
| Ctrl+1 … Ctrl+5 | Switch Test / Replace / Split / Generate / GREP |

## Learn more

- [Testing regular expressions](testing-regexes.md)  
- [Replacing](replacing.md)  
- [GREP (file search & replace)](grepping.md)  
- [Generating code](generating-code.md)  
- [Library and History](library-and-history.md)  
- [Architecture](../development/architecture.md)  

## Logs

Rolling logs live under `logs/` (for example `logs/regexcraft-20260711.log`). Retention defaults to **7 days** via `appsettings.json`.
