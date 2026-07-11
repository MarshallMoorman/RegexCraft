# Testing Regular Expressions

Phase 1 turns RegexCraft into a practical multi-engine test bench. This guide covers the **Test** panel, highlighting, groups, Replace preview, and engine switching.

## Layout

| Area | Purpose |
|------|---------|
| **Toolbar** | Flavor selector, Match / Replace / Options, theme |
| **Tokens** (left) | Searchable text-only palette — click to insert at the caret |
| **Regex Editor** (center) | AvaloniaEdit with blue regex syntax highlighting |
| **Analysis Tree** (center bottom) | Live hierarchical explanation of the pattern |
| **Test / Replace** (right) | Subject, match highlighting, groups, replace preview |
| **Status bar** | Flavor, engine, match count, timing |

## Match testing

1. Choose a **flavor** (.NET or PCRE2) in the toolbar.
2. Enter a pattern in the **Regex Editor** (or click tokens from the left).
3. Enter or edit the **Subject** text on the right.
4. Results update **live** (debounced) as you type. Click **Match** or **Run** to force an immediate run.

### Highlighting

Matches are painted in the subject editor:

- **Full match** — yellow/gold style (`MatchHighlight`)
- **Groups** — rotating blues/greens/warm accents (`GroupHighlight0`–`3`)

These colors come from the theme dictionaries and work in light and dark mode.

### Groups list

Each match expands to show capturing groups:

- Numbered groups (`Group 1`, `Group 2`, …)
- Named groups (`Group 1 / user`)
- Index + length for each successful capture
- Unsuccessful groups appear as `(no match)`

## Switching engines

Change the flavor dropdown between **.NET** and **PCRE2**. RegexCraft re-runs the current test automatically so you can compare behavior side by side.

The status bar shows:

```
Flavor: .NET | Engine: .NET    Matches: 2    Time: 0.35 ms
```

## Invalid patterns

If the pattern cannot be compiled by the active engine, an error banner appears on the right and highlights are cleared. The Analysis Tree may still show a partial structure for incomplete syntax (for example an unclosed `(`).

## Replace preview

1. Open the **Replace** tab (toolbar or right panel).
2. Enter a replacement string (`$1`, `${name}`, etc. depending on the engine).
3. Click **Preview Replace** (or rely on live updates when the Replace tab is active).
4. Inspect the preview text and replacement count.

## Options

Open **Options** in the toolbar:

- Ignore case  
- Multiline  
- Singleline  
- Explicit capture  
- Ignore pattern whitespace  

Options apply to both Match and Replace for the selected engine.

## Tips

- Use the token search box (`lookahead`, `\d`, `group`, …) to find constructs quickly.
- Token tooltips show a short description and example.
- Library and History placeholders are reserved for Phase 2 — they do not persist yet.
- Split is intentionally disabled in Phase 1.
