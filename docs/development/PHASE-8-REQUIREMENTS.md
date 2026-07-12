# RegexCraft – Phase 8 Requirements

**Project**: RegexCraft  
**Version after this phase**: `0.9.0`  
**Depends on**: Phase 7 complete (`0.8.0`)  
**Date**: 2026-07-11  

---

## 1. Goal of Phase 8

Close the biggest remaining gaps toward practical RegexBuddy parity:

1. Harden and expand multi-flavor support
2. Add **significant automated tests per flavor / engine**
3. Improve fidelity communication and token/option accuracy per flavor
4. Raise overall test confidence dramatically

This phase is about depth and correctness rather than brand-new major UI features.

---

## 2. Multi-Flavor Improvements

### 2.1 Real Engines
- Keep and strengthen the three existing real engines (.NET, PCRE2, JavaScript/Jint).
- Evaluate and, if practical within reasonable effort, add or improve:
  - Better JavaScript fidelity (update Jint or document exact limitations).
  - Python support (Python.NET or high-quality pure approximation with clear notes).
  - RE2 if a maintained .NET wrapper exists and is easy to integrate.
- Any new engine must have Full or High fidelity tests.

### 2.2 Flavor Definitions
- Expand and make accurate the `FlavorDefinition` (or equivalent) data for every selectable flavor.
- Each flavor must declare:
  - Mapped engine
  - Fidelity level (Full / High / Approximate)
  - Supported / unsupported options
  - Token support (which tokens are available or should be hidden/disabled)
  - Known important behavioral differences
  - Codegen language mapping
- When a user selects an Approximate flavor, the UI banner must be clear and accurate.
- Token palette must respect the current flavor (disable or hide unsupported tokens).

### 2.3 Minimum Flavor Set to Solidify
Ensure excellent support (definitions + tests) for at least:
- .NET (Full)
- PCRE2 (Full)
- JavaScript + TypeScript (High)
- PHP (High)
- Python (High or best possible Approximate)
- Java (best possible)
- Ruby, Go, Rust, Perl (clear Approximate with good notes)

Add 2–4 more if easy (Kotlin, Swift, etc. already listed).

---

## 3. Significant Tests Per Flavor / Engine (Critical)

This is the #1 quality goal of the phase.

### Requirements
- Create dedicated test classes or clear test groups for each major flavor/engine.
- For every **real engine**:
  - Comprehensive Match, Replace, Split tests
  - Named groups, options, edge cases, Unicode, large input, invalid patterns
  - Performance smoke tests if useful
- For every **selectable flavor**:
  - At least a core set of tests that prove the mapping works
  - Tests that verify fidelity banner appears when expected
  - Tests that verify tokens/options are correctly enabled/disabled
  - Codegen tests that the generated code is valid for that flavor’s target language
- Behavioral difference tests (examples):
  - Constructs that work in PCRE2 but not in .NET (or vice versa)
  - RE2 limitations (no backreferences, limited lookaround)
  - JS specific behavior
- Aim for the test suite to give high confidence that switching flavors does not silently produce wrong results.
- Keep using NUnit categories so we can run `dotnet test --filter Category=Flavors` or `Category=Engines` easily.
- Headless UI tests should also exercise flavor switching and verify the banner / token state.

### Success Metric
After this phase it should be honest to say:  
“We have significant automated tests for every selectable flavor and deep tests for every real engine.”

---

## 4. Library & Documentation

- Review built-in Library patterns. Add or update entries with clear “Recommended flavors / Engines” notes.
- Expand `docs/user/flavors.md` with accurate fidelity tables and practical advice.
- Update root README flavor section.
- Document how to run the flavor/engine test subsets.

---

## 5. Other Hardening

- Fix any bugs found while writing the new tests.
- Ensure GREP respects the selected flavor’s engine correctly.
- Make sure Generate produces correct, idiomatic code for the newly solidified flavors.
- Any remaining small UX issues around flavor selection.

---

## 6. Technical Requirements

- Do not break existing .NET / PCRE2 / JS behavior.
- New engines must be optional and cleanly integrated via the existing factory/abstraction.
- All new tests must be deterministic and fast enough for regular use.
- Continue Serilog, theme variables, settings persistence, etc.
- Do not commit screenshots unless they are final documentation assets.

---

## 7. Versioning & Process

- Bump version to **`0.9.0`** in `Directory.Build.props`
- Update root `AGENTS.md`
- Completely rewrite root `HANDOFF.md` with clear path to 1.0 (Debug, packaging, remaining engines, website, etc.)
- All tests green
- Clean commit on `main`:  
  `Phase 8 complete: multi-flavor hardening, significant per-flavor tests, fidelity improvements (v0.9.0)`

---

## 8. Definition of Done

- [ ] Flavor definitions are accurate and complete for all selectable flavors
- [ ] Token palette and options respect the selected flavor
- [ ] Fidelity banners are correct and helpful
- [ ] Significant automated tests exist for every real engine (deep) and every selectable flavor (core + mapping)
- [ ] Codegen tested per major flavor/language
- [ ] Library entries note flavor compatibility where useful
- [ ] `docs/user/flavors.md` and README are accurate
- [ ] All tests pass
- [ ] Version = 0.9.0
- [ ] AGENTS.md + HANDOFF.md updated
- [ ] Clean commit on main

---

## 9. Out of Scope

- Full Debug stepper
- Perfect fidelity for every obscure RegexBuddy flavor
- New major UI features
- Website or installers

---

**This document is the single source of truth for Phase 8.**  
Focus on making multi-flavor support trustworthy through better definitions and significant automated tests per flavor.