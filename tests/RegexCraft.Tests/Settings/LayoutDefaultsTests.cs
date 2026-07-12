using RegexCraft.Core.Settings;

namespace RegexCraft.Tests.Settings;

[TestFixture]
[Category("Library")]
public sealed class LayoutDefaultsTests
{
    [Test]
    public void ResolveRightPanelWidth_Compare_UsesDefaultWhenUnstored()
    {
        var w = LayoutDefaults.ResolveRightPanelWidth(compareMode: true, storedNormal: null, storedCompare: null);
        Assert.That(w, Is.EqualTo(LayoutDefaults.RightPanelCompareDefault));
        Assert.That(w, Is.GreaterThanOrEqualTo(LayoutDefaults.RightPanelCompareMin));
    }

    [Test]
    public void ResolveRightPanelWidth_Normal_UsesDefaultWhenUnstored()
    {
        var w = LayoutDefaults.ResolveRightPanelWidth(compareMode: false, storedNormal: null, storedCompare: null);
        Assert.That(w, Is.EqualTo(LayoutDefaults.RightPanelNormalDefault));
    }

    [Test]
    public void ResolveRightPanelWidth_Compare_UsesStoredAndClamps()
    {
        var w = LayoutDefaults.ResolveRightPanelWidth(true, 350, 600);
        Assert.That(w, Is.EqualTo(600));

        var tooWide = LayoutDefaults.ResolveRightPanelWidth(true, null, 5000);
        Assert.That(tooWide, Is.EqualTo(LayoutDefaults.RightPanelMax));
    }

    [Test]
    public void ResolveRightPanelWidth_Compare_ExpandsTooNarrowStored()
    {
        // Stored 350 is below CompareMin — expand to default so cards are usable.
        var w = LayoutDefaults.ResolveRightPanelWidth(true, 400, 350);
        Assert.That(w, Is.EqualTo(LayoutDefaults.RightPanelCompareDefault));
    }

    [Test]
    public void ClampNormal_RespectsMinMax()
    {
        Assert.That(LayoutDefaults.ClampNormal(10), Is.EqualTo(LayoutDefaults.RightPanelMin));
        Assert.That(LayoutDefaults.ClampNormal(9999), Is.EqualTo(LayoutDefaults.RightPanelMax));
        Assert.That(LayoutDefaults.ClampNormal(420), Is.EqualTo(420));
    }

    [Test]
    public void ClampCompare_UsesCompareMinimum()
    {
        Assert.That(LayoutDefaults.ClampCompare(200), Is.EqualTo(LayoutDefaults.RightPanelCompareMin));
        Assert.That(LayoutDefaults.ClampCompare(540), Is.EqualTo(540));
    }
}
