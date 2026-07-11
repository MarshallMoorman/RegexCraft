# Library and History

RegexCraft ships real, persistent **Library** and **History** stores so patterns survive app restarts. Preferences (theme, flavor, GREP paths, window size) live in a separate **settings** file.

## Where data lives

JSON files under the user application data folder:

| Platform | Typical path |
|----------|----------------|
| macOS | `~/Library/Application Support/RegexCraft/` |
| Windows | `%AppData%\RegexCraft\` |
| Linux | `~/.config/RegexCraft/` (or XDG config) |

Files:

- `library.json` — saved patterns **and built-in defaults**  
- `history.json` — recent patterns  
- `settings.json` — theme, flavor, options, GREP defaults, window bounds  

## Library

Open the **Library** tab on the left sidebar.

### Built-in patterns

RegexCraft includes a curated set of **built-in** regular expressions (email, URL, IPv4/IPv6, phone numbers, ISO/US/EU dates, time, hex color, UUID, credit card, strong password, HTML tags, whitespace runs, log levels, ISO datetime, slug, semver, and more).

- Shown with a **Built-in** badge  
- **Cannot be deleted** (delete control is hidden)  
- Pattern body is refreshed from the app on upgrade  
- You **can favorite** built-ins; favorites are preserved across restarts  
- **Load** works the same as for user entries  

### Save (user entries)

1. Set the pattern (and optional subject/replacement/options/flavor) in the main UI.  
2. Enter a **Name** and optional **Description**.  
3. Optionally set **Category**, **Tags** (comma-separated), and **Favorite**.  
4. Click **Save to Library**.  

Saved entries store:

- Name, description, category, tags, favorite flag  
- Pattern, subject, replacement  
- Flavor id  
- Regex options (ignore case, multiline, singleline, explicit capture, ignore pattern whitespace)  

### Load / search / delete / favorite

- Use the search box to filter by name, description, pattern, subject, category, tags, or “built-in”.  
- Favorites sort to the top of the list.  
- **★ / ☆** toggles favorite without re-entering the form.  
- **Load** restores the entry into the editor and switches to **Test**.  
- **✕** deletes **user** entries only.  

## History

Open the **History** tab.

- After successful tests (and when the pattern stabilizes during live use), RegexCraft records a history entry.  
- Entries are de-duplicated and capped (default ~40).  
- Use **Search history…** to filter by pattern, subject, replacement, or flavor id.  
- Click an entry to restore pattern (and subject/replacement/flavor when available).  
- **Clear** empties history.  

History does **not** store full option checkboxes (flavor + pattern + subject + replacement only). Use the Library for full snapshots.

## Tips

- Name library items by intent (`Email addresses`, `ISO dates`) rather than the raw pattern.  
- Start from a built-in, tweak, then **Save** as a new user entry.  
- Use categories/tags for large libraries (e.g. `Validation`, `Logs`).  
- Use History for quick undo of exploratory edits; use Library for keepers.  
- Library saves are logged via Serilog.  
