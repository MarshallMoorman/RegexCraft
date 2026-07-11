# Library and History

Phase 2 ships real, persistent **Library** and **History** stores so patterns survive app restarts.

## Where data lives

JSON files under the user application data folder:

| Platform | Typical path |
|----------|----------------|
| macOS | `~/Library/Application Support/RegexCraft/` |
| Windows | `%AppData%\RegexCraft\` |
| Linux | `~/.config/RegexCraft/` (or XDG config) |

Files:

- `library.json` — saved patterns  
- `history.json` — recent patterns  

## Library

Open the **Library** tab on the left sidebar.

### Save

1. Set the pattern (and optional subject/replacement/options/flavor) in the main UI.  
2. Enter a **Name** and optional **Description**.  
3. Click **Save to Library**.  

Saved entries store:

- Name, description  
- Pattern, subject, replacement  
- Flavor id  
- Regex options (ignore case, multiline, singleline, explicit capture, ignore pattern whitespace)  

### Load / search / delete

- Use the search box to filter by name, description, pattern, or subject.  
- **Load** restores the entry into the editor and switches to **Test**.  
- **✕** deletes the entry permanently.  

## History

Open the **History** tab.

- After successful tests (and when the pattern stabilizes during live use), RegexCraft records a history entry.  
- Entries are de-duplicated and capped (default ~40).  
- Click an entry to restore pattern (and subject/replacement/flavor when available).  
- **Clear** empties history.  

History does **not** store full option checkboxes (flavor + pattern + subject + replacement only). Use the Library for full snapshots.

## Tips

- Name library items by intent (`Email addresses`, `ISO dates`) rather than the raw pattern.  
- Use History for quick undo of exploratory edits; use Library for keepers.  
- Library saves are logged via Serilog.  
