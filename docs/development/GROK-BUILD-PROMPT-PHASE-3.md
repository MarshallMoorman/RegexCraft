# Grok Build Prompt – Phase 3

Copy and paste the entire block below into a new Grok conversation to start Phase 3.

---

**PROMPT START**

You are implementing **Phase 3** of the RegexCraft project.

### Critical Context
- Phase 0–2 are complete. Current version is **0.3.0**.
- Current visual baseline is the screenshot at `docs/development/current_screenshot.png` (do **not** commit this file).
- The complete requirements are in `docs/development/PHASE-3-REQUIREMENTS.md`. **Read that file first and follow it exactly.**

### Highest Priorities
1. **Fix the window title** — it still shows “Avalonia Application”. Change it to “RegexCraft”. This is mandatory.
2. Rewrite the root `README.md` into a complete, professional, timeless project README (remove “Phase 2” language).
3. Implement full **GREP** (search + replace across files and folders) using the existing engines. Must be async, cancellable, with good results UI and preview.
4. Further polish of Analysis Tree, Generate, Library/History, Options, and overall light/dark themes.

### Process Rules
- Work only on `main`.
- Do **not** commit `docs/development/current_screenshot.png`.
- After everything is complete and tests pass:
  - Set version to `0.4.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with precise next steps for Phase 4
  - Update all documentation
  - Create a clean commit:  
    `Phase 3 complete: GREP, window title fix, polished README, further UX improvements (v0.4.0)`

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`.

**PROMPT END**

---

### How to use
1. Place `PHASE-3-REQUIREMENTS.md` into `docs/development/PHASE-3-REQUIREMENTS.md`
2. Make sure the latest screenshot is at `docs/development/current_screenshot.png` (and is gitignored or not staged)
3. Open a new Grok conversation and paste the prompt above
4. Let it complete and commit v0.4.0