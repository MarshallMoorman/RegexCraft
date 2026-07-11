# Grok Build Prompt – Phase 7

Copy and paste the entire block below into a new Grok conversation to start Phase 7.

---

**PROMPT START**

You are implementing **Phase 7** of the RegexCraft project.

### Critical Context
- Phases 0–6 are complete. Current version is **0.7.0**.
- The complete requirements are in `docs/development/PHASE-7-REQUIREMENTS.md`. **Read that file first and follow it exactly.**
- Current screenshot (if any) is at `docs/development/current_screenshot.png` — do not commit it.

### Highest Priorities
1. Create and set a proper **RegexCraft application icon**.
2. Replace the default Avalonia About dialog with a professional custom **About RegexCraft** dialog. Fix the menu item text.
3. Dramatically expand automated testing:
   - More NUnit unit tests
   - Avalonia.Headless UI tests for main workflows
   - Automated screenshot capture using `CaptureRenderedFrame()` that produces good images for the README and docs
4. Fix any bugs discovered while writing the new tests.

### Process Rules
- Work only on `main`.
- Do **not** commit temporary or bad screenshots.
- After everything is complete and tests pass:
  - Set version to `0.8.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md`
  - Update documentation (include the new screenshots)
  - Create a clean commit:  
    `Phase 7 complete: app icon, custom About dialog, expanded automated testing + screenshot capture (v0.8.0)`

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`.

**PROMPT END**

---

### How to use
1. Copy `PHASE-7-REQUIREMENTS.md` into `docs/development/PHASE-7-REQUIREMENTS.md`
2. Open a new Grok conversation
3. Paste the prompt
4. Let it complete and commit v0.8.0