# GREP — search & replace across files

RegexCraft **GREP** applies the current pattern, engine, and options to files under a folder. Use it to find matches in a codebase or to replace them safely with dry-run and optional backups.

## Open GREP

- Toolbar **GREP** button, or right-panel **GREP** tab  
- Keyboard: **Ctrl+5** (⌘+5 on macOS)  
- **Ctrl+Enter** runs **Search** while GREP is active  

GREP uses the same **Flavor** (.NET / PCRE2), **pattern**, **replacement string**, and **Options** as the rest of the app.

## Search

1. Enter or paste a folder path, or click **Browse…**  
2. Adjust **Include** globs (semicolon-separated), e.g. `*.cs;*.json;*.md`  
3. Adjust **Exclude** globs, e.g. `bin/**;obj/**;.git/**;node_modules/**`  
4. Toggle **Recursive** if you want subfolders  
5. Click **Search** (or Ctrl+Enter)  

Results show **file name : line number** and a truncated line. Select a result to open a **preview** of the file with match highlighting. Use **Test** on a hit to load that line into Match mode.

### Progress & cancel

Search runs **asynchronously** so the UI stays responsive. Progress text shows the current file and hit counts. Click **Cancel** to stop; partial results are kept when cancelled.

### Limits

- Default max file size ~2 MB (larger files are skipped)  
- Binary-looking files (NUL bytes) are skipped  
- Match and file counts are capped to protect memory on huge trees  

## Replace across files

1. Set the **Replacement** field (same as Replace mode: `$1`, `${name}`, `$&`, …)  
2. Prefer **Dry-run replace** first — previews which files would change **without writing**  
3. Uncheck **Dry-run** only when ready to write  
4. Optionally keep **Backup (.bak)** so each modified file is copied to `filename.bak` first  
5. Click **Replace**  

After a successful write, GREP re-runs Search so the list stays current.

> **Warning:** Turning off dry-run writes to disk. Use backups or version control for safety.

## Tips

- Start with a tight include list (`*.cs`) to keep scans fast  
- Exclude build artifacts and VCS folders by default  
- Engine choice matters for advanced constructs (lookbehind, named groups, etc.)  
- Invalid patterns surface as errors before scanning begins  

## Related

- [Testing regexes](testing-regexes.md)  
- [Replacing](replacing.md)  
- [Getting started](getting-started.md)  
