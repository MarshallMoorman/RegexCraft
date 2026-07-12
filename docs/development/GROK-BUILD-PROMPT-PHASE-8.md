# Grok Build Prompt – Phase 8

Copy and paste the entire block below into a new Grok conversation to start Phase 8.

---

**PROMPT START**

You are implementing **Phase 8** of the RegexCraft project.

### Critical Context
- Phases 0–7 are complete. Current version is **0.8.0**.
- Read `docs/development/PHASE-8-REQUIREMENTS.md` and also the review in `docs/development/CURRENT-STATE-REVIEW.md` (or the package file) first.
- Follow the requirements exactly.

### Highest Priorities
1. Harden multi-flavor support: accurate FlavorDefinitions, token/option awareness, clear fidelity banners.
2. Add **significant automated tests per flavor and per real engine** (this is the #1 quality goal).
3. Improve or add real engines only where practical (strengthen JS, evaluate Python/RE2).
4. Update Library notes, flavors.md, and README for accuracy.

### Process Rules
- Work only on `main`.
- After everything is complete and tests pass:
  - Set version to `0.9.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with the path to 1.0
  - Update documentation
  - Create a clean commit:  
    `Phase 8 complete: multi-flavor hardening, significant per-flavor tests, fidelity improvements (v0.9.0)`

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`. Also report approximate test counts for Engines and Flavors categories.

**PROMPT END**

---

### How to use
1. Copy both `PHASE-8-REQUIREMENTS.md` and `CURRENT-STATE-REVIEW.md` into `docs/development/`
2. Open a new Grok conversation
3. Paste the prompt
4. Let it complete and commit v0.9.0