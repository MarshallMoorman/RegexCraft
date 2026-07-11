# RegexCraft – Phase 4 Requirements

**Project**: RegexCraft  
**Version after this phase**: `0.5.0`  
**Depends on**: Phase 3 complete (`0.4.0`)  
**Current screenshot**: `docs/development/current_screenshot.png` (light mode, do not commit)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 4

Make the application **visually excellent and consistent** in both light and dark themes, fix the critical readability issues, and raise overall quality to near-1.0 level.

Primary focus: **Light theme polish + UX consistency + remaining high-value improvements**.

Debug remains deferred.

---

## 2. Critical Fixes from Current Screenshot (v0.4.0 Light Mode)

These are mandatory:

1. **Regex Editor text is not readable in light mode**  
   The pattern text has extremely poor contrast (almost invisible).  
   → Completely overhaul the light-theme syntax highlighting colors so that:
   - Base text is dark and highly readable
   - Groups, named groups, escapes, quantifiers, character classes, comments, etc. have distinct, high-contrast, professional colors
   - Both light and dark themes must look excellent side-by-side
   - Test with the exact pattern from the screenshot and several complex patterns

2. **Token groups on the left must all be the same width**  
   The category panels (Literals, Character Classes, Quantifiers, …) currently have inconsistent widths.  
   → Make every token category expander/panel the exact same width for a clean, professional look.  
   Prefer a fixed width or a consistent stretch that looks balanced.

3. Full light-theme audit  
   Walk every panel (Tokens, Library, History, Analysis Tree, Matches & Groups, Options bar, status bar, GREP if visible, Generate, etc.) and fix any low-contrast text, icons, borders, or backgrounds.

---

## 3. Feature & Polish Requirements

### 3.1 Theme System Hardening
- All colors still come from the variable ResourceDictionaries.
- Add or refine any missing semantic brushes (e.g. `EditorForeground`, `EditorBackground`, `SyntaxGroupBrush`, `SyntaxEscapeBrush`, `SyntaxQuantifierBrush`, `SyntaxClassBrush`, `MatchHighlightBrush`, etc.).
- Ensure DynamicResource is used everywhere so theme switching is instant and complete.
- Light theme must feel as premium as the dark theme.

### 3.2 Token Palette
- Consistent width for all category groups (critical).
- Improve visual density and spacing.
- Add more high-value tokens that are missing (especially common lookarounds, non-capturing groups, named group syntax variations, Unicode properties that both engines support, etc.).
- Keep text-only (no icons for tokens).
- Better search (fuzzy or multi-word if easy).

### 3.3 Editor & Analysis
- Syntax highlighting must be beautiful and readable in **both** themes (see critical fix).
- Analysis Tree further refinements if any nodes are still unclear.
- Click-to-select in editor should be smooth.

### 3.4 GREP Polish
- Review the GREP UI that was added in Phase 3.
- Ensure it looks perfect in light mode.
- Improve results presentation, progress feedback, and error handling if anything feels rough.
- Make sure large directory scans stay responsive.

### 3.5 Generate Panel
- Improve quality and completeness of generated snippets.
- Better comments and language-idiomatic style.
- Clear indication when a feature is engine-specific.

### 3.6 Library & History
- Polish the UI (search, empty states, visual consistency with token palette width).
- Ensure light theme looks good.

### 3.7 Options & Status
- Options row should be clean and not feel cramped.
- Status bar information should be clear in both themes.
- Keyboard shortcut hints remain visible.

### 3.8 General UX
- Consistent corner radii, padding, font sizes, and control heights across the whole app.
- Excellent empty states and error messages.
- Remember user preferences (theme, last folder for GREP, window size, etc.) more completely if not already done.
- Accessibility: good focus indicators, screen-reader friendly labels where reasonable.

---

## 4. Documentation & Project Hygiene

- Update root `README.md` if any new features or screenshots need mentioning (keep it timeless).
- Update user docs for any UX changes.
- Add a short “Theme & Appearance” note if useful.
- Update `docs/CHANGELOG.md` for 0.5.0.
- Keep all phase requirements in `docs/development/`.
- Do **not** commit `current_screenshot.png`.

---

## 5. Technical Requirements

- No new major architectural changes.
- Continue using existing engines and abstractions.
- All visual changes must be driven by theme resources (no hard-coded colors).
- Add or update NUnit tests only where logic changes (e.g. new tokens, GREP edge cases). Visual tests are not required.
- Serilog continues as before.
- Performance: no regressions; editor and live analysis must stay snappy.

---

## 6. Versioning & Process

- Bump version to **`0.5.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with exact Phase 5 (or final) next steps
- All tests green
- Clean commit on `main`:  
  `Phase 4 complete: light theme readability, consistent token widths, full visual polish (v0.5.0)`

---

## 7. Definition of Done

- [ ] Regex editor text is highly readable with excellent syntax highlighting in **light mode**
- [ ] Regex editor looks equally good in dark mode
- [ ] All token category panels have identical width
- [ ] Full light-theme audit completed — no low-contrast elements remain
- [ ] GREP, Generate, Library, History, Analysis Tree all look polished in light mode
- [ ] Overall spacing, alignment, and visual consistency are professional
- [ ] Version = 0.5.0
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] Documentation updated
- [ ] All tests pass
- [ ] Clean commit on main
- [ ] `current_screenshot.png` is **not** committed

---

## 8. Out of Scope for Phase 4

- Debug / step-through
- New regex engines
- Major new features (beyond polish of existing ones)
- Breaking UI changes

---

**This document is the single source of truth for Phase 4.**  
The light-mode screenshot is the primary baseline. The two critical visual bugs (unreadable editor text + inconsistent token widths) must be completely resolved, and the entire light theme must feel premium.