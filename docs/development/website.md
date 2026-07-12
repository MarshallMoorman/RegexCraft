# RegexCraft website — GitHub Pages + DNS setup

**Site URL (target):** https://regexcraft.com  
**Source in repo:** [`website/`](../../website/)  
**Deploy workflow:** [`.github/workflows/pages.yml`](../../.github/workflows/pages.yml)  
**Last updated:** 2026-07-11 (Phase 12)

This document is the human checklist for launching and maintaining the public site.  
No passwords or API keys need to be shared with agents — only public repo settings and DNS at your registrar.

---

## 1. How the site is built

| Item | Detail |
|------|--------|
| Tech | Plain HTML + CSS under `website/` (no npm build) |
| Pages | `index.html`, `download.html`, `docs.html`, `about.html` |
| CNAME | `website/CNAME` contains exactly: `regexcraft.com` |
| Screenshots | Copies under `website/assets/screenshots/` (from `docs/screenshots/`) |
| Branding | Blues match the app (`#0078D4`); logo/favicon from app Assets |

Local preview:

```bash
python3 -m http.server 8080 --directory website
# open http://localhost:8080
```

---

## 2. Enable GitHub Pages (do this once after Phase 12 is on `main`)

1. Open the repo on GitHub:  
   https://github.com/MarshallMoorman/RegexCraft
2. Go to **Settings** → **Pages** (left sidebar under “Code and automation”).
3. Under **Build and deployment** → **Source**, choose:
   - **GitHub Actions**  
   (Not “Deploy from a branch”. The site lives in `website/`, which is not the special `/` or `/docs` root.)
4. Save if needed.
5. Open the **Actions** tab → run workflow **Deploy website** (or push any change under `website/`).
6. When the workflow is green, GitHub provides a URL such as:  
   `https://marshallmoorman.github.io/RegexCraft/`  
   That is the temporary Pages URL until the custom domain works.
7. Still under **Settings → Pages**, find **Custom domain**:
   - Enter: `regexcraft.com`
   - Save.
   - GitHub should detect the committed `CNAME` file and may fill this automatically after the first deploy.
8. After DNS is correct (section 3), check **Enforce HTTPS** and enable it when GitHub allows (can take a few minutes after DNS verifies).

### If the workflow fails with a permissions / environment error

- Confirm **Settings → Pages → Source** is **GitHub Actions**.
- Confirm the first successful run created the `github-pages` environment (Settings → Environments).
- You do **not** need a personal access token; the workflow uses the built-in `GITHUB_TOKEN` with `pages: write` and `id-token: write`.

---

## 3. DNS records (at your domain registrar)

You own **regexcraft.com**. Point it at GitHub Pages.

GitHub’s documented Pages host for user/org sites and project sites is usually:

```text
marshallmoorman.github.io
```

Use exactly what **Settings → Pages** shows if it differs.

### Recommended records

#### A) Apex domain — `regexcraft.com`

Create **four A records** (same host, different values):

| Type | Name / Host | Value | TTL |
|------|-------------|-------|-----|
| `A` | `@` (or blank / `regexcraft.com`) | `185.199.108.153` | Auto or 3600 |
| `A` | `@` | `185.199.109.153` | Auto or 3600 |
| `A` | `@` | `185.199.110.153` | Auto or 3600 |
| `A` | `@` | `185.199.111.153` | Auto or 3600 |

> Confirm current apex IPs in [GitHub’s custom domain docs](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site/managing-a-custom-domain-for-your-github-pages-site) if these ever change.

#### B) www — `www.regexcraft.com`

| Type | Name / Host | Value | TTL |
|------|-------------|-------|-----|
| `CNAME` | `www` | `marshallmoorman.github.io` | Auto or 3600 |

(Optional but recommended so both apex and www work. In GitHub custom domain, using the apex usually covers www after DNS is set; follow GitHub’s UI prompts.)

#### C) Do **not**

- Do not point the apex at a random CDN CNAME unless you know what you are doing.
- Do not remove the `website/CNAME` file while using the custom domain.
- Do not set Pages source to “Deploy from a branch” while the Action-based deploy is in use (pick one approach).

### After saving DNS

1. Wait for propagation (often 5–60 minutes; sometimes up to 24–48 hours).
2. Check DNS:

   ```bash
   dig regexcraft.com +short
   dig www.regexcraft.com +short
   ```

   Apex should return the four `185.199.*` addresses (or a subset as they answer).  
   `www` should CNAME to `marshallmoorman.github.io`.

3. In GitHub **Settings → Pages**, wait until the custom domain shows as verified / DNS check passed.
4. Enable **Enforce HTTPS**.
5. Open https://regexcraft.com — you should see the RegexCraft landing page.

---

## 4. Quick human checklist (print / tick)

### GitHub

- [ ] Phase 12 commit is on `main` (includes `website/` + `pages.yml`)
- [ ] **Settings → Pages → Source** = **GitHub Actions**
- [ ] **Actions → Deploy website** completed successfully at least once
- [ ] Temporary URL loads: `https://marshallmoorman.github.io/RegexCraft/`
- [ ] **Custom domain** set to `regexcraft.com`
- [ ] **Enforce HTTPS** enabled after DNS is green

### Registrar DNS

- [ ] Four **A** records for apex → GitHub Pages IPs (`185.199.108–111.153`)
- [ ] **CNAME** `www` → `marshallmoorman.github.io`
- [ ] Old conflicting A/AAAA/CNAME records for `@` or `www` removed
- [ ] `dig` / browser confirms https://regexcraft.com

### Optional

- [ ] Bookmark `docs/development/website.md` for future updates
- [ ] After major app UI changes, refresh website screenshots (section 5)

---

## 5. Updating the site later

1. Edit files under `website/` (or copy new screenshots into `website/assets/screenshots/`).
2. Commit on `main` and push.
3. Workflow **Deploy website** runs automatically when `website/**` changes.
4. Hard-refresh the browser if you still see a cached page.

### Refresh screenshots from the app

```bash
dotnet test --filter Category=Screenshots
# Review docs/screenshots/*.png, then:
cp docs/screenshots/main-test-light.png \
   docs/screenshots/main-test-dark.png \
   docs/screenshots/main-compare.png \
   docs/screenshots/main-generate.png \
   docs/screenshots/main-grep.png \
   docs/screenshots/main-replace.png \
   docs/screenshots/main-library.png \
   docs/screenshots/about-light.png \
   website/assets/screenshots/
```

### Change custom domain later

1. Edit `website/CNAME` to the new hostname (one line, no scheme).
2. Update registrar DNS.
3. Update **Settings → Pages → Custom domain**.
4. Redeploy / wait for DNS check.

---

## 6. Relationship to app CI

| Workflow | Purpose | Blocks app? |
|----------|---------|-------------|
| `ci.yml` | Build + test the .NET solution | No interaction with `website/` |
| `publish.yml` | Portable binaries + GitHub Releases | Unrelated to Pages |
| `pages.yml` | Static site only | Does not run `dotnet test` |

The website must not break `dotnet build` / `dotnet test`. It is not part of the solution projects.

---

## 7. Troubleshooting

| Symptom | Likely fix |
|---------|------------|
| 404 on `*.github.io/RegexCraft/` | Run **Deploy website** once; confirm Source = GitHub Actions |
| Custom domain “DNS check failed” | Fix A/CNAME records; remove conflicting records; wait for TTL |
| HTTPS not available | Wait until DNS verifies, then enable Enforce HTTPS |
| Wrong old site / cache | Hard refresh; confirm latest workflow run used current `main` |
| CNAME conflict warning | Ensure only this repo claims `regexcraft.com` on GitHub Pages |

---

## 8. Reference links

- [GitHub Pages — custom domains](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site)
- [Managing a custom domain](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site/managing-a-custom-domain-for-your-github-pages-site)
- [Using custom workflows with GitHub Pages](https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site#publishing-with-a-custom-github-actions-workflow)
- Site source: [`website/README.md`](../../website/README.md)
