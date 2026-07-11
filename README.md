# RegexCraft

**Modern cross-platform regular expression tool** for exploring, testing, and comparing regex flavors.

| | |
|---|---|
| **Version** | 0.3.0 (Phase 2) |
| **Domain** | [regexcraft.com](https://regexcraft.com) |
| **Stack** | .NET 10 · Avalonia 12 · AvaloniaEdit · NUnit · Serilog |
| **License** | MIT |

## Features (Phase 2)

- Multi-panel professional UI with light/dark blue theme  
- AvaloniaEdit regex editor with **professional syntax highlighting**  
- Searchable **text-only token palette** (no token icons)  
- Rich live **analysis tree** (click node → select in editor)  
- **Test** with match + group highlighting for **.NET** and **PCRE2**  
- Full **Replace** preview with substitution highlighting and backreferences  
- Full **Split** with parts list and delimiter highlighting  
- **Code generation** for C#, JS, Python, PHP, Java, Go, Rust  
- Persistent **Library** and **History**  
- Keyboard shortcuts (Ctrl+Enter, Ctrl+1–4)  

## How to Build & Run

```bash
# Prerequisites: .NET 10 SDK
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

Logs: `logs/regexcraft-*.log` (gitignored).

## Solution Structure

```
RegexCraft/
├── src/
│   ├── RegexCraft.Core/       # Engines API, tokens, analysis, codegen, library
│   ├── RegexCraft.Engines/    # DotNet + PCRE2 (Match / Replace / Split)
│   └── RegexCraft.App/        # Avalonia UI + AvaloniaEdit
├── tests/RegexCraft.Tests/
├── docs/
│   ├── user/                  # Getting started, testing, replace, codegen, library
│   └── development/           # Architecture, phase requirements
├── Directory.Build.props      # Version 0.3.0
└── AGENTS.md / HANDOFF.md
```

## Documentation

- [Getting started](docs/user/getting-started.md)  
- [Testing regexes](docs/user/testing-regexes.md)  
- [Replacing](docs/user/replacing.md)  
- [Generating code](docs/user/generating-code.md)  
- [Library and History](docs/user/library-and-history.md)  
- [Architecture](docs/development/architecture.md)  
- [Changelog](docs/CHANGELOG.md)  
- [Phase 2 requirements](docs/development/PHASE-2-REQUIREMENTS.md)  

## Engines

| Id | Display | Match | Replace | Split |
|----|---------|-------|---------|-------|
| `dotnet` | .NET | Yes | Yes | Yes |
| `pcre2` | PCRE2 | Yes | Yes | Yes |

## License

MIT — see [LICENSE](LICENSE).
