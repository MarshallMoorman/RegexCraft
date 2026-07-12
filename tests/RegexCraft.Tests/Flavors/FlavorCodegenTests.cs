using RegexCraft.Core.Codegen;
using RegexCraft.Core.Flavors;
using RegexCraft.Core.Models;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Flavors;

/// <summary>
/// Codegen correctness for each flavor's preferred language and engine mapping.
/// </summary>
[TestFixture]
[Category("Flavors")]
[Category("Codegen")]
public sealed class FlavorCodegenTests
{
    private FlavorService _flavors = null!;
    private CodeGenerationService _codegen = null!;

    [SetUp]
    public void SetUp()
    {
        _flavors = new FlavorService(EngineFactory.CreateDefaultEngines());
        _codegen = new CodeGenerationService();
    }

    private static IEnumerable<string> AllFlavorIds() =>
        FlavorService.BuildDefaultFlavors().Select(f => f.Id);

    private static CodeLanguage ParseLanguage(string id) => id.ToLowerInvariant() switch
    {
        "csharp" => CodeLanguage.CSharp,
        "javascript" => CodeLanguage.JavaScript,
        "typescript" => CodeLanguage.TypeScript,
        "python" => CodeLanguage.Python,
        "php" => CodeLanguage.Php,
        "java" => CodeLanguage.Java,
        "go" => CodeLanguage.Go,
        "rust" => CodeLanguage.Rust,
        "ruby" => CodeLanguage.Ruby,
        "perl" => CodeLanguage.Perl,
        "kotlin" => CodeLanguage.Kotlin,
        "swift" => CodeLanguage.Swift,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown language"),
    };

    [TestCaseSource(nameof(AllFlavorIds))]
    public void PreferredLanguage_GeneratesNonEmptySnippet(string flavorId)
    {
        var flavor = _flavors.GetFlavor(flavorId)!;
        var lang = ParseLanguage(flavor.CodegenLanguageId);

        var code = _codegen.Generate(
            lang,
            CodegenOperation.Matches,
            @"(?<n>\w+)",
            "hello world",
            null,
            RegexOptionsEx.IgnoreCase,
            flavor.EngineId);

        Assert.That(code, Is.Not.Null.And.Not.Empty, flavorId);
        Assert.That(code, Does.Not.Contain("Unsupported"), flavorId);
        Assert.That(code.Length, Is.GreaterThan(30), flavorId);
    }

    [TestCaseSource(nameof(AllFlavorIds))]
    public void PreferredLanguage_AllOperations_Generate(string flavorId)
    {
        var flavor = _flavors.GetFlavor(flavorId)!;
        var lang = ParseLanguage(flavor.CodegenLanguageId);

        foreach (CodegenOperation op in Enum.GetValues<CodegenOperation>())
        {
            var code = _codegen.Generate(
                lang,
                op,
                @"\d+",
                "a1b2",
                op == CodegenOperation.Replace ? "[$1]" : null,
                RegexOptionsEx.None,
                flavor.EngineId);

            Assert.That(code, Is.Not.Null.And.Not.Empty, $"{flavorId}/{op}");
            Assert.That(code, Does.Not.Contain("Unsupported"), $"{flavorId}/{op}");
        }
    }

    [Test]
    public void DotNet_Codegen_IsCSharp()
    {
        var f = _flavors.GetFlavor("dotnet")!;
        Assert.That(f.CodegenLanguageId, Is.EqualTo("csharp"));
        var code = _codegen.Generate(
            CodeLanguage.CSharp, CodegenOperation.IsMatch, @"\w+", "x", null,
            RegexOptionsEx.None, f.EngineId);
        Assert.That(code, Does.Contain("System.Text.RegularExpressions"));
    }

    [Test]
    public void JavaScript_Codegen_UsesRegExp()
    {
        var f = _flavors.GetFlavor("javascript")!;
        var code = _codegen.Generate(
            CodeLanguage.JavaScript, CodegenOperation.Matches, @"\d+", "1", null,
            RegexOptionsEx.IgnoreCase, f.EngineId);
        Assert.That(code, Does.Contain("RegExp").Or.Contain("match"));
    }

    [Test]
    public void Python_Codegen_UsesReModule()
    {
        var f = _flavors.GetFlavor("python")!;
        var code = _codegen.Generate(
            CodeLanguage.Python, CodegenOperation.Matches, @"\d+", "1", null,
            RegexOptionsEx.IgnoreCase, f.EngineId);
        Assert.That(code, Does.Contain("import re").Or.Contain("re."));
    }

    [Test]
    public void Php_Codegen_UsesPreg()
    {
        var f = _flavors.GetFlavor("php")!;
        var code = _codegen.Generate(
            CodeLanguage.Php, CodegenOperation.Matches, @"\d+", "1", null,
            RegexOptionsEx.None, f.EngineId);
        Assert.That(code, Does.Contain("preg_").IgnoreCase);
    }

    [Test]
    public void GoAndRust_Codegen_MentionLimitations()
    {
        var go = _codegen.Generate(
            CodeLanguage.Go, CodegenOperation.IsMatch, @"(?<=a)b", "ab", null,
            RegexOptionsEx.None, "dotnet");
        var rust = _codegen.Generate(
            CodeLanguage.Rust, CodegenOperation.IsMatch, @"(\w)\1", "aa", null,
            RegexOptionsEx.None, "dotnet");

        // Notes about RE2 / limitations should appear somewhere in generated output.
        Assert.That(go + rust, Does.Contain("RE2").Or.Contain("look").Or.Contain("backref").Or.Contain("engine").IgnoreCase);
    }

    [Test]
    public void Java_Codegen_UsesPattern()
    {
        var f = _flavors.GetFlavor("java")!;
        var code = _codegen.Generate(
            CodeLanguage.Java, CodegenOperation.Matches, @"\w+", "x", null,
            RegexOptionsEx.None, f.EngineId);
        Assert.That(code, Does.Contain("Pattern").Or.Contain("Matcher"));
    }
}
