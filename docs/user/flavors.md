# Regex Flavors & Testing Fidelity

RegexCraft is built around a **flavor registry**: each flavor maps to a concrete testing engine and declares how close live Test / Replace / Split / GREP results are to the real dialect.

## Selecting a flavor

Use the **Flavor** dropdown in the toolbar. Switching flavors:

- Switches the testing engine  
- Rebuilds the token palette (unsupported tokens are dimmed)  
- Enables / disables options that the flavor does not support  
- Re-runs live Test / Replace / Split  
- Selects the preferred **Generate** language for that flavor  
- May show a **fidelity banner** when testing is not full fidelity  

The status bar shows **Flavor** (with fidelity when not full) and **Engine**.

## Engines

| Engine id | Implementation | Used by | Debug step-through |
|-----------|----------------|---------|--------------------|
| `dotnet` | `System.Text.RegularExpressions` | .NET, Python*, Java*, Go*, Rust*, Kotlin*, Swift* | **Yes** (1.1+) |
| `pcre2` | PCRE.NET (PCRE2) | PCRE2, PHP, Ruby*, Perl* | Not yet |
| `javascript` | Jint (ECMAScript) | JavaScript, TypeScript | Not yet |

**Debug** is engine-based: any flavor whose testing engine is `.NET` can use the Debug tab. PCRE2 and JavaScript show a clear “not available” message. See [Debugging matches](debugging.md).

\* Approximate testing — see fidelity table below.

### Engine evaluation notes (Phase 8)

| Candidate | Decision | Reason |
|-----------|----------|--------|
| **Jint (JS)** | Kept & strengthened | High fidelity for modern ES; lookbehind, named groups, `s`/`u` flags covered by tests |
| **Python.NET** | Not integrated | Requires embedding CPython; poor fit for portable Avalonia packaging |
| **RE2.Managed** | Not integrated | Last meaningful NuGet activity ~2023; RE2 limits expressed via flavor token/option matrices + notes for Go/Rust instead |

## Fidelity levels

| Level | Meaning |
|-------|---------|
| **Full** | Native engine for this flavor |
| **High** | Native or same-family engine; minor dialect gaps possible |
| **Approximate** | Closest available engine; results may differ from production |
| **Codegen only** | Reserved for future flavors without a test path |

When fidelity is not **Full**, a blue info banner explains which engine is used and what may differ.

## Flavor catalog (v0.9)

| Flavor | Engine | Fidelity | Preferred codegen | Key notes |
|--------|--------|----------|-------------------|-----------|
| **.NET** | dotnet | Full | C# | Balancing groups, ExplicitCapture, timeouts |
| **PCRE2** | pcre2 | Full | C# | Possessive quantifiers, atomic groups; ExplicitCapture approximate |
| **JavaScript** | javascript | High | JavaScript | Jint ES; no free-spacing / ExplicitCapture / possessive |
| **TypeScript** | javascript | High | TypeScript | Same RegExp semantics as JS |
| **PHP** | pcre2 | High | PHP | `preg_*` is PCRE-based; delimiter packaging may differ |
| **Python** | dotnet | Approximate | Python | Real `re` is not full PCRE; codegen targets `re` |
| **Java** | dotnet | Approximate | Java | Close for common patterns; real Java has possessive |
| **Ruby** | pcre2 | Approximate | Ruby | Real Ruby uses Onigmo |
| **Go** | dotnet | Approximate | Go | Real RE2: **no** lookaround / backrefs — tokens dimmed |
| **Rust** | dotnet | Approximate | Rust | `regex` crate is RE2-like; use `fancy-regex` for lookaround |
| **Perl** | pcre2 | Approximate | Perl | PCRE covers everyday Perl |
| **Kotlin** | dotnet | Approximate | Kotlin | JVM `Regex` / `java.util.regex` |
| **Swift** | dotnet | Approximate | Swift | ICU / Swift Regex differ from .NET |

## Options per flavor

| Option | .NET | PCRE2 | JS/TS | PHP | Python | Java | Go/Rust | Ruby/Perl | Kotlin | Swift |
|--------|------|-------|-------|-----|--------|------|---------|-----------|--------|-------|
| Ignore case | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Multiline | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Singleline | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Explicit capture | ✓ | ~ | — | ~ | — | — | — | ~ / ~ | — | — |
| Ignore pattern whitespace | ✓ | ✓ | — | ✓ | ✓ | ✓ | — | ✓ | ✓ | — |

✓ supported · ~ approximate · — not available (checkbox disabled)

## Token palette awareness

Tokens are **dimmed** when unsupported for the selected flavor:

- **Go / Rust (RE2-like)**: lookaround, backreferences, possessive, atomic, conditionals, balancing  
- **JavaScript / TypeScript**: possessive, atomic, conditionals, balancing, free-spacing `(?x)`, `\A`/`\z`/`\Z`/`\G`  
- **Python**: possessive, atomic, balancing, .NET-only constructs  
- **.NET-only** tokens (balancing, ExplicitCapture inline): dimmed on non-.NET engines  

Common constructs (`\d`, groups, anchors, basic quantifiers) remain available on every flavor.

## Practical guidance

1. Author and debug against the flavor closest to production.  
2. For **Go** / **Rust**, avoid lookaround and backreferences if you need portable RE2 patterns — the palette dims these for you.  
3. For **Python**, prefer simple `re` features; use the third-party `regex` package when you need PCRE-like power.  
4. Use **Generate** for host-language snippets — switching flavor auto-selects the preferred language.  
5. **GREP** uses the same engine as the selected flavor.  
6. Trust **Full** and **High** results most; treat **Approximate** as guidance and re-test in the real runtime when correctness matters.

## Running flavor / engine tests

```bash
# Deep tests for every real engine + significant per-flavor coverage
dotnet test --filter Category=Engines
dotnet test --filter Category=Flavors

# Combined
dotnet test --filter "Category=Engines|Category=Flavors"
```

## Related

- [Testing regular expressions](testing-regexes.md)  
- [Generating code](generating-code.md)  
- [Architecture](../development/architecture.md)  
