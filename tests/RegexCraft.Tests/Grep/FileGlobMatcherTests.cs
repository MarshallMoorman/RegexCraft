using RegexCraft.Core.Grep;

namespace RegexCraft.Tests.Grep;

[TestFixture]
[Category("Grep")]
public sealed class FileGlobMatcherTests
{
    [Test]
    public void ParseList_SplitsOnSemicolonAndComma()
    {
        var list = FileGlobMatcher.ParseList("*.cs; *.json, bin/**");
        Assert.That(list, Is.EqualTo(new[] { "*.cs", "*.json", "bin/**" }));
    }

    [Test]
    public void Matches_FileNameOnlyGlob()
    {
        Assert.That(FileGlobMatcher.Matches("src/Foo.cs", "*.cs"), Is.True);
        Assert.That(FileGlobMatcher.Matches("src/Foo.cs", "*.json"), Is.False);
        Assert.That(FileGlobMatcher.Matches("readme.md", "*.md"), Is.True);
    }

    [Test]
    public void Matches_DoubleStar()
    {
        Assert.That(FileGlobMatcher.Matches("src/obj/Debug/x.cs", "obj/**"), Is.False);
        Assert.That(FileGlobMatcher.Matches("obj/Debug/x.cs", "obj/**"), Is.True);
        Assert.That(FileGlobMatcher.Matches("a/b/c.txt", "**/c.txt"), Is.True);
    }

    [Test]
    public void IsIncluded_RespectsExclude()
    {
        var includes = new[] { "*.cs", "*.json" };
        var excludes = new[] { "bin/**", "obj/**" };

        Assert.That(FileGlobMatcher.IsIncluded("src/App.cs", includes, excludes), Is.True);
        Assert.That(FileGlobMatcher.IsIncluded("bin/App.cs", includes, excludes), Is.False);
        Assert.That(FileGlobMatcher.IsIncluded("obj/Debug/x.cs", includes, excludes), Is.False);
        Assert.That(FileGlobMatcher.IsIncluded("src/data.json", includes, excludes), Is.True);
        Assert.That(FileGlobMatcher.IsIncluded("src/readme.md", includes, excludes), Is.False);
    }

    [Test]
    public void IsIncluded_EmptyInclude_MatchesAllExceptExclude()
    {
        var includes = Array.Empty<string>();
        var excludes = new[] { ".git/**" };
        Assert.That(FileGlobMatcher.IsIncluded("a.txt", includes, excludes), Is.True);
        Assert.That(FileGlobMatcher.IsIncluded(".git/config", includes, excludes), Is.False);
    }
}
