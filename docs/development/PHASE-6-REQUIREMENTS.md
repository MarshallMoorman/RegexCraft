# RegexCraft – Phase 6 Requirements

**Project**: RegexCraft  
**Version after this phase**: `0.7.0`  
**Depends on**: Phase 5 complete (`0.6.0`)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 6

Three concrete fixes + the biggest remaining capability:

1. Persist the theme (and other important settings)
2. Fix Generate tab auto-generation when C# (or any default) is selected
3. Ship a useful default Library of common regular expressions
4. **Major**: Significantly expand multi-flavor support beyond .NET + PCRE2

This phase makes the multi-flavor architecture that was designed in Phase 0 finally pay off for users.

---

## 2. Critical Bug / UX Fixes (Must Fix)

### 2.1 Theme Persistence
- The selected theme (Light / Dark / System) is **not** persisted across application restarts.
- Fix: Save the user’s theme preference and restore it on startup.
- Prefer storing it with the other application settings (JSON file in the proper user config location, or alongside Library/History).
- Also persist other obvious user preferences if easy (window size/position, last used engine, last GREP folder, etc.).

### 2.2 Generate Tab – Auto-generate on Load / Default Language
- When the Generate tab is first shown (or when the app starts with Generate active), C# is selected but **no code is generated** until the user changes the Language dropdown and changes it back.
- Fix:  
  - Automatically generate code whenever the Generate tab becomes active.  
  - Automatically re-generate whenever the current regex, options, engine, or selected language changes.  
  - The default language (C#) must produce correct output immediately without any extra user action.

### 2.3 Default Library Content
- The Library currently starts empty.
- Ship a curated set of **common, high-quality regular expressions** as built-in / read-only (or copy-on-write) entries.
- Suggested starter set (at minimum 12–20 patterns):
  - Email address
  - URL / URI
  - IPv4 / IPv6
  - US/International phone numbers
  - Dates (ISO, US, EU common formats)
  - Time
  - Hex color
  - UUID / GUID
  - Credit card (basic)
  - Strong password
  - HTML/XML tags
  - Whitespace normalizer
  - Common log patterns, etc.
- Each entry should have a clear name, description, the regex, a sample subject, and notes about which engines it works best with.
- Users must still be able to add, edit, and delete their own entries. Built-in ones can be marked “Built-in” and optionally made read-only or “Reset to default”.

---

## 3. Major Feature: Expand Multi-Flavor Support

This is the highest-priority new work.

### 3.1 Goals
- Users can select many more flavors from the Flavor dropdown.
- For every flavor we must support at least:
  - Correct token insertion (hide or mark unsupported tokens)
  - Analysis Tree (as accurate as possible)
  - Code generation (where applicable)
  - Clear indication of testing fidelity

### 3.2 Recommended Flavor Tiers for This Phase

**Tier A – Full or Near-Full Engine Support (highest priority)**
- JavaScript / ECMAScript (modern) — use a high-quality JS engine interop (Jint is pure .NET and good enough for most cases, or ClearScript/V8 if feasible)
- Python (`re` module) — via Python.NET or a well-tested approximation + clear notes
- Java — if a good pure .NET or easy interop option exists; otherwise high-quality approximation

**Tier B – High-Quality Approximation + Excellent Metadata**
- PHP (PCRE-based, can largely share PCRE2 engine with notes)
- Ruby
- Go (RE2 semantics)
- Rust
- Perl (basic)
- .NET (already have) and PCRE2 (already have)

**Tier C – Definition + Codegen + Analysis only (testing falls back with warning)**
- Remaining common ones: TypeScript, Kotlin, Swift, R, etc.

### 3.3 Implementation Requirements
- Extend the existing `FlavorDefinition` / registry system.
- Each flavor must declare:
  - Which engine it maps to (or “approx”)
  - Supported options
  - Token support matrix
  - Codegen templates
  - Human-readable notes about differences
- When a flavor has only approximate testing, the UI must show a clear, non-alarming banner:  
  “Testing uses closest engine (PCRE2 / .NET). Results may differ slightly from real [Flavor].”
- Switching flavors must update options, available tokens, and re-run the current Test/Replace/Generate as appropriate.
- Keep the architecture clean so future engines can be added without rewriting the UI.

### 3.4 Minimum Viable Expansion for Phase 6
At a minimum deliver:
- JavaScript (good testing)
- Python (good or approximate testing)
- Java (approximate or better)
- PHP (share PCRE2 + notes)
- At least 3–4 more with solid definitions + codegen

---

## 4. Other Polish

- Ensure Library UI clearly distinguishes built-in vs user entries.
- Generate panel should feel instant and reliable after the auto-generate fix.
- Any small remaining layout or contrast issues.
- Update status bar / tooltips to mention the current flavor more clearly when it differs from the engine.

---

## 5. Documentation

- Update root README with the expanded flavor list.
- Create or update `docs/user/flavors.md` explaining fidelity levels.
- Update Library docs to mention the built-in patterns.
- Update `docs/CHANGELOG.md`.
- Update architecture docs with the new flavor/engine mapping.
- Keep phase requirements in `docs/development/`.

---

## 6. Technical Requirements

- Theme + settings persistence must use a proper user-scoped location (not next to the exe if possible).
- Default Library can be embedded as JSON/resource and copied to the user Library on first run (or merged).
- New engines must not break existing .NET + PCRE2 behavior.
- All new code covered by NUnit tests where logic exists (flavor resolution, default library seeding, settings load/save, generate auto-trigger).
- Continue Serilog.
- Do **not** commit screenshots.

---

## 7. Versioning & Process

- Bump version to **`0.7.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with next priorities (Debug, more engines, 1.0 release, website, etc.)
- All tests green
- Clean commit on `main`:  
  `Phase 6 complete: theme persistence, Generate auto-run, default Library, expanded multi-flavor support (v0.7.0)`

---

## 8. Definition of Done

- [ ] Theme (Light/Dark/System) is persisted and restored on startup
- [ ] Generate tab automatically produces code for the default language (C#) without requiring a language change
- [ ] Library ships with a solid set of common built-in regular expressions
- [ ] At least JavaScript, Python, Java, PHP + several more flavors are selectable
- [ ] Flavor system clearly communicates testing fidelity
- [ ] Token palette and options respect the selected flavor
- [ ] Code generation works for the new flavors
- [ ] All tests pass
- [ ] Documentation updated
- [ ] Version = 0.7.0
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] Clean commit on main

---

## 9. Out of Scope

- Full Debug stepper
- Every possible obscure flavor at 100% fidelity
- Cloud sync of Library
- Breaking changes to existing .NET/PCRE2 behavior

---

**This document is the single source of truth for Phase 6.**  
The three user-reported issues must be fixed completely, and the multi-flavor expansion is the main new capability.