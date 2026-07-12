# Grok Build Prompt – Phase 12

Copy and paste the entire block below into a new Grok conversation to start Phase 12.

---

**PROMPT START**

You are implementing **Phase 12** of the RegexCraft project: the public website on GitHub Pages.

### Critical Context
- App is at **1.1.0** (Debug shipped). Stable releases exist.
- Requirements: `docs/development/PHASE-12-REQUIREMENTS.md` — **read it fully first**.
- Site must live **in this same repository** (source-controlled). No separate repo required.
- Marshall has never set up GitHub Pages; you must leave crystal-clear instructions for DNS + Pages settings.

### Highest Priorities
1. Create a professional static website (plain HTML/CSS preferred) under `website/` (or documented equivalent).
2. Include: landing/hero, features, screenshots, download CTA → GitHub Releases, docs links, About, footer.
3. Blue professional theme matching the app (no purple). Responsive.
4. Add `CNAME` file with `regexcraft.com`.
5. Write `docs/development/website.md` with exact steps for:
   - Enabling GitHub Pages in repo settings
   - DNS records Marshall must create at his registrar
   - How to update the site later
6. Update root README, CHANGELOG, AGENTS.md, HANDOFF.md.
7. Do not break the app build or tests.

### Process Rules
- Work only on `main`.
- Prefer version **1.1.1** only if you touch app code; pure website can stay on 1.1.0 with a CHANGELOG note.
- Final commit message:  
  `Phase 12 complete: regexcraft.com website on GitHub Pages + setup docs`
- When finished, confirm Definition of Done and print the exact DNS + Pages checklist Marshall must perform.

**PROMPT END**

---

### How to use
1. Copy `PHASE-12-REQUIREMENTS.md` → `docs/development/PHASE-12-REQUIREMENTS.md`
2. Open a new Grok conversation
3. Paste the prompt
4. Let it finish
5. Follow the DNS/Pages checklist the agent prints so the site goes live
