# Grok Build Prompt – Phase 14 (Stripe Payment Link)

Copy and paste the entire block below into a **new** Grok conversation (or into Grok Build) dedicated to Phase 14.

---

**PROMPT START**

You are implementing **Phase 14** of RegexCraft.

### Critical Context
- Read `docs/development/PHASE-14-REQUIREMENTS.md` completely first.
- Phase 13 already shipped 1.2.0 (EULA, monorepo + Actions public dist/site, Export, no license keys).
- This phase is a **small, focused commercial polish**: wire the real Stripe sandbox Payment Link and clean the pricing page.
- Do **not** bump the version. Stay on 1.2.0.
- Work only on `main`.

### Highest Priorities
1. Update exactly these three files:
   - `website/site-config.js` → set `buyUrl` to  
     `https://buy.stripe.com/test_00w5kFgOHc4ucQnc8u3oA00`
   - `src/RegexCraft.Core/Commercial/CommercialLinks.cs` → set `BuyLicenseUrl` to the same URL
   - `website/pricing.html` → remove “placeholder” helper text; ensure Buy button works
2. Keep the honor-system model (no keys, same binaries for everyone).
3. Do not touch app logic, tests, workflows, or version props unless required for a clean build.
4. Final commit message:  
   `chore: wire Stripe sandbox Payment Link for business license`

### Process Rules
- Prefer the cleanest, minimal diff that satisfies the requirements.
- After the commit, print Marshall’s exact next steps from the commercial checklist (tag v1.2.0 → verify Actions → incognito download + site → make main private → re-verify).

**PROMPT END**

---

### How to use
1. Copy `PHASE-14-REQUIREMENTS.md` → `docs/development/PHASE-14-REQUIREMENTS.md`
2. Open a new Grok conversation / Grok Build
3. Paste the prompt above
4. Let it finish and commit
5. Marshall continues with the commercial checklist (tag, test, private)
