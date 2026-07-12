# Getting Started with RegexCraft

## What is RegexCraft?

RegexCraft is a modern, cross-platform regular expression workbench built for **multiple regex flavors**. Test patterns under **.NET**, **PCRE2**, and **JavaScript**, with approximate testing for Python, Java, PHP, and more — plus professional highlighting, replace/split previews, **GREP across files**, **multi-flavor Compare**, code generation, and a live analysis tree.

**Current version**: 1.2.0

## Download (pre-built)

Portable self-contained builds for Windows, Linux, and macOS are on the public
**[RegexCraft-Releases](https://github.com/MarshallMoorman/RegexCraft-Releases/releases/latest)** page
(or [regexcraft.com/download](https://regexcraft.com/download.html)). Unzip and run `RegexCraft.App` / `RegexCraft.App.exe`.

**License:** free for personal use; business use requires a paid license (honor system — no keys). See [Pricing](https://regexcraft.com/pricing.html) and the [EULA](https://regexcraft.com/eula.html).

## First five minutes

1. Launch the app — the window title is **RegexCraft**, with a sample email pattern pre-filled.
2. Watch matches highlight in the **Subject** editor and expand groups on the right.
3. Export matches with **CSV** / **JSON** / **Copy JSON** next to **Run** (see [Exporting](exporting.md)).
4. Expand the **Analysis Tree** — named groups and sequence parts should appear nested; click a node to select it in the editor.
5. Open the **Flavor** dropdown — try **JavaScript**, **Python**, or **PHP**. Note the fidelity banner when testing is approximate.
6. Search **Tokens** for `named` and click to insert a named group.
7. Open **Replace**, set a replacement like `[$1]` or `[${user}]`, and preview highlighted substitutions.
8. Try **Split** with a pattern like `,\s*` on a comma-separated subject.
9. Open **Generate** — **C# code appears immediately**; switch language to see other snippets and **Copy code**.
10. Open **GREP**, pick a project folder, set include globs (e.g. `*.cs`), and **Search**.
11. Open **Compare** (Ctrl+6) — select 2–4 flavors and review side-by-side match results and differences.
12. Open **Library** — load a **Built-in** pattern (email, UUID, …), or save your own with category/tags.
13. Cycle **Theme** (System → Light → Dark), quit, and relaunch — your theme should restore.
14. Open **Help → About RegexCraft** for version, EULA/pricing links, and the optional business-license checkbox.

## Keyboard shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+Enter (⌘+Enter on macOS) | Run current mode (Test / Replace / Split / GREP Search / Compare) |
| Ctrl+1 … Ctrl+7 | Switch Test / Replace / Split / Generate / GREP / Compare / Debug |
| F10 / F11 | Debug step (when Debug mode is available) |

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
