# Regex Flavors & Testing Fidelity

RegexCraft is built around a **flavor registry**: each flavor maps to a concrete testing engine and declares how close live Test / Replace / Split / GREP results are to the real dialect.

## Selecting a flavor

Use the **Flavor** dropdown in the toolbar. Switching flavors:

- Switches the testing engine  
- Rebuilds the token palette (engine-specific tokens dim or drop)  
- Re-runs live Test / Replace / Split  
- Regenerates code snippets  
- May show a **fidelity banner** when testing is approximate  

The status bar shows **Flavor** (with fidelity when not full) and **Engine**.

## Engines

| Engine id | Implementation | Used by |
|-----------|----------------|---------|
| `dotnet` | `System.Text.RegularExpressions` | .NET, Python*, Java*, Go*, Rust*, Kotlin*, Swift* |
| `pcre2` | PCRE.NET (PCRE2) | PCRE2, PHP, Ruby*, Perl* |
| `javascript` | Jint (ECMAScript) | JavaScript, TypeScript |

\* Approximate testing — see fidelity table below.

## Fidelity levels

| Level | Meaning |
|-------|---------|
| **Full** | Native engine for this flavor |
| **High** | Native or same-family engine; minor dialect gaps possible |
| **Approximate** | Closest available engine; results may differ from production |
| **Codegen only** | Reserved for future flavors without a test path |

When fidelity is not **Full**, a blue info banner explains which engine is used.

## Flavor catalog (v0.7)

| Flavor | Engine | Fidelity | Notes |
|--------|--------|----------|-------|
| **.NET** | dotnet | Full | Balancing groups, ExplicitCapture, timeouts |
| **PCRE2** | pcre2 | Full | Possessive quantifiers, rich Perl-like set |
| **JavaScript** | javascript | High | Jint ES engine; modern lookbehind / named groups |
| **TypeScript** | javascript | High | Same RegExp semantics as JS |
| **PHP** | pcre2 | High | PHP `preg_*` is PCRE-based |
| **Python** | dotnet | Approximate | Real `re` is not full PCRE; codegen targets `re` |
| **Java** | dotnet | Approximate | Close for common patterns; dialect still differs |
| **Ruby** | pcre2 | Approximate | Real Ruby uses Onigmo |
| **Go** | dotnet | Approximate | Real RE2 has **no** lookbehind / backrefs |
| **Rust** | dotnet | Approximate | `regex` crate is RE2-like; use `fancy-regex` for lookaround |
| **Perl** | pcre2 | Approximate | PCRE covers much of everyday Perl |
| **Kotlin** | dotnet | Approximate | JVM `Regex` / `java.util.regex` |
| **Swift** | dotnet | Approximate | ICU / Swift Regex differ from .NET |

## Practical guidance

1. Author and debug against the flavor closest to production.  
2. For **Go** / **Rust**, avoid lookbehind and backreferences if you need portable RE2 patterns.  
3. For **Python**, prefer simple `re` features; use the third-party `regex` package in Python when you need PCRE-like power.  
4. Use **Generate** for host-language snippets — they always target the real language API, with comments naming the RegexCraft source engine.  
5. **GREP** uses the same engine as the selected flavor.

## Related

- [Testing regular expressions](testing-regexes.md)  
- [Generating code](generating-code.md)  
- [Architecture](../development/architecture.md)  
