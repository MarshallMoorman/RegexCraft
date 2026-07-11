# Grok Build Prompt – Phase 4

Copy and paste the entire block below into a new Grok conversation to start Phase 4.

---

**PROMPT START**

You are implementing **Phase 4** of the RegexCraft project.

### Critical Context
- Phases 0–3 are complete. Current version is **0.4.0**.
- Current visual baseline (light mode) is at `docs/development/current_screenshot.png` (do **not** commit this file).
- The complete requirements are in `docs/development/PHASE-4-REQUIREMENTS.md`. **Read that file first and follow it exactly.**

### Highest Priorities (in order)
1. **Fix unreadable regex editor text in light mode** — completely overhaul light-theme syntax highlighting colors for excellent contrast and professional appearance. Dark theme must also remain excellent.
2. **Make every token category panel the same width** on the left sidebar.
3. Full light-theme visual audit and polish of every panel.
4. Further polish of GREP, Generate, Library/History, Analysis Tree, and overall consistency.

### Process Rules
- Work only on `main`.
- Do **not** commit `docs/development/current_screenshot.png`.
- After everything is complete and tests pass:
  - Set version to `0.5.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with precise next steps for the following phase
  - Update documentation
  - Create a clean commit:  
    `Phase 4 complete: light theme readability, consistent token widths, full visual polish (v0.5.0)`

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`.

**PROMPT END**

---

### How to use
1. Copy `PHASE-4-REQUIREMENTS.md` into `docs/development/PHASE-4-REQUIREMENTS.md`
2. Update `docs/development/current_screenshot.png` with the latest light-mode screenshot if needed
3. Open a new Grok conversation and paste the prompt
4. Let it complete and commit v0.5.0