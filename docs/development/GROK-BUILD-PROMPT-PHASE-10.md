# Grok Build Prompt – Phase 10

Copy and paste the entire block below into a new Grok conversation to start Phase 10.

---

**PROMPT START**

You are implementing **Phase 10** of the RegexCraft project (the 1.0.0 release).

### Critical Context
- Phases 0–9 are complete. Current version is **1.0.0-rc1**.
- The complete requirements are in `docs/development/PHASE-10-REQUIREMENTS.md`. **Read that file first and follow it exactly.**

### Highest Priorities
1. **Smart right-panel sizing**:
   - When switching to Compare → expand right panel to a usable width (remembered Compare width or sensible minimum).
   - When switching away from Compare → restore the previous Normal width.
   - Persist both widths. Respect manual splitter drags.
2. **GitHub Releases**:
   - On version tag (v1.0.0) create a proper GitHub Release with published binaries attached for major RIDs.
   - Document the release process.
3. Final polish for 1.0 quality.
4. Set version to **1.0.0**, update all docs, and write a clear post-1.0 HANDOFF (Debug planned for 1.1).

### Process Rules
- Work only on `main`.
- After everything is complete and tests pass:
  - Set version to `1.0.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with the post-1.0 roadmap (Debug as top candidate for 1.1)
  - Update README, CHANGELOG, packaging docs
  - Ensure GitHub Actions for Releases are in place
  - Create a clean commit:  
    `Phase 10 complete: smart Compare panel sizing, GitHub Releases, final polish — 1.0.0`
  - Tag `v1.0.0` (or document exactly how the human should tag and push)

When finished, confirm every item in the Definition of Done, show `git status` + `git log -1`, and list the release-related workflow files.

**PROMPT END**

---

### How to use
1. Copy `PHASE-10-REQUIREMENTS.md` into `docs/development/PHASE-10-REQUIREMENTS.md`
2. Open a new Grok conversation
3. Paste the prompt above
4. Let it complete and commit **1.0.0**
5. Push the tag so the GitHub Release is created
