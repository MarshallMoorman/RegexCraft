# Grok Build Prompt – Phase 6

Copy and paste the entire block below into a new Grok conversation to start Phase 6.

---

**PROMPT START**

You are implementing **Phase 6** of the RegexCraft project.

### Critical Context
- Phases 0–5 are complete. Current version is **0.6.0**.
- The complete requirements are in `docs/development/PHASE-6-REQUIREMENTS.md`. **Read that file first and follow it exactly.**
- Current screenshot (if present) is at `docs/development/current_screenshot.png` — do not commit it.

### Highest Priorities (in order)
1. Persist the theme preference (Light / Dark / System) across restarts. Also persist other sensible settings.
2. Fix Generate tab: C# (and any selected language) must auto-generate code immediately when the tab is shown or when the regex/options change. No need to toggle the language dropdown.
3. Seed the Library with a good set of common built-in regular expressions (email, URL, IP, dates, etc.).
4. **Major work**: Expand multi-flavor support. Add JavaScript, Python, Java, PHP and several more. Use the existing flavor/engine architecture. Clearly indicate testing fidelity for each flavor.

### Process Rules
- Work only on `main`.
- Do **not** commit screenshots.
- After everything is complete and tests pass:
  - Set version to `0.7.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with the next roadmap
  - Update all documentation
  - Create a clean commit:  
    `Phase 6 complete: theme persistence, Generate auto-run, default Library, expanded multi-flavor support (v0.7.0)`

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`.

**PROMPT END**

---

### How to use
1. Copy `PHASE-6-REQUIREMENTS.md` into `docs/development/PHASE-6-REQUIREMENTS.md`
2. Open a new Grok conversation
3. Paste the prompt above
4. Let it complete and commit v0.7.0