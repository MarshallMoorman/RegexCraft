# RegexCraft – AGENTS.md

**Last updated**: 2026-07-11 — **v1.1.0** + Phase 12 website (`website/` → regexcraft.com)  
**Owner**: Marshall Moorman  

Living guide for AI agents and humans working on RegexCraft.

## Project Conventions

- **Language / Framework**: C# / .NET 10 + Avalonia 12 + AvaloniaEdit + Jint  
- **UI Pattern**: MVVM (CommunityToolkit.Mvvm)  
- **Testing**: NUnit only. All new code must have tests. `dotnet test`  
  - Unit categories: Engines, Analysis, Highlighting, Tokens, Codegen, Library, Grep, Compare, **Debug**, ViewModels, Flavors, Branding  
  - UI / headless: `Category=UI`, `Category=Headless` (Avalonia.Headless.NUnit + Skia)  
  - Screenshots: `Category=Screenshots` → `docs/screenshots/` via `CaptureRenderedFrame()`  
  - **Quality bar**: significant tests for every real engine (deep) and every selectable flavor (mapping + fidelity + tokens + codegen); Compare + Debug have dedicated service + VM + headless tests  
- **Logging**: Microsoft.Extensions.Logging + Serilog file sink. No `Console.WriteLine` for real logging  
- **Theme**: Named resources only from `Themes/Colors.axaml`. No hard-coded UI colors  
- **Tokens**: Text-only palette — **no icons for individual tokens**; support is **flavor-aware** (not only engine-aware)  
- **Versioning**: Only in `Directory.Build.props`  
- **Packages**: Central management in `Directory.Packages.props`  
- **Commits**: One clean commit per completed phase on `main`  
- **Planning docs**: Phase requirements live under `docs/development/`; root keeps AGENTS/HANDOFF/README only  
- **Persistence**: Library/History/Settings JSON under OS ApplicationData `RegexCraft/`  
- **Window identity**: `Application.Name` and window title must be **RegexCraft** (never leave Avalonia defaults)  
- **Branding**: App icon in `src/RegexCraft.App/Assets/regexcraft-icon.*`; About is custom (`AboutWindow`), menu **About RegexCraft**  
- **Website**: Static site source in-repo under **`website/`** (plain HTML/CSS); public URL **https://regexcraft.com**; deploy via `.github/workflows/pages.yml`; human DNS/Pages steps in `docs/development/website.md`; keep blue theme (no purple)  
- **CI**: GitHub Actions under `.github/workflows/` (ci.yml, publish.yml, **pages.yml**) — must stay green without interactive secrets for basic CI  
- **Releases**: Tag `v*` → Publish workflow tests, multi-RID publish, GitHub Release with zip artifacts (see packaging.md)  
- **Layout**: Right-panel Normal vs Compare widths live in `AppSettings` + `LayoutDefaults` — no magic pixels in views  
- **NuGet**: Solution `NuGet.config` uses nuget.org (public packages)  

## Architecture Quick Reference

| Project | Role |
|---------|------|
| `RegexCraft.Core` | `IRegexEngine`, models, **flavors + fidelity + options/token matrices**, **Compare**, **Debug**, tokens, analysis, highlight builders, token insertion, codegen, library/history/**settings + layout defaults**, **GREP**, built-in library |
| `RegexCraft.Engines` | `DotNetRegexEngine`, `PcreRegexEngine`, **`JavaScriptRegexEngine` (Jint)**, `EngineFactory` |
| `RegexCraft.App` | Avalonia UI, AvaloniaEdit, theme, Serilog, ViewModels, **About dialog**, **app icon**, **Compare panel**, **Debug panel**, **smart right-panel sizing** |
| `RegexCraft.Tests` | NUnit unit + **Avalonia headless UI** + **screenshot capture** |
| `website/` (not a .NET project) | Public marketing site for **regexcraft.com** (GitHub Pages) |

### Website (Phase 12)

- Source: `website/` — `index.html`, `download.html`, `docs.html`, `about.html`, `styles.css`, `CNAME`  
- Deploy: GitHub Actions **Deploy website** uploads `website/` to Pages (Source must be **GitHub Actions** in repo settings)  
- Custom domain: `website/CNAME` = `regexcraft.com`; DNS A/CNAME at registrar — see `docs/development/website.md`  
- Screenshots for the site are copies under `website/assets/screenshots/` (refresh from `docs/screenshots/` after UI changes)  
- Do not break app CI; website is independent of `dotnet build` / `dotnet test`  

### UI map (Phase 6–11)

- Left: Tokens / Library / History — Library shows **Built-in** badge; built-ins not deletable  
- Center: Pattern editor (AvaloniaEdit) + Analysis Tree  
- Right: **single mode host** — Test / Replace / Split / Generate / GREP / Compare / **Debug**  
- **Right-panel widths**: Normal absolute width for non-Compare modes (including Debug); **Compare collapses the center editor to ~280px and gives the right panel star/majority space**; leave restores Normal; splitter drags update memory; stale narrow Compare widths ignored  
- Toolbar: **expanded Flavor list**, modes (Ctrl+1–7), Options, Theme (persisted correctly)  
- Fidelity **banner** when testing is High/Approximate  
- Options: flavor-aware enable/disable (e.g. JS has no ExplicitCapture / free-spacing)  
- Tokens: dimmed when unsupported for the selected flavor (engine + flavor matrices)  
- Status: flavor (+ fidelity) / engine, counts, timing, shortcuts  
- Generate: auto-runs; **preferred language follows selected flavor**  
- **Compare**: 2–4 flavors, live re-run, cards + cross-flavor notes + copy summary  
- **Debug**: educational step-through for **.NET** engine; F10/F11; unavailable message for other engines  
- **Matches & Groups**: equal-width stretched cards (`ListBox.matchList`)  
- **Help → About RegexCraft** (native menu) opens custom About dialog  

### Still relevant from Phase 3–10

- `IGrepService` / GREP models, settings store, library favorites, resizable columns  
- `MainWindowViewModel` live test/replace/split, GREP async, settings, **Compare**, **Debug**, **panel width memory**  
- `TokenCatalog` / `TokenInsertion` / `RegexToken.SupportedEngines` + **`FlavorDefinition.IsTokenSupported`**  
- `RegexAnalysisService`, highlight builders, codegen service  
- `IRegexCompareService` / `RegexCompareService`  
- `IRegexDebugService` / `RegexDebugService`  
- Branding + headless UI + screenshots  
- `LayoutDefaults` + `AppSettings.RightPanelNormalWidth` / `RightPanelCompareWidth`  

## Current Engines

| Id | Display | Full Testing | Replace | Split | GREP | Debug | Notes |
|----|---------|--------------|---------|-------|------|-------|-------|
| `dotnet` | .NET | Yes | Yes | Yes | Yes | **Yes** | Also backs approximate Python/Java/Go/Rust/Kotlin/Swift |
| `pcre2` | PCRE2 | Yes | Yes | Yes | Yes | No | Also backs PHP (High) / Ruby / Perl (Approximate) |
| `javascript` | JavaScript (Jint) | Yes | Yes | Yes | Yes | No | JS + TypeScript flavors |

**Not integrated (evaluated Phase 8):** Python.NET (CPython embed), RE2.Managed (maintenance). Go/Rust RE2 limits are modeled via `UnsupportedTokenIds` + fidelity notes.

### Flavors (registry)

Defined in `FlavorService.BuildDefaultFlavors()` with:

- `TestingFidelity` + `FidelityNote`  
- `SupportedOptions` / `ApproximateOptions`  
- `UnsupportedTokenIds` (see `FlavorTokenSets`)  
- `CodegenLanguageId`  
- `KnownDifferences`  

Only flavors whose `EngineId` is registered are shown.

### Compare

- Core: `src/RegexCraft.Core/Compare/`  
- UI: Compare tab in `MainWindow.axaml` + VM properties/commands  
- Constraints: 2–4 flavors; parallel Match; no new engines  
- Layout: Compare uses wider right panel (see `LayoutDefaults`)  

### Debug

- Core: `src/RegexCraft.Core/Debug/` (`IRegexDebugService`, `RegexDebugService`, models)  
- UI: Debug tab + subject editor + step list; pattern range selection via existing events  
- Approach: **hybrid educational** — real Match results + Analysis Tree walk (not full NFA re-implementation)  
- Primary engine: `dotnet`; others show unavailable reason  
- Shortcuts: Ctrl+7, F10 / F11, Ctrl+← / Ctrl+→  

## How to Run

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

### Tests & screenshots

```bash
dotnet test --filter Category=Engines
dotnet test --filter Category=Flavors
dotnet test --filter Category=Compare
dotnet test --filter Category=Debug
dotnet test --filter "Category=Engines|Category=Flavors|Category=Compare|Category=Debug"
dotnet test --filter Category=UI
dotnet test --filter Category=Screenshots   # writes docs/screenshots/*.png
```

Do not commit temporary or bad screenshots; only keep final good captures under `docs/screenshots/`.

### CI locally (mirrors GitHub Actions)

```bash
dotnet restore
dotnet build -c Debug
dotnet build -c Release
dotnet test -c Release
```

### Cut a release

```bash
# After version + CHANGELOG are on main:
git tag -a v1.1.0 -m "RegexCraft 1.1.0"
git push origin v1.1.0
```

See `docs/development/packaging.md`.

## Theme Colors

`src/RegexCraft.App/Themes/Colors.axaml` — Light/Dark dictionaries.

Use brushes: `{DynamicResource PrimaryBlueBrush}`, `EditorForegroundBrush`, `EditorBackgroundBrush`, `SyntaxGroupBrush`, `MatchHighlightBrush`, `GroupHighlight0Brush`–`3`, etc.

**Never hard-code UI colors.**

## Settings / theme persistence

- Theme must be restored from `settings.json` on startup.  
- **Critical**: suppress settings saves while applying loaded settings in the VM constructor (setting `SelectedFlavor` must not overwrite theme with the default).  
- Re-apply theme on window open via `ReapplyThemeFromSettings()` (uses in-memory `ThemeLabel`, not a disk re-read that would clobber cycles).  
- **Right panel**: `RightPanelNormalWidth` / `RightPanelCompareWidth` updated on splitter drag and mode switch; defaults in `LayoutDefaults`.  

## After Completing a Milestone

1. All tests green  
2. Update this AGENTS.md if conventions changed  
3. Rewrite HANDOFF.md with exact next steps  
4. Bump version in `Directory.Build.props`  
5. Update `docs/CHANGELOG.md` and user/dev docs  
6. Commit on `main` with a clear message  
7. Ensure GitHub Actions still apply (workflow YAML committed)  
8. For public releases: tag `vX.Y.Z` and push so Publish creates the GitHub Release  

## Useful Commands

```bash
dotnet test --filter Category=Engines
dotnet test --filter Category=Analysis
dotnet test --filter Category=Highlighting
dotnet test --filter Category=Tokens
dotnet test --filter Category=Codegen
dotnet test --filter Category=Library
dotnet test --filter Category=Grep
dotnet test --filter Category=Compare
dotnet test --filter Category=Debug
dotnet test --filter Category=ViewModels
dotnet test --filter Category=Flavors
dotnet test --filter Category=UI
dotnet test --filter Category=Headless
dotnet test --filter Category=Screenshots
dotnet test --filter Category=Branding
```

Logs: `logs/` (gitignored).  
Library/History/Settings: `%AppData%/RegexCraft` (Windows) or `~/Library/Application Support/RegexCraft` (macOS) / `~/.config/RegexCraft` (Linux).

## Key Paths

- Requirements: `docs/development/PHASE-*-REQUIREMENTS.md`  
- Packaging / Releases: `docs/development/packaging.md`  
- Shell: `src/RegexCraft.App/Views/MainWindow.axaml`  
- About: `src/RegexCraft.App/Views/AboutWindow.axaml`  
- Icon: `src/RegexCraft.App/Assets/regexcraft-icon.ico` (+ `.png`, `.icns`)  
- VM: `src/RegexCraft.App/ViewModels/MainWindowViewModel.cs`  
- Compare: `src/RegexCraft.Core/Compare/`  
- Debug: `src/RegexCraft.Core/Debug/`  
- Layout: `src/RegexCraft.Core/Settings/LayoutDefaults.cs`, `AppSettings` panel width fields  
- Flavors: `src/RegexCraft.Core/Flavors/` (`FlavorDefinition`, `FlavorService`, `FlavorTokenSets`)  
- JS engine: `src/RegexCraft.Engines/JavaScript/JavaScriptRegexEngine.cs`  
- Built-in library: `src/RegexCraft.Core/Library/BuiltInLibrary.cs`  
- Theme: `src/RegexCraft.App/Themes/Colors.axaml`  
- CI: `.github/workflows/ci.yml`, `.github/workflows/publish.yml`, `.github/workflows/pages.yml`  
- Website: `website/` · setup: `docs/development/website.md`  
- Headless tests: `tests/RegexCraft.Tests/Headless/`  
- Compare tests: `tests/RegexCraft.Tests/Compare/`  
- Debug tests: `tests/RegexCraft.Tests/Debug/`  
- Flavor tests: `tests/RegexCraft.Tests/Flavors/`  
- Engine tests: `tests/RegexCraft.Tests/Engines/`  
- Settings tests: `tests/RegexCraft.Tests/Settings/`  
- Screenshots: `docs/screenshots/`  
- User Compare doc: `docs/user/comparing.md`  
- User Debug doc: `docs/user/debugging.md`  
- User flavors doc: `docs/user/flavors.md`  
