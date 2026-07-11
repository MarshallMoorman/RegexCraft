# RegexCraft – Phase 1 Requirements

**Project**: RegexCraft  
**Version after this phase**: `0.2.0`  
**Depends on**: Phase 0 complete (`0.1.0`)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 1

Deliver the **highest-priority features** with excellent quality:

1. Professional multi-panel Avalonia UI shell (the real layout)
2. Full regex editor (AvaloniaEdit) with blue syntax highlighting
3. Text-based searchable **Token palette** (no icons for tokens)
4. Live Analysis Tree
5. Complete **Test** panel with beautiful match highlighting + group details
6. Both engines (.NET + PCRE2) fully supported for Test
7. All new features covered by NUnit tests
8. User documentation for the new testing experience
9. Move the original Phase 0 planning files into `docs/development/`

This phase turns the foundation into a genuinely useful regex testing tool.

---

## 2. File Organization Decision (Important)

**Going forward (including this phase):**

- Living documents that every agent needs immediately stay at **repository root**:
  - `AGENTS.md`
  - `HANDOFF.md`
  - `README.md` (the real project one)
  - `Directory.Build.props`, `LICENSE`, etc.

- Phase requirements and historical planning documents go into:
  ```
  docs/development/
  ├── PHASE-0-REQUIREMENTS.md          ← move the original here
  ├── PHASE-1-REQUIREMENTS.md          ← this file
  ├── PHASE-2-REQUIREMENTS.md          ← future
  └── ...
  ```

- Any temporary “package README” or Grok prompt files should be placed in `docs/development/` as well (or deleted after use). Never overwrite the root `README.md`.

**Action required in this phase**:  
Move the original Phase 0 files (`PHASE-0-REQUIREMENTS.md`, `GROK-BUILD-PROMPT-PHASE-0.md`, etc.) that currently sit in the root into `docs/development/`. Keep only the living root files.

---

## 3. UI Layout Requirements (Must Match Vision)

Implement a clean, modern, professional multi-panel layout:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ RegexCraft          [.NET ▼]   [Match] [Replace] [Split]   [Options] [Copy] │
├──────────┬──────────────────────────────────────────────┬───────────────────┤
│ Tokens   │  Regex Editor (AvaloniaEdit)                 │  Test             │
│ (search) │                                              │  ───────────────  │
│          │  ──────────────────────────────────────────  │  Subject          │
│ text     │  Analysis Tree (live)                        │  Results + Groups │
│ list     │                                              │  Highlighted      │
│ + cats   │                                              │                   │
│          │                                              │                   │
│ Library  │                                              │                   │
│ History  │                                              │                   │
└──────────┴──────────────────────────────────────────────┴───────────────────┘
│ Flavor: .NET | Engine: Native | Matches: 3 | Time: 0.4 ms                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

- Left sidebar: Tokens (searchable text list + categories) + Library/History placeholders
- Center: Regex editor on top, Analysis Tree below (resizable)
- Right: Tabbed panel starting with **Test** (Replace and Split can be stubs or basic for now)
- All colors from the blue theme variables
- Excellent spacing, tooltips on all toolbar actions, professional look
- Light / Dark mode fully working

---

## 4. Feature Requirements (Priority Order)

### 4.1 AvaloniaEdit Regex Editor
- Use AvaloniaEdit
- Custom blue syntax highlighting for regular expressions (groups, escapes, character classes, quantifiers, etc.)
- Line numbers, current line highlight, basic editing features
- Two-way bound to ViewModel

### 4.2 Text-Based Token Palette (Critical)
- **No icons for individual tokens** (as decided)
- Searchable list
- Grouped by categories (Literals, Character Classes, Quantifiers, Groups, Lookarounds, Anchors, Unicode, Mode Modifiers, etc.)
- Clicking a token inserts the corresponding text into the editor at the caret (or replaces selection)
- Tooltips with short description + example
- Must work for both engines (tokens that are engine-specific should be marked or filtered later)

### 4.3 Live Analysis Tree
- Parse the current regex into a hierarchical explanation
- Update live as the user types (debounced)
- Show structure clearly (groups, alternations, quantifiers, etc.)
- Graceful handling of incomplete/invalid regexes

### 4.4 Test Panel (Highest Priority Feature)
- Subject text box (multi-line)
- “Run” / live testing (prefer live with debounce, plus explicit Run button)
- **Beautiful match highlighting** in the subject text
- Results list/table showing:
  - Match number
  - Index / Length
  - Full match value
  - Expandable groups (numbered + named)
- Support for both .NET and PCRE2 engines
- Clear error display when the pattern is invalid
- Performance timing shown in status bar

### 4.5 Replace Panel (Basic for now)
- Replacement text box
- Live or button-triggered replace preview
- Highlighted result
- Must work with both engines

### 4.6 Engine / Flavor Selector
- Dropdown that switches between .NET and PCRE2
- Changing it re-runs the current test automatically
- Status bar clearly shows which engine is active

---

## 5. Technical Requirements

- Continue using the existing `IRegexEngine` abstraction and result models from Phase 0.
- All new ViewModels must be properly testable.
- Use CommunityToolkit.Mvvm.
- Logging: continue using the Serilog setup. Log editor actions, test runs, errors at appropriate levels.
- Theme: continue using only the variable blue resources. No hard-coded colors.
- NUnit tests must cover:
  - Token insertion logic
  - Analysis tree generation (at least basic cases)
  - Match result mapping and highlighting data
  - Engine switching
  - Edge cases for both engines

---

## 6. Documentation Requirements

- Update `docs/user/getting-started.md`
- Create `docs/user/testing-regexes.md` (how to use the Test panel, highlighting, groups, switching engines)
- Update `docs/development/architecture.md` with the new UI and editor details
- Update `docs/CHANGELOG.md` for 0.2.0
- Move all Phase 0 planning files into `docs/development/`

---

## 7. Versioning & Process

- Bump version to **`0.2.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with exact Phase 2 next steps
- All tests green
- Clean commit on `main`:  
  `Phase 1 complete: editor, token palette, analysis tree, excellent Test panel with highlighting (v0.2.0)`

---

## 8. Definition of Done

- [ ] Original Phase 0 planning files moved to `docs/development/`
- [ ] Full multi-panel professional UI implemented and looking beautiful in light + dark
- [ ] AvaloniaEdit editor with blue regex syntax highlighting working
- [ ] Text-based searchable Token palette working (inserts correctly)
- [ ] Live Analysis Tree working
- [ ] Test panel with excellent match highlighting + group details for **both** engines
- [ ] Basic Replace preview working
- [ ] Engine selector switches correctly and re-tests
- [ ] All NUnit tests pass
- [ ] User docs for testing written
- [ ] Version = 0.2.0
- [ ] AGENTS.md and HANDOFF.md updated
- [ ] Clean commit on main

---

## 9. Out of Scope for Phase 1

- Full GREP
- Code generation UI
- Library / History persistence (placeholders only)
- Debug stepping
- More engines
- Advanced token dialogs (simple insert is enough for now)
- Split panel (can be a stub)

---

**This document is the single source of truth for Phase 1.**  
Implement exactly what is specified. When finished, the application should already feel like a real, high-quality regex testing tool.