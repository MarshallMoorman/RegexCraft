# RegexCraft

**Modern, cross-platform regular expression workbench** for exploring, testing, replacing, grepping, and generating code with multiple regex engines.

| | |
|---|---|
| **Version** | 0.6.0 |
| **Domain** | [regexcraft.com](https://regexcraft.com) |
| **Stack** | .NET 10 · Avalonia 12 · AvaloniaEdit · NUnit · Serilog |
| **License** | MIT |

---

## Features

- **Multi-engine testing** — run the same pattern under **.NET** (`System.Text.RegularExpressions`) and **PCRE2** (PCRE.NET)
- **Professional editor** — AvaloniaEdit with high-contrast light/dark regex syntax highlighting, line numbers, and live analysis
- **Live Match mode** — subject highlighting for matches and capture groups, expandable match list with Copy / Go
- **Replace & Split** — live preview that fills the panel cleanly, substitution highlighting, backreferences (`$1`, `${name}`, …)
- **GREP** — search and replace across folders with include/exclude globs, async progress, cancellation, dry-run, and backups
- **Code generation** — C#, JavaScript, Python, PHP, Java, Go, Rust for IsMatch / Match / Matches / Replace / Split
- **Analysis Tree** — hierarchical breakdown of the pattern; click a node to select it in the editor
- **Token palette** — searchable text-only tokens (no per-token icons), engine-aware hints
- **Library & History** — save favorites with categories/tags; searchable recent-pattern history
- **Light / Dark / System themes** — persisted preference, consistent blue design tokens, dedicated editor/syntax brushes
- **Resizable panels** — drag splitters between sidebar, editor, and mode panel
- **Keyboard shortcuts** — Ctrl+Enter (⌘+Enter) to run; Ctrl+1–5 for modes

## Screenshots

Product screenshots for the website may be added under `docs/user/` later. Development baselines live under `docs/development/` (not required for end users).

## Engines

| Id | Display | Match | Replace | Split | GREP |
|----|---------|-------|---------|-------|------|
| `dotnet` | .NET | Yes | Yes | Yes | Yes |
| `pcre2` | PCRE2 | Yes | Yes | Yes | Yes |

Both engines share the same result models so highlighting, groups, GREP, and codegen stay engine-agnostic.

## Build & run

```bash
# Prerequisites: .NET 10 SDK
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

Publish a self-contained binary (example):

```bash
dotnet publish src/RegexCraft.App -c Release -r osx-arm64 --self-contained
```

Logs are written under `logs/` (gitignored).  
Library, History, and Settings live in the OS application-data folder:

- **macOS**: `~/Library/Application Support/RegexCraft`
- **Windows**: `%AppData%/RegexCraft`
- **Linux**: `~/.config/RegexCraft` (or equivalent ApplicationData)

## Solution layout

```
RegexCraft/
├── src/
│   ├── RegexCraft.Core/       # Engines API, tokens, analysis, GREP, codegen, library, settings
│   ├── RegexCraft.Engines/    # DotNet + PCRE2 (Match / Replace / Split)
│   └── RegexCraft.App/        # Avalonia UI + AvaloniaEdit
├── tests/RegexCraft.Tests/
├── docs/
│   ├── user/                  # End-user guides
│   └── development/           # Architecture & phase requirements
├── Directory.Build.props      # Version
└── AGENTS.md / HANDOFF.md
```

## Keyboard shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+Enter (⌘+Enter on macOS) | Run current mode |
| Ctrl+1 | Match / Test |
| Ctrl+2 | Replace |
| Ctrl+3 | Split |
| Ctrl+4 | Generate |
| Ctrl+5 | GREP |

## Documentation

- [Getting started](docs/user/getting-started.md)
- [Testing regexes](docs/user/testing-regexes.md)
- [Replacing](docs/user/replacing.md)
- [GREP (file search & replace)](docs/user/grepping.md)
- [Generating code](docs/user/generating-code.md)
- [Library and History](docs/user/library-and-history.md)
- [Theme & appearance](docs/user/theme-and-appearance.md)
- [Architecture](docs/development/architecture.md)
- [Changelog](docs/CHANGELOG.md)

## Roadmap (post-0.6)

Planned after this polish release (see `HANDOFF.md`):

- Debug / step-through matching
- Additional engines (Oniguruma, RE2, …)
- Compare mode (.NET vs PCRE2 side-by-side)
- Export matches / GREP results
- Official 1.0 and regexcraft.com site content

## License

MIT — see [LICENSE](LICENSE).
