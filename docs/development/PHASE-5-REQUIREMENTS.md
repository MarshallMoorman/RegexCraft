# RegexCraft – Phase 5 Requirements (Final Polish & 1.0 Readiness)

**Project**: RegexCraft  
**Version after this phase**: `0.6.0` (or `1.0.0-rc1` if you prefer — decide in HANDOFF)  
**Depends on**: Phase 4 complete (`0.5.0`)  
**Current screenshot**: `docs/development/current_screenshot.png` (Replace tab, light mode)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 5

Bring RegexCraft to a high-quality, polished, release-candidate state.

Focus areas:
- Fix remaining layout issues (especially right-hand panels)
- Consistent, professional look & feel across **all** modes (Test / Replace / Split / Generate / GREP)
- Final UX polish, empty states, error handling, and responsiveness
- Documentation and project hygiene for a public-facing release
- Leave the door open for future Debug / more engines without blocking
- The selected theme should be persisted

This is intentionally the last major phase in the original 5-phase plan.

---

## 2. Critical Fixes from Current Screenshot (v0.5.0)

1. **Right-hand Replace pane does not fill its panel**  
   The content on the right (Replacement pattern + Preview) does not properly expand to fill the available width/height of the right panel.  
   → Fix the layout so that **every** right-hand mode (Test, Replace, Split, Generate, GREP) fully and cleanly fills the right panel with proper stretching, no large empty wasted space, and consistent margins.

2. Any remaining minor visual inconsistencies visible in the latest screenshot (spacing, alignment, control sizes).

---

## 3. Layout & Visual Consistency Requirements

### 3.1 Right Panel System
- Create (or refine) a consistent host for the right-hand content.
- All five modes must:
  - Fill the entire available space
  - Have consistent padding/margins
  - Look balanced in both light and dark themes
  - Handle resizing gracefully (window resize, splitter drag)
- Special attention to Replace (current issue), Generate, and GREP which tend to need more vertical space.

### 3.2 Overall Layout Polish
- Left sidebar (Tokens / Library / History) remains consistent width.
- Center (Editor + Analysis Tree) and right panel use clean GridSplitters.
- Status bar is informative and never feels cramped.
- Top toolbar and options row have consistent height and spacing.
- No large empty regions or awkward stretching.

### 3.3 Theme Final Pass
- Both light and dark themes must look premium.
- All interactive states (hover, pressed, disabled, focused) are clear.
- Syntax highlighting remains excellent (already fixed in Phase 4 — do not regress).

---

## 4. Feature Completeness & Polish

### 4.1 All Modes
- **Test** — already strong; minor polish only.
- **Replace** — fix layout + ensure live/preview + backreferences feel excellent.
- **Split** — make sure results presentation is clean and fills space well.
- **Generate** — high-quality snippets, easy copy, good language coverage, clear engine notes.
- **GREP** — results list + preview must feel solid; progress/cancellation reliable; light/dark perfect.

### 4.2 Tokens, Library, History
- Token palette: complete enough for daily use, consistent widths (already addressed), good search.
- Library & History: reliable persistence, good empty states, search if missing, polished UI.

### 4.3 Analysis Tree
- Keep the rich expandable tree and click-to-select.
- Minor wording or visual tweaks only if needed.

### 4.4 Options & Engine Switching
- Options are clearly engine-aware.
- Switching engines re-runs the current operation cleanly.
- Status bar always shows current flavor/engine + useful stats.

### 4.5 Keyboard & Accessibility
- Document and verify all important shortcuts.
- Good keyboard navigation and focus visibility.
- Reasonable ARIA / automation properties where easy.

---

## 5. Documentation & Release Readiness

- Root `README.md` must be excellent for GitHub and for regexcraft.com visitors (timeless, with clear feature list, screenshots section or placeholders, build instructions, engines table).
- All user docs under `docs/user/` updated and accurate.
- `docs/CHANGELOG.md` complete up to this version.
- `docs/development/` contains all phase requirements + architecture.
- `AGENTS.md` and `HANDOFF.md` updated.
- Decide in HANDOFF whether the next version after this should be `1.0.0` or continue with `0.x` (recommend staying on 0.6.0 for now and planning a real 1.0 later if desired).
- LICENSE remains MIT.
- No temporary files or screenshots committed.

---

## 6. Technical Requirements

- No major new architecture.
- Keep using existing `IRegexEngine` + result models.
- Layout fixes must be clean XAML / Avalonia (prefer proper Grid / Dock / Star sizing over hacks).
- All existing tests must continue to pass. Add tests only for any new logic.
- Logging remains Serilog.
- Performance: no regressions; UI stays responsive under normal use.
- Do **not** implement the full Debug stepper unless it is trivial (it is still out of scope).

---

## 7. Versioning & Process

- Bump version to **`0.6.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with clear recommendations for what comes after Phase 5 (Debug, more engines, 1.0 release, website, etc.)
- All tests green
- Clean commit on `main`:  
  `Phase 5 complete: right-panel layout fixes, full visual consistency, 1.0-readiness polish (v0.6.0)`

---

## 8. Definition of Done

- [ ] Right-hand panels (especially Replace) fully and cleanly fill their available space
- [ ] All five modes (Test/Replace/Split/Generate/GREP) have consistent, professional layout
- [ ] No large empty wasted space or broken stretching
- [ ] Light and dark themes both look polished and consistent
- [ ] All major workflows feel solid and production-ready
- [ ] Root README is excellent and timeless
- [ ] Documentation is complete and accurate
- [ ] Version = 0.6.0
- [ ] AGENTS.md + HANDOFF.md updated with clear future roadmap
- [ ] All tests pass
- [ ] Clean commit on main
- [ ] `current_screenshot.png` is **not** committed

---

## 9. Out of Scope for Phase 5

- Full Debug / step-through engine
- New regex engines
- Major new features
- Website implementation
- Installer / packaging beyond what is already easy with `dotnet publish`

These can be planned in the HANDOFF for post-0.6.0 work.

---

**This document is the single source of truth for Phase 5.**  
The latest Replace-tab light-mode screenshot is the baseline. Fix the right-panel layout completely, make every mode look and feel consistent, and leave the project in excellent shape for a future 1.0.