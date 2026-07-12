# RegexCraft – Phase 9 Requirements

**Project**: RegexCraft  
**Version after this phase**: `1.0.0-rc1` (or `0.10.0` if you prefer to stay on 0.x — recommend `1.0.0-rc1`)  
**Depends on**: Phase 8 complete (`0.9.0`)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 9

Prepare RegexCraft for a real public release candidate while adding one high-value multi-flavor feature:

1. **Compare** panel (side-by-side / multi-flavor comparison)
2. Final polish pass
3. Solid packaging documentation
4. **GitHub Actions CI/CD** (build, test, artifacts, optional screenshots/release)

This phase should leave the project in a state where a public 1.0.0 (or full RC) is realistic.

---

## 2. Major Feature: Compare Panel

### 2.1 Purpose
Allow users to compare the same regular expression across multiple flavors/engines side-by-side (or in a clear multi-column/tabbed view).

### 2.2 Requirements
- New top-level mode or dedicated panel/tab: **Compare**
- User selects 2–4 flavors (from the existing list)
- For each selected flavor show:
  - Engine + fidelity badge
  - Whether the pattern is valid
  - Match results (count, first few matches, groups) using the same subject
  - Key differences (options that differ, tokens that are unsupported, behavioral notes)
  - Optional: generated code snippet differences
- Clear visual layout (cards, columns, or split view) that works in both light and dark themes
- Changing the main regex or subject automatically re-runs the comparison
- “Copy differences” or export summary is a nice-to-have
- Must reuse the existing `IRegexEngine` / FlavorDefinition system (no new engines required)

### 2.3 UX Notes
- Keep it fast (run comparisons in parallel where possible)
- Show loading indicators if any engine is slow
- Empty / error states must be clean
- Keyboard accessible

---

## 3. GitHub Actions (Required)

Create a solid CI pipeline under `.github/workflows/`.

### 3.1 Minimum Required Workflows

**1. CI (on push + pull_request to main)**
- Restore + build the solution (Debug + Release)
- Run all NUnit tests (`dotnet test`)
- Fail the job if any test fails
- Upload test results / TRX if easy
- Optionally run the Headless screenshot tests and upload the generated images as artifacts

**2. Build artifacts (on tag or manual / on main)**
- `dotnet publish` for the main platforms you care about (at least win-x64, linux-x64, osx-x64 / osx-arm64 if feasible)
- Upload the published folders or zips as GitHub Actions artifacts
- Optional: create a simple GitHub Release when a version tag is pushed

**3. Optional but recommended**
- CodeQL or basic security scanning
- Cache NuGet packages for speed
- Matrix build if useful (though single .NET 10 is fine)

### 3.2 Requirements
- Workflows must be reliable and reasonably fast
- Document how to trigger a release build
- Do not break local development
- Use modern `actions/checkout`, `actions/setup-dotnet`, etc.
- Secrets should not be required for the basic CI path

---

## 4. Final Polish & 1.0 Prep

### 4.1 UX / Visual
- Full pass on spacing, alignment, empty states, error messages, focus states
- Ensure Compare panel matches the quality of Test / Replace / GREP
- Any remaining light/dark theme inconsistencies
- Window title, About dialog, icon already done — verify they are perfect

### 4.2 Packaging Documentation
- Clear instructions in README and/or `docs/development/packaging.md` for:
  - `dotnet publish` commands for Windows / macOS / Linux
  - How icons are included
  - How to create a simple portable zip or self-contained executable
  - Notes for future installer work (optional)
- Version is driven only from `Directory.Build.props`

### 4.3 Documentation
- Update root README for 1.0-rc readiness (features, screenshots, flavors table, CI badge)
- Update `docs/CHANGELOG.md` with a proper 1.0.0-rc1 / 0.10.0 entry
- Update `docs/user/` for the new Compare feature
- Keep all phase requirements under `docs/development/`
- HANDOFF.md must describe the path from this RC to a final 1.0.0

### 4.4 Quality Gates
- All existing tests + new Compare tests + CI must be green
- No known critical bugs
- Performance of Compare must be acceptable for normal patterns

---

## 5. Technical Requirements

- Compare must use the existing multi-flavor architecture
- New tests: unit tests for comparison logic + Headless UI tests for the Compare panel
- GitHub Actions must not require interactive login
- Continue Serilog, settings persistence, theme variables, etc.
- Do not commit temporary files or low-quality screenshots

---

## 6. Versioning & Process

- Bump version to **`1.0.0-rc1`** (preferred) or `0.10.0` in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with clear steps to final 1.0.0 (what is left after this RC)
- All tests green
- CI green on GitHub
- Clean commit on `main`:  
  `Phase 9 complete: Compare panel, GitHub Actions CI, polish, 1.0.0-rc1 prep`

---

## 7. Definition of Done

- [ ] Compare panel is implemented and polished (multi-flavor side-by-side or equivalent)
- [ ] Compare has good automated tests
- [ ] GitHub Actions CI workflow builds and runs all tests on push/PR
- [ ] Publish / artifact workflow exists and is documented
- [ ] Packaging documentation is clear
- [ ] Root README is 1.0-rc ready (includes CI badge if possible)
- [ ] All previous tests still pass
- [ ] Version = 1.0.0-rc1 (or 0.10.0)
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] Clean commit on main
- [ ] CI is green on GitHub after the push

---

## 8. Out of Scope for Phase 9

- Full Debug / step-through debugger
- New real engines
- Website implementation
- Full native installers (MSI, DMG, etc.) — documentation only is enough
- Breaking changes

---

**This document is the single source of truth for Phase 9.**  
Deliver a production-quality Compare feature, reliable GitHub Actions, and leave the project ready for a public release candidate.