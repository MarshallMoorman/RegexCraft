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
| **Publish** | `.github/workflows/publish.yml` | Manual (`workflow_dispatch`) or tag `v*` | `dotnet publish` for win-x64, linux-x64, osx-x64, osx-arm64; upload artifacts; on tag create a GitHub Release |

### How to trigger a release build

1. **Manual artifacts (no release)**  
   - GitHub → **Actions** → **Publish** → **Run workflow**  
   - Choose configuration (Release) and self-contained (true)  
   - Download platform artifacts from the run summary  

2. **Tagged release**  
   ```bash
   # After version bump in Directory.Build.props and CHANGELOG
   git tag v1.0.0-rc1
   git push origin v1.0.0-rc1
   ```  
   The Publish workflow builds all RIDs and creates a GitHub Release with archives attached (using `GITHUB_TOKEN`).

---

## Future installer work (out of scope for packaging docs)

Not required for 1.0-rc, but useful later:

| Platform | Direction |
|----------|-----------|
| Windows | MSIX / WiX / Inno Setup wrapping the `win-x64` publish folder + `.ico` |
| macOS | `.app` bundle + `.icns`, optional notarization, DMG |
| Linux | AppImage, Flatpak, or distro packages from `linux-x64` |

Until then, **portable self-contained zips** are the supported distribution format.

---

## Versioning checklist

1. Edit only `Directory.Build.props` → `<Version>1.0.0-rc1</Version>`  
2. Update `docs/CHANGELOG.md`  
3. Commit, tag `v1.0.0-rc1`, push tag  
4. Confirm Publish workflow + Release on GitHub  

---

## Local verification before release

```bash
dotnet build -c Release
dotnet test -c Release
dotnet publish src/RegexCraft.App -c Release -r osx-arm64 --self-contained -o /tmp/rc-test
# Run /tmp/rc-test/RegexCraft.App (path may vary slightly by OS)
```
