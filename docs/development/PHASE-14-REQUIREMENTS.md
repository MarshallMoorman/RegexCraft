# RegexCraft – Phase 14 Requirements (Stripe Payment Link)

**Wire live checkout URL + commercial polish for go-live**  
**Version after this phase**: still `1.2.0` (no Directory.Build.props change required)  
**Depends on**: Phase 13 complete (commit that set version 1.2.0, EULA, Export, monorepo Actions)  
**Date**: 2026-07-12  

---

## 1. Goal

1. Replace the placeholder Buy URL with the real Stripe **sandbox** Payment Link.
2. Keep the honor-system model (no keys, same binaries).
3. Clean any “placeholder” helper text on the pricing page.
4. Leave Marshall able to immediately tag `v1.2.0` and finish the commercial checklist.

---

## 2. Stripe Details (locked)

| Item | Value |
|------|--------|
| Payment Link (sandbox) | `https://buy.stripe.com/test_00w5kFgOHc4ucQnc8u3oA00` |
| Suggested price | $49 one-time |
| Live mode | Pending Stripe account review (2–3 days). When live link is ready, replace the two URL constants only. |

---

## 3. Files the Agent Must Update (exactly these three)

### 3.1 `website/site-config.js`
- Set `buyUrl` to the sandbox Payment Link above.
- Keep `businessPrice: "$49"`.
- Keep all other keys (version, distRepo, asset helper, etc.).

### 3.2 `src/RegexCraft.Core/Commercial/CommercialLinks.cs`
- Set `BuyLicenseUrl` to the same sandbox Payment Link.
- Update the XML comment to note it is the Stripe sandbox link (until live).
- Leave every other constant unchanged.

### 3.3 `website/pricing.html`
- Ensure the Buy button uses `data-buy-link` (or hard-coded fallback to the sandbox URL).
- Remove or soften any “Checkout URL is a placeholder…” helper text.
- Keep the honor-system note and $49 display.

**Do not** change any other files unless required for the three above to compile/build cleanly.

---

## 4. Branding guidance (for Marshall’s Stripe Dashboard)

- Brand color: `#0078D4`
- Hover / secondary: `#106EBE`
- Logo: use `website/assets/logo.png` (or the favicon if a square icon is preferred)

Agent does **not** need to edit Stripe; just document the values in the commit message or a short note in commercial.md if useful.

---

## 5. Documentation / Process

- If `docs/development/commercial.md` still shows a placeholder URL, update the example line to the sandbox link (optional but nice).
- No CHANGELOG version bump (this is a config/content polish on top of 1.2.0).
- HANDOFF / AGENTS: no major rewrite needed; a one-line note that Buy URL is now live (sandbox) is sufficient.
- Final commit message:

  ```
  chore: wire Stripe sandbox Payment Link for business license
  ```

---

## 6. Definition of Done

- [ ] `website/site-config.js` → `buyUrl` points at the Stripe sandbox link
- [ ] `CommercialLinks.BuyLicenseUrl` points at the same link
- [ ] `website/pricing.html` Buy button works (via data-buy-link or direct href) and no longer says “placeholder”
- [ ] App still builds and existing tests pass
- [ ] Clean single commit on `main`
- [ ] Agent prints Marshall’s next steps (tag v1.2.0, verify Actions, incognito download + site, then make main private)

---

## 7. Out of Scope

- Creating a new Stripe product or changing price
- Live Payment Link (will be a future one-line update after Stripe approval)
- Version bump to 1.2.1
- Any new features, engines, or installers
- Making the main repo private (Marshall does this only after public downloads work)

---

**This document is the single source of truth for Phase 14.**  
Grok Build applies the three files, commits, and hands control back for the commercial checklist.
