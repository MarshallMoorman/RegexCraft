using RegexCraft.Core.Grep;
using RegexCraft.Core.Models;
using RegexCraft.Engines;

namespace RegexCraft.Tests.Grep;

[TestFixture]
[Category("Grep")]
public sealed class GrepServiceTests
{
    private string _dir = null!;
    private GrepService _svc = null!;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "regexcraft-grep-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _svc = new GrepService();
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }
        catch
        {
            // ignore
        }
    }

    private static void Write(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, content);
    }

    [Test]
    public async Task Search_FindsMatchesWithLineNumbers_DotNet()
    {
        Write(Path.Combine(_dir, "a.txt"), "hello world\nfoo 123 bar\n");
        Write(Path.Combine(_dir, "b.txt"), "no digits here\n");
        Write(Path.Combine(_dir, "c.cs"), "int x = 99;\n");

        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "dotnet");

        var result = await _svc.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = _dir,
            Pattern = @"\d+",
            Options = RegexOptionsEx.None,
            Recursive = true,
            IncludeGlobs = "*.txt;*.cs",
            ExcludeGlobs = "",
        });

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches.Count, Is.EqualTo(2));
        Assert.That(result.FilesMatched, Is.EqualTo(2));
        Assert.That(result.Matches.Select(m => m.MatchValue), Is.EquivalentTo(new[] { "123", "99" }));
        Assert.That(result.Matches.Any(m => m.LineNumber == 2 && m.MatchValue == "123"), Is.True);
    }

    [Test]
    public async Task Search_WorksWithPcre2()
    {
        Write(Path.Combine(_dir, "pcre.txt"), "email: user@example.com\n");

        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "pcre2");

        var result = await _svc.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = _dir,
            Pattern = @"\w+@\w+\.\w+",
            Options = RegexOptionsEx.None,
            IncludeGlobs = "*.txt",
            ExcludeGlobs = "",
        });

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Matches, Has.Count.EqualTo(1));
        Assert.That(result.Matches[0].MatchValue, Is.EqualTo("user@example.com"));
    }

    [Test]
    public async Task Search_InvalidPattern_FailsGracefully()
    {
        Write(Path.Combine(_dir, "a.txt"), "x");
        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "dotnet");

        var result = await _svc.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = _dir,
            Pattern = "(",
            IncludeGlobs = "*",
            ExcludeGlobs = "",
        });

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Empty);
    }

    [Test]
    public async Task Search_ExcludeGlobs_SkipBin()
    {
        Write(Path.Combine(_dir, "src", "App.cs"), "match_me 1");
        Write(Path.Combine(_dir, "bin", "App.cs"), "match_me 2");

        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "dotnet");

        var result = await _svc.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = _dir,
            Pattern = @"match_me",
            IncludeGlobs = "*.cs",
            ExcludeGlobs = "bin/**",
            Recursive = true,
        });

        Assert.That(result.Success, Is.True);
        Assert.That(result.Matches, Has.Count.EqualTo(1));
        Assert.That(result.Matches[0].FilePath, Does.Contain("src"));
    }

    [Test]
    public async Task Search_CanCancel()
    {
        // Create many small files
        for (var i = 0; i < 50; i++)
            Write(Path.Combine(_dir, $"f{i}.txt"), $"value {i}");

        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "dotnet");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _svc.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = _dir,
            Pattern = @"\d+",
            IncludeGlobs = "*.txt",
            ExcludeGlobs = "",
        }, cancellationToken: cts.Token);

        Assert.That(result.Cancelled || result.Success, Is.True);
        Assert.That(result.Cancelled, Is.True);
    }

    [Test]
    public async Task Replace_DryRun_DoesNotWrite()
    {
        var path = Path.Combine(_dir, "edit.txt");
        Write(path, "a1b2c");

        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "dotnet");

        var result = await _svc.ReplaceAsync(engine, new GrepReplaceRequest
        {
            RootPath = _dir,
            Pattern = @"\d",
            Replacement = "X",
            IncludeGlobs = "*.txt",
            ExcludeGlobs = "",
            DryRun = true,
            CreateBackup = false,
        });

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.DryRun, Is.True);
        Assert.That(result.TotalReplacements, Is.EqualTo(2));
        Assert.That(result.FilesModified, Is.EqualTo(1));
        Assert.That(File.ReadAllText(path), Is.EqualTo("a1b2c"));
        Assert.That(result.Files[0].Written, Is.False);
        Assert.That(result.Files[0].PreviewAfter, Is.EqualTo("aXbXc"));
    }

    [Test]
    public async Task Replace_WritesWithBackup()
    {
        var path = Path.Combine(_dir, "edit.txt");
        Write(path, "foo bar foo");

        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "dotnet");

        var result = await _svc.ReplaceAsync(engine, new GrepReplaceRequest
        {
            RootPath = _dir,
            Pattern = @"foo",
            Replacement = "baz",
            IncludeGlobs = "*.txt",
            ExcludeGlobs = "",
            DryRun = false,
            CreateBackup = true,
        });

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.TotalReplacements, Is.EqualTo(2));
        Assert.That(File.ReadAllText(path), Is.EqualTo("baz bar baz"));
        Assert.That(File.Exists(path + ".bak"), Is.True);
        Assert.That(File.ReadAllText(path + ".bak"), Is.EqualTo("foo bar foo"));
        Assert.That(result.Files[0].Written, Is.True);
        Assert.That(result.Files[0].BackedUp, Is.True);
    }

    [Test]
    public async Task Replace_Pcre2_Works()
    {
        var path = Path.Combine(_dir, "p.txt");
        Write(path, "one two");

        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "pcre2");

        var result = await _svc.ReplaceAsync(engine, new GrepReplaceRequest
        {
            RootPath = _dir,
            Pattern = @"(\w+)",
            Replacement = "[$1]",
            IncludeGlobs = "*.txt",
            ExcludeGlobs = "",
            DryRun = false,
            CreateBackup = false,
        });

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(File.ReadAllText(path), Is.EqualTo("[one] [two]"));
    }

    [Test]
    public void EnumerateFiles_RespectsMaxAndGlobs()
    {
        Write(Path.Combine(_dir, "a.cs"), "1");
        Write(Path.Combine(_dir, "b.json"), "2");
        Write(Path.Combine(_dir, "c.md"), "3");
        Write(Path.Combine(_dir, "bin", "x.cs"), "4");

        var files = _svc.EnumerateFiles(
            _dir,
            recursive: true,
            includeGlobs: "*.cs;*.json",
            excludeGlobs: "bin/**",
            maxFileSizeBytes: 1_000_000,
            maxFiles: 100,
            out var skipped).ToList();

        Assert.That(files.Count, Is.EqualTo(2));
        Assert.That(files.All(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                                   && !f.Contains("/bin/")), Is.True);
        Assert.That(skipped, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task Search_MissingFolder_Fails()
    {
        var engines = EngineFactory.CreateDefaultEngines();
        var engine = engines.First(e => e.Id == "dotnet");
        var result = await _svc.SearchAsync(engine, new GrepSearchRequest
        {
            RootPath = Path.Combine(_dir, "does-not-exist"),
            Pattern = "a",
        });
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("not found").IgnoreCase);
    }
}
