using RegexCraft.Core.Models;

namespace RegexCraft.Tests.Models;

[TestFixture]
[Category("Core")]
public sealed class ResultModelTests
{
    [Test]
    public void MatchCollectionResult_Failed_SetsError()
    {
        var result = MatchCollectionResult.Failed("dotnet", "bad pattern");
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("bad pattern"));
        Assert.That(result.Matches, Is.Empty);
        Assert.That(result.EngineId, Is.EqualTo("dotnet"));
    }

    [Test]
    public void MatchCollectionResult_FromMatches_SetsSuccess()
    {
        var matches = new[]
        {
            new MatchResult
            {
                Index = 0,
                Length = 3,
                Value = "abc",
                Groups =
                [
                    new GroupResult
                    {
                        Number = 0,
                        Name = "0",
                        Index = 0,
                        Length = 3,
                        Value = "abc",
                        Success = true,
                    },
                ],
            },
        };

        var result = MatchCollectionResult.FromMatches("pcre2", matches, TimeSpan.FromMilliseconds(1));
        Assert.That(result.Success, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Matches, Has.Count.EqualTo(1));
        Assert.That(result.EngineId, Is.EqualTo("pcre2"));
    }

    [Test]
    public void ReplaceResult_Failed_And_FromResult()
    {
        var failed = ReplaceResult.Failed("dotnet", "oops");
        Assert.That(failed.Success, Is.False);
        Assert.That(failed.Result, Is.Empty);

        var ok = ReplaceResult.FromResult("dotnet", "out", 2, TimeSpan.FromMilliseconds(5));
        Assert.That(ok.Success, Is.True);
        Assert.That(ok.Result, Is.EqualTo("out"));
        Assert.That(ok.ReplacementCount, Is.EqualTo(2));
    }

    [Test]
    public void RegexOptionsEx_Flags_Combine()
    {
        var opts = RegexOptionsEx.IgnoreCase | RegexOptionsEx.Multiline;
        Assert.That(opts.HasFlag(RegexOptionsEx.IgnoreCase), Is.True);
        Assert.That(opts.HasFlag(RegexOptionsEx.Multiline), Is.True);
        Assert.That(opts.HasFlag(RegexOptionsEx.Singleline), Is.False);
    }
}
