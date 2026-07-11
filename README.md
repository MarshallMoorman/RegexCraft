# RegexCraft

**Modern cross-platform regular expression tool** for exploring, testing, and comparing regex flavors.

| | |
|---|---|
| **Version** | 0.1.0 (Phase 0) |
| **Domain** | [regexcraft.com](https://regexcraft.com) |
| **Stack** | .NET 10 · Avalonia 12 · NUnit · Serilog |
| **License** | MIT |

## Status

Phase 0 establishes the multi-engine foundation:

- `IRegexEngine` abstraction with consistent Match / Replace result models (groups + named groups)
- Two working engines: **.NET** (`System.Text.RegularExpressions`) and **PCRE2** (via PCRE.NET)
- Flavor registry so new engines plug in with a definition + optional engine class
- Variable-driven professional **blue** light/dark theme
- Serilog file logging (7-day rolling, configurable)
- Minimal Avalonia shell to select an engine and run Match / Replace
- NUnit suite covering engines and core services

Full editor, token palette, analysis tree, and richer testing UI arrive in **Phase 1**.

## How to Build & Run

```bash
# Prerequisites: .NET 10 SDK

dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

Logs are written to `logs/regexcraft-*.log` (gitignored).

## Solution Structure

```
RegexCraft/
├── src/
│   ├── RegexCraft.Core/       # Interfaces, models, flavor system
│   ├── RegexCraft.Engines/    # DotNet + PCRE2 engines
│   └── RegexCraft.App/        # Avalonia UI shell
├── tests/
│   └── RegexCraft.Tests/      # NUnit tests
├── docs/
│   ├── user/                  # Getting started
│   ├── development/           # Architecture
│   └── CHANGELOG.md
├── Directory.Build.props      # Version 0.1.0
└── Directory.Packages.props   # Central package management
```

## Documentation

- [Getting started](docs/user/getting-started.md)
- [User docs index](docs/user/README.md)
- [Architecture](docs/development/architecture.md)
- [Changelog](docs/CHANGELOG.md)
- [AGENTS.md](AGENTS.md) — conventions for contributors and AI agents
- [HANDOFF.md](HANDOFF.md) — phase handoff notes

## Engines (Phase 0)

| Id | Display | Full testing | Implementation |
|----|---------|--------------|----------------|
| `dotnet` | .NET | Yes | `System.Text.RegularExpressions` |
| `pcre2` | PCRE2 | Yes | PCRE.NET |

## License

MIT — see [LICENSE](LICENSE).
