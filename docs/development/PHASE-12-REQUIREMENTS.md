# RegexCraft – Phase 12 Requirements (Website + GitHub Pages)

**Project**: RegexCraft  
**Version after this phase**: `1.1.1` (docs/site only) or keep `1.1.0` and just ship the site — prefer a small bump to `1.1.1` if any app fixes are included  
**Depends on**: 1.1.0  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 12

Launch a professional public website for **https://regexcraft.com** hosted on **GitHub Pages**, fully source-controlled in this same repository.

Also include 1–2 small product polish items if they are quick (optional).

---

## 2. What YOU (Marshall) Need to Do (Human Checklist)

The agent will create all the site files and the CNAME. You only need to do the domain + GitHub settings steps below.

### A. GitHub repo settings (after the site is merged)
1. Go to the repo → **Settings** → **Pages**
2. Under “Build and deployment”:
   - Source: **Deploy from a branch**
   - Branch: `main` (or `gh-pages` if the agent uses that approach — follow what ends up in the docs)
   - Folder: `/docs` or `/ (root)` or whatever the agent documents (commonly `/docs` or a `website` folder published via Action)
3. Save. GitHub will give you a URL like `https://marshallmoorman.github.io/RegexCraft/`
4. After the CNAME is in the repo, you can add the custom domain `regexcraft.com` in the same Pages settings (GitHub often detects the CNAME file).

### B. DNS at your domain registrar (for regexcraft.com)
You need to point the domain to GitHub Pages.

**Recommended (CNAME for www + optional apex):**

1. **www.regexcraft.com**  
   Type: `CNAME`  
   Name/Host: `www`  
   Value: `marshallmoorman.github.io`  
   (Use the exact Pages hostname GitHub shows you if different.)

2. **Apex regexcraft.com** (optional but nice)  
   GitHub documents these A records for apex domains:
   - `185.199.108.153`
   - `185.199.109.153`
   - `185.199.110.153`
   - `185.199.111.153`  
   (Confirm current values in GitHub Pages docs if they ever change.)

3. In GitHub Pages settings, set Custom domain to `regexcraft.com` (and enable “Enforce HTTPS” once DNS has propagated).

4. Wait for DNS propagation (can be minutes to a few hours). Then https://regexcraft.com should load the site.

**You do NOT need to give the agent any passwords or API keys for this.** Everything is public DNS + public repo settings.

---

## 3. Site Requirements (Agent Implements)

### 3.1 Source control
- All website source lives **in this same repo** (no separate private site repo required).
- Preferred layout (agent may choose the cleanest variant and document it):

```
website/                    # or docs/ if using /docs for Pages
  index.html
  styles.css (or assets/)
  assets/
    screenshots/            # copy or link from docs/screenshots if present
    favicon / logo
  CNAME                     # contains: regexcraft.com
  README.md                 # how the site is built/deployed
```

Alternatively a static generator is fine if kept simple (plain HTML/CSS is preferred for zero build complexity unless already using something).

### 3.2 Pages to include (minimum)

1. **Home / Landing**
   - Clear value proposition: modern cross-platform regex tool (Test, Replace, Generate, GREP, Compare, Debug)
   - Hero with app name + short tagline
   - Primary CTA: Download (links to latest GitHub Release)
   - Secondary CTA: View on GitHub / Documentation
   - Feature highlights (cards or grid): Test, Replace, Multi-flavor, Generate, GREP, Compare, Debug, Library
   - Screenshots (use existing ones from `docs/screenshots/` if available; otherwise placeholders + note)
   - Engines/flavors summary
   - Footer: copyright, GitHub link, license (MIT)

2. **Download** (can be a section on Home or separate page)
   - Link to GitHub Releases
   - Note supported platforms (Windows, macOS, Linux) from the published RIDs
   - “Or build from source” → link to repo README

3. **Docs** (can link out to GitHub `docs/user/` for v1, or mirror key pages)
   - Minimum: links to Getting Started, Testing, Debugging, Flavors, etc. on GitHub
   - Optional: copy the most important user docs into the site as static HTML

4. **About / Project**
   - Short story, MIT license, built with Avalonia + .NET
   - Link to GitHub

### 3.3 Design
- Professional, matches the app’s **blue** theme (no purple)
- Light page background by default; optional simple dark section or respect `prefers-color-scheme` if easy
- Clean typography, good spacing, mobile-friendly (responsive)
- Fast, no heavy frameworks required (plain HTML + CSS is ideal)
- Favicon / app icon if available from the Avalonia assets

### 3.4 Technical
- `CNAME` file with exact content: `regexcraft.com`
- Document in `website/README.md` (or `docs/development/website.md`):
  - How the site is structured
  - How GitHub Pages is configured
  - What DNS the human must set (copy the checklist above)
  - How to update screenshots later
- If using a GitHub Action to deploy (optional but nice), keep it simple and documented
- Do not break the existing app CI

### 3.5 Optional small app polish (only if quick)
- Any trivial leftover from HANDOFF (e.g. free-spacing `#` comment highlighting) **only if** it does not risk delaying the site
- Prefer shipping the site over more app features in this phase

---

## 4. Documentation Updates in the Repo

- Root `README.md`: add a prominent “Website: https://regexcraft.com” (once live) and “Site source: `/website`”
- `docs/CHANGELOG.md`: entry for the website launch (and 1.1.1 if version bumped)
- `docs/development/website.md`: full setup + DNS instructions for future you
- `HANDOFF.md`: rewritten — website done, next priorities (Export, Debug expansion, engines, installers, etc.)
- `AGENTS.md`: note that website source lives in-repo

---

## 5. Versioning & Process

- Prefer bump to **`1.1.1`** if any app code changes; if pure website, version bump is optional but a CHANGELOG entry is still required
- All app tests must still pass (site must not break the solution build)
- Clean commit:  
  `Phase 12 complete: regexcraft.com website on GitHub Pages + setup docs`
- After merge, human enables Pages + sets DNS (see checklist)
- Tag only if version was bumped

---

## 6. Definition of Done

- [ ] Static website source committed in this repo (`website/` or equivalent)
- [ ] `CNAME` present with `regexcraft.com`
- [ ] Landing page with features, CTAs, screenshots, download links
- [ ] Design matches blue professional theme, responsive
- [ ] `docs/development/website.md` (or equivalent) explains Pages + DNS steps for the human
- [ ] Root README references the site
- [ ] CHANGELOG updated
- [ ] HANDOFF.md updated with post-website roadmap
- [ ] App still builds and tests pass
- [ ] Clean commit on main
- [ ] (Human) Pages enabled + DNS configured → site live at https://regexcraft.com

---

## 7. Out of Scope

- Complex JS framework SPA (keep it simple static)
- Blog, accounts, analytics (unless trivial)
- Debug expansion, Export, new engines
- Paid hosting

---

**This document is the single source of truth for Phase 12.**  
Ship a clean GitHub Pages site for regexcraft.com and leave Marshall clear DNS/Pages instructions.
