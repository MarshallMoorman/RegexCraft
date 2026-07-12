# RegexCraft public website (source)

Marketing site for **https://regexcraft.com**. Built with plain HTML/CSS (no SPA framework).

## Structure

| File | Role |
|------|------|
| `index.html` | Landing |
| `download.html` | Public dist download links |
| `pricing.html` | Personal free / business paid |
| `eula.html` | End user license |
| `docs.html` | User docs index |
| `about.html` | About / stack / license summary |
| `site-config.js` | Buy URL / version placeholders |
| `styles.css` | Blue professional theme |
| `CNAME` | `regexcraft.com` |
| `assets/` | Favicon, logo, screenshots |

User guide HTML under `docs/` is **generated** from `docs/user/*.md` by `scripts/build-site.sh` (pandoc) and is not committed as HTML.

## Local preview

```bash
# Marketing pages only
python3 -m http.server 8080 --directory website

# Full public build (marketing + user docs)
bash scripts/build-site.sh
python3 -m http.server 8080 --directory site-dist
```

## Deploy

GitHub Action **Deploy website** builds `site-dist` and pushes to the public dist repo `gh-pages` branch.  
See `docs/development/website.md` and `docs/development/commercial.md`.

**Never** include `docs/development/` in the public site.
