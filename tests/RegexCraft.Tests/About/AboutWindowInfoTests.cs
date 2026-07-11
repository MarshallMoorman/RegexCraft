using RegexCraft.App.Views;

namespace RegexCraft.Tests.About;

[TestFixture]
[Category("Branding")]
public sealed class AboutWindowInfoTests
{
    [Test]
    public void GetAppVersion_IsSemVerShape()
    {
        var v = AboutWindow.GetAppVersion();
        Assert.That(v, Does.Match(@"^\d+\.\d+\.\d+$"));
    }

    [Test]
    public void WebsiteAndGitHubUrls_AreHttps()
    {
        Assert.That(AboutWindow.WebsiteUrl, Does.StartWith("https://"));
        Assert.That(AboutWindow.WebsiteUrl, Does.Contain("regexcraft.com"));
        Assert.That(AboutWindow.GitHubUrl, Does.StartWith("https://"));
        Assert.That(AboutWindow.GitHubUrl, Does.Contain("github.com"));
    }
}
