# RegexCraft – Phase 13 Requirements (Final)

**Private-source monorepo + Actions publish public surface + commercial model + Export**  
**Version after this phase**: `1.2.0`  
**Depends on**: 1.1.x + website already in main repo  
**Date**: 2026-07-12  

---

## 1. Goal

1. **Free personal / paid business** use, **no license keys** (honor system)
2. **Single working repo** (main) for all source, website code, user docs, and development docs
3. After each release, **GitHub Actions** publish:
   - Binaries → **public** dist repo (downloads)
   - Website + **user** docs only → public site (GitHub Pages / public surface)
4. Development docs, phase files, source, tests stay **private** (never published)
5. **Export** matches to CSV + JSON
6. Ship **1.2.0** and leave Marshall a clear checklist to go private after downloads work

---

## 2. Working Model (Monorepo + Actions)

### 2.1 Main repo (will become private)
Contains **everything** Marshall and Grok Build edit day-to-day:
- Application source (`src/`, `tests/`)
- `website/` (landing, pricing, download page templates, styles)
- `docs/user/` (user-facing documentation source)
- `docs/development/` (phase requirements, commercial.md, HANDOFF, AGENTS — **private only**)
- CI workflows that **build** and **publish outward**

Marshall does **not** hand-edit the public dist repo for normal releases.

### 2.2 Public surfaces (Actions-only)

| Public target | Contents | Updated by |
|---------------|----------|------------|
| **Dist repo** e.g. `MarshallMoorman/RegexCraft-Releases` (public) | Release zips only + minimal README + link to regexcraft.com / EULA | Action on version tag |
| **Website** (regexcraft.com via Pages) | Landing, pricing, download links, **user docs only** | Action builds from `website/` + `docs/user/` and deploys |

### 2.3 Never publish publicly
- Application source
- `docs/development/**` (PHASE-*, engineering HANDOFF detail, AGENTS internals)
- Tests, internal architecture playbooks that recreate the product

### 2.4 Release flow (single tag on main)

```
git tag v1.2.0 && git push origin v1.2.0
        │
        ▼
Workflow on main
  1. Restore, build, test
  2. dotnet publish (win-x64, linux-x64, osx-x64, osx-arm64)
  3. Create/update GitHub Release on PUBLIC dist repo; upload zips (+ SHA256 if easy)
  4. Build static site = website/ + rendered/copied user docs only
  5. Deploy site to GitHub Pages (or public site branch/repo as documented)
```

Download buttons on the site → **public** dist release URLs (must work logged-out / incognito).

---

## 3. Public Dist Repo

### 3.1 Marshall creates once
- Empty **public** repository, suggested name: `RegexCraft-Releases`
- No application source ever committed there by hand

### 3.2 Agent implements
- Workflow job (on tag `v*`) that uses a secret (e.g. `DIST_REPO_TOKEN`) with permission to create releases/upload assets **only** on the dist repo
- Asset names clear per RID
- Dist repo README: product name, “binaries only”, link to regexcraft.com, EULA summary, “source is proprietary”
- Document secret setup in `docs/development/commercial.md`

### 3.3 Website Download page
- Links to `https://github.com/<user>/RegexCraft-Releases/releases/latest` (or direct asset URLs)
- Platform labels: Windows, macOS Intel, macOS Apple Silicon, Linux
- Same binaries for personal and business (legal difference only)

---

## 4. Website Publish from Main

- Site source stays in main under `website/`
- User docs from `docs/user/` are included in the **built** site (static HTML; generate from markdown if already feasible, else maintain HTML copies under website/docs)
- Deploy via Action (peaceiris/actions-gh-pages, or Pages from a public branch, or push to a public site repo — pick one reliable approach and document it)
- If GitHub Pages from a **private** main repo is unavailable on Marshall’s plan, deploy by pushing built static files to the **public** dist repo’s `gh-pages` branch or a dedicated public `RegexCraft-Website` repo — **document the chosen path**
- CNAME `regexcraft.com` remains correct
- **Do not** copy `docs/development/` into the site

---

## 5. Commercial / License (No Keys)

- **Personal / non-commercial / education**: free  
- **Business / commercial / organizational**: paid one-time  
- **No keys, no activation, no DRM, no phone-home**

### EULA
- Remove MIT as product license
- Add clear EULA (repo + site): free personal; commercial requires purchase; no warranty; no redistribution as competing product; source not licensed for public copying
- About dialog + footer match
- Optional local honor checkbox: “I use this for business and hold a license”
- Buy button → external checkout URL (configurable placeholder for Gumroad/Lemon Squeezy/Stripe Payment Link)

---

## 6. Export Feature (App)

From Test / Matches & Groups:
- **Export CSV** (match index, value, index, length, groups)
- **Export JSON** (matches, groups, pattern, flavor, options, timestamp)
- Save dialog + sensible filenames
- Optional clipboard copy
- Nice-to-have: GREP export, library JSON export/import
- Unit/ViewModel tests for export format

---

## 7. Docs & Messaging

- Full **user** docs on website (Getting started, Test, Replace, Generate, GREP, Compare, Debug, Flavors, Library, Export, shortcuts)
- Pricing page: personal free vs business paid, honor-system line, editable price placeholder (suggest $49)
- Landing: remove open-source/MIT claims; product positioning
- Root README (private): proprietary product, public site + public binaries, how releases work
- **`docs/development/commercial.md`** (private): complete checklist:
  1. Create public dist repo  
  2. Create fine-scoped token → `DIST_REPO_TOKEN`  
  3. Payment product + paste URL into site config  
  4. Tag v1.2.0 → verify Actions  
  5. Incognito download test  
  6. Verify website  
  7. Make **main** repo private  
  8. Re-verify downloads + site  
- CHANGELOG 1.2.0  
- HANDOFF post-1.2 priorities  

---

## 8. Technical Requirements

- One primary release workflow driven by version tags on main  
- No license-key code  
- Existing tests green + Export tests  
- CI must not publish development docs or source to public targets  
- Serilog, themes, multi-flavor stack unchanged  

---

## 9. Versioning & Process

- Version **`1.2.0`** in `Directory.Build.props`  
- Commit:  
  `Phase 13 complete: monorepo + Actions public dist/site, commercial EULA (no keys), Export — 1.2.0`  
- Tag when Marshall has dist repo + secret ready (or tag after first dry-run documented)  
- Print Marshall’s ordered checklist at end of agent run  

---

## 10. Definition of Done

- [ ] EULA in place; MIT removed for product  
- [ ] No license keys  
- [ ] Main repo remains the only day-to-day working repo  
- [ ] Action publishes binaries to **public** dist repo on tag  
- [ ] Action publishes website + user docs only (no development docs)  
- [ ] Download page uses public dist URLs  
- [ ] Export CSV + JSON implemented and tested  
- [ ] `commercial.md` complete (dist repo, token, payment, go private, verify)  
- [ ] About/README/site match commercial model  
- [ ] Version 1.2.0, CHANGELOG, HANDOFF updated  
- [ ] Tests + CI green  
- [ ] Clean commit  

---

## 11. Out of Scope

- License key systems  
- Manual dual-repo feature development  
- Publishing phase/development docs publicly  
- New engines / Debug for non-.NET  
- Full native installers beyond publish zips  

---

**Single source of truth for Phase 13.**  
Work only in main; Actions own the public side; private source after public downloads are proven.
