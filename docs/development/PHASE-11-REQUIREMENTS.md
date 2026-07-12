# RegexCraft – Phase 11 Requirements (Debug + Polish)

**Project**: RegexCraft  
**Version after this phase**: `1.1.0`  
**Depends on**: 1.0.1  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 11

Deliver the long-deferred **Debug / step-through** feature as the headline of 1.1.0, plus one small but visible UI fix.

1. **Debug panel** – interactive step-through of the matching process (start with .NET engine; expand if practical)
2. **Matches & Groups cards** – force consistent equal width for all match cards in the list
3. Solid tests + documentation for the new Debug experience

---

## 2. Critical UI Fix (Quick Win)

### Matches & Groups equal width
In the Test (and related) panels, the individual match cards under “Matches & Groups” currently have inconsistent widths.

**Requirement**: All match cards in that list must have the **same width** (stretch to fill the available panel width, or a consistent fixed/min width that looks clean). No ragged right edges. Works in both light and dark themes and when the right panel is resized.

---

## 3. Major Feature: Debug / Step-Through

### 3.1 Vision
A usable debugger that lets the user step through how the regex engine attempts to match the subject, similar in spirit to RegexBuddy’s debugger (without needing pixel-perfect parity on day one).

### 3.2 Minimum Viable Debug (must have for 1.1.0)

**Scope for this phase – start with the .NET engine only** (Full fidelity). Other engines can show “Debug not yet available for this engine” or a simplified view.

Required capabilities:

- New **Debug** mode / tab (alongside Test, Replace, Split, Generate, GREP, Compare)
- After a successful (or partial) match attempt, show:
  - Current step number / total (or “step X”)
  - Which part of the **regex** is currently being considered (highlight in the editor or a dedicated regex view)
  - Which part of the **subject** is currently being examined (highlight in the subject)
  - Success / failure of the current step
  - Simple explanation text (“Trying to match \w+ at position 14”, “Backtracking…”, “Group 1 captured ‘support’”, etc.)
- Controls:
  - Step Forward
  - Step Backward (if feasible)
  - Jump to start / end (or “Reset”)
  - Optional: Play / pause auto-step (nice-to-have)
- The debugger should work on the current pattern + subject + options from the main UI
- Clear empty / error states (“Run a match first”, “Debug currently supported only for .NET”, etc.)
- Keyboard shortcuts (e.g. F10 / F11 style or Ctrl+→ / Ctrl+←)

### 3.3 Implementation Approach (recommended)

Because .NET’s `System.Text.RegularExpressions` does not expose a public step-by-step API, choose one of these pragmatic approaches (document which one you pick):

**Preferred pragmatic options (in order):**
1. **Instrumented / educational stepper** – Build a simplified NFA-style or recursive stepper that explains the *intended* matching process for teaching purposes (even if it is not a 100% perfect re-implementation of the real .NET engine internals). Highlight regex nodes + subject positions. This is what many educational tools do.
2. **Match result + capture walk-through** – After a real Match, provide a high-quality walk-through of the successful path and the captured groups with positions (less “step-through the engine”, more “explain this match”). Still very useful.
3. Hybrid of the above.

Do **not** attempt a perfect re-implementation of the full .NET regex engine in this phase. Aim for something genuinely useful and visually clear.

### 3.4 UI Placement
- Add a **Debug** tab in the right-hand mode switcher (or a clear sub-panel).
- When Debug is active, the right panel should use a comfortable width (can share the “Normal” or a dedicated Debug width if needed; at minimum respect the existing smart sizing system).
- Left side (Tokens / Library / History) and center (Editor + Analysis Tree) stay available so the user can still see the pattern structure.

### 3.5 Fidelity & Multi-engine
- Primary target: **.NET** (Full).
- For PCRE2 / JavaScript / other flavors: show a clear message that step-through Debug is currently only available for .NET, and fall back to the regular Test results view or a simplified explanation.
- The architecture should make it possible to add more engines later without rewriting the UI.

---

## 4. Tests

- Unit tests for the stepper / explanation logic.
- Headless UI tests that open Debug, step forward a few times, and verify highlights / text update.
- Tests for the equal-width Matches & Groups fix (layout-related assertions where practical).
- Existing tests must remain green.

---

## 5. Documentation

- New user doc: `docs/user/debugging.md` (how to use the Debug tab, what the steps mean, current limitations).
- Update root README feature list.
- Update `docs/CHANGELOG.md` for 1.1.0.
- Update `docs/user/flavors.md` or notes if Debug is engine-specific.
- HANDOFF.md rewritten with post-1.1 priorities (website, more engines, export, etc.).

---

## 6. Technical Notes

- Reuse existing Analysis Tree / AST if it helps drive the stepper.
- Keep the blue theme variables.
- Persist any new Debug-related settings if useful (optional).
- Serilog: log Debug session start/step errors at appropriate levels.
- Do not break Compare smart sizing or any 1.0 behavior.

---

## 7. Versioning & Process

- Bump version to **`1.1.0`** in `Directory.Build.props`
- Update `AGENTS.md`
- Completely rewrite `HANDOFF.md` with next priorities after Debug
- All tests green
- CI green
- Clean commit:  
  `Phase 11 complete: Debug step-through (.NET), equal-width Matches cards — 1.1.0`
- Tag `v1.1.0` so the GitHub Release is created

---

## 8. Definition of Done

- [ ] Matches & Groups cards all have equal / consistent width
- [ ] Debug tab exists and is reachable from the main UI
- [ ] User can step forward (and ideally backward) through a match explanation for .NET
- [ ] Regex position and subject position are visually highlighted during stepping
- [ ] Clear messaging when Debug is not available for the current engine
- [ ] Solid automated tests for the new Debug logic and the width fix
- [ ] User documentation for Debug written
- [ ] Version = 1.1.0
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] CI green + GitHub Release for v1.1.0

---

## 9. Out of Scope for 1.1.0

- Perfect cycle-accurate re-implementation of the .NET (or PCRE) engine
- Debug support for every flavor on day one
- Website
- New real engines
- Export features

---

## 10. Side Note for Later (Website)

**Yes – you can point regexcraft.com to GitHub Pages.**

Typical setup:
1. Create a `gh-pages` branch (or use `/docs` folder on main).
2. In the GitHub repo Settings → Pages, enable GitHub Pages.
3. Add a `CNAME` file containing `regexcraft.com`.
4. At your domain registrar, create a CNAME record: `regexcraft.com` (or `www`) → `MarshallMoorman.github.io` (or the Pages URL GitHub shows you).
5. Optional: also set A records if you want apex domain support the way GitHub documents.

We can do a proper website phase later; no need to implement it in Phase 11.

---

**This document is the single source of truth for Phase 11.**  
Deliver a genuinely useful Debug experience for .NET + the equal-width Matches fix, and ship 1.1.0.
