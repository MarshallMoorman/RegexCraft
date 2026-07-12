# RegexCraft website — public site setup (Phase 13)

**Site URL (target):** https://regexcraft.com  
**Source in monorepo:** [`website/`](../../website/) + [`docs/user/`](../user/)  
**Build script:** [`scripts/build-site.sh`](../../scripts/build-site.sh)  
**Deploy workflow:** [`.github/workflows/pages.yml`](../../.github/workflows/pages.yml)  
**Last updated:** 2026-07-12 (Phase 13)

Public site = marketing HTML + **user docs only**.  
**Never** publish `docs/development/`, AGENTS internals, or application source.

---

## 1. How the site is built

| Item | Detail |
|------|--------|
| Marketing | Plain HTML/CSS under `website/` |
| User docs | `docs/user/*.md` → HTML via **pandoc** at build time |
| Output | `site-dist/` (gitignored) |
| CNAME | `regexcraft.com` (from `website/CNAME`, also set by Action) |
| Theme | Blue professional (`#0078D4`); no purple |

Local preview of **marketing only**:

```bash
python3 -m http.server 8080 --directory website
```

Full public build (requires pandoc):

```bash
bash scripts/build-site.sh
python3 -m http.server 8080 --directory site-dist
```

---

## 2. Deploy target (important for private main)

Phase 13 deploys the built site to the **public dist repo**:

| Setting | Value |
|---------|--------|
| Repository | `MarshallMoorman/RegexCraft-Releases` |
| Branch | `gh-pages` |
| Auth | `DIST_REPO_TOKEN` secret on the **main** monorepo |

### One-time on the dist repo

1. Open https://github.com/MarshallMoorman/RegexCraft-Releases  
2. **Settings → Pages**  
3. Build and deployment → Source: **Deploy from a branch**  
4. Branch: **`gh-pages`** / `/ (root)`  
5. Custom domain: **`regexcraft.com`**  
6. After DNS verifies, enable **Enforce HTTPS**

This continues to work when the monorepo is private (Pages lives on the public dist repo).

### Secret

Same as releases: **`DIST_REPO_TOKEN`** — see [commercial.md](commercial.md).

---

## 3. DNS records (at your domain registrar)

Point **regexcraft.com** at GitHub Pages.

### Apex — `regexcraft.com`

Four **A** records:

| Type | Name | Value |
|------|------|-------|
| A | `@` | `185.199.108.153` |
| A | `@` | `185.199.109.153` |
| A | `@` | `185.199.110.153` |
| A | `@` | `185.199.111.153` |

Confirm IPs in [GitHub custom domain docs](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site/managing-a-custom-domain-for-your-github-pages-site) if they change.

### www — `www.regexcraft.com`

| Type | Name | Value |
|------|------|-------|
| CNAME | `www` | `marshallmoorman.github.io` |

(Or the Pages host GitHub shows for the dist repo.)

---

## 4. What must never appear on the site

- `docs/development/**` (PHASE requirements, commercial ops detail beyond public EULA/pricing, HANDOFF engineering)
- Application source / tests
- Internal architecture playbooks that recreate the product

The workflow fails the job if development paths leak into `site-dist`.

---

## 5. Content map

| Page | Role |
|------|------|
| `index.html` | Landing |
| `download.html` | Public dist download links |
| `pricing.html` | Personal free / business paid + buy placeholder |
| `eula.html` | End user license |
| `docs.html` | Index of user guides |
| `docs/*.html` | Generated from `docs/user/` |
| `about.html` | Story, stack, license summary |
| `site-config.js` | Buy URL / version placeholders |

After major UI changes: refresh screenshots under `docs/screenshots/` → copy to `website/assets/screenshots/`.

---

## 6. Related

- Full commercial go-live order: [commercial.md](commercial.md)  
- Packaging / tags: [packaging.md](packaging.md)  
