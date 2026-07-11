# RegexCraft – Phase 0 Requirements

**Project**: RegexCraft  
**Domain**: regexcraft.com  
**Repository**: https://github.com/MarshallMoorman/RegexCraft  
**Local path (user)**: ~/dev/RegexCraft  
**Target Framework**: .NET 10  
**UI Framework**: Avalonia 12  
**Version after this phase**: `0.1.0`  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 0

Establish a solid, professional foundation that proves the multi-flavor architecture works from day one.  

At the end of Phase 0 we must have:

- A clean Avalonia 12 + .NET 10 solution
- Fully variable-driven blue light/dark theme
- Working multi-engine abstraction with **two real engines** (.NET + PCRE2)
- Both engines able to perform **Match** and **Replace** operations
- Serilog file logging (7-day retention, configurable)
- NUnit test project with solid coverage of the core
- Documentation skeleton + first user docs
- Versioning via `Directory.Build.props`
- AGENTS.md and HANDOFF.md ready for future phases
- Everything committed to `main`

This phase deliberately focuses on architecture, engines, theme, logging, and testing. The full UI editor/token palette comes in Phase 1.

---

## 2. Solution Structure (Required)

```
RegexCraft/
├── Directory.Build.props                 # Version, common props
├── Directory.Packages.props               # Central package management (recommended)
├── RegexCraft.sln
├── src/
│   ├── RegexCraft.Core/                  # Domain models, interfaces, Flavor system
│   ├── RegexCraft.Engines/               # Concrete engine implementations
│   └── RegexCraft.App/                   # Avalonia UI application
├── tests/
│   └── RegexCraft.Tests/                 # NUnit tests (unit + headless)
├── docs/
│   ├── user/
│   │   ├── getting-started.md
│   │   └── README.md
│   ├── development/
│   │   └── architecture.md
│   └── CHANGELOG.md
├── logs/                                 # (gitignored) Serilog output
├── appsettings.json                      # Logging config (copied to output)
├── appsettings.Development.json
├── AGENTS.md
├── HANDOFF.md
├── LICENSE                               # MIT
├── README.md
└── .gitignore
```

---

## 3. Core Architecture Requirements

### 3.1 IRegexEngine Abstraction (Critical)

```csharp
public interface IRegexEngine
{
    string Id { get; }                    // "dotnet", "pcre2"
    string DisplayName { get; }
    bool SupportsFullTesting { get; }     // true for Tier 1 engines
    bool SupportsReplace { get; }

    MatchCollectionResult Match(string pattern, string subject, RegexOptionsEx options);
    ReplaceResult Replace(string pattern, string subject, string replacement, RegexOptionsEx options);
    // Future: Split, GetDebugSteps, etc.
}
```

- `MatchCollectionResult` must contain:
  - List of matches
  - Each match has: Index, Length, Value, Groups (including named groups)
  - Success flag, Error message (if any)
- Highlighting data must be easy to consume by the future UI (list of ranges + group info).

### 3.2 Two Engines Must Work in Phase 0

1. **DotNetRegexEngine** – uses `System.Text.RegularExpressions`
2. **PcreRegexEngine** – uses **PCRE.NET** (latest stable)

Both must:
- Support common options (IgnoreCase, Multiline, Singleline, ExplicitCapture, etc.)
- Return consistent `MatchCollectionResult` / `ReplaceResult` objects
- Handle invalid patterns gracefully (return error instead of throwing where possible)

### 3.3 Flavor System (Foundation)

- `FlavorDefinition` class (or record) that can later be loaded from YAML/JSON
- At minimum hard-code two flavors that map to the two engines
- `IFlavorService` or simple registry that returns the correct `IRegexEngine` for a flavor

This must be designed so adding a third flavor later only requires a new definition + optional new engine.

---

## 4. Theme Requirements (Blue, Variable-Driven)

- Use Avalonia’s built-in **Fluent** theme.
- Create `Themes/Colors.axaml` (or similar) with `ResourceDictionary.ThemeDictionaries` for Light and Dark.
- All colors must be defined as named resources, e.g.:

```xml
<Color x:Key="PrimaryBlue">#0078D4</Color>
<Color x:Key="PrimaryBlueHover">#106EBE</Color>
<Color x:Key="AccentBlue">#00A4EF</Color>
<Color x:Key="BackgroundPrimary">...</Color>
<!-- etc. -->
```

- Every brush/color used in the app must reference these via `{DynamicResource ...}`.
- Support system theme switching + forced Light/Dark.
- No purple anywhere. Professional blues only.

---

## 5. Logging Requirements

- Use **Microsoft.Extensions.Logging** abstractions throughout the code.
- Provider: **Serilog** with File sink.
- Configuration via `appsettings.json` (and Development override).
- Defaults:
  - Rolling daily files
  - Retain last **7 days**
  - Path: `logs/regexcraft-.log` (or similar)
  - Minimum level: Information (Debug in Development)
- Logging must be set up in the Avalonia `App` or host builder.
- Log startup, engine selection, match/replace operations (at appropriate levels), and errors.

---

## 6. Testing Requirements (NUnit)

- Project: `RegexCraft.Tests` using NUnit + Avalonia.Headless.NUnit if needed.
- Must cover:
  - Both engines: successful Match, Replace, named groups, options
  - Invalid patterns
  - Edge cases (empty string, large input, Unicode)
  - Flavor registry / engine resolution
- Tests must be fast and deterministic.
- All tests must pass before the phase is considered complete.
- Use meaningful categories if helpful (`[Category("Engines")]`, etc.).

---

## 7. Documentation Requirements (Phase 0)

Create:

- `docs/user/getting-started.md` – short “what is RegexCraft + how to run”
- `docs/user/README.md` – index
- `docs/development/architecture.md` – high-level overview of engines, flavors, theme
- `docs/CHANGELOG.md` – entry for 0.1.0
- Root `README.md` – project overview, how to build/run, status

---

## 8. Versioning & Project Files

- `Directory.Build.props` must contain:

```xml
<Project>
  <PropertyGroup>
    <Version>0.1.0</Version>
    <Authors>Marshall Moorman</Authors>
    <Product>RegexCraft</Product>
    <Description>Modern cross-platform regular expression tool</Description>
    <!-- other common props -->
  </PropertyGroup>
</Project>
```

- Prefer Central Package Management (`Directory.Packages.props`).

---

## 9. AGENTS.md and HANDOFF.md (Required)

### AGENTS.md
Living document that describes:
- Project conventions
- How to run tests
- Theme color variables
- Engine architecture
- Current phase status

### HANDOFF.md
Special file designed for conversation handoff. Must contain:
- Current version
- What was completed in this phase
- Exact next steps for Phase 1
- Known issues / TODOs
- How to continue in a new chat

---

## 10. Application Shell (Minimal but Professional)

Even though full UI comes later, Phase 0 must include a runnable Avalonia window that:

- Shows the blue theme (light/dark toggle)
- Displays current version
- Has a simple dropdown to select flavor/engine (.NET / PCRE2)
- Has a basic “Test Match” and “Test Replace” area (text boxes + buttons) that actually call the engines and show results (including groups)
- Logs activity
- Looks clean and professional

This proves the engines work end-to-end.

---

## 11. Definition of Done (Must All Be True)

- [ ] Solution builds cleanly on .NET 10
- [ ] All NUnit tests pass
- [ ] Both engines perform correct Match + Replace (verified by tests + manual check)
- [ ] Theme is fully variable-driven blue (light + dark)
- [ ] Serilog writes rolling logs (7-day retention configurable)
- [ ] `docs/` folder exists with the required files
- [ ] Version is `0.1.0` in `Directory.Build.props`
- [ ] `AGENTS.md` and `HANDOFF.md` are complete and accurate
- [ ] Root `README.md` is good
- [ ] `.gitignore` includes `logs/`, `bin/`, `obj/`, etc.
- [ ] MIT LICENSE present
- [ ] Everything committed to `main` with a clear commit message
- [ ] Application runs and the basic shell works

---

## 12. Out of Scope for Phase 0 (Do Not Implement Yet)

- AvaloniaEdit / full regex editor
- Token palette
- Analysis tree
- Full Library / History
- GREP
- Code generation UI
- Debug stepping
- More than two engines
- Complex options UI

These belong to later phases.

---

## 13. Recommended Packages (minimum)

- Avalonia 12.x + Avalonia.Desktop + Avalonia.Themes.Fluent
- PCRE.NET (latest)
- Serilog.Extensions.Logging + Serilog.Sinks.File + Serilog.Settings.Configuration
- Microsoft.Extensions.Configuration.Json
- NUnit + NUnit3TestAdapter + Microsoft.NET.Test.Sdk
- Avalonia.Headless.NUnit (if doing UI tests)
- CommunityToolkit.Mvvm (recommended for future ViewModels)

---

**End of Phase 0 Requirements**

This document is the single source of truth for Phase 0.  
Any implementation must satisfy every item above.