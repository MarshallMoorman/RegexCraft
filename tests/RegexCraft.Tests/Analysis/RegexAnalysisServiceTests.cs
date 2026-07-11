using RegexCraft.Core.Analysis;

namespace RegexCraft.Tests.Analysis;

[TestFixture]
[Category("Analysis")]
public sealed class RegexAnalysisServiceTests
{
    private RegexAnalysisService _svc = null!;

    [SetUp]
    public void SetUp() => _svc = new RegexAnalysisService();

    [Test]
    public void Analyze_Empty_ReturnsRootMessage()
    {
        var root = _svc.Analyze("");
        Assert.That(root.Kind, Is.EqualTo(AnalysisNodeKind.Root));
        Assert.That(root.Title, Does.Contain("Empty"));
    }

    [Test]
    public void Analyze_SimpleLiterals()
    {
        var root = _svc.Analyze("abc");
        Assert.That(root.Children, Is.Not.Empty);
        Assert.That(Flatten(root).Any(n => n.Kind == AnalysisNodeKind.Literal), Is.True);
    }

    [Test]
    public void Analyze_CapturingGroup()
    {
        var root = _svc.Analyze("(ab)+");
        var kinds = Flatten(root).Select(n => n.Kind).ToList();
        Assert.That(kinds, Does.Contain(AnalysisNodeKind.Group));
        Assert.That(kinds, Does.Contain(AnalysisNodeKind.Quantifier));
    }

    [Test]
    public void Analyze_NamedGroup()
    {
        var root = _svc.Analyze(@"(?<id>\d+)");
        Assert.That(Flatten(root).Any(n => n.Kind == AnalysisNodeKind.NamedGroup), Is.True);
        Assert.That(Flatten(root).Any(n => n.Detail == "id"), Is.True);
    }

    [Test]
    public void Analyze_Alternation()
    {
        var root = _svc.Analyze("cat|dog");
        Assert.That(Flatten(root).Any(n => n.Kind == AnalysisNodeKind.Alternation), Is.True);
    }

    [Test]
    public void Analyze_CharacterClass()
    {
        var root = _svc.Analyze("[a-z]+");
        Assert.That(Flatten(root).Any(n => n.Kind == AnalysisNodeKind.CharacterClass), Is.True);
    }

    [Test]
    public void Analyze_Lookahead()
    {
        var root = _svc.Analyze(@"\d(?=px)");
        Assert.That(Flatten(root).Any(n => n.Kind == AnalysisNodeKind.Lookaround), Is.True);
    }

    [Test]
    public void Analyze_IncompleteGroup_DoesNotThrow()
    {
        var root = _svc.Analyze("(abc");
        Assert.That(root, Is.Not.Null);
        Assert.That(Flatten(root).Any(n => n.IsError || n.Kind is AnalysisNodeKind.Incomplete or AnalysisNodeKind.Error), Is.True);
    }

    [Test]
    public void Analyze_IncompleteClass_DoesNotThrow()
    {
        var root = _svc.Analyze("[abc");
        Assert.That(Flatten(root).Any(n => n.IsError || n.Kind == AnalysisNodeKind.Incomplete), Is.True);
    }

    [Test]
    public void Analyze_Escapes()
    {
        var root = _svc.Analyze(@"\d+\w*");
        Assert.That(Flatten(root).Any(n => n.Kind == AnalysisNodeKind.Escape), Is.True);
    }

    private static IEnumerable<AnalysisNode> Flatten(AnalysisNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        foreach (var n in Flatten(child))
            yield return n;
    }
}
