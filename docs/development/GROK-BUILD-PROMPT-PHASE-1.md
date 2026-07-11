# Grok Build Prompt – Phase 1

Copy and paste the entire block below into a new Grok conversation to start Phase 1.

---

**PROMPT START**

You are implementing **Phase 1** of the RegexCraft project.

### Critical Instructions
1. The complete requirements are in `docs/development/PHASE-1-REQUIREMENTS.md`.  
   **Read that file first and follow it exactly.**
2. Phase 0 is already complete (v0.1.0). You are continuing from a working multi-engine foundation.
3. Work only on the `main` branch of https://github.com/MarshallMoorman/RegexCraft.
4. **Important file move**: At the beginning of this phase, move all original Phase 0 planning files (`PHASE-0-REQUIREMENTS.md`, `GROK-BUILD-PROMPT-PHASE-0.md`, and any other temporary package files that are still in the root) into `docs/development/`. Do **not** overwrite the real root `README.md`.
5. After everything is done and all tests pass:
   - Set version to `0.2.0` in `Directory.Build.props`
   - Update root `AGENTS.md`
   - Completely rewrite root `HANDOFF.md` with precise next steps for Phase 2
   - Update all required documentation
   - Create a clean commit:  
     `Phase 1 complete: editor, token palette, analysis tree, excellent Test panel with highlighting (v0.2.0)`

### Highest Priorities (in order)
1. Beautiful multi-panel UI
2. AvaloniaEdit + blue syntax highlighting
3. Text-based searchable Token palette (NO icons for tokens)
4. Live Analysis Tree
5. Test panel with **excellent match highlighting and group display** for both .NET and PCRE2 engines

Do not implement GREP, full Library, code generation UI, or Debug yet.

When finished, confirm every item in the Definition of Done and show `git status` + `git log -1`.

**PROMPT END**

---

### How to use
1. Copy `PHASE-1-REQUIREMENTS.md` into `docs/development/` in your repo.
2. Open a new Grok conversation.
3. Paste the prompt above.
4. Let it run until Phase 1 is complete and committed.