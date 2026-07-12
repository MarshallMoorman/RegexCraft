# RegexCraft – Phase 10 Requirements

**Project**: RegexCraft  
**Version after this phase**: `1.0.0`  
**Depends on**: Phase 9 complete (`1.0.0-rc1`)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 10

Ship the first stable **1.0.0** release.

Focus areas:
1. Smart right-panel sizing (especially for the Compare tab)
2. Proper **GitHub Releases**
3. Final polish and quality bar for 1.0
4. Clean HANDOFF for everything that comes after 1.0 (Debug, more engines, website, etc.)

Debug remains **out of scope** for 1.0 (planned for a future 1.x release).

---

## 2. Smart Right-Panel Sizing (Critical UX Fix)

### Problem
When the user switches to the **Compare** tab, the right panel is often too narrow to see the multi-flavor cards usefully. When they switch away, we want to restore the previous comfortable width.

### Requirements
- Maintain two remembered widths (or equivalent):
  - **Normal width** — used by Test, Replace, Split, Generate, GREP (and any future non-Compare modes)
  - **Compare width** — used only when Compare is active
- Behavior:
  - Switching **to** Compare → set the right panel to the Compare width (or a sensible minimum if none stored, e.g. 480–560 px or ~40-45% of window).
  - Switching **away** from Compare → restore the previous Normal width.
- Both widths should be persisted across application restarts (with the other settings).
- Respect user manual resizing:
  - If the user drags the splitter while on Compare, update the stored Compare width.
  - If the user drags the splitter on any other tab, update the stored Normal width.
- Works correctly with window resizing and on both light/dark themes.
- No layout jumpiness or broken splitters.

This is the highest-priority UI fix for this phase.

---

## 3. GitHub Releases

### Requirements
- When a version tag is pushed (e.g. `v1.0.0`), a GitHub Actions workflow must:
  - Build the project
  - Run tests
  - `dotnet publish` for the main runtimes (at minimum: `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64` if feasible)
  - Create a **GitHub Release** for that tag
  - Attach the published artifacts (zips preferred) to the release
  - Generate decent release notes (from CHANGELOG.md or conventional commits)
- Document the exact process:
  - How to cut a release (`git tag v1.0.0 && git push origin v1.0.0`)
  - What artifacts appear
  - How version is sourced from `Directory.Build.props`
- The existing CI (build + test on push/PR) must continue to work.
- Prefer soft fail or clear messaging if a particular RID fails; do not leave the release half-created without artifacts.

---

## 4. Final Polish for 1.0.0

- Full visual and interaction pass on all modes (including Compare).
- Ensure empty states, error states, loading indicators, and focus states are clean.
- Verify keyboard shortcuts still work and are documented.
- Confirm theme persistence, Library defaults, Generate auto-run, etc. still behave correctly.
- About dialog, icon, window title — final check.
- No obvious layout bugs or low-contrast elements in light or dark theme.
- Performance: Compare and live testing remain snappy for normal patterns.

---

## 5. Documentation & Project Hygiene

- Root `README.md` must be 1.0-ready:
  - Clear feature list
  - Flavors / engines table with fidelity
  - Screenshots (use the automated ones if available)
  - CI badge
  - How to download from GitHub Releases
  - Build from source instructions
- `docs/CHANGELOG.md` — proper 1.0.0 entry
- `docs/user/` — updated for Compare and any sizing notes if needed
- `docs/development/packaging.md` (or equivalent) — how releases work
- All phase requirements remain under `docs/development/`
- `AGENTS.md` updated
- `HANDOFF.md` completely rewritten with a clear **post-1.0 roadmap**:
  - Debug (step-through) as the main candidate for 1.1
  - More real engines / higher fidelity
  - Website (regexcraft.com)
  - Native installers if desired
  - Any other future ideas

---

## 6. Technical Requirements

- Right-panel width memory must be clean (no magic numbers scattered in code). Prefer a small settings service.
- GitHub Actions must be reliable and use current actions (`actions/checkout`, `actions/setup-dotnet`, softprops/action-gh-release or equivalent).
- All existing tests must pass. Add tests for the new width-persistence logic where practical.
- Continue Serilog, theme variables, multi-flavor architecture, etc.
- Do not commit temporary files or screenshots that are not final documentation assets.

---

## 7. Versioning & Process

- Bump version to **`1.0.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with the post-1.0 plan
- All tests green
- CI green
- Create a clean commit on `main`:  
  `Phase 10 complete: smart Compare panel sizing, GitHub Releases, final polish — 1.0.0`
- After the commit, create and push the `v1.0.0` tag so the release workflow runs (or document exactly how the human should do it).

---

## 8. Definition of Done

- [ ] Right panel expands to a usable width when switching to Compare and restores the previous width when leaving Compare
- [ ] Both widths are remembered across sessions and respect manual splitter drags
- [ ] GitHub Release is created automatically (or via documented tag push) with published binaries attached
- [ ] Release process is documented
- [ ] Final polish pass completed — app feels 1.0 quality
- [ ] README, CHANGELOG, and docs are 1.0-ready
- [ ] Version = 1.0.0
- [ ] AGENTS.md + HANDOFF.md updated with clear post-1.0 roadmap (Debug → 1.1, etc.)
- [ ] All tests pass
- [ ] CI green
- [ ] Clean commit + tag for 1.0.0

---

## 9. Out of Scope for 1.0.0

- Debug / step-through debugger (planned for post-1.0)
- New major engines
- Website implementation
- Full native installers (MSI/DMG) beyond what `dotnet publish` already gives
- Breaking changes

---

**This document is the single source of truth for Phase 10.**  
Deliver a polished 1.0.0 with smart Compare sizing and real GitHub Releases. Debug waits until after 1.0.
