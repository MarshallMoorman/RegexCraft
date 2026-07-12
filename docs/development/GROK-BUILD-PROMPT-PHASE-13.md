# Grok Build Prompt – Phase 13 (Final)

Copy and paste the entire block below into a **new** Grok conversation dedicated to building Phase 13 (or into Grok Build as appropriate).

---

**PROMPT START**

You are implementing **Phase 13** of RegexCraft.

### Critical Context
- Read `docs/development/PHASE-13-REQUIREMENTS.md` completely first (final monorepo + Actions revision).
- **Single working repo**: main contains app, website, user docs, and private development docs.
- Marshall will make main **private** after public downloads work.
- **Actions must**:
  1. On version tag: build, test, publish RID zips to a **public** dist repo (e.g. RegexCraft-Releases)
  2. Deploy website + **user docs only** to the public site (no `docs/development/`)
- **No license keys.** Free personal / paid business, honor system.
- Implement **Export** CSV + JSON for match results.
- Replace MIT with commercial EULA.

### Highest Priorities
1. EULA + messaging (About, README, site)
2. Release workflow: main tag → public dist binaries
3. Site deploy workflow: user-facing site only
4. Download page → public dist URLs
5. Full user docs on site
6. Export feature + tests
7. `docs/development/commercial.md` with Marshall’s ordered checklist
8. Version **1.2.0**

### Process Rules
- Work on `main` only for product work.
- Final commit message:  
  `Phase 13 complete: monorepo + Actions public dist/site, commercial EULA (no keys), Export — 1.2.0`
- End by printing Marshall’s exact post-merge checklist (dist repo, token, payment, tag, incognito test, make private, re-verify).

**PROMPT END**
