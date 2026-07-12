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
        // Wide enough for six mode tabs including full "Compare" label
        Assert.That(w, Is.GreaterThanOrEqualTo(440));
    }

    [Test]
    public void ResolveCompareWidth_TakesMajorityOfBody()
    {
        const double body = 1320;
        var w = LayoutDefaults.ResolveCompareWidth(body, storedCompare: null);
        var content = body - LayoutDefaults.LeftSidebarWidthWhenCompare - 10;
        Assert.That(w, Is.EqualTo(content * LayoutDefaults.CompareShareOfBody).Within(1));
        Assert.That(w, Is.GreaterThan(content * 0.7), "Compare must take most of the content row");
        Assert.That(w, Is.GreaterThanOrEqualTo(LayoutDefaults.RightPanelCompareMin));
    }

    [Test]
    public void ResolveCompareWidth_IgnoresStaleNarrowStoredWidth()
    {
        var w = LayoutDefaults.ResolveCompareWidth(1320, storedCompare: 520);
        Assert.That(w, Is.GreaterThanOrEqualTo(LayoutDefaults.RightPanelCompareMin));
        Assert.That(w, Is.GreaterThan(520));
        Assert.That(LayoutDefaults.IsUsableCompareWidth(520), Is.False);
    }

    [Test]
    public void ResolveCompareWidth_HonorsLargeUserDrag()
    {
        var w = LayoutDefaults.ResolveCompareWidth(1600, storedCompare: 1000);
        Assert.That(w, Is.InRange(900, 1200));
    }

    [Test]
    public void ResolveRightPanelWidth_Compare_UsesBodyWhenProvided()
    {
        var w = LayoutDefaults.ResolveRightPanelWidth(true, null, null, bodyWidth: 1320);
        Assert.That(w, Is.GreaterThan(800));
    }

    [Test]
    public void ClampNormal_RespectsMinMax()
    {
        Assert.That(LayoutDefaults.ClampNormal(10), Is.EqualTo(LayoutDefaults.RightPanelMin));
        Assert.That(LayoutDefaults.ClampNormal(9999), Is.EqualTo(LayoutDefaults.RightPanelNormalMax));
        Assert.That(LayoutDefaults.ClampNormal(420), Is.EqualTo(420));
    }

    [Test]
    public void ClampCompare_UsesCompareMinimum()
    {
        Assert.That(LayoutDefaults.ClampCompare(200), Is.EqualTo(LayoutDefaults.RightPanelCompareMin));
        Assert.That(LayoutDefaults.ClampCompare(900), Is.EqualTo(900));
        Assert.That(LayoutDefaults.ClampCompare(5000), Is.EqualTo(LayoutDefaults.RightPanelCompareMax));
    }
}
