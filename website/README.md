# RegexCraft public website

Static site for **https://regexcraft.com**, source-controlled in this repository under `website/`.

## Stack

- Plain HTML + CSS (no build step, no JS framework)
- Optional tiny nav toggle script for mobile
- Brand blues match the desktop app (`#0078D4`, light/dark via `prefers-color-scheme`)

## Layout

```
website/
  index.html          # Landing
  download.html       # Platforms + GitHub Releases CTA
  docs.html           # Links into docs/user/ on GitHub
  about.html          # Project / license / stack
  styles.css
  CNAME               # regexcraft.com
  assets/
    favicon.png
    logo.png
    screenshots/      # Copies of docs/screenshots for deploy isolation
  README.md           # This file
```

## Deploy

GitHub Pages is deployed by **GitHub Actions** from this folder:

- Workflow: [`.github/workflows/pages.yml`](../.github/workflows/pages.yml)
- Trigger: push to `main` that touches `website/**`, or manual `workflow_dispatch`
- Artifact path: entire `website/` directory (including `CNAME`)

**Human setup** (Pages source + DNS) is documented in detail in:

**[docs/development/website.md](../docs/development/website.md)**

## Local preview

No server required for a quick look:

```bash
# From repo root
open website/index.html          # macOS
# or
python3 -m http.server 8080 --directory website
# then http://localhost:8080
```

## Updating content

| Task | What to edit |
|------|----------------|
| Copy / features | `index.html` |
| Platforms | `download.html` |
| Doc links | `docs.html` |
| Story / license | `about.html` |
| Colors / layout | `styles.css` |
| Screenshots | Replace files under `assets/screenshots/`; prefer regenerating from the app first (`dotnet test --filter Category=Screenshots`) and copying from `docs/screenshots/` |
| Favicon / logo | Copy from `src/RegexCraft.App/Assets/` |

After editing, commit and push to `main`. The Pages workflow publishes automatically once Pages is enabled with **Source: GitHub Actions**.
