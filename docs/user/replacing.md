# Replacing Text with Regular Expressions

The **Replace** mode previews substitutions using the currently selected flavor/engine (.NET or PCRE2).

## Open Replace

- Click **Replace** in the toolbar (or right-panel tab), or press **Ctrl+2**.
- Enter a **replacement pattern** such as `[$1]`, `${user}`, or a literal string.
- The preview updates live while the Replace tab is active. Click **Preview Replace** or **Ctrl+Enter** to force a run.

## Backreferences

Supported forms (both engines):

| Form | Meaning |
|------|---------|
| `$1`, `$2`, … | Numbered capturing group |
| `${name}` | Named capturing group |
| `$&` | Entire match |
| `\1`, `\2`, … | Numbered group (especially useful with PCRE-style habits) |
| `$$` | Literal `$` |

Examples with the sample email pattern `(?<user>\w+)@(?<domain>\w+\.\w+)`:

- `[$1]` → `[support]` for `support@regexcraft.com`  
- `[${user} at ${domain}]` → `[support at regexcraft.com]`  

## Preview highlighting

Substituted regions in the result are highlighted using the same match-highlight theme brush, so you can see exactly what changed.

The status area reports **Replacements: N** and timing.

## Errors

If the pattern is invalid for the active engine, the error banner appears and the preview is cleared. Fix the pattern or switch flavor as needed.

## Split (related)

**Split** (Ctrl+3) divides the subject on each match of the pattern:

- Numbered parts list  
- Optional **Remove empty entries**  
- Delimiter matches can be highlighted when viewing the subject after a split run  

## Tips

- Switch .NET ↔ PCRE2 to compare replacement behavior.  
- Use **Generate → Replace** to emit the same pattern/replacement as code.  
- Save useful replace workflows to the **Library** (pattern + subject + replacement).  
