# RegexCraft

**Modern, cross-platform regular expression workbench** for exploring, testing, replacing, grepping, comparing, debugging, exporting, and generating code across many regex flavors.

**Website:** [https://regexcraft.com](https://regexcraft.com) · **Public binaries:** [RegexCraft-Releases](https://github.com/MarshallMoorman/RegexCraft-Releases)

| | |
|---|---|
| **Version** | **1.2.0** |
| **Website** | [regexcraft.com](https://regexcraft.com) |
| **Stack** | .NET 10 · Avalonia 12 · AvaloniaEdit · Jint · NUnit · Serilog |
| **License** | Commercial [EULA](LICENSE) — free personal / paid business (no keys) |
| **Status** | Stable 1.2 |

> **This monorepo is the day-to-day working repository** (app, website source, user docs, private development docs).  
> After public downloads work, it is intended to be **private**. Public users get binaries and the marketing/docs site only — see [docs/development/commercial.md](docs/development/commercial.md).

![RegexCraft Match mode (light)](docs/screenshots/main-test-light.png)

---

## License (not open source)

RegexCraft is **proprietary** software under the product [EULA](LICENSE):

- **Personal / non-commercial / education** — free  
- **Business / commercial / organizational** — paid one-time license (suggested **$49**; see [pricing](https://regexcraft.com/pricing.html))  
- **No license keys**, activation, DRM, or phone-home — honor system  

Do not treat this repository as MIT/open-source for redistribution of the product.

---

## Public downloads

Portable self-contained zips are published by Actions on every version tag to the **public** dist repository:

**[→ Latest release](https://github.com/MarshallMoorman/RegexCraft-Releases/releases/latest)** · **[→ regexcraft.com/download](https://regexcraft.com/download.html)**

| Asset | Platform |
|-------|----------|
| `RegexCraft-win-x64.zip` | Windows x64 |
| `RegexCraft-linux-x64.zip` | Linux x64 |
| `RegexCraft-osx-x64.zip` | macOS Intel |
| `RegexCraft-osx-arm64.zip` | macOS Apple Silicon |

Unzip and run `RegexCraft.App` (or `RegexCraft.App.exe` on Windows).

---

## Features

- **Multi-flavor testing** — .NET, PCRE2, **JavaScript** (Jint), TypeScript, Python, Java, PHP, Ruby, Go, Rust, Perl, Kotlin, Swift  
- **Export** — Match results to **CSV** and **JSON** (groups, pattern, flavor, options, timestamp)  
- **Compare mode** — side-by-side results for 2–4 flavors; smart right-panel width  
- **Debug step-through** — educational walk-through for the **.NET** engine (F10/F11)  
- **Hardened flavor definitions** — options, token matrices, known differences, preferred codegen language  
- **Clear fidelity** — Full / High / Approximate banners; token palette dims unsupported constructs  
- **Professional editor** — AvaloniaEdit, light/dark syntax highlighting, live analysis tree  
- **Live Match mode** — subject highlighting, equal-width match cards  
- **Replace & Split** — live preview, substitution highlighting  
- **GREP** — search/replace across folders with globs, async progress, dry-run, backups  
- **Code generation** — C#, JS/TS, Python, PHP, Java, Go, Rust, Ruby, Perl, Kotlin, Swift  
- **Library & History** — 20 built-ins + user patterns; local persistence  
- **Themes** — Light / Dark / System, persisted correctly  

---

## Engines & flavors

| Engine Id | Display | Full Testing | Replace | Split | GREP | Debug |
|-----------|---------|--------------|---------|-------|------|-------|
| `dotnet` | .NET | Yes | Yes | Yes | Yes | **Yes** |
| `pcre2` | PCRE2 | Yes | Yes | Yes | Yes | No |
| `javascript` | JavaScript (Jint) | Yes | Yes | Yes | Yes | No |

Approximate flavors (Python, Java, Go, …) map to the closest real engine with fidelity notes. Details: [docs/user/flavors.md](docs/user/flavors.md).

---

## How releases work

```
git tag vX.Y.Z && git push origin vX.Y.Z
        │
        ▼
Publish workflow on main
  1. Restore, build, test
  2. dotnet publish (win-x64, linux-x64, osx-x64, osx-arm64)
  3. Create GitHub Release on PUBLIC dist repo + upload zips
Deploy website workflow
  4. Build website/ + docs/user only → dist repo gh-pages
```

Requires secret **`DIST_REPO_TOKEN`**. Checklist: [docs/development/commercial.md](docs/development/commercial.md).  
Packaging details: [docs/development/packaging.md](docs/development/packaging.md).

---

## Build & run (maintainers)

```bash
dotnet build
dotnet test
dotnet run --project src/RegexCraft.App
```

```bash
dotnet test --filter Category=Export
dotnet test --filter "Category=Engines|Category=Flavors|Category=Compare|Category=Debug|Category=Export"
```

### Site local build

```bash
bash scripts/build-site.sh   # needs pandoc
python3 -m http.server 8080 --directory site-dist
```

---

## Documentation

| Audience | Location |
|----------|----------|
| **Users** | [docs/user/](docs/user/) · published on [regexcraft.com/docs](https://regexcraft.com/docs.html) |
| **Maintainers / agents** | [AGENTS.md](AGENTS.md), [HANDOFF.md](HANDOFF.md), [docs/development/](docs/development/) (**private**) |
| **Commercial go-live** | [docs/development/commercial.md](docs/development/commercial.md) |
| **Changelog** | [docs/CHANGELOG.md](docs/CHANGELOG.md) |

---

## Solution layout

| Project | Role |
|---------|------|
| `RegexCraft.Core` | Engines API, flavors, Compare, Debug, Export, tokens, analysis, codegen, library/history/settings, GREP |
| `RegexCraft.Engines` | .NET, PCRE2, JavaScript (Jint) |
| `RegexCraft.App` | Avalonia UI, ViewModels, About, themes |
| `RegexCraft.Tests` | NUnit unit + headless UI |
| `website/` | Public marketing site source |

---

## Copyright

Copyright © Marshall Moorman 2026. All rights reserved. See [LICENSE](LICENSE).
