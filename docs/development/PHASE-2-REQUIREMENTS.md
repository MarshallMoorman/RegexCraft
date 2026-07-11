# RegexCraft – Phase 2 Requirements

**Project**: RegexCraft  
**Version after this phase**: `0.3.0`  
**Depends on**: Phase 1 complete (`0.2.0`) – current screenshot state  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 2

Take the solid Phase 1 foundation (visible in the provided screenshot) and turn it into a polished, fully useful tool for the three highest-priority workflows:

1. **Test** (already good – make it excellent)
2. **Replace** (make fully functional with live preview + highlighting)
3. **Generate** (code snippets)

Also fix every visible issue from the current screenshot and significantly improve polish, Analysis Tree, and overall UX.

At the end of Phase 2 the application should feel production-ready for daily Test / Replace / Generate work.

---

## 2. Analysis of Current Screenshot (v0.2.0) – Issues to Fix

The current UI is already very good. These are the concrete problems that must be fixed in Phase 2:

### Critical / High Priority Fixes
- **Window title** still shows “Avalonia Application” → must be “RegexCraft”
- **Analysis Tree** is extremely shallow (only shows “Sequence 3 parts …”). It needs to be a rich, expandable, useful hierarchical breakdown of the regex.
- **Replace** and **Split** tabs exist but are incomplete / stubs → make them fully working.
- **Syntax highlighting** in the editor needs to be more visible and professional (groups, named groups, escapes, quantifiers, etc. should stand out clearly in both light and dark themes).
- Token insertion must be reliable (clicking a token inserts at caret / replaces selection and keeps focus).
- Options row feels a bit cramped and the checkboxes should be more clearly associated with the current flavor/engine.
- Status bar and overall spacing/padding need refinement for a more premium feel.
- Library and History are still “Phase 2+” placeholders → implement basic versions.

### Medium Priority Polish
- Better visual hierarchy and spacing throughout.
- Improve the Matches & Groups panel (make group badges clearer, add copy buttons, better expansion).
- Make the top toolbar cleaner (Match/Replace/Split should clearly indicate the current mode).
- Ensure light theme is equally polished (screenshot is dark).
- Add keyboard shortcuts for common actions (Run test, switch modes, etc.).
- Better empty states and error states.

---

## 3. Feature Requirements for Phase 2

### 3.1 Window & Chrome
- Set correct window title: `RegexCraft`
- Version badge remains.
- Theme switcher remains (System / Light / Dark).

### 3.2 Analysis Tree (Major Upgrade)
- Real hierarchical AST-style tree.
- Expandable nodes for:
  - Groups (capturing / non-capturing / named)
  - Alternations
  - Quantifiers
  - Lookarounds
  - Character classes
  - Literals / escapes
- Each node should show a short human-readable description.
- Live updates (debounced) as the user types.
- Clicking a node in the tree should optionally highlight the corresponding part of the regex in the editor (nice-to-have but highly desired).
- Graceful degradation on invalid / incomplete regexes.

### 3.3 Replace Panel (Full Implementation)
- Replacement pattern text box (with its own simple syntax highlighting if possible).
- Live preview (or explicit Run) of the replacement result.
- Highlight the changed parts in the result.
- Support backreferences (`$1`, `${name}`, `\1`, etc.) correctly for both engines.
- Show number of replacements performed.
- Error handling for invalid replacement patterns.

### 3.4 Split Panel (Full Implementation)
- Show the split results as a clean list or numbered parts.
- Highlight the split points in the original subject if possible.
- Options for removing empty entries, etc. (basic is fine).

### 3.5 Code Generation (“Generate”)
- New top-level mode or dedicated panel/tab: **Generate**
- Select language (start with: C#, JavaScript, Python, PHP, Java, Go, Rust – easy to extend)
- Generate clean, correct, idiomatic code snippets for:
  - Match / IsMatch
  - Matches (all)
  - Replace
  - Split
- Include proper using/import statements and common options.
- One-click Copy button.
- Code should respect the currently selected flavor/engine as much as possible.
- Preview the generated code in a nice read-only editor (AvaloniaEdit or TextBox with monospace).

### 3.6 Token Palette Improvements
- Ensure every token inserts correctly and restores focus to the editor.
- Add more useful tokens (especially common named groups, non-capturing groups, lookarounds, Unicode categories that both engines support).
- Better visual indication of which tokens are supported by the current engine (if any difference).
- Keep it text-based (no icons for individual tokens).

### 3.7 Library & History (Basic but Real)
- **History**: Automatically keep the last N (e.g. 20–50) regexes the user has tested. Click to restore.
- **Library**: Allow user to save the current regex + description + test subject into a simple local store (JSON or SQLite). List, search, load, delete.
- Persist across app restarts.
- Both should live in the left sidebar as currently sketched.

### 3.8 Options & Engine Handling
- Options row should clearly apply to the current engine.
- When switching flavor/engine, re-run the current Test/Replace/Split automatically.
- Disable or hide options that the current engine does not support.
- Make the options more compact and professional.

### 3.9 Highlighting & Matches Panel
- Keep the excellent green match highlighting.
- Improve the Matches & Groups cards:
  - Clearer group badges
  - Click a group to highlight just that capture in the subject
  - Copy match / copy group value buttons
  - Better expansion/collapse

### 3.10 Overall Polish
- Consistent spacing, padding, corner radii, and visual weight.
- Professional empty states (“No matches”, “Enter a regex…”, etc.).
- Loading / “Running…” indicators for longer operations (even if rare).
- Keyboard shortcuts (at least Ctrl+Enter = Run, Ctrl+1/2/3 for modes).
- Ensure both light and dark themes look excellent.

---

## 4. Technical Requirements

- Continue using the existing `IRegexEngine` abstraction.
- All new features (Replace, Split, Generate, Library, History, richer Analysis) must have solid NUnit tests.
- Generated code must be correct for the target language + current engine semantics as much as possible.
- Library/History persistence must be simple, reliable, and fast (prefer JSON files in a user data folder or a lightweight SQLite).
- Logging: continue Serilog. Log generation requests, library saves, etc.
- Theme: still 100% variable-driven blue. No hard-coded colors.
- Performance: Analysis Tree and live Test/Replace must stay snappy (debounce where needed).

---

## 5. Documentation Requirements

- Update `docs/user/testing-regexes.md` with any new Test features.
- Create `docs/user/replacing.md`
- Create `docs/user/generating-code.md`
- Create `docs/user/library-and-history.md`
- Update `docs/development/architecture.md`
- Update `docs/CHANGELOG.md` for 0.3.0
- Keep all phase requirements under `docs/development/`

---

## 6. Versioning & Process

- Bump version to **`0.3.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with exact Phase 3 next steps
- All tests green
- Clean commit on `main`:  
  `Phase 2 complete: polished UI, full Replace/Split, code generation, Library/History, rich Analysis Tree (v0.3.0)`

---

## 7. Definition of Done

- [ ] Window title is “RegexCraft”
- [ ] Analysis Tree is rich, expandable, and actually useful
- [ ] Replace panel fully functional with preview + highlighting + backreferences
- [ ] Split panel fully functional
- [ ] Code Generation panel working for at least 5–6 major languages
- [ ] Library and History are real (persist, load, save)
- [ ] Token insertion is reliable
- [ ] Syntax highlighting is clearly visible and professional
- [ ] Matches & Groups panel improved (copy, better groups)
- [ ] Overall polish (spacing, empty states, light+dark) is excellent
- [ ] All NUnit tests pass
- [ ] User documentation for the new features written
- [ ] Version = 0.3.0
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] Clean commit on main

---

## 8. Out of Scope for Phase 2

- GREP / file search
- Debug / step-through
- Additional engines beyond .NET + PCRE2
- Advanced token dialogs (wizards)
- Cloud sync of library
- Plugin system

These come later.

---

**This document is the single source of truth for Phase 2.**  
The current screenshot is the baseline. Every listed issue must be resolved and the three core workflows (Test / Replace / Generate) must feel excellent.