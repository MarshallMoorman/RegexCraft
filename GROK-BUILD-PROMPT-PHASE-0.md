# Grok Build Prompt – Phase 0

Copy and paste the entire block below into a new Grok conversation (or Grok Build) to start Phase 0.

---

**PROMPT START**

You are implementing **Phase 0** of the RegexCraft project.

### Critical Instructions
1. The complete and authoritative requirements are in the file `PHASE-0-REQUIREMENTS.md` (it is already in the repository root or will be provided). You **must** follow every requirement in that document exactly.
2. Work in the existing repository: https://github.com/MarshallMoorman/RegexCraft (already cloned to the working directory).
3. Use .NET 10 and Avalonia 12.
4. After all work is complete and tests pass, you must:
   - Ensure version is set to `0.1.0` in `Directory.Build.props`
   - Update `AGENTS.md`
   - Create/update `HANDOFF.md` with exact handoff notes for Phase 1
   - Write/update all required documentation under `docs/`
   - Create a clean commit on `main` with message:  
     `Phase 0 complete: foundation, multi-engine (.NET + PCRE2), blue theme, logging, NUnit tests (v0.1.0)`

### What you must deliver
- Full solution structure as specified
- Two working engines (DotNetRegexEngine + PcreRegexEngine) that both support Match and Replace with consistent result models (including groups and named groups)
- Variable-driven professional blue light/dark theme
- Serilog file logging (7-day rolling, configurable via appsettings.json)
- NUnit test suite that fully covers the engines and core services
- Minimal but professional Avalonia shell that lets you select engine and run Match/Replace to prove everything works
- All documentation and handoff files

Do **not** implement Phase 1 features (full editor, token palette, analysis tree, etc.).

Start by reading `PHASE-0-REQUIREMENTS.md` carefully, then create the solution structure and implement everything required for a green Definition of Done.

When finished, confirm that all items in the Definition of Done are satisfied and show the final `git status` and `git log -1`.

**PROMPT END**

---

### How to use this prompt
1. Make sure `PHASE-0-REQUIREMENTS.md` is in the root of `~/dev/RegexCraft`.
2. Open a new Grok / Grok Build session.
3. Paste the entire “PROMPT START … PROMPT END” block.
4. Attach or ensure the requirements file is available if needed.
5. Let it run until it declares Phase 0 complete and shows a clean commit.