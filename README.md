# RegexCraft

**Modern, cross-platform regular expression workbench** for exploring, testing, replacing, generating, and grepping with multiple regex engines.

| | |
|---|---|
| **Version** | 0.5.0 |
| **Domain** | [regexcraft.com](https://regexcraft.com) |
| **Stack** | .NET 10 · Avalonia 12 · AvaloniaEdit · NUnit · Serilog |
| **License** | MIT |

---

## Features

- **Multi-engine testing** — run the same pattern under **.NET** (`System.Text.RegularExpressions`) and **PCRE2** (PCRE.NET)
- **Professional editor** — AvaloniaEdit with high-contrast light/dark regex syntax highlighting, line numbers, and live analysis
- **Live Match mode** — subject highlighting for matches and capture groups, expandable match list with Copy / Go
- **Replace & Split** — live preview, substitution highlighting, backreferences (`$1`, `${name}`, …)
- **GREP** — search and replace across folders with include/exclude globs, async progress, cancellation, dry-run, and backups
- **Code generation** — C#, JavaScript, Python, PHP, Java, Go, Rust for IsMatch / Match / Matches / Replace / Split
- **Analysis Tree** — hierarchical breakdown of the pattern; click a node to select it in the editor
- **Token palette** — searchable text-only tokens (no per-token icons), engine-aware hints
- **Library & History** — save favorites with categories/tags; automatic recent-pattern history
- **Light / Dark / System themes** — consistent blue design tokens, dedicated editor/syntax brushes (no hard-coded UI colors)
- **Keyboard shortcuts** — Ctrl+Enter to run; Ctrl+1–5 for modes

## Screenshots

See `docs/development/` for development baselines. Product screenshots for the website may be added under `docs/user/` later.

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
- [Architecture](docs/development/architecture.md)
- [Changelog](docs/CHANGELOG.md)

## License

MIT — see [LICENSE](LICENSE).
