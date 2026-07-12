using RegexCraft.Core.Analysis;
using RegexCraft.Core.Debug;
using RegexCraft.Core.Models;
using RegexCraft.Engines.DotNet;

namespace RegexCraft.Tests.Debug;

[TestFixture]
[Category("Debug")]
public sealed class RegexDebugServiceTests
{
    private RegexDebugService _service = null!;
    private DotNetRegexEngine _dotnet = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new RegexDebugService(new RegexAnalysisService());
        _dotnet = new DotNetRegexEngine();
    }

    [Test]
    public void SupportsEngine_OnlyDotNet()
    {
        Assert.That(_service.SupportsEngine("dotnet"), Is.True);
        Assert.That(_service.SupportsEngine("DOTNET"), Is.True);
        Assert.That(_service.SupportsEngine("pcre2"), Is.False);
        Assert.That(_service.SupportsEngine("javascript"), Is.False);
    }

    [Test]
    public void BuildSession_Unavailable_ForPcre2()
    {
        var match = _dotnet.Match(@"\d+", "abc123", RegexOptionsEx.None);
        var session = _service.BuildSession(
            @"\d+", "abc123", RegexOptionsEx.None, "pcre2", "PCRE2", match);

        Assert.That(session.IsAvailable, Is.False);
        Assert.That(session.UnavailableReason, Does.Contain(".NET").IgnoreCase);
        Assert.That(session.Steps, Is.Empty);
    }

    [Test]
    public void BuildSession_SuccessfulMatch_ProducesStepsWithCaptures()
    {
        const string pattern = @"(?<user>\w+)@(?<domain>\w+\.\w+)";
        const string subject = "hello@example.com";
        var match = _dotnet.Match(pattern, subject, RegexOptionsEx.None);
        Assert.That(match.Success, Is.True);
        Assert.That(match.Matches.Count, Is.EqualTo(1));

        var session = _service.BuildSession(
            pattern, subject, RegexOptionsEx.None, "dotnet", ".NET", match);

        Assert.That(session.IsAvailable, Is.True);
        Assert.That(session.PatternValid, Is.True);
        Assert.That(session.MatchCount, Is.EqualTo(1));
        Assert.That(session.Steps.Count, Is.GreaterThan(3));
        Assert.That(session.ApproachNote, Does.Contain("Educational").IgnoreCase);

        Assert.That(session.Steps[0].Kind, Is.EqualTo(DebugStepKind.Start));
        Assert.That(session.Steps.Any(s => s.Kind == DebugStepKind.MatchSuccess), Is.True);
        Assert.That(session.Steps.Any(s => s.Kind == DebugStepKind.Capture), Is.True);
        Assert.That(session.Steps.Any(s => s.Kind == DebugStepKind.Complete), Is.True);

        var capture = session.Steps.First(s => s.Kind == DebugStepKind.Capture);
        Assert.That(capture.HasSubjectRange, Is.True);
        Assert.That(capture.Explanation, Does.Contain("user").Or.Contain("Group"));
        Assert.That(capture.Success, Is.True);

        var withPattern = session.Steps.Where(s => s.HasPatternRange).ToList();
        Assert.That(withPattern, Is.Not.Empty);

        var withSubject = session.Steps.Where(s => s.HasSubjectRange && s.SubjectLength > 0).ToList();
        Assert.That(withSubject, Is.Not.Empty);
    }

    [Test]
    public void BuildSession_NoMatch_EndsWithFailure()
    {
        const string pattern = @"\d+";
        const string subject = "no-digits-here";
        var match = _dotnet.Match(pattern, subject, RegexOptionsEx.None);
        Assert.That(match.Success, Is.True);
        Assert.That(match.Matches, Is.Empty);

        var session = _service.BuildSession(
            pattern, subject, RegexOptionsEx.None, "dotnet", ".NET", match);

        Assert.That(session.IsAvailable, Is.True);
        Assert.That(session.MatchCount, Is.EqualTo(0));
        Assert.That(session.Steps.Any(s => s.Kind == DebugStepKind.Failure), Is.True);
        Assert.That(session.Steps.Last().Kind, Is.EqualTo(DebugStepKind.Complete));
    }

    [Test]
    public void BuildSession_InvalidPattern_ErrorSteps()
    {
        const string pattern = @"(unclosed";
        var match = _dotnet.Match(pattern, "abc", RegexOptionsEx.None);
        Assert.That(match.Success, Is.False);

        var session = _service.BuildSession(
            pattern, "abc", RegexOptionsEx.None, "dotnet", ".NET", match);

        Assert.That(session.IsAvailable, Is.True);
        Assert.That(session.PatternValid, Is.False);
        Assert.That(session.Steps.Any(s => s.Kind == DebugStepKind.Error), Is.True);
        Assert.That(session.Steps.Any(s => s.Success == false), Is.True);
    }

    [Test]
    public void BuildSession_EmptyPattern()
    {
        var match = MatchCollectionResult.FromMatches("dotnet", Array.Empty<MatchResult>(), TimeSpan.Zero);
        var session = _service.BuildSession(
            "", "subject", RegexOptionsEx.None, "dotnet", ".NET", match);

        Assert.That(session.IsAvailable, Is.True);
        Assert.That(session.Steps.Any(s => s.Explanation.Contains("empty", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void BuildSession_MultipleMatches_EnumeratesEach()
    {
        const string pattern = @"\d+";
        const string subject = "a12b34c";
        var match = _dotnet.Match(pattern, subject, RegexOptionsEx.None);
        Assert.That(match.Matches.Count, Is.EqualTo(2));

        var session = _service.BuildSession(
            pattern, subject, RegexOptionsEx.None, "dotnet", ".NET", match);

        Assert.That(session.MatchCount, Is.EqualTo(2));
        var successes = session.Steps.Where(s => s.Kind == DebugStepKind.MatchSuccess).ToList();
        Assert.That(successes.Count, Is.EqualTo(2));
        Assert.That(successes[0].MatchIndex, Is.EqualTo(0));
        Assert.That(successes[1].MatchIndex, Is.EqualTo(1));
    }

    [Test]
    public void BuildSession_StepIndicesAreContiguous()
    {
        const string pattern = @"\w+";
        const string subject = "hello world";
        var match = _dotnet.Match(pattern, subject, RegexOptionsEx.None);
        var session = _service.BuildSession(
            pattern, subject, RegexOptionsEx.None, "dotnet", ".NET", match);

        for (var i = 0; i < session.Steps.Count; i++)
            Assert.That(session.Steps[i].Index, Is.EqualTo(i));
    }

    [Test]
    public void CollectWalkableNodes_ReturnsSignificantNodes()
    {
        var root = new RegexAnalysisService().Analyze(@"(?<a>\d+)\w+");
        var nodes = RegexDebugService.CollectWalkableNodes(root);
        Assert.That(nodes, Is.Not.Empty);
        Assert.That(nodes.All(n => n.HasRange), Is.True);
    }
}
