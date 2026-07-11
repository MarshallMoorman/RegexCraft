using RegexCraft.Core.Library;

namespace RegexCraft.Tests.Library;

[TestFixture]
[Category("Library")]
public sealed class BuiltInLibraryTests
{
    [Test]
    public void GetDefaults_HasAtLeastEighteenPatterns()
    {
        var defaults = BuiltInLibrary.GetDefaults();
        Assert.That(defaults.Count, Is.GreaterThanOrEqualTo(18));
    }

    [Test]
    public void GetDefaults_AllHaveStableBuiltinIds()
    {
        foreach (var e in BuiltInLibrary.GetDefaults())
        {
            Assert.That(e.Id, Does.StartWith(BuiltInLibrary.IdPrefix), e.Name);
            Assert.That(e.IsBuiltIn, Is.True, e.Name);
            Assert.That(e.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(e.Pattern, Is.Not.Null.And.Not.Empty);
            Assert.That(e.Subject, Is.Not.Null.And.Not.Empty, e.Name);
        }
    }

    [Test]
    public void GetDefaults_IdsAreUnique()
    {
        var ids = BuiltInLibrary.GetDefaults().Select(e => e.Id).ToList();
        Assert.That(ids, Is.Unique);
    }

    [Test]
    public void GetDefaults_IncludesHighValuePatterns()
    {
        var names = BuiltInLibrary.GetDefaults().Select(e => e.Name).ToList();
        Assert.That(names.Any(n => n.Contains("Email", StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(names.Any(n => n.Contains("URL", StringComparison.OrdinalIgnoreCase)
                                   || n.Contains("HTTP", StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(names.Any(n => n.Contains("UUID", StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(names.Any(n => n.Contains("IPv4", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void BuiltInPatterns_CompileOnDotNet()
    {
        var engine = new global::RegexCraft.Engines.DotNet.DotNetRegexEngine();
        foreach (var entry in BuiltInLibrary.GetDefaults())
        {
            var result = engine.Match(entry.Pattern, entry.Subject ?? "", global::RegexCraft.Core.Models.RegexOptionsEx.None);
            Assert.That(result.Success, Is.True, $"{entry.Name}: {result.ErrorMessage}");
            // Most curated samples should produce at least one match
            Assert.That(result.Matches.Count, Is.GreaterThanOrEqualTo(1), entry.Name);
        }
    }
}
