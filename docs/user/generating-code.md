# Generating Code

**Generate** mode turns the current pattern, subject, replacement, and options into idiomatic snippets for many languages.

## Open Generate

- Click **Generate** in the toolbar or right panel, or press **Ctrl+4**.

Code for the **default language (C#)** appears immediately — no need to change the language dropdown first.

## Options

1. **Language** — C#, JavaScript, TypeScript, Python, PHP, Java, Go, Rust, Ruby, Perl, Kotlin, Swift  
2. **Operation** — IsMatch, Match (first), Matches (all), Replace, Split  
3. Review the snippet in the read-only editor  
4. Click **Copy code** to place it on the clipboard  

Snippets update automatically when you change the pattern, subject, replacement, options, **flavor**, language, or operation.

## What is included

- Imports / usings where appropriate  
- Pattern and subject as properly escaped string literals  
- Option flags mapped to the language’s API when possible (e.g. `RegexOptions.IgnoreCase`, `re.IGNORECASE`, JS `i`/`m`/`s` flags)  
- Comments pointing at group access (`Groups[1]`, `match.group(1)`, etc.)  
- A short comment naming the **source engine** used in RegexCraft  

## Engine notes

RegexCraft can **test** with .NET, PCRE2, and JavaScript (Jint), plus approximate testing for other flavors. Generated code always targets the **host language’s** native API.

| Language | Runtime API | Caveats |
|----------|-------------|---------|
| C# | `System.Text.RegularExpressions` | Closest to .NET flavor; includes match timeout |
| JavaScript / TypeScript | `RegExp` | ES2018+ for lookbehind / named groups |
| Python | `re` | Not full PCRE; consider `regex` package for advanced work |
| PHP | `preg_*` | PCRE under the hood |
| Java / Kotlin | `Pattern` / `Regex` | JVM dialect |
| Go | `regexp` (RE2) | No lookbehind / backreferences |
| Rust | `regex` crate | RE2-like; notes mention `fancy-regex` |
| Ruby | `Regexp` | Onigmo dialect |
| Perl | `m//` `s///` | Illustrative snippets |
| Swift | `NSRegularExpression` | ICU-based |

Always re-test generated code in the target runtime for edge cases. See also [Flavors & testing fidelity](flavors.md).

## Tips

- Prefer **Matches (all)** when you want iteration samples.  
- Set a realistic **replacement** string before generating Replace snippets.  
- Switch flavor before generating if you want the “source engine” comment to match your target dialect family.  
- Generation requests are logged (language + operation) when you copy.  
