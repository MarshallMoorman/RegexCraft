using RegexCraft.Core.Tokens;

namespace RegexCraft.Tests.Tokens;

[TestFixture]
[Category("Tokens")]
public sealed class TokenCatalogTests
{
    private TokenCatalog _catalog = null!;

    [SetUp]
    public void SetUp() => _catalog = new TokenCatalog();

    [Test]
    public void GetAllTokens_ReturnsNonEmptyCatalog()
    {
        var tokens = _catalog.GetAllTokens();
        Assert.That(tokens, Is.Not.Empty);
        Assert.That(tokens.All(t => !string.IsNullOrWhiteSpace(t.Label)), Is.True);
        Assert.That(tokens.All(t => !string.IsNullOrWhiteSpace(t.InsertText)), Is.True);
        Assert.That(tokens.All(t => !string.IsNullOrWhiteSpace(t.Category)), Is.True);
    }

    [Test]
    public void GetCategories_IncludesExpectedGroups()
    {
        var cats = _catalog.GetCategories();
        Assert.That(cats, Does.Contain("Quantifiers"));
        Assert.That(cats, Does.Contain("Groups"));
        Assert.That(cats, Does.Contain("Lookarounds"));
        Assert.That(cats, Does.Contain("Anchors"));
    }

    [Test]
    public void Search_Empty_ReturnsAll()
    {
        Assert.That(_catalog.Search(""), Has.Count.EqualTo(_catalog.GetAllTokens().Count));
        Assert.That(_catalog.Search(null), Has.Count.EqualTo(_catalog.GetAllTokens().Count));
    }

    [Test]
    public void Search_ByLabel_Filters()
    {
        var results = _catalog.Search("digit");
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(t => t.SearchText.Contains("digit", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void Search_ByInsertText_FindsToken()
    {
        var results = _catalog.Search(@"\d");
        Assert.That(results.Any(t => t.InsertText == @"\d"), Is.True);
    }

    [Test]
    public void Search_NoMatch_ReturnsEmpty()
    {
        Assert.That(_catalog.Search("zzzz-not-a-token"), Is.Empty);
    }

    [Test]
    public void Tokens_HaveNoIconRequirement_TextOnly()
    {
        // Contract: tokens are identified by Label + InsertText only.
        foreach (var t in _catalog.GetAllTokens())
        {
            Assert.That(t.Label, Is.Not.Empty);
            Assert.That(t.InsertText, Is.Not.Empty);
        }
    }
}
