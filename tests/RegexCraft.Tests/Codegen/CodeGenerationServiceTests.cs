using RegexCraft.Core.Codegen;
using RegexCraft.Core.Models;

namespace RegexCraft.Tests.Codegen;

[TestFixture]
[Category("Codegen")]
public sealed class CodeGenerationServiceTests
{
    private CodeGenerationService _svc = null!;

    [SetUp]
    public void SetUp() => _svc = new CodeGenerationService();

    [Test]
    public void SupportedLanguages_HasAtLeastSix()
    {
        Assert.That(_svc.SupportedLanguages.Count, Is.GreaterThanOrEqualTo(6));
    }

    [TestCase(CodeLanguage.CSharp, "using System.Text.RegularExpressions")]
    [TestCase(CodeLanguage.JavaScript, "new RegExp")]
    [TestCase(CodeLanguage.Python, "import re")]
    [TestCase(CodeLanguage.Php, "preg_")]
    [TestCase(CodeLanguage.Java, "Pattern.compile")]
    [TestCase(CodeLanguage.Go, "regexp")]
    [TestCase(CodeLanguage.Rust, "Regex::new")]
    public void Generate_Matches_IncludesLanguageMarkers(CodeLanguage lang, string expectedFragment)
    {
        var code = _svc.Generate(
            lang,
            CodegenOperation.Matches,
            @"\d+",
            "a1b2",
            null,
            RegexOptionsEx.None,
            "dotnet");

        Assert.That(code, Does.Contain(expectedFragment));
        Assert.That(code.Length, Is.GreaterThan(40));
    }

    [Test]
    public void Generate_Replace_IncludesReplacement()
    {
        var code = _svc.Generate(
            CodeLanguage.CSharp,
            CodegenOperation.Replace,
            @"(\w+)",
            "hi",
            "[$1]",
            RegexOptionsEx.IgnoreCase,
            "dotnet");

        Assert.That(code, Does.Contain("Replace"));
        Assert.That(code, Does.Contain("[$1]").Or.Contain("$1"));
        Assert.That(code, Does.Contain("IgnoreCase"));
    }

    [Test]
    public void Generate_Split_WorksForPython()
    {
        var code = _svc.Generate(
            CodeLanguage.Python,
            CodegenOperation.Split,
            @",\s*",
            "a, b",
            null,
            RegexOptionsEx.None,
            "pcre2");

        Assert.That(code, Does.Contain("split"));
    }

    [Test]
    public void Generate_IsMatch_WorksForAllLanguages()
    {
        foreach (var lang in _svc.SupportedLanguages)
        {
            var code = _svc.Generate(lang, CodegenOperation.IsMatch, "a", "a", null, RegexOptionsEx.None, "dotnet");
            Assert.That(code, Is.Not.Empty, lang.ToString());
        }
    }

    [Test]
    public void Generate_EscapesQuotesInPattern()
    {
        var code = _svc.Generate(
            CodeLanguage.CSharp,
            CodegenOperation.Match,
            "a\"b",
            "x",
            null,
            RegexOptionsEx.None,
            "dotnet");

        Assert.That(code, Does.Contain("a\"\"b").Or.Contain("a\\\"b").Or.Contain("@\""));
    }

    [Test]
    public void Generate_IncludesEngineNote_ForPcreSource()
    {
        var code = _svc.Generate(
            CodeLanguage.JavaScript,
            CodegenOperation.IsMatch,
            @"\d+",
            "1",
            null,
            RegexOptionsEx.None,
            "pcre2");

        Assert.That(code, Does.Contain("PCRE2").Or.Contain("pcre"));
    }

    [Test]
    public void Generate_GoAndRust_IncludeEngineLimitNotes()
    {
        var go = _svc.Generate(CodeLanguage.Go, CodegenOperation.Match, "a", "a", null, RegexOptionsEx.None, "dotnet");
        var rust = _svc.Generate(CodeLanguage.Rust, CodegenOperation.Match, "a", "a", null, RegexOptionsEx.None, "dotnet");
        Assert.That(go, Does.Contain("RE2").Or.Contain("lookbehind").Or.Contain("backref"));
        Assert.That(rust, Does.Contain("lookaround").Or.Contain("fancy-regex").Or.Contain("backreference"));
    }
}
