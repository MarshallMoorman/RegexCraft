# Grok Build Prompt – Phase 9

Copy and paste the entire block below into a new Grok conversation to start Phase 9.

---

**PROMPT START**

You are implementing **Phase 9** of the RegexCraft project.

### Critical Context
- Phases 0–8 are complete. Current version is **0.9.0**.
- The complete requirements are in `docs/development/PHASE-9-REQUIREMENTS.md`. **Read that file first and follow it exactly.**

### Highest Priorities
1. Implement a polished **Compare** panel that lets users compare the same regex across multiple flavors/engines side-by-side (results, validity, key differences, fidelity).
2. Add **GitHub Actions**:
   - CI workflow (build + test on push/PR)
   - Publish/artifact workflow (dotnet publish + upload)
   - Make CI reliable and documented
3. Final polish pass + packaging documentation
4. Prepare the project for **1.0.0-rc1** (or 0.10.0)

### Process Rules
- Work only on `main`.
- After everything is complete and tests pass:
  - Set version to `1.0.0-rc1` in `Directory.Build.props` (preferred)
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with the remaining steps to final 1.0.0
  - Update all documentation (including Compare + CI badges in README)
  - Ensure GitHub Actions are committed and will run cleanly
  - Create a clean commit:  
    `Phase 9 complete: Compare panel, GitHub Actions CI, polish, 1.0.0-rc1 prep`

When finished, confirm every item in the Definition of Done, show `git status` + `git log -1`, and list the new workflow files.

**PROMPT END**

---

### How to use
1. Copy `PHASE-9-REQUIREMENTS.md` into `docs/development/PHASE-9-REQUIREMENTS.md`
2. Open a new Grok conversation
3. Paste the prompt above
4. Let it complete and commit 1.0.0-rc1
5. After push, verify that GitHub Actions run successfully