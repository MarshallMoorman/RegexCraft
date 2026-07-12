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
        // Accept stable (1.0.0) and pre-release (1.0.0-rc1) informational versions.
        Assert.That(v, Does.Match(@"^\d+\.\d+\.\d+(-[A-Za-z0-9.]+)?$"));
    }

    [Test]
    public void WebsiteAndGitHubUrls_AreHttps()
    {
        Assert.That(AboutWindow.WebsiteUrl, Does.StartWith("https://"));
        Assert.That(AboutWindow.WebsiteUrl, Does.Contain("regexcraft.com"));
        Assert.That(AboutWindow.GitHubUrl, Does.StartWith("https://"));
        Assert.That(AboutWindow.GitHubUrl, Does.Contain("github.com"));
        Assert.That(AboutWindow.GitHubUrl, Does.Contain("RegexCraft-Releases"));
        Assert.That(AboutWindow.PricingUrl, Does.Contain("pricing"));
        Assert.That(AboutWindow.EulaUrl, Does.Contain("eula"));
    }
}
