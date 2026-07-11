using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegexCraft.Core.Engines;

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
    /// Default Phase 6 flavor catalog. Each flavor maps to a registered engine
    /// (dotnet, pcre2, or javascript) and declares testing fidelity.
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
            Notes = "Native .NET regex. Balancing groups, ExplicitCapture, and Timeout are available.",
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
            Notes = "Native PCRE2. Possessive quantifiers and many Perl-like constructs are supported.",
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
                "Testing uses an embedded ECMAScript engine (Jint). Results match modern JS closely; host engines may still differ slightly.",
            Notes =
                "ES2018+ features (lookbehind, named groups, `s` flag) require modern runtimes. No recursive patterns.",
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
            SortOrder = 11,
        },
        new FlavorDefinition
        {
            Id = "python",
            DisplayName = "Python",
            EngineId = "dotnet",
            Description = "Python re module (approximate testing)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Results may differ slightly from real Python re.",
            Notes =
                "Python re is not full PCRE. For PCRE-like features in Python, use the third-party `regex` package. Codegen targets `re`.",
            SortOrder = 20,
        },
        new FlavorDefinition
        {
            Id = "java",
            DisplayName = "Java",
            EngineId = "dotnet",
            Description = "java.util.regex (approximate testing)",
            SupportsFullTesting = true,
            Fidelity = TestingFidelity.Approximate,
            FidelityNote =
                "Testing uses closest engine (.NET). Results may differ slightly from real Java Pattern.",
            Notes =
                "Java Pattern is close to Perl/PCRE for common constructs; possessive quantifiers and some Unicode differ.",
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
                "Testing uses PCRE2 (same family as PHP preg). PHP delimiter/modifier packaging may still differ.",
            Notes = "PHP uses PCRE under the hood. Codegen emits preg_* with ~ delimiters.",
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
                "Testing uses closest engine (PCRE2). Results may differ slightly from real Ruby Onigmo.",
            Notes = "Ruby uses Onigmo; many common constructs align with PCRE. Named groups and lookarounds are supported in modern Ruby.",
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
                "Testing uses closest engine (.NET). Real Go RE2 has no lookbehind, backreferences, or recursion.",
            Notes = "RE2 is linear-time and deliberately limited. Prefer patterns without backrefs/lookbehind for Go.",
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
                "Testing uses closest engine (.NET). Real Rust `regex` crate has no lookaround or backreferences.",
            Notes = "Use the `fancy-regex` crate in Rust when you need lookaround/backrefs.",
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
                "Testing uses closest engine (PCRE2). Full Perl has additional constructs not covered here.",
            Notes = "PCRE was inspired by Perl; basic and intermediate patterns usually transfer well.",
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
                "Testing uses closest engine (.NET). Results may differ slightly from real Kotlin/JVM Pattern.",
            Notes = "Kotlin uses java.util.regex on JVM. Codegen emits Kotlin-friendly snippets.",
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
            SortOrder = 41,
        },
    ];
}
