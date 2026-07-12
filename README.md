# RegexCraft

**Modern, cross-platform regular expression workbench** for exploring, testing, replacing, grepping, and generating code across many regex flavors.

| | |
|---|---|
| **Version** | 0.9.0 |
| **Domain** | [regexcraft.com](https://regexcraft.com) |
| **Stack** | .NET 10 · Avalonia 12 · AvaloniaEdit · Jint · NUnit · Serilog |
| **License** | MIT |

![RegexCraft Match mode (light)](docs/screenshots/main-test-light.png)

<p align="center">
  <img src="docs/screenshots/main-test-dark.png" alt="RegexCraft Match mode (dark)" width="49%" />
  <img src="docs/screenshots/main-generate.png" alt="RegexCraft Generate mode" width="49%" />
</p>

---

## Features

- **Multi-flavor testing** — .NET, PCRE2, **JavaScript** (Jint), TypeScript, Python, Java, PHP, Ruby, Go, Rust, Perl, Kotlin, Swift  
- **Hardened flavor definitions** — supported options, token support matrices, known differences, preferred codegen language  
- **Clear fidelity** — Full / High / Approximate banners; token palette dims unsupported constructs (e.g. RE2 limits for Go/Rust)  
- **Significant automated tests** — deep coverage per real engine; core + mapping + banner + token + codegen tests per selectable flavor  
- **Professional editor** — AvaloniaEdit with high-contrast light/dark regex syntax highlighting, line numbers, and live analysis  
- **Live Match mode** — subject highlighting for matches and capture groups, expandable match list with Copy / Go  
- **Replace & Split** — live preview that fills the panel cleanly, substitution highlighting, backreferences (`$1`, `${name}`, …)  
- **GREP** — search and replace across folders with include/exclude globs, async progress, cancellation, dry-run, and backups  
- **Code generation** — C#, JavaScript, TypeScript, Python, PHP, Java, Go, Rust, Ruby, Perl, Kotlin, Swift  
- **Analysis Tree** — hierarchical breakdown of the pattern; click a node to select it in the editor  
- **Token palette** — searchable text-only tokens, engine-aware support  
- **Library & History** — **20 built-in** common patterns (email, URL, IP, dates, UUID, …) plus user saves with categories/tags; searchable history  
- **Light / Dark / System themes** — preference **persisted and restored** correctly across restarts  
- **Custom About dialog** + **RegexCraft application icon** (no Avalonia defaults)  
- **Resizable panels** — drag splitters between sidebar, editor, and mode panel  
- **Keyboard shortcuts** — Ctrl+Enter (⌘+Enter) to run; Ctrl+1–5 for modes  

## Engines

| Id | Display | Match | Replace | Split | GREP | Notes |
|----|---------|-------|---------|-------|------|-------|
| `dotnet` | .NET | Yes | Yes | Yes | Yes | `System.Text.RegularExpressions` |
| `pcre2` | PCRE2 | Yes | Yes | Yes | Yes | PCRE.NET |
| `javascript` | JavaScript (Jint) | Yes | Yes | Yes | Yes | ECMAScript for JS / TypeScript flavors |

Additional flavors map onto these engines with documented fidelity, option support, and token matrices. See [docs/user/flavors.md](docs/user/flavors.md).

**Engine evaluation (v0.9):** Python.NET and RE2.Managed were evaluated and not integrated (embedding cost / maintenance). Go/Rust RE2 limits are modeled in the flavor layer; JS fidelity is strengthened via Jint tests and documented gaps.

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

## Running the tests

```bash
# Full suite (unit + headless UI + screenshots)
dotnet test

# By category
dotnet test --filter Category=Engines      # deep Match/Replace/Split per real engine
dotnet test --filter Category=Flavors      # every selectable flavor: mapping, fidelity, tokens, codegen
dotnet test --filter Category=Analysis
dotnet test --filter Category=Codegen
dotnet test --filter Category=Library
dotnet test --filter Category=Grep
dotnet test --filter Category=ViewModels
dotnet test --filter Category=UI
dotnet test --filter Category=Headless
dotnet test --filter Category=Screenshots
dotnet test --filter Category=Branding

# Engines + Flavors together
dotnet test --filter "Category=Engines|Category=Flavors"
```

Unit tests are fast. Headless UI tests use **Avalonia.Headless.NUnit** with Skia so they run on CI without a display.

### Regenerating screenshots

Screenshot tests render the main window and About dialog via `CaptureRenderedFrame()` and write PNGs to `docs/screenshots/`:

```bash
dotnet test --filter Category=Screenshots
```

| File | Description |
|------|-------------|
| `main-test-light.png` | Match mode, light theme |
| `main-test-dark.png` | Match mode, dark theme |
| `main-replace.png` | Replace mode |
| `main-generate.png` | Generate mode (C#) |
| `main-grep.png` | GREP mode |
| `main-library.png` | Library sidebar |
| `about-light.png` / `about-dark.png` | About RegexCraft dialog |

Do **not** commit temporary or low-quality captures; only regenerate and keep images that look good in docs.

## Solution layout

```
RegexCraft/
├── src/
│   ├── RegexCraft.Core/       # Flavors, tokens, analysis, GREP, codegen, library, settings
│   ├── RegexCraft.Engines/    # DotNet + PCRE2 + JavaScript (Jint)
│   └── RegexCraft.App/        # Avalonia UI + AvaloniaEdit + icon + About
├── tests/RegexCraft.Tests/    # NUnit unit + Avalonia headless UI + screenshots
├── docs/
│   ├── screenshots/           # Auto-captured PNGs for README/docs
│   ├── user/                  # End-user guides (incl. flavors.md)
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
- [Flavors & testing fidelity](docs/user/flavors.md)
- [Testing regexes](docs/user/testing-regexes.md)
- [Replacing](docs/user/replacing.md)
- [GREP (file search & replace)](docs/user/grepping.md)
- [Generating code](docs/user/generating-code.md)
- [Library and History](docs/user/library-and-history.md)
- [Theme & appearance](docs/user/theme-and-appearance.md)
- [Architecture](docs/development/architecture.md)
