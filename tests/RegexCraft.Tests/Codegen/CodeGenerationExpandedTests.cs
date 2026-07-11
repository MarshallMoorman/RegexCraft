using RegexCraft.Core.Codegen;
using RegexCraft.Core.Models;

namespace RegexCraft.Tests.Codegen;

[TestFixture]
[Category("Codegen")]
[Category("Generate")]
public sealed class CodeGenerationExpandedTests
{
    private CodeGenerationService _svc = null!;

    [SetUp]
    public void SetUp() => _svc = new CodeGenerationService();

    [Test]
    public void SupportedLanguages_ContainsAllPhase6Targets()
    {
        var ids = _svc.SupportedLanguages.Select(l => l.Id()).ToList();
        foreach (var expected in new[]
                 {
                     "csharp", "javascript", "typescript", "python", "php", "java",
                     "go", "rust", "ruby", "perl", "kotlin", "swift",
                 })
        {
            Assert.That(ids, Does.Contain(expected), expected);
        }
    }

    [Test]
    public void Generate_AllLanguages_AllOperations_NonEmpty(
        [Values] CodeLanguage lang,
        [Values] CodegenOperation op)
    {
        var code = _svc.Generate(
            lang,
            op,
            @"(?<n>\w+)",
            "hello",
            op == CodegenOperation.Replace ? "[$1]" : null,
            RegexOptionsEx.IgnoreCase | RegexOptionsEx.Multiline,
            "dotnet");

        Assert.That(code, Is.Not.Null.And.Not.Empty, $"{lang}/{op}");
        Assert.That(code, Does.Not.Contain("Unsupported"), $"{lang}/{op}");
        Assert.That(code.Length, Is.GreaterThan(20), $"{lang}/{op}");
    }

    [Test]
    public void Generate_CSharp_IncludesOptionsFlags()
    {
        var code = _svc.Generate(
            CodeLanguage.CSharp,
            CodegenOperation.Matches,
            @"\d+",
            "1",
            null,
            RegexOptionsEx.IgnoreCase | RegexOptionsEx.Multiline | RegexOptionsEx.Singleline,
            "dotnet");

        Assert.That(code, Does.Contain("IgnoreCase"));
        Assert.That(code, Does.Contain("Multiline"));
        Assert.That(code, Does.Contain("Singleline"));
    }

    [Test]
    public void Generate_JavaScript_EscapesPattern()
    {
        var code = _svc.Generate(
            CodeLanguage.JavaScript,
            CodegenOperation.IsMatch,
            @"a\b""c",
            "x",
            null,
            RegexOptionsEx.None,
            "javascript");

        Assert.That(code, Does.Contain("RegExp").Or.Contain("new RegExp"));
    }

    [Test]
    public void Generate_Go_MentionsRe2OrRegexp()
    {
        var code = _svc.Generate(
            CodeLanguage.Go,
            CodegenOperation.Match,
            @"\d+",
            "1",
            null,
            RegexOptionsEx.None,
            "dotnet");

        Assert.That(code, Does.Contain("regexp").IgnoreCase);
    }

    [Test]
    public void Generate_Python_UsesReModule()
    {
        var code = _svc.Generate(
            CodeLanguage.Python,
            CodegenOperation.Replace,
            @"(\w+)",
            "hi",
            @"[\1]",
            RegexOptionsEx.IgnoreCase,
            "dotnet");

        Assert.That(code, Does.Contain("import re"));
        Assert.That(code, Does.Contain("re."));
    }

    [Test]
    public void Generate_Php_UsesPregFunctions()
    {
        var code = _svc.Generate(
            CodeLanguage.Php,
            CodegenOperation.Split,
            @",\s*",
            "a, b",
            null,
            RegexOptionsEx.None,
            "pcre2");

        Assert.That(code, Does.Contain("preg_"));
    }

    [Test]
    public void Generate_EmptyPattern_StillProducesSnippet()
    {
        var code = _svc.Generate(
            CodeLanguage.CSharp,
            CodegenOperation.Matches,
            "",
            "",
            null,
            RegexOptionsEx.None,
            "dotnet");

        Assert.That(code, Is.Not.Empty);
    }
}
