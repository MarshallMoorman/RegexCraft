# Grok Build Prompt – Phase 5

Copy and paste the entire block below into a new Grok conversation to start Phase 5.

---

**PROMPT START**

You are implementing **Phase 5** (Final Polish & 1.0 Readiness) of the RegexCraft project.

### Critical Context
- Phases 0–4 are complete. Current version is **0.5.0**.
- Current visual baseline is the screenshot at `docs/development/current_screenshot.png` (Replace tab, light mode). Do **not** commit this file.
- The complete requirements are in `docs/development/PHASE-5-REQUIREMENTS.md`. **Read that file first and follow it exactly.**

### Highest Priorities
1. Fix the right-hand panel layout so that Replace (and all other modes: Test, Split, Generate, GREP) fully and cleanly fill the available space with no large empty regions.
2. Make layout, spacing, and visual consistency excellent across the entire application in both light and dark themes.
3. Final polish of all workflows, empty states, and documentation.
4. Leave the project in a clean, professional, nearly-1.0 state.

### Process Rules
- Work only on `main`.
- Do **not** commit `docs/development/current_screenshot.png`.
- After everything is complete and tests pass:
  - Set version to `0.6.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with a clear post-Phase-5 roadmap (Debug, more engines, 1.0, website, etc.)
  - Update all documentation and the root README
  - Create a clean commit:  
    `Phase 5 complete: right-panel layout fixes, full visual consistency, 1.0-readiness polish (v0.6.0)`

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`.

**PROMPT END**

---

### How to use
1. Copy `PHASE-5-REQUIREMENTS.md` into `docs/development/PHASE-5-REQUIREMENTS.md`
2. Update the screenshot at `docs/development/current_screenshot.png` if needed
3. Open a new Grok conversation and paste the prompt
4. Let it complete and commit v0.6.0