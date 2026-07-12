# Grok Build Prompt – Phase 11

Copy and paste the entire block below into a new Grok conversation to start Phase 11.

---

**PROMPT START**

You are implementing **Phase 11** of the RegexCraft project (Debug + 1.1.0).

### Critical Context
- Current version is **1.0.1**. 1.0 is stable and released.
- The complete requirements are in `docs/development/PHASE-11-REQUIREMENTS.md`. **Read that file first and follow it exactly.**

### Highest Priorities
1. **Matches & Groups cards** must all have equal / consistent width (quick visual fix).
2. **Debug / step-through** feature:
   - New Debug tab
   - Primary target: .NET engine
   - Step forward (and ideally backward)
   - Highlight current position in regex + subject
   - Clear human-readable step explanations
   - Pragmatic implementation (educational stepper or high-quality match walk-through — do NOT try to perfectly re-implement the entire .NET engine)
   - Clear “not available” messaging for other engines
3. Tests + user documentation for Debug.
4. Ship as **1.1.0** with GitHub Release.

### Process Rules
- Work only on `main`.
- After everything is complete and tests pass:
  - Set version to `1.1.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with post-1.1 priorities (website, more engines, export, etc.)
  - Update README, CHANGELOG, and add `docs/user/debugging.md`
  - Create a clean commit:  
    `Phase 11 complete: Debug step-through (.NET), equal-width Matches cards — 1.1.0`
  - Tag `v1.1.0` so the GitHub Release is created

When finished, confirm every item in the Definition of Done, show `git status` + `git log -1`, and briefly describe the Debug approach you implemented.

**PROMPT END**

---

### How to use
1. Copy `PHASE-11-REQUIREMENTS.md` into `docs/development/PHASE-11-REQUIREMENTS.md`
2. Open a new Grok conversation
3. Paste the prompt above
4. Let it complete and commit **1.1.0**
5. Push the `v1.1.0` tag
