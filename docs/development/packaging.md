# RegexCraft – Packaging & Publish Guide

**Version source of truth**: `Directory.Build.props` (`<Version>…</Version>` only).  
Do not hard-code version numbers in project files or UI strings beyond reading the assembly informational version.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Platform workloads are not required for Avalonia desktop publish of this project

---

## Local publish commands

Self-contained binaries include the .NET runtime (larger, portable). Framework-dependent builds are smaller but require the runtime installed on the target machine.

### Self-contained (recommended for distribution)

```bash
# Windows x64
dotnet publish src/RegexCraft.App -c Release -r win-x64 --self-contained -o artifacts/win-x64

# Linux x64
dotnet publish src/RegexCraft.App -c Release -r linux-x64 --self-contained -o artifacts/linux-x64

# macOS Intel
dotnet publish src/RegexCraft.App -c Release -r osx-x64 --self-contained -o artifacts/osx-x64

# macOS Apple Silicon
dotnet publish src/RegexCraft.App -c Release -r osx-arm64 --self-contained -o artifacts/osx-arm64
```

### Framework-dependent

```bash
dotnet publish src/RegexCraft.App -c Release -r win-x64 --self-contained false -o artifacts/win-x64-fd
```

### Portable zip (example)

```bash
dotnet publish src/RegexCraft.App -c Release -r osx-arm64 --self-contained -o artifacts/RegexCraft-osx-arm64
cd artifacts && zip -r RegexCraft-osx-arm64.zip RegexCraft-osx-arm64
```

On Windows (PowerShell):

```powershell
dotnet publish src/RegexCraft.App -c Release -r win-x64 --self-contained -o artifacts/RegexCraft-win-x64
Compress-Archive -Path artifacts/RegexCraft-win-x64 -DestinationPath artifacts/RegexCraft-win-x64.zip
```

---

## Icons

| Asset | Path | Use |
|-------|------|-----|
| Windows ICO | `src/RegexCraft.App/Assets/regexcraft-icon.ico` | Window icon, `ApplicationIcon` in csproj |
| macOS ICNS | `src/RegexCraft.App/Assets/regexcraft-icon.icns` | Future `.app` bundle / installer |
| PNG | `src/RegexCraft.App/Assets/regexcraft-icon.png` | Docs, About, general |

The Avalonia app project already references the ICO as the application icon. When creating a macOS `.app` or Windows installer, include the `.icns` / `.ico` in the bundle resources.

Check `RegexCraft.App.csproj` for:

```xml
<ApplicationIcon>Assets\regexcraft-icon.ico</ApplicationIcon>
```

---

## What a publish folder contains

A typical self-contained output includes:

- `RegexCraft.App` (or `RegexCraft.App.exe` on Windows) — entry point  
- Managed assemblies (`RegexCraft.Core.dll`, `RegexCraft.Engines.dll`, Avalonia, engines, …)  
- Native libraries (PCRE, Skia, etc. as required by RIDs)  
- `appsettings.json` (if copied)  

Run the executable from that folder (or ship the whole folder as a portable zip).

---

## GitHub Actions

| Workflow | File | Trigger | Purpose |
|----------|------|---------|---------|
| **CI** | `.github/workflows/ci.yml` | Push / PR to `main` | Restore, Debug + Release build, full `dotnet test`, TRX + optional screenshots |
| **Publish** | `.github/workflows/publish.yml` | Manual (`workflow_dispatch`) or tag `v*` | Test → `dotnet publish` for win-x64, linux-x64, osx-x64, osx-arm64 → artifacts; on tag create a **GitHub Release** with zips attached |

### How to cut a public release (1.0.0 and later)

Version lives only in `Directory.Build.props`. Changelog lives in `docs/CHANGELOG.md`.

```bash
# 1. Ensure main is green and version/CHANGELOG already committed
git checkout main
git pull origin main
grep '<Version>' Directory.Build.props   # e.g. 1.0.0

# 2. Tag and push the tag (this triggers Publish + GitHub Release)
git tag -a v1.0.0 -m "RegexCraft 1.0.0"
git push origin v1.0.0
```

What happens next:

1. **Test** job on Ubuntu runs `dotnet build` + `dotnet test` (screenshots skipped for speed).  
2. **Publish** matrix builds self-contained binaries for:
   - `win-x64`
   - `linux-x64`
   - `osx-x64`
   - `osx-arm64`  
   Strategy is `fail-fast: false` so one RID failure does not cancel the others.  
3. Each successful RID uploads a folder artifact and a **zip** archive (`RegexCraft-<rid>.zip`).  
4. **GitHub Release** job (tag only):
   - Downloads matrix artifacts  
   - Attaches zips (or tar.gz fallback) to a new Release for that tag  
   - Writes release notes from the matching `docs/CHANGELOG.md` section plus auto-generated commit notes  
   - Marks pre-release when the tag contains `-`, `rc`, `beta`, or `alpha`  
   - **Refuses** to create a release with zero archives (no empty half-created releases)

### Manual artifacts (no GitHub Release)

1. GitHub → **Actions** → **Publish** → **Run workflow**  
2. Choose configuration (Release) and self-contained (true)  
3. Download platform artifacts from the run summary  

### Expected release assets

| Asset | Platform |
|-------|----------|
| `RegexCraft-win-x64.zip` | Windows x64 |
| `RegexCraft-linux-x64.zip` | Linux x64 |
| `RegexCraft-osx-x64.zip` | macOS Intel |
| `RegexCraft-osx-arm64.zip` | macOS Apple Silicon |

Unzip and run `RegexCraft.App` / `RegexCraft.App.exe` from the extracted folder. No installer is required for portable use.

### Permissions

- Basic CI: no secrets.  
- Publish / Release: default `GITHUB_TOKEN` with `contents: write` (configured in the workflow).  
- No interactive secrets are required for open-source tag releases.

---

## Future installer work (out of scope for 1.0 portable zips)

| Platform | Direction |
|----------|-----------|
| Windows | MSIX / WiX / Inno Setup wrapping the `win-x64` publish folder + `.ico` |
| macOS | `.app` bundle + `.icns`, optional notarization, DMG |
| Linux | AppImage, Flatpak, or distro packages from `linux-x64` |

Until then, **portable self-contained zips** from GitHub Releases are the supported distribution format.

---

## Versioning checklist

1. Edit only `Directory.Build.props` → `<Version>1.0.0</Version>`  
2. Update `docs/CHANGELOG.md` with a `## [1.0.0]` section  
3. Update README / AGENTS / HANDOFF as needed  
4. Commit on `main`  
5. Tag `v1.0.0` and `git push origin v1.0.0`  
6. Confirm **Publish** workflow + GitHub Release assets on GitHub  

---

## Local verification before release

```bash
dotnet build -c Release
dotnet test -c Release
dotnet publish src/RegexCraft.App -c Release -r osx-arm64 --self-contained -o /tmp/rc-test
# Run /tmp/rc-test/RegexCraft.App (path may vary slightly by OS)
```
