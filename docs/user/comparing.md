# Comparing regexes across flavors

**Compare** mode runs the same pattern and subject on multiple flavors at once so you can see validity, match results, fidelity, and key dialect differences side-by-side.

## Open Compare

- Toolbar or right panel tab: **Compare**
- Keyboard: **Ctrl+6** (⌘+6 on macOS)
- **Ctrl+Enter** re-runs the comparison

## Select flavors (2–4)

Use the checkboxes at the top of the Compare panel. RegexCraft defaults to a useful set (for example .NET, PCRE2, JavaScript, and another flavor). You can change the selection at any time:

- **Minimum**: 2 flavors  
- **Maximum**: 4 flavors  

Comparisons re-run automatically when you change the pattern, subject, options, or flavor selection (with the same live debounce as Test mode).

## What each card shows

For every selected flavor:

| Field | Meaning |
|-------|---------|
| **Header** | Flavor name, testing engine, fidelity badge (Full / High / Approximate) |
| **Valid / Invalid** | Whether the pattern compiles on that flavor’s engine |
| **Match count & timing** | How many matches and how long Match took |
| **First matches** | Sample match lines with group summaries |
| **Key notes** | Dropped options, unsupported tokens detected in the pattern, fidelity notes, known differences |

A summary strip above the cards lists **cross-flavor differences** (validity splits, match-count diffs, first-match offsets, option drops, unsupported constructs, fidelity mix).

## Copy summary

**Copy summary** places a plain-text report on the clipboard: pattern, subject, per-flavor results, and cross-flavor notes. Useful for bug reports or sharing with teammates.

## Options and fidelity

Compare uses the **toolbar option checkboxes** as the *requested* options, then applies each flavor’s support matrix (`SupportedOptions`). If JavaScript drops ExplicitCapture or free-spacing, that appears under **Dropped options** for that card.

Approximate flavors (Python, Go, Rust, …) still run on the closest real engine (.NET or PCRE2). Cards show the fidelity badge and note so you never confuse approximate testing with native dialect behavior. See [Flavors & testing fidelity](flavors.md).

## Tips

- Keep the subject short while exploring differences; Compare is optimized for interactive patterns, not multi-megabyte subjects.  
- Use Test mode for deep highlighting of a single flavor; use Compare when you care about portability.  
- If every selected flavor is invalid, fix the pattern syntax first (unclosed groups, etc.).  
- Token palette dimming in the left sidebar remains driven by the **main** flavor dropdown; Compare cards call out unsupported constructs independently.  
