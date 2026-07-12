# Getting Started with RegexCraft

## What is RegexCraft?

RegexCraft is a modern, cross-platform regular expression workbench built for **multiple regex flavors**. Test patterns under **.NET**, **PCRE2**, and **JavaScript**, with approximate testing for Python, Java, PHP, and more — plus professional highlighting, replace/split previews, **GREP across files**, **multi-flavor Compare**, code generation, and a live analysis tree.

**Current version**: 1.0.0-rc1

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
4. Open the **Flavor** dropdown — try **JavaScript**, **Python**, or **PHP**. Note the fidelity banner when testing is approximate.
5. Search **Tokens** for `named` and click to insert a named group.
6. Open **Replace**, set a replacement like `[$1]` or `[${user}]`, and preview highlighted substitutions.
7. Try **Split** with a pattern like `,\s*` on a comma-separated subject.
8. Open **Generate** — **C# code appears immediately**; switch language to see other snippets and **Copy code**.
9. Open **GREP**, pick a project folder, set include globs (e.g. `*.cs`), and **Search**.
10. Open **Compare** (Ctrl+6) — select 2–4 flavors and review side-by-side match results and differences.
11. Open **Library** — load a **Built-in** pattern (email, UUID, …), or save your own with category/tags.
12. Cycle **Theme** (System → Light → Dark), quit, and relaunch — your theme should restore.
13. Open **Help → About RegexCraft** (macOS application menu, or the Help native menu) for version, copyright, and links.

## Keyboard shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+Enter (⌘+Enter on macOS) | Run current mode (Test / Replace / Split / GREP Search / Compare) |
| Ctrl+1 … Ctrl+6 | Switch Test / Replace / Split / Generate / GREP / Compare |

## Learn more

- [Flavors & testing fidelity](flavors.md)  
- [Testing regular expressions](testing-regexes.md)  
- [Comparing flavors](comparing.md)  
- [Replacing](replacing.md)  
- [GREP (file search & replace)](grepping.md)  
- [Generating code](generating-code.md)  
- [Library and History](library-and-history.md)  
- [Theme & appearance](theme-and-appearance.md)  
- [Architecture](../development/architecture.md)  


## Logs

Rolling logs live under `logs/` (for example `logs/regexcraft-20260711.log`). Retention defaults to **7 days** via `appsettings.json`.
