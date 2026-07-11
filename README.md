# RegexCraft

**Modern cross-platform regular expression tool** for exploring, testing, and comparing regex flavors.

| | |
|---|---|
| **Version** | 0.2.0 (Phase 1) |
| **Domain** | [regexcraft.com](https://regexcraft.com) |
| **Stack** | .NET 10 · Avalonia 12 · AvaloniaEdit · NUnit · Serilog |
| **License** | MIT |

## Features (Phase 1)

- Multi-panel professional UI with light/dark blue theme  
- AvaloniaEdit regex editor with **blue syntax highlighting**  
- Searchable **text-only token palette** (no token icons)  
- Live **analysis tree** of the pattern structure  
- **Test** panel with excellent match + group highlighting for **.NET** and **PCRE2**  
- Basic **Replace** preview  
- Live debounced testing + explicit Run  

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
│   ├── RegexCraft.Core/       # Engines API, tokens, analysis, highlighting helpers
│   ├── RegexCraft.Engines/    # DotNet + PCRE2
│   └── RegexCraft.App/        # Avalonia UI + AvaloniaEdit
├── tests/RegexCraft.Tests/
├── docs/
│   ├── user/                  # Getting started, testing guide
│   └── development/           # Architecture, phase requirements
├── Directory.Build.props      # Version 0.2.0
└── AGENTS.md / HANDOFF.md
```

## Documentation

- [Getting started](docs/user/getting-started.md)  
- [Testing regexes](docs/user/testing-regexes.md)  
- [Architecture](docs/development/architecture.md)  
- [Changelog](docs/CHANGELOG.md)  
- [Phase 1 requirements](docs/development/PHASE-1-REQUIREMENTS.md)  

## Engines

| Id | Display | Notes |
|----|---------|-------|
| `dotnet` | .NET | `System.Text.RegularExpressions` |
| `pcre2` | PCRE2 | PCRE.NET |

## License

MIT — see [LICENSE](LICENSE).
