# Grok Build Prompt – Phase 2

Copy and paste the entire block below into a new Grok conversation to start Phase 2.

---

**PROMPT START**

You are implementing **Phase 2** of the RegexCraft project.

### Critical Context
- Phase 0 and Phase 1 are complete. Current version is **0.2.0**.
- A screenshot of the current application has been provided to the previous agent / is known. Use it as the baseline. `docs/development/current_screenshot.png`
- Do not commit the current screenshot
- The complete requirements are in `docs/development/PHASE-2-REQUIREMENTS.md`. **Read that file first and follow it exactly.**

### Highest Priorities for This Phase
1. Fix every visible issue from the current screenshot (especially window title “Avalonia Application” → “RegexCraft”, and the very shallow Analysis Tree).
2. Make **Replace** fully functional with live/preview + highlighting + correct backreferences for both engines.
3. Make **Split** fully functional.
4. Implement **Code Generation** (Generate) for multiple languages with one-click copy.
5. Implement real **Library** and **History** (persistent).
6. Significantly upgrade the Analysis Tree so it is actually useful.
7. Overall polish so the app feels premium in both light and dark themes.

### Process Rules
- Work only on `main`.
- After all work is complete and tests pass:
  - Set version to `0.3.0` in `Directory.Build.props`
  - Update root `AGENTS.md`
  - Completely rewrite root `HANDOFF.md` with precise next steps for Phase 3
  - Update all required user and development documentation
  - Create a clean commit:  
    `Phase 2 complete: polished UI, full Replace/Split, code generation, Library/History, rich Analysis Tree (v0.3.0)`

Do **not** implement GREP, Debug stepping, or additional engines yet.

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`.

**PROMPT END**

---

### How to use
1. Place `PHASE-2-REQUIREMENTS.md` into `docs/development/PHASE-2-REQUIREMENTS.md` in the repo.
2. Open a new Grok conversation.
3. Paste the prompt above (you can also attach the screenshot again if helpful).
4. Let the agent complete Phase 2 and commit v0.3.0.