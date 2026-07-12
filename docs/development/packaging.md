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

## GitHub Actions (Phase 13+)

| Workflow | File | Trigger | Purpose |
|----------|------|---------|---------|
| **CI** | `.github/workflows/ci.yml` | Push / PR to `main` | Restore, Debug + Release build, full `dotnet test` |
| **Publish** | `.github/workflows/publish.yml` | Tag `v*` (or manual) | Test → multi-RID publish → **public dist repo** GitHub Release |
| **Deploy website** | `.github/workflows/pages.yml` | Push `website/**` / `docs/user/**`, or manual | Build site (user docs only) → public dist **`gh-pages`** |

Public binaries live on **`MarshallMoorman/RegexCraft-Releases`**, not on the private monorepo.  
Full go-live order: [commercial.md](commercial.md).

### How to cut a public release

Version lives only in `Directory.Build.props`. Changelog lives in `docs/CHANGELOG.md`.  
Prerequisites: public dist repo exists + monorepo secret **`DIST_REPO_TOKEN`**.

```bash
# 1. Ensure main is green and version/CHANGELOG already committed
git checkout main
git pull origin main
grep '<Version>' Directory.Build.props   # e.g. 1.2.0

# 2. Tag and push the tag (this triggers Publish → public dist release)
git tag -a v1.2.0 -m "RegexCraft 1.2.0"
git push origin v1.2.0
```

What happens next:

1. **Test** job: `dotnet build` + `dotnet test` (screenshots skipped).  
2. **Publish** matrix: self-contained `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64` (`fail-fast: false`).  
3. Each RID uploads folder + **`RegexCraft-<rid>.zip`**.  
4. **Public dist release** job (tag only; needs `DIST_REPO_TOKEN`):
   - Creates/updates a Release on `MarshallMoorman/RegexCraft-Releases`
   - Uploads zips + `SHA256SUMS.txt` when present  
   - Notes from CHANGELOG + EULA/pricing links  
   - Refuses empty releases  

Download page: https://regexcraft.com/download.html → public dist URLs.

### Publish pitfalls (still relevant)

| Issue | Cause | Fix in workflow |
|-------|--------|-----------------|
| **win-x64** `MSB1008` | Git Bash mangles `/p:` | Use `-p:` and `MSYS2_ARG_CONV_EXCL=*` |
| **linux-x64** exit 2 | `ls \| head` + pipefail | Do not pipe `ls` to `head` |
| Dist release fails immediately | Missing `DIST_REPO_TOKEN` | Create PAT + secret (commercial.md) |

### Manual artifacts (no dist release)

1. **Actions → Publish → Run workflow**  
2. Leave **publish_to_dist** false to only keep workflow artifacts  
3. Set **publish_to_dist** true to also create a dist release (needs token)

### Expected release assets

| Asset | Platform |
|-------|----------|
| `RegexCraft-win-x64.zip` | Windows x64 |
| `RegexCraft-linux-x64.zip` | Linux x64 |
| `RegexCraft-osx-x64.zip` | macOS Intel |
| `RegexCraft-osx-arm64.zip` | macOS Apple Silicon |
| `SHA256SUMS.txt` | Checksums (when generated) |

Unzip and run `RegexCraft.App` / `RegexCraft.App.exe`. No installer required for portable use.

### Permissions / secrets

- **CI**: no secrets.  
- **Publish to public dist**: repository secret **`DIST_REPO_TOKEN`** (contents write on dist repo).  
- **Site deploy**: same **`DIST_REPO_TOKEN`** (push `gh-pages` on dist repo).  

---

## Future installer work (out of scope for portable zips)

| Platform | Direction |
|----------|-----------|
| Windows | MSIX / WiX / Inno Setup wrapping the `win-x64` publish folder + `.ico` |
| macOS | `.app` bundle + `.icns`, optional notarization, DMG |
| Linux | AppImage, Flatpak, or distro packages from `linux-x64` |

Until then, **portable self-contained zips** from the public dist Releases are the supported distribution format.

---

## Versioning checklist

1. Edit only `Directory.Build.props` → `<Version>1.2.0</Version>`  
2. Update `docs/CHANGELOG.md` with a `## [1.2.0]` section  
3. Update README / AGENTS / HANDOFF as needed  
4. Commit on `main`  
5. Tag `v1.2.0` and `git push origin v1.2.0`  
6. Confirm **Publish** → assets on **RegexCraft-Releases**  
7. Confirm **Deploy website** if site/docs changed  


---

## Local verification before release

```bash
dotnet build -c Release
dotnet test -c Release
dotnet publish src/RegexCraft.App -c Release -r osx-arm64 --self-contained -o /tmp/rc-test
# Run /tmp/rc-test/RegexCraft.App (path may vary slightly by OS)
```
