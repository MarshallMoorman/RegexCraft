# Generating Code

**Generate** mode turns the current pattern, subject, replacement, and options into idiomatic snippets for several languages.

## Open Generate

- Click **Generate** in the toolbar or right panel, or press **Ctrl+4**.

## Options

1. **Language** — C#, JavaScript, Python, PHP, Java, Go, Rust  
2. **Operation** — IsMatch, Match (first), Matches (all), Replace, Split  
3. Review the snippet in the read-only editor  
4. Click **Copy code** to place it on the clipboard  

Snippets update when you change the pattern, subject, replacement, options, language, or operation.

## What is included

- Imports / usings where appropriate  
- Pattern and subject as properly escaped string literals  
- Option flags mapped to the language’s API when possible (e.g. `RegexOptions.IgnoreCase`, `re.IGNORECASE`, JS `i`/`m`/`s` flags)  
- Comments pointing at group access (`Groups[1]`, `match.group(1)`, etc.)  

## Engine notes

RegexCraft tests with **.NET** and **PCRE2**. Generated code targets the **host language’s** native regex engine. Snippets include a short comment naming the **source engine** you used in RegexCraft so you can review dialect differences.

- **C#** → `System.Text.RegularExpressions` (closest to the .NET flavor; includes match timeout)  
- **JavaScript / Python / PHP / Java** → PCRE-like or language-specific engines; advanced constructs may differ  
- **Go** → RE2 (no lookbehind/backreferences) — snippets call this out  
- **Rust** → `regex` crate (RE2-like; no lookaround/backrefs — notes mention `fancy-regex` if needed)  

Always re-test generated code in the target runtime for edge cases.

## Tips

- Prefer **Matches (all)** when you want iteration samples.  
- Set a realistic **replacement** string before generating Replace snippets.  
- Generation requests are logged (language + operation) when you copy.  
