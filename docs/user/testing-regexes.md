# Testing Regular Expressions

This guide covers the **Test** panel, highlighting, groups, engines, and options. Replace, Split, Generate, and GREP share the same right-hand panel host.

## Layout

| Area | Purpose |
|------|---------|
| **Toolbar** | Flavor, Match / Replace / Split / Generate / GREP modes, Options, theme |
| **Tokens / Library / History** (left) | Insert constructs, save/load patterns, searchable history |
| **Regex Editor** (center) | AvaloniaEdit with professional regex syntax highlighting |
| **Analysis Tree** (center bottom) | Live hierarchical explanation — click a node to select it in the editor |
| **Test / Replace / Split / Generate / GREP** (right) | Subject, match highlighting, groups, replace/split/codegen/file search — panels fill available space |
| **Status bar** | Flavor, engine, counts, timing, shortcut hints |
| **Splitters** | Drag to resize left sidebar, center, and right panel |

## Match testing

1. Choose a **flavor** in the toolbar (.NET, PCRE2, JavaScript, Python, PHP, …). See [Flavors & testing fidelity](flavors.md).
2. If testing is approximate, read the blue **fidelity banner** so you know which engine is actually running.
3. Enter a pattern in the **Regex Editor** (or click tokens from the left).
4. Enter or edit the **Subject** text on the right (**Test** tab).
5. Results update **live** (debounced) as you type. Click **Run** or press **Ctrl+Enter** for an immediate run.

### Highlighting

Matches are painted in the subject editor:

- **Full match** — yellow/gold style (`MatchHighlight`)
- **Groups** — rotating blues/greens/warm accents (`GroupHighlight0`–`3`)

These colors come from the theme dictionaries and work in light and dark mode.

### Groups list

Each match expands to show capturing groups:

- Numbered groups (`G1`, `G2`, …)
- Named groups (`G1/user`)
- Index + length for each successful capture
- **Copy** — copy the match or group value to the clipboard  
- **Go** — select that range in the subject editor  

Empty state: when there are no matches, a short message explains why (no matches vs pattern error).

## Analysis Tree

The analysis tree is a live structural breakdown of the pattern:

- Sequences, alternations, quantifiers (with greedy/lazy/possessive notes)
- Capturing, named, non-capturing, atomic groups
- Lookaheads / lookbehinds
- Character classes, escapes, anchors, Unicode properties
- Incomplete patterns degrade gracefully with error nodes

Click a node to select the corresponding fragment in the Regex Editor.

## Switching engines

Change the flavor dropdown between **.NET** and **PCRE2**. RegexCraft re-runs the current Test/Replace/Split automatically.

The status bar shows:

```
Flavor: .NET | Engine: .NET    Matches: 2    Time: 0.35 ms
```

## Invalid patterns

If the pattern cannot be compiled by the active engine, an error banner appears on the right and highlights are cleared. The Analysis Tree may still show a partial structure for incomplete syntax (for example an unclosed `(`).

## Options

Open **Options** in the toolbar (expanded by default in Phase 2):

- Ignore case  
- Multiline  
- Singleline  
- Explicit capture  
- Ignore pattern whitespace  

The options strip labels which flavor/engine the options apply to. Changing any option re-runs the active mode.

## Tokens

- Search by label, insert text, or category  
- Click inserts at the caret (or replaces the selection) and restores focus to the editor  
- Tokens with limited engine support appear slightly dimmed with a tooltip note  

## Related

- [Replacing](replacing.md)  
- [Generating code](generating-code.md)  
- [Library and History](library-and-history.md)  
