using System.Text.Json;
using NUnit.Framework;
using RegexCraft.Core.Export;
using RegexCraft.Core.Models;

namespace RegexCraft.Tests.Export;

[TestFixture]
[Category("Export")]
public sealed class MatchExportServiceTests
{
    private static MatchCollectionResult SampleResult()
    {
        var matches = new List<MatchResult>
        {
            new()
            {
                Index = 10,
                Length = 19,
                Value = "support@example.com",
                Groups =
                [
                    new GroupResult
                    {
                        Number = 0, Name = "0", Index = 10, Length = 19,
                        Value = "support@example.com", Success = true,
                    },
                    new GroupResult
                    {
                        Number = 1, Name = "user", Index = 10, Length = 7,
                        Value = "support", Success = true,
                    },
                    new GroupResult
                    {
                        Number = 2, Name = "domain", Index = 18, Length = 11,
                        Value = "example.com", Success = true,
                    },
                ],
            },
            new()
            {
                Index = 40,
                Length = 15,
                Value = "a@b.co",
                Groups =
                [
                    new GroupResult
                    {
                        Number = 0, Name = "0", Index = 40, Length = 15,
                        Value = "a@b.co", Success = true,
                    },
                    new GroupResult
                    {
                        Number = 1, Name = "user", Index = 40, Length = 1,
                        Value = "a", Success = true,
                    },
                    new GroupResult
                    {
                        Number = 2, Name = "domain", Index = 42, Length = 4,
                        Value = "b.co", Success = true,
                    },
                ],
            },
        };

        return MatchCollectionResult.FromMatches("dotnet", matches, TimeSpan.FromMilliseconds(1.5));
    }

    private static MatchExportContext SampleContext() => new()
    {
        Pattern = @"(?<user>\w+)@(?<domain>[\w.]+)",
        Subject = "Contact support@example.com and a@b.co",
        FlavorId = "dotnet",
        FlavorDisplayName = ".NET",
        EngineId = "dotnet",
        EngineDisplayName = ".NET",
        IgnoreCase = true,
        Multiline = false,
        ExportedAt = new DateTimeOffset(2026, 7, 12, 12, 0, 0, TimeSpan.Zero),
    };

    [Test]
    public void ToCsv_IncludesHeader_MatchRows_AndGroupColumns()
    {
        var csv = MatchExportService.ToCsv(SampleResult(), SampleContext());

        Assert.That(csv, Does.StartWith("MatchIndex,Value,Index,Length"));
        Assert.That(csv, Does.Contain("Group1_Name"));
        Assert.That(csv, Does.Contain("Group2_Name"));
        Assert.That(csv, Does.Contain("support@example.com"));
        Assert.That(csv, Does.Contain("user"));
        Assert.That(csv, Does.Contain("domain"));

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.That(lines.Length, Is.EqualTo(3)); // header + 2 matches
        Assert.That(lines[1], Does.StartWith("0,"));
        Assert.That(lines[2], Does.StartWith("1,"));
    }

    [Test]
    public void ToCsv_EscapesCommasAndQuotes()
    {
        var result = MatchCollectionResult.FromMatches("dotnet",
        [
            new MatchResult
            {
                Index = 0,
                Length = 5,
                Value = "a,b\"c",
                Groups = Array.Empty<GroupResult>(),
            },
        ], TimeSpan.Zero);

        var csv = MatchExportService.ToCsv(result, SampleContext());
        Assert.That(csv, Does.Contain("\"a,b\"\"c\""));
    }

    [Test]
    public void ToCsv_EmptyMatches_HeaderOnly()
    {
        var result = MatchCollectionResult.FromMatches("dotnet", Array.Empty<MatchResult>(), TimeSpan.Zero);
        var csv = MatchExportService.ToCsv(result, SampleContext());
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.That(lines.Length, Is.EqualTo(1));
        Assert.That(lines[0], Does.Contain("MatchIndex"));
    }

    [Test]
    public void ToJson_IncludesMetadata_Matches_AndGroups()
    {
        var json = MatchExportService.ToJson(SampleResult(), SampleContext());
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("pattern").GetString(), Is.EqualTo(SampleContext().Pattern));
        Assert.That(root.GetProperty("flavorId").GetString(), Is.EqualTo("dotnet"));
        Assert.That(root.GetProperty("engineId").GetString(), Is.EqualTo("dotnet"));
        Assert.That(root.GetProperty("success").GetBoolean(), Is.True);
        Assert.That(root.GetProperty("options").GetProperty("ignoreCase").GetBoolean(), Is.True);

        var matches = root.GetProperty("matches");
        Assert.That(matches.GetArrayLength(), Is.EqualTo(2));
        Assert.That(matches[0].GetProperty("value").GetString(), Is.EqualTo("support@example.com"));
        Assert.That(matches[0].GetProperty("matchIndex").GetInt32(), Is.EqualTo(0));
        Assert.That(matches[0].GetProperty("groups").GetArrayLength(), Is.GreaterThanOrEqualTo(2));
        Assert.That(root.GetProperty("exportedAt").GetString(), Does.Contain("2026-07-12"));
    }

    [Test]
    public void ToJson_FailedResult_IncludesError()
    {
        var failed = MatchCollectionResult.Failed("dotnet", "bad pattern");
        var json = MatchExportService.ToJson(failed, SampleContext());
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.GetProperty("success").GetBoolean(), Is.False);
        Assert.That(doc.RootElement.GetProperty("errorMessage").GetString(), Is.EqualTo("bad pattern"));
        Assert.That(doc.RootElement.GetProperty("matches").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public void SuggestedFileName_UsesExtensionAndTimestampShape()
    {
        var name = MatchExportService.SuggestedFileName("json", new DateTimeOffset(2026, 7, 12, 15, 30, 45, TimeSpan.Zero));
        Assert.That(name, Is.EqualTo("regexcraft-matches-20260712-153045.json"));
    }

    [Test]
    public void CsvEscape_QuotesWhenNeeded()
    {
        Assert.That(MatchExportService.CsvEscape("plain"), Is.EqualTo("plain"));
        Assert.That(MatchExportService.CsvEscape("a,b"), Is.EqualTo("\"a,b\""));
        Assert.That(MatchExportService.CsvEscape("say \"hi\""), Is.EqualTo("\"say \"\"hi\"\"\""));
    }
}
