# RegexCraft – Phase 3 Requirements

**Project**: RegexCraft  
**Version after this phase**: `0.4.0`  
**Depends on**: Phase 2 complete (`0.3.0`)  
**Current screenshot**: `docs/development/current_screenshot.png` (do not commit this file)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 3

Deliver the next major capability (**GREP** – search & replace across files/folders) while doing serious polish and fixing the remaining issues visible in the v0.3.0 screenshot.

At the end of Phase 3 the application should be extremely useful for day-to-day regex work and feel like a polished 1.0-ready product in most areas (Debug still deferred).

---

## 2. Critical Fixes from Current Screenshot (v0.3.0)

These **must** be fixed:

1. **Window title**  
   The macOS (and Windows/Linux) window title still shows **“Avalonia Application”**.  
   It must be set to **“RegexCraft”** (and ideally show the version or current mode in the title if easy).  
   This was required in Phase 2 and is still broken — highest priority fix.

2. **Root README.md**  
   Rewrite the root `README.md` so it is a complete, timeless project README (not “Features (Phase 2)”).  
   It should look professional for GitHub visitors and for the eventual regexcraft.com landing.  
   Include badges if desired, clear feature list (without phase numbers), screenshots section (or placeholder), build instructions, engines table, license, etc.

3. Any remaining visual/UX issues visible in `docs/development/current_screenshot.png`.

---

## 3. Major New Feature: GREP

Implement a solid **GREP** experience (search and optionally replace across files and folders).

### Requirements
- New top-level mode or dedicated panel/tab: **GREP**
- Folder picker + recursive option
- File include/exclude patterns (e.g. `*.cs;*.json`, exclude `bin/**;obj/**`)
- Use the current regex + current engine + current options
- Results list with:
  - File path
  - Line number
  - Matching line (with match highlighting)
  - Ability to click a result to open/preview the file or jump to the match
- Preview pane for the selected file with highlighted matches
- **Replace across files** (with confirmation and backup option or dry-run)
- Progress indicator and cancellation support
- Respect .gitignore-style excludes if easy, otherwise simple glob excludes
- Performance: should handle reasonably large codebases without freezing the UI (use async + background work)

This is the biggest new feature of Phase 3.

---

## 4. Other Feature & Polish Requirements

### 4.1 Analysis Tree & Editor
- Further improve the Analysis Tree if any gaps remain (deeper explanations, better click-to-select).
- Ensure syntax highlighting is excellent in both themes.
- “Copy pattern” button already exists — make sure it is reliable.

### 4.2 Generate Panel
- Expand language support if needed.
- Make generated code higher quality (better comments, more idiomatic options, correct flavor notes).
- Add “Copy” and “Copy & Close” or similar UX improvements.

### 4.3 Options & Multi-flavor
- Make options fully engine-aware (disable unsupported options, show tooltips).
- When switching engines, clearly indicate any behavioral differences if possible.
- Status bar should always show current engine clearly.

### 4.4 Library & History
- Improve UX (search, tags/categories for Library, pin favorites, etc.).
- Ensure persistence is robust.

### 4.5 General Polish
- Consistent spacing, visual density, and professional feel in **both** light and dark themes.
- Better empty states, error messages, and loading indicators.
- Keyboard shortcuts expanded and documented.
- High-DPI / multi-monitor friendly.
- Remember window size/position and panel layout if reasonable.

### 4.6 Documentation
- Update all user docs for GREP.
- Create `docs/user/grepping.md`
- Make the root README excellent (see critical fix #2).
- Update `docs/CHANGELOG.md` and architecture docs.
- Keep phase requirements under `docs/development/`.

---

## 5. Technical Requirements

- GREP must use the existing `IRegexEngine` abstraction (so both .NET and PCRE2 work).
- All heavy work (file scanning, replace) must be async and cancellable. UI must stay responsive.
- Solid NUnit tests for GREP core logic (file matching, result models, replace dry-run, etc.).  
  Full end-to-end file system tests can be limited but core algorithms must be tested.
- Continue Serilog logging (log GREP operations, files scanned, replacements made, errors).
- Theme remains 100% variable-driven blue.
- Do **not** commit `docs/development/current_screenshot.png`.

---

## 6. Versioning & Process

- Bump version to **`0.4.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with exact Phase 4 next steps
- All tests green
- Clean commit on `main`:  
  `Phase 3 complete: GREP, window title fix, polished README, further UX improvements (v0.4.0)`

---

## 7. Definition of Done

- [ ] Window title is exactly “RegexCraft” (no more “Avalonia Application”)
- [ ] Root `README.md` is complete, professional, and timeless (no “Phase X” language)
- [ ] GREP is fully implemented (search + replace across files/folders) and works with both engines
- [ ] GREP has progress, cancellation, preview, and good results UI
- [ ] Analysis Tree, Generate, Library, History, and Options further polished
- [ ] Both light and dark themes look excellent
- [ ] All NUnit tests pass (including new GREP tests)
- [ ] User documentation for GREP written
- [ ] Version = 0.4.0
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] Clean commit on main
- [ ] `current_screenshot.png` is **not** committed

---

## 8. Out of Scope for Phase 3

- Debug / step-through debugger
- Additional regex engines beyond .NET + PCRE2
- Cloud features
- Plugin system
- Advanced visual regex builder

---

**This document is the single source of truth for Phase 3.**  
The provided screenshot (`docs/development/current_screenshot.png`) is the visual baseline. Fix the window title first, deliver a great GREP experience, and leave the app looking and feeling premium.