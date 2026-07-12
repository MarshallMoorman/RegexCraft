# Exporting match results

RegexCraft can export **Match** results from the **Test** panel (Matches & Groups) as **CSV** or **JSON**. There are no license keys involved — export is available in the same free personal / paid business build.

## When to use Export

- Share match samples with a teammate
- Feed results into a spreadsheet or script
- Archive how a pattern behaved against a subject snapshot
- Debug group captures offline

## How to export

1. Open **Test** mode (Ctrl+1).
2. Enter a pattern and subject so matches appear under **Matches & Groups**.
3. Use the export buttons next to **Run**:
   - **CSV** — save a spreadsheet-friendly file
   - **JSON** — save a structured file with metadata
   - **Copy JSON** — put the JSON document on the clipboard (no save dialog)

Suggested filenames look like `regexcraft-matches-YYYYMMDD-HHMMSS.csv` / `.json`.

## CSV format

Columns:

| Column | Meaning |
|--------|---------|
| `MatchIndex` | Zero-based match number |
| `Value` | Full match text |
| `Index` | Start offset in the subject |
| `Length` | Match length |
| `GroupN_Name` | Capture group name (or number) |
| `GroupN_Success` | `true` / `false` |
| `GroupN_Value` | Captured text |
| `GroupN_Index` | Group start offset |
| `GroupN_Length` | Group length |

Group columns are expanded for every group number **greater than 0** that appears in the result set. Group 0 (full match) is already represented by `Value` / `Index` / `Length`.

Values that contain commas, quotes, or newlines are CSV-escaped per usual rules.

## JSON format

The JSON document includes:

- `exportedAt` — UTC timestamp
- `pattern`, `subject`
- `flavorId`, `flavorDisplayName`
- `engineId`, `engineDisplayName`
- `success`, `errorMessage`, `durationMs`
- `options` — ignoreCase, multiline, singleline, explicitCapture, ignorePatternWhitespace
- `matches[]` — each with `matchIndex`, `value`, `index`, `length`, and `groups[]`

This is the preferred format for tools and scripts.

## Notes

- Export uses the **last successful Test run**. Fix pattern errors before exporting.
- Empty match lists still export (header-only CSV / empty `matches` array) after a successful run.
- GREP file hits and Library import/export are separate features and may arrive in a later release.
- Compare mode has its own **Copy summary** for a text report across flavors — that is not the same as match CSV/JSON export.

## Related

- [Testing regular expressions](testing-regexes.md)
- [Comparing flavors](comparing.md)
- [Getting started](getting-started.md)
