#!/usr/bin/env bash
# Build the public static site: website/ + docs/user only (no docs/development).
# Output: site-dist/
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/site-dist"
WEB="$ROOT/website"
USER_DOCS="$ROOT/docs/user"
TEMPLATE="$ROOT/scripts/site-doc-template.html"

rm -rf "$OUT"
mkdir -p "$OUT"

# Copy marketing site (HTML/CSS/assets/CNAME)
cp -R "$WEB"/. "$OUT"/

# Remove anything that should not be public from website tree (none expected)
rm -f "$OUT/README.md" 2>/dev/null || true

mkdir -p "$OUT/docs"

if ! command -v pandoc >/dev/null 2>&1; then
  echo "error: pandoc is required to convert user docs (install pandoc)" >&2
  exit 1
fi

if [ ! -f "$TEMPLATE" ]; then
  echo "error: missing $TEMPLATE" >&2
  exit 1
fi

# Convert each user markdown guide to HTML under site-dist/docs/
shopt -s nullglob
for md in "$USER_DOCS"/*.md; do
  base="$(basename "$md" .md)"
  # Skip the index README — we generate docs.html from website and a docs index
  if [ "$base" = "README" ]; then
    continue
  fi
  title="$(grep -m1 '^# ' "$md" | sed 's/^# //' || echo "$base")"
  out_html="$OUT/docs/${base}.html"
  pandoc "$md" \
    -f gfm \
    -t html5 \
    --standalone \
    --template="$TEMPLATE" \
    -V "title=${title}" \
    -V "pagetitle=${title} — RegexCraft Docs" \
    -o "$out_html"
  echo "  docs/${base}.html"
done

# Lightweight docs index pages that link to local HTML (also keep website/docs.html as hub)
echo "Built public site at $OUT"
