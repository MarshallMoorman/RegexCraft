# Theme & Appearance

RegexCraft ships **System**, **Light**, and **Dark** themes with a consistent blue design language.

## Switching themes

Use the **Theme** button in the toolbar to cycle:

**System → Light → Dark → …**

Your last choice is saved in `settings.json` and restored on the next launch (including after a full quit). Theme cycles **System → Light → Dark → System** based on your saved preference, not the OS’s momentary effective variant.

## What is themed

All UI colors come from named resources in `Themes/Colors.axaml` (no hard-coded panel colors):

| Area | Notes |
|------|--------|
| Chrome | Toolbar, tabs, options strip, status bar |
| Editors | Pattern, subject, replace preview, generate, GREP preview |
| Syntax | Groups, named groups, character classes, quantifiers, escapes, anchors, lookarounds, alternation, comments |
| Highlights | Match and capture-group backgrounds in subject / GREP preview |

Light mode uses a near-black editor foreground and high-contrast syntax colors so complex patterns stay readable. Dark mode uses a vivid palette on near-black surfaces.

## Branding

- Window and taskbar/dock use the **RegexCraft** application icon (blue “RC” monogram).  
- **About RegexCraft** (Help menu / application menu) shows the icon, version, copyright, and links in both themes.

## Tips

- Prefer **Light** or **Dark** explicitly when taking screenshots so system appearance does not change mid-capture.  
- Automated screenshots for docs: `dotnet test --filter Category=Screenshots` → `docs/screenshots/`.  
- Token category panels on the left are full-width and equal width for a clean palette layout.  
- Right-hand modes (Test / Replace / Split / Generate / GREP) share the same panel host so previews and lists fill the available height in both themes.  
- Drag the column splitters to balance sidebar, editor, and mode panel width.  
- If something looks washed out after a theme change, toggle the theme once more — editors reapply brushes on every theme switch.  

