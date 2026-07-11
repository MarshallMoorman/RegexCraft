# Getting Started with RegexCraft

## What is RegexCraft?

RegexCraft is a modern, cross-platform regular expression tool. It is designed from day one for **multiple regex flavors** so you can test the same pattern under .NET, PCRE2, and (later) other engines with consistent results.

Phase 0 (v0.1.0) delivers the foundation: two working engines, a blue light/dark theme, logging, tests, and a small shell that proves Match and Replace end-to-end.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- macOS, Windows, or Linux (Avalonia desktop)

## Build and run

From the repository root:

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

## Using the Phase 0 shell

1. **Flavor / Engine** — choose **.NET** or **PCRE2** from the dropdown.
2. **Options** — toggle Ignore case, Multiline, and/or Singleline.
3. **Pattern** — enter a regular expression.
4. **Subject** — enter the text to test.
5. **Replacement** — used only for Replace (supports `$1`-style references).
6. Click **Test Match** or **Test Replace** and inspect the Results panel (including groups).
7. Use the **Theme** button in the header to cycle System → Light → Dark.

Sample pattern and subject are pre-filled so you can click Match immediately.

## Logs

Activity is written to rolling log files under `logs/` (for example `logs/regexcraft-20260711.log`). Retention defaults to **7 days** and is configurable in `appsettings.json`.

## Next

Phase 1 will replace this shell with the full multi-panel editor, token palette, analysis tree, and rich match highlighting.
