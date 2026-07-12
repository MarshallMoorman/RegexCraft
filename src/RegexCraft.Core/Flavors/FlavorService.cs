using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;
using RegexCraft.Core.Models;

namespace RegexCraft.Core.Flavors;

/// <summary>
/// In-memory flavor registry mapping flavors to engines with fidelity metadata.
/// </summary>
public sealed class FlavorService : IFlavorService
{
    private readonly IReadOnlyList<FlavorDefinition> _flavors;
    private readonly Dictionary<string, IRegexEngine> _enginesById;
    private readonly ILogger<FlavorService> _logger;

    public FlavorService(IEnumerable<IRegexEngine> engines, ILogger<FlavorService>? logger = null)
    {
        _logger = logger ?? NullLogger<FlavorService>.Instance;
        _enginesById = engines.ToDictionary(e => e.Id, StringComparer.OrdinalIgnoreCase);

        // Only expose flavors whose testing engine is registered (keeps UI clean if an engine is missing).
        _flavors = BuildDefaultFlavors()
            .Where(f => _enginesById.ContainsKey(f.EngineId))
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _logger.LogDebug(
            "FlavorService initialized with {EngineCount} engines and {FlavorCount} flavors",
            _enginesById.Count,
            _flavors.Count);
    }

    public IReadOnlyList<FlavorDefinition> GetFlavors() => _flavors;

    public FlavorDefinition? GetFlavor(string flavorId) =>
        _flavors.FirstOrDefault(f => string.Equals(f.Id, flavorId, StringComparison.OrdinalIgnoreCase));

    public IRegexEngine? GetEngineForFlavor(string flavorId)
    {
        var flavor = GetFlavor(flavorId);
        if (flavor is null)
        {
            _logger.LogWarning("Unknown flavor id: {FlavorId}", flavorId);
            return null;
        }

        return GetEngine(flavor.EngineId);
    }

    public IRegexEngine? GetEngine(string engineId)
    {
        if (_enginesById.TryGetValue(engineId, out var engine))
            return engine;

        _logger.LogWarning("Unknown engine id: {EngineId}", engineId);
        return null;
    }

    public IReadOnlyList<IRegexEngine> GetEngines() => _enginesById.Values.ToList();

    /// <summary>
    /// Default flavor catalog. Each flavor maps to a registered engine
    /// (dotnet, pcre2, or javascript) and declares testing fidelity, options,
    /// token support, codegen language, and known differences.
    /// </summary>
    public static IReadOnlyList<FlavorDefinition> BuildDefaultFlavors() =>
    [
        new FlavorDefinition
        {
            Id = "dotnet",
            DisplayName = ".NET",
            EngineId = "dotnet",
            Description = "System.Text.RegularExpressions (.NET)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Full,
            Notes = "Native .NET regex. Balancing groups, ExplicitCapture, and MatchTimeout are available.",
            SupportedOptions = FlavorDefinition.AllCommonOptions,
            UnsupportedTokenIds = Array.Empty<string>(),
            CodegenLanguageId = "csharp",
            KnownDifferences =
            [
                "Native engine — results match production System.Text.RegularExpressions.",
                "Supports balancing groups and ExplicitCapture.",
                "Does not support possessive quantifiers or recursive patterns.",
            ],
            SortOrder = 0,
        },
        new FlavorDefinition
        {
            Id = "pcre2",
            DisplayName = "PCRE2",
            EngineId = "pcre2",
            Description = "Perl Compatible Regular Expressions (PCRE2 via PCRE.NET)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Full,
            Notes = "Native PCRE2. Possessive quantifiers, atomic groups, and many Perl-like constructs are supported.",
            SupportedOptions = FlavorDefinition.AllCommonOptions,
            ApproximateOptions = RegexOptionsEx.ExplicitCapture,
            UnsupportedTokenIds = FlavorTokenSets.DotNetOnly,
            CodegenLanguageId = "csharp",
            KnownDifferences =
            [
                "Native PCRE2 via PCRE.NET.",
                "ExplicitCapture is approximate (PCRE PCRE2_NO_AUTO_CAPTURE).",
                "No .NET balancing groups.",
            ],
            SortOrder = 1,
        },
        new FlavorDefinition
        {
            Id = "javascript",
            DisplayName = "JavaScript",
            EngineId = "javascript",
            Description = "ECMAScript RegExp via Jint",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.High,
            FidelityNote =
                "Testing uses an embedded ECMAScript engine (Jint). Results match modern JS closely; host engines (V8, SpiderMonkey, JavaScriptCore) may still differ slightly.",
            Notes =
                "ES2018+ features (lookbehind, named groups, `s` flag) require modern runtimes. No recursive patterns, possessive quantifiers, or free-spacing mode.",
            SupportedOptions = FlavorDefinition.JavaScriptOptions,
            UnsupportedTokenIds = FlavorTokenSets.JavaScriptUnsupported,
            CodegenLanguageId = "javascript",
            KnownDifferences =
            [
                "No free-spacing / IgnorePatternWhitespace (no x flag in standard JS).",
                "No ExplicitCapture option.",
                "No possessive quantifiers, atomic groups, or conditionals.",
                "No \\A, \\z, \\Z, or \\G anchors.",
                "Named replacement ${name} is mapped to $<name> for Jint testing.",
                "Unicode property escapes auto-enable the u flag in RegexCraft.",
            ],
            SortOrder = 10,
        },
        new FlavorDefinition
        {
            Id = "typescript",
            DisplayName = "TypeScript",
            EngineId = "javascript",
            Description = "TypeScript / JavaScript RegExp (same engine as JavaScript)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.High,
            FidelityNote =
                "Testing uses the JavaScript engine (Jint). TypeScript has no separate regex dialect.",
            Notes = "Same RegExp semantics as JavaScript; codegen emits TypeScript-flavored snippets.",
            SupportedOptions = FlavorDefinition.JavaScriptOptions,
            UnsupportedTokenIds = FlavorTokenSets.JavaScriptUnsupported,
            CodegenLanguageId = "typescript",
            KnownDifferences =
            [
                "Identical regex semantics to JavaScript.",
                "Codegen prefers TypeScript-typed snippets.",
            ],
            SortOrder = 11,
        },
        new FlavorDefinition
        {
            Id = "python",
            DisplayName = "Python",
            EngineId = "dotnet",
            Description = "Python re module (approximate testing via .NET)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Results may differ from real Python re. Python.NET was evaluated but not integrated (requires embedding CPython).",
            Notes =
                "Python re is not full PCRE. For PCRE-like features in Python, use the third-party `regex` package. Codegen targets `re`.",
            SupportedOptions =
                RegexOptionsEx.IgnoreCase
                | RegexOptionsEx.Multiline
                | RegexOptionsEx.Singleline
                | RegexOptionsEx.IgnorePatternWhitespace,
            UnsupportedTokenIds = FlavorTokenSets.PythonReUnsupported,
            CodegenLanguageId = "python",
            KnownDifferences =
            [
                "Real Python re has no possessive quantifiers or atomic groups.",
                "Variable-length lookbehind is limited in re (stricter than .NET).",
                "Named groups use (?P<name>...) in Python; RegexCraft tests .NET (?<name>) syntax.",
                "Inline flags and verbose mode (re.VERBOSE) differ in edge cases.",
                "No balancing groups or ExplicitCapture.",
            ],
            SortOrder = 20,
        },
        new FlavorDefinition
        {
            Id = "java",
            DisplayName = "Java",
            EngineId = "dotnet",
            Description = "java.util.regex (approximate testing via .NET)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Results may differ from real Java Pattern (possessive/Unicode edge cases).",
            Notes =
                "Java Pattern is close to Perl/PCRE for common constructs; possessive quantifiers exist in real Java but are not native to the .NET test engine.",
            SupportedOptions =
                RegexOptionsEx.IgnoreCase
                | RegexOptionsEx.Multiline
                | RegexOptionsEx.Singleline
                | RegexOptionsEx.IgnorePatternWhitespace,
            UnsupportedTokenIds = FlavorTokenSets.JavaUnsupported,
            CodegenLanguageId = "java",
            KnownDifferences =
            [
                "Real Java supports possessive quantifiers and atomic groups; .NET test engine does not.",
                "Named group syntax and backreference forms differ slightly.",
                "Unicode character classes and \\X differ between JVM and .NET.",
                "No .NET balancing groups.",
            ],
            SortOrder = 21,
        },
        new FlavorDefinition
        {
            Id = "php",
            DisplayName = "PHP",
            EngineId = "pcre2",
            Description = "PHP PCRE (preg_*) — shares PCRE2 engine",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.High,
            FidelityNote =
                "Testing uses PCRE2 (same family as PHP preg). PHP delimiter/modifier packaging and some UTF-8 edge cases may still differ.",
            Notes = "PHP uses PCRE under the hood. Codegen emits preg_* with ~ delimiters.",
            SupportedOptions = FlavorDefinition.AllCommonOptions,
            ApproximateOptions = RegexOptionsEx.ExplicitCapture,
            UnsupportedTokenIds = FlavorTokenSets.DotNetOnly,
            CodegenLanguageId = "php",
            KnownDifferences =
            [
                "PHP wraps patterns in delimiters (e.g. ~pattern~iu); RegexCraft tests the raw PCRE pattern.",
                "preg modifiers map to PCRE options; not every PHP-specific quirk is modeled.",
                "Replacement uses $n / ${n} in PHP; codegen shows PHP style.",
            ],
            SortOrder = 22,
        },
        new FlavorDefinition
        {
            Id = "ruby",
            DisplayName = "Ruby",
            EngineId = "pcre2",
            Description = "Ruby Regexp (approximate — Onigmo-like via PCRE2)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (PCRE2). Results may differ from real Ruby Onigmo.",
            Notes = "Ruby uses Onigmo; many common constructs align with PCRE. Named groups and lookarounds are supported in modern Ruby.",
            SupportedOptions =
                RegexOptionsEx.IgnoreCase
                | RegexOptionsEx.Multiline
                | RegexOptionsEx.Singleline
                | RegexOptionsEx.IgnorePatternWhitespace,
            UnsupportedTokenIds = FlavorTokenSets.DotNetOnly,
            CodegenLanguageId = "ruby",
            KnownDifferences =
            [
                "Real Ruby uses Onigmo, not PCRE2 — character properties and some quantifiers differ.",
                "Ruby has unique free-spacing and comment rules.",
                "String#scan / MatchData APIs differ from PCRE match objects.",
            ],
            SortOrder = 30,
        },
        new FlavorDefinition
        {
            Id = "go",
            DisplayName = "Go",
            EngineId = "dotnet",
            Description = "Go regexp (RE2) — approximate testing",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Real Go RE2 has no lookaround, backreferences, or recursion. Dedicated RE2 wrappers were evaluated (RE2.Managed last updated 2023) and not integrated.",
            Notes = "RE2 is linear-time and deliberately limited. Prefer patterns without backrefs/lookaround for Go. Token palette dims RE2-unsupported constructs.",
            SupportedOptions =
                RegexOptionsEx.IgnoreCase
                | RegexOptionsEx.Multiline
                | RegexOptionsEx.Singleline,
            UnsupportedTokenIds = FlavorTokenSets.Re2Unsupported,
            CodegenLanguageId = "go",
            KnownDifferences =
            [
                "RE2 forbids backreferences and all lookaround.",
                "No possessive quantifiers, atomic groups, or recursion.",
                "RE2 is guaranteed linear time; .NET may backtrack.",
                "Codegen targets Go's regexp package (RE2).",
            ],
            SortOrder = 31,
        },
        new FlavorDefinition
        {
            Id = "rust",
            DisplayName = "Rust",
            EngineId = "dotnet",
            Description = "Rust regex crate (RE2-like) — approximate testing",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Real Rust `regex` crate has no lookaround or backreferences (use fancy-regex for those).",
            Notes = "Use the `fancy-regex` crate in Rust when you need lookaround/backrefs. Token palette dims RE2-unsupported constructs.",
            SupportedOptions =
                RegexOptionsEx.IgnoreCase
                | RegexOptionsEx.Multiline
                | RegexOptionsEx.Singleline,
            UnsupportedTokenIds = FlavorTokenSets.Re2Unsupported,
            CodegenLanguageId = "rust",
            KnownDifferences =
            [
                "Rust regex crate is RE2-like: no backrefs or lookaround.",
                "fancy-regex adds lookaround/backrefs with different performance characteristics.",
                "Unicode classes are strong in Rust regex; still approximate under .NET testing.",
            ],
            SortOrder = 32,
        },
        new FlavorDefinition
        {
            Id = "perl",
            DisplayName = "Perl",
            EngineId = "pcre2",
            Description = "Perl regular expressions (approximate via PCRE2)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (PCRE2). Full Perl has additional constructs (code embeds, more recursion forms) not covered here.",
            Notes = "PCRE was inspired by Perl; basic and intermediate patterns usually transfer well.",
            SupportedOptions = FlavorDefinition.AllCommonOptions,
            ApproximateOptions = RegexOptionsEx.ExplicitCapture,
            UnsupportedTokenIds = FlavorTokenSets.DotNetOnly,
            CodegenLanguageId = "perl",
            KnownDifferences =
            [
                "Full Perl allows embedded code and more recursive patterns than PCRE2.",
                "PCRE covers everyday Perl well; advanced dialect features may differ.",
            ],
            SortOrder = 33,
        },
        new FlavorDefinition
        {
            Id = "kotlin",
            DisplayName = "Kotlin",
            EngineId = "dotnet",
            Description = "Kotlin / JVM regex (approximate)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Results may differ from real Kotlin/JVM Pattern.",
            Notes = "Kotlin uses java.util.regex on JVM. Codegen emits Kotlin-friendly snippets.",
            SupportedOptions =
                RegexOptionsEx.IgnoreCase
                | RegexOptionsEx.Multiline
                | RegexOptionsEx.Singleline
                | RegexOptionsEx.IgnorePatternWhitespace,
            UnsupportedTokenIds = FlavorTokenSets.JavaUnsupported,
            CodegenLanguageId = "kotlin",
            KnownDifferences =
            [
                "JVM Pattern semantics (same family as Java flavor).",
                "Kotlin stdlib Regex is a thin wrapper over java.util.regex.",
            ],
            SortOrder = 40,
        },
        new FlavorDefinition
        {
            Id = "swift",
            DisplayName = "Swift",
            EngineId = "dotnet",
            Description = "Swift Regex / NSRegularExpression (approximate)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Results may differ from Apple ICU / Swift Regex.",
            Notes = "Modern Swift has Regex builders and ICU-based matching; dialect differs from .NET/PCRE.",
            SupportedOptions =
                RegexOptionsEx.IgnoreCase
                | RegexOptionsEx.Multiline
                | RegexOptionsEx.Singleline,
            UnsupportedTokenIds =
            [
                ..FlavorTokenSets.DotNetOnly,
                "possessive-plus",
                "possessive-star",
                "atomic",
                "branch-reset",
                "cond-named",
                "h-space",
                "v-space",
            ],
            CodegenLanguageId = "swift",
            KnownDifferences =
            [
                "Apple platforms use ICU-based matching; not the same as .NET.",
                "Swift Regex builders are a different API surface from NSRegularExpression.",
                "Unicode and grapheme cluster matching differ from .NET.",
            ],
            SortOrder = 41,
        },
    ];
}
