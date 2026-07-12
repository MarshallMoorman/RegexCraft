namespace RegexCraft.Core.Settings;

/// <summary>
/// Named layout constants for main-window column widths.
/// Keep pixel defaults here — not scattered as magic numbers in views/VMs.
/// </summary>
public static class LayoutDefaults
{
    /// <summary>Default left sidebar (Tokens / Library / History) width in px.</summary>
    public const double LeftSidebarWidth = 280;

    /// <summary>Default right panel width for Test / Replace / Split / Generate / GREP.</summary>
    public const double RightPanelNormalDefault = 400;

    /// <summary>
    /// Default right panel width when Compare is active.
    /// Wide enough for multi-flavor cards (~40–45% of a typical 1320px window).
    /// </summary>
    public const double RightPanelCompareDefault = 520;

    /// <summary>Hard minimum for the right panel (matches MainWindow MinWidth on host).</summary>
    public const double RightPanelMin = 300;

    /// <summary>Sensible minimum when expanding to Compare if no stored width exists.</summary>
    public const double RightPanelCompareMin = 480;

    /// <summary>Upper clamp so a wide monitor drag cannot dominate the layout forever.</summary>
    public const double RightPanelMax = 900;

    /// <summary>Clamp a Normal-mode width to the allowed range.</summary>
    public static double ClampNormal(double width) =>
        Math.Clamp(width, RightPanelMin, RightPanelMax);

    /// <summary>
    /// Clamp a Compare-mode width. Uses the Compare minimum so cards stay usable.
    /// </summary>
    public static double ClampCompare(double width) =>
        Math.Clamp(width, RightPanelCompareMin, RightPanelMax);

    /// <summary>
    /// Resolve the width to apply for the given mode from optional stored values.
    /// </summary>
    public static double ResolveRightPanelWidth(bool compareMode, double? storedNormal, double? storedCompare)
    {
        if (compareMode)
        {
            var value = storedCompare is > 0 ? storedCompare.Value : RightPanelCompareDefault;
            // Ensure first-time / too-narrow stored values still expand usefully.
            if (value < RightPanelCompareMin)
                value = RightPanelCompareDefault;
            return ClampCompare(value);
        }

        var normal = storedNormal is > 0 ? storedNormal.Value : RightPanelNormalDefault;
        return ClampNormal(normal);
    }
}
