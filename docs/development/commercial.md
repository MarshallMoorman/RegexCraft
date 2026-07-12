# RegexCraft — Commercial go-live checklist (Phase 13)

**Private monorepo + public dist binaries + public site**  
**No license keys** — free personal / paid business (honor system)  
**Last updated:** 2026-07-12 (1.2.0)

This document is for **Marshall**. Agents implement product + Actions; you do the one-time GitHub/payment/DNS steps.

---

## Model (do not re-litigate)

| Surface | Visibility | Contents |
|---------|------------|----------|
| **Main repo** (`MarshallMoorman/RegexCraft`) | Will become **private** | App source, tests, `website/`, `docs/user/`, `docs/development/`, CI |
| **Dist repo** (`MarshallMoorman/RegexCraft-Releases`) | **Public** | Release zips + README only |
| **Website** (regexcraft.com) | **Public** | Landing, pricing, download, EULA, **user docs only** (built from `website/` + `docs/user/`) |

Actions on the main repo publish outward. You do **not** hand-edit the dist repo for normal releases.

---

## Ordered checklist (do in this order)

### 1. Create the public dist repo

1. On GitHub: **New repository**
2. Name: **`RegexCraft-Releases`** (or match `DIST_REPO` in workflows)
3. Visibility: **Public**
4. **Do not** add a README/license if you prefer the Action to seed README (either is fine)
5. Do **not** push application source here

### 2. Create a fine-scoped token → `DIST_REPO_TOKEN`

1. GitHub → **Settings → Developer settings → Personal access tokens**
2. Prefer a **fine-grained PAT**:
   - Resource owner: your user
   - Repository access: **Only** `RegexCraft-Releases` (and optionally a dedicated website repo if you split later)
   - Permissions: **Contents: Read and write** (create releases, upload assets, push `gh-pages`)
   - Optional: **Metadata: Read**
3. Copy the token
4. Open the **main** repo → **Settings → Secrets and variables → Actions**
5. New repository secret:
   - Name: **`DIST_REPO_TOKEN`**
   - Value: the PAT
6. Do **not** put this token in source or commit messages

### 3. Payment product + paste URL into site config

1. Create a one-time product on **Gumroad**, **Lemon Squeezy**, or **Stripe Payment Link**
2. Suggested price: **$49** (editable anytime)
3. Update checkout URL in:
   - `website/site-config.js` → `buyUrl`
   - `website/pricing.html` (button / `#buy` section)
   - `src/RegexCraft.Core/Commercial/CommercialLinks.cs` → `BuyLicenseUrl` (About dialog)
4. Commit those URL changes on `main` (can be a tiny follow-up commit after 1.2.0)

### 4. Tag `v1.2.0` → verify Actions

On `main` with Phase 13 committed and green CI:

```bash
git checkout main
git pull
grep '<Version>' Directory.Build.props   # expect 1.2.0
git tag -a v1.2.0 -m "RegexCraft 1.2.0"
git push origin v1.2.0
```

Then on GitHub **Actions**:

1. **Publish** workflow runs: test → multi-RID publish → **Public dist release**
2. Confirm a **Release** on `MarshallMoorman/RegexCraft-Releases` with:
   - `RegexCraft-win-x64.zip`
   - `RegexCraft-linux-x64.zip`
   - `RegexCraft-osx-x64.zip`
   - `RegexCraft-osx-arm64.zip`
   - `SHA256SUMS.txt` (when generated)
3. **Deploy website** workflow (runs on `website/**` / `docs/user/**` push, or run manually) builds `site-dist` and pushes to dist repo **`gh-pages`**

If `DIST_REPO_TOKEN` is missing, those jobs fail with a clear error — fix the secret and re-run.

### 5. Incognito download test

1. Open a private/incognito browser window (logged out of GitHub)
2. Visit:  
   https://github.com/MarshallMoorman/RegexCraft-Releases/releases/latest  
3. Download a zip for your platform
4. Unzip and run `RegexCraft.App` / `RegexCraft.App.exe`
5. Confirm download links on https://regexcraft.com/download.html work the same way

### 6. Verify website

1. On **`RegexCraft-Releases`**: **Settings → Pages**
   - Source: **Deploy from a branch**
   - Branch: **`gh-pages`** / root  
   (This is the reliable path once main is private.)
2. Custom domain: **`regexcraft.com`** (CNAME is committed in the site build)
3. Confirm:
   - Landing loads
   - **Pricing**, **EULA**, **Docs** (including Export) load
   - No links to private `docs/development/` content
   - Download CTAs hit the **public** dist release URLs
4. DNS: keep apex A records + www CNAME as in `docs/development/website.md`

### 7. Make the **main** repo private

1. Only after steps 4–6 succeed
2. Main repo → **Settings → General → Danger zone → Change visibility → Private**
3. Collaborators / tokens: ensure your machines and Actions still have access

### 8. Re-verify downloads + site

1. Incognito: latest release download still works
2. Incognito: https://regexcraft.com loads (Pages is on the **public** dist repo)
3. Spot-check docs and pricing
4. Optional: announce release

---

## Workflow reference

| Workflow | File | Trigger | Publishes |
|----------|------|---------|-----------|
| CI | `.github/workflows/ci.yml` | push/PR `main` | Nothing public |
| Publish | `.github/workflows/publish.yml` | tag `v*` (or manual + `publish_to_dist`) | Zips → **dist repo Releases** |
| Deploy website | `.github/workflows/pages.yml` | push `website/**`, `docs/user/**`, or manual | Built site → dist **`gh-pages`** |

Env var / convention: `DIST_REPO=MarshallMoorman/RegexCraft-Releases`

Site build script: `scripts/build-site.sh` (copies `website/`, converts `docs/user/*.md` via pandoc).  
**Never** copies `docs/development/`.

---

## App commercial UX (already in 1.2.0)

- Root **`LICENSE`** = EULA (not MIT)
- About dialog: license summary, Buy license, EULA link, optional honor checkbox
- Settings: `BusinessLicenseAcknowledged` (local only)
- No activation, no keys, no DRM code paths

---

## If something fails

| Symptom | Check |
|---------|--------|
| Dist release job fails on token | Secret name exactly `DIST_REPO_TOKEN`; PAT has write on dist repo |
| Empty release refused | Publish matrix produced no zips — inspect Publish job logs |
| Site not updating | Re-run **Deploy website**; confirm `gh-pages` branch on dist repo |
| Custom domain broken after private main | Pages must be on **public** dist repo, not private monorepo |
| 404 on asset URL | Release asset names must be `RegexCraft-<rid>.zip` |

---

## Post-go-live product notes

- Price and checkout URL can change without a new app binary (site config + About URL)
- Tag every public ship: `vX.Y.Z` on main → Actions own the public surface
- Keep user docs in `docs/user/`; keep phase/engineering docs in `docs/development/` only
