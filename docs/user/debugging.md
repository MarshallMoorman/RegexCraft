# Debugging regex matches

**Debug** mode walks through how a pattern relates to a subject, step by step. It is designed for learning and explaining matches (and misses), not as a cycle-accurate simulator of the engine’s internal NFA.

## Open Debug

- Toolbar or right panel tab: **Debug**
- Keyboard: **Ctrl+7** (⌘+7 on macOS)
- While debugging: **F10** (or **Ctrl+→**) step forward · **F11** (or **Ctrl+←**) step back
- **Ctrl+Enter** rebuilds the session from the current pattern, subject, and options

## Engine support

| Testing engine | Debug |
|----------------|--------|
| **.NET** (`dotnet`) | Supported |
| PCRE2 | Not yet — clear message in the panel |
| JavaScript (Jint) | Not yet — clear message in the panel |

Any **flavor** that uses the .NET engine for testing (including approximate flavors such as Python/Java when they map to `dotnet`) can use Debug. Flavors on PCRE2 or JavaScript show:

> Step-through Debug is currently available only for the .NET engine…

Switch the flavor to **.NET** (or another .NET-backed flavor) to enable stepping.

## What you see

| UI element | Meaning |
|-----------|---------|
| **Step N / M** | Current position in the walk-through |
| **Kind** badge | Start, Overview, Pattern, Attempt, Capture, Match, Fail, Done, … |
| **Status** | OK / Fail / Info |
| **Explanation** | Human-readable description of the step |
| **Pattern focus** | Fragment of the regex under consideration (also selected in the center editor) |
| **Subject focus** | Fragment of the subject under consideration (highlighted in the Debug subject editor) |
| **Step list** | Click any row to jump to that step |
| **Approach note** | Reminds you this is an educational overlay on real Match results |

Controls: **↺** first step · **◀** back · **▶** forward · **⇥** last step · **Refresh** rebuild.

## How it works (approach)

RegexCraft does **not** re-implement the full .NET regex engine for Debug. Instead it builds a **hybrid educational session**:

1. Runs a real **.NET Match** (same results as Test mode).
2. Walks the **Analysis Tree** (pattern structure) left to right.
3. Overlays real **match offsets** and **capture groups** onto those steps.

So:

- Match counts, group values, and positions match live Test for the .NET engine.
- Intermediate “applying this construct” steps teach structure; some subject ranges for meta-heavy fragments are **estimated** within the successful match span.
- Failed searches explain the pattern structure and report “no match” from the real engine.

## Tips

- Use the **Analysis Tree** in the center column alongside Debug — clicking a node still selects that range in the pattern.
- Keep **Test** results in mind: Debug re-runs Match when you open or refresh Debug so both stay aligned.
- For multi-match subjects, the session walks **each** match (attempt → pattern nodes → captures → success).
- Invalid patterns produce Error steps with the engine’s message; fix the pattern, then Refresh.

## Limitations (1.1)

- No step-through for PCRE2 / JavaScript yet (architecture allows adding more engines later).
- Not a perfect backtracking / NFA simulation (no guaranteed “engine cycle” fidelity).
- Lookarounds, complex quantifier retries, and catastrophic backtracking are **not** simulated step-by-step.
- Subject positions for some abstract nodes are approximate within the match.

See also: [Testing regular expressions](testing-regexes.md), [Flavors & fidelity](flavors.md).
