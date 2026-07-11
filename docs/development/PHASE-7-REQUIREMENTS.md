# RegexCraft – Phase 7 Requirements

**Project**: RegexCraft  
**Version after this phase**: `0.8.0`  
**Depends on**: Phase 6 complete (`0.7.0`)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 7

Three main themes:

1. **Branding & About** — proper application icon + custom About dialog (no more “About Avalonia”)
2. **Massive increase in automated testing** — unit + headless UI tests + automatic screenshot capture for documentation
3. Polish any remaining rough edges discovered during testing

This phase makes the project much more maintainable and professional.

---

## 2. Branding Fixes (Critical)

### 2.1 Application Icon
- Create or add a proper **RegexCraft** application icon (multi-resolution .ico / .icns / PNG set).
- Prefer a clean, modern icon that works well on light and dark backgrounds and matches the blue theme.
- Set it as:
  - Window.Icon
  - Application icon (so it appears in the dock/taskbar, window title bar, and About)
  - macOS .icns / Windows .ico as needed for publish
- If generating an icon is hard for the agent, use a high-quality placeholder (simple blue “RC” monogram or regex-related symbol) and document how to replace it later. Ideal if a good SVG → multi-size icon can be produced.

### 2.2 About Dialog
- Completely replace the default Avalonia About dialog.
- New About dialog must show:
  - RegexCraft name + version
  - Short description
  - Your name / copyright
  - Link to regexcraft.com (and/or GitHub)
  - “Built with Avalonia” credit (small, secondary)
  - Proper RegexCraft icon
- Menu item must read **“About RegexCraft”** (not “About Avalonia”).
- The dialog should match the blue theme and look professional in both light and dark modes.

### 2.3 Other Branding
- Ensure the application name is consistently “RegexCraft” everywhere (window title, menus, About, logs, etc.).

---

## 3. Automated Testing & Screenshot Capture (Highest Priority New Work)

Manual testing is too slow. We need strong automation.

### 3.1 Expand NUnit Unit Tests
- Aim for high coverage of Core + Engines + ViewModels + services.
- Especially: flavor resolution, code generation for all languages, Library seeding, settings persistence, GREP logic, analysis tree, token insertion, replace/split edge cases.
- Use meaningful categories (`[Category("Engines")]`, `[Category("Generate")]`, `[Category("Library")]`, etc.).

### 3.2 Avalonia Headless UI Tests
- Use the official **Avalonia.Headless** + **Avalonia.Headless.NUnit** packages.
- Write tests that:
  - Launch the main window
  - Switch modes (Test / Replace / Generate / GREP…)
  - Change flavor
  - Enter regex + subject and verify matches/groups appear
  - Open Library / History
  - Verify theme switching
  - Verify Generate produces output for the default language
  - Exercise the new About dialog

### 3.3 Automatic Screenshot Capture for Docs
Avalonia Headless supports rendering frames:

```csharp
var frame = window.CaptureRenderedFrame(); // requires Skia + UseHeadlessDrawing = false
frame.Save("screenshot.png");
```

Requirements:
- Create a set of **screenshot tests** (or a dedicated test class) that:
  - Open the main window in known states (Test with sample data, Replace, Generate, GREP, light theme, dark theme, About dialog, etc.)
  - Capture high-quality PNGs
  - Save them into `docs/screenshots/` (or `docs/user/images/`)
- These screenshots should be usable in the root README and user documentation.
- Make the screenshot generation easy to run (`dotnet test --filter Category=Screenshots` or a dedicated target).
- Document how to regenerate screenshots.
- Optionally compare against baseline images later (visual regression), but the minimum is reliable generation of good screenshots.

### 3.4 Test Infrastructure
- Ensure tests run cleanly on CI (headless).
- Fast feedback for unit tests; headless + screenshot tests can be a separate category.
- All tests must pass before the phase is complete.

---

## 4. Other Improvements

- Any small bugs found while writing the new tests should be fixed.
- Improve error messages and empty states if the tests reveal gaps.
- Make sure the new About dialog and icon look perfect in both themes.

---

## 5. Documentation

- Update root README with real screenshots (from the new automated capture).
- Add a short section on “Running the tests” and “Regenerating screenshots”.
- Update `docs/user/` as needed.
- Update `docs/CHANGELOG.md`.
- Keep phase requirements in `docs/development/`.

---

## 6. Technical Requirements

- Use Avalonia.Headless + Avalonia.Headless.NUnit (matching Avalonia 12).
- Enable Skia for frame capture when needed.
- Icon assets must be properly included as Avalonia resources / embedded.
- About dialog should be a proper Window or ContentDialog that matches the app style.
- Do **not** commit temporary or low-quality screenshots; only the final generated ones that look good.
- Continue Serilog and all previous conventions.

---

## 7. Versioning & Process

- Bump version to **`0.8.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with next priorities (more engines, Debug, 1.0, website, packaging, etc.)
- All tests green (including new headless + screenshot tests)
- Clean commit on `main`:  
  `Phase 7 complete: app icon, custom About dialog, expanded automated testing + screenshot capture (v0.8.0)`

---

## 8. Definition of Done

- [ ] Proper RegexCraft application icon is set and visible
- [ ] Menu shows “About RegexCraft”
- [ ] Custom About dialog looks professional and shows correct info + icon
- [ ] Large expansion of NUnit unit tests
- [ ] Avalonia Headless UI tests covering main workflows
- [ ] Automated screenshot capture that produces good images for docs/README
- [ ] Screenshots are integrated into documentation
- [ ] All tests pass
- [ ] Version = 0.8.0
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] Clean commit on main

---

## 9. Out of Scope

- Full Debug stepper
- New major engines (can be next)
- Website
- Full installer packaging

---

**This document is the single source of truth for Phase 7.**  
Focus on branding (icon + About) and making the test suite so strong that manual testing becomes minimal.