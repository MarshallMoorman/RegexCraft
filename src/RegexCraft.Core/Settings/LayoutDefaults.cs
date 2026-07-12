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
    /// Fallback absolute Compare width when window size is unknown (tests / first layout).
    /// Prefer <see cref="ResolveCompareWidth"/> with live body width.
    /// </summary>
    public const double RightPanelCompareDefault = 780;

    /// <summary>Hard minimum for the right panel (matches MainWindow MinWidth on host).</summary>
    public const double RightPanelMin = 300;

    /// <summary>
    /// Minimum usable Compare width. Stored values below this are treated as stale
    /// (e.g. from the old ~520px default) and re-expanded.
    /// </summary>
    public const double RightPanelCompareMin = 640;

    /// <summary>
    /// When Compare is active, the center (editor + analysis) column is fixed to this
    /// width so the right panel can take the rest of the body (star).
    /// </summary>
    public const double CenterWidthWhenCompare = 280;

    /// <summary>Floor for the center column while Compare is active (still readable).</summary>
    public const double CenterMinWhenCompare = 200;

    /// <summary>
    /// Fraction of the body width (left sidebar excluded) that Compare should claim
    /// when applying an absolute pixel fallback.
    /// </summary>
    public const double CompareShareOfBody = 0.72;

    /// <summary>Upper clamp for remembered Normal widths.</summary>
    public const double RightPanelNormalMax = 900;

    /// <summary>Upper clamp for remembered Compare widths (large monitors).</summary>
    public const double RightPanelCompareMax = 1600;

    /// <summary>Clamp a Normal-mode width to the allowed range.</summary>
    public static double ClampNormal(double width) =>
        Math.Clamp(width, RightPanelMin, RightPanelNormalMax);

    /// <summary>
    /// Clamp a Compare-mode absolute width.
    /// </summary>
    public static double ClampCompare(double width) =>
        Math.Clamp(width, RightPanelCompareMin, RightPanelCompareMax);

    /// <summary>
    /// Whether a stored Compare width is large enough to honor (not a pre-1.0.1 small default).
    /// </summary>
    public static bool IsUsableCompareWidth(double? stored) =>
        stored is >= RightPanelCompareMin;

    /// <summary>
    /// Resolve an absolute right-panel width for Normal mode.
    /// </summary>
    public static double ResolveNormalWidth(double? storedNormal)
    {
        var normal = storedNormal is > 0 ? storedNormal.Value : RightPanelNormalDefault;
        return ClampNormal(normal);
    }

    /// <summary>
    /// Resolve the preferred absolute Compare right-panel width from the live body width.
    /// Used when star layout is not applied, or to seed remembered widths.
    /// </summary>
    /// <param name="bodyWidth">Full main body grid width (includes left sidebar).</param>
    /// <param name="storedCompare">Optional user-remembered Compare width.</param>
    public static double ResolveCompareWidth(double bodyWidth, double? storedCompare)
    {
        // Content row after left sidebar + two splitters (~5+5).
        var contentWidth = bodyWidth > LeftSidebarWidth + 20
            ? bodyWidth - LeftSidebarWidth - 10
            : Math.Max(bodyWidth * 0.75, RightPanelCompareDefault);

        var preferred = contentWidth * CompareShareOfBody;
        // Leave a thin strip for the editor so the pattern remains visible.
        var maxForRight = Math.Max(RightPanelCompareMin, contentWidth - CenterMinWhenCompare);
        preferred = Math.Clamp(preferred, RightPanelCompareMin, maxForRight);

        if (IsUsableCompareWidth(storedCompare))
        {
            // Honor a user drag, but never collapse below the share floor for this window.
            var floor = Math.Min(preferred, maxForRight);
            return Math.Clamp(storedCompare!.Value, floor * 0.9, maxForRight);
        }

        return preferred;
    }

    /// <summary>
    /// Resolve the width to apply for the given mode from optional stored values.
    /// When <paramref name="bodyWidth"/> is provided and &gt; 0, Compare uses the share formula.
    /// </summary>
    public static double ResolveRightPanelWidth(
        bool compareMode,
        double? storedNormal,
        double? storedCompare,
        double bodyWidth = 0)
    {
        if (compareMode)
        {
            if (bodyWidth > 0)
                return ResolveCompareWidth(bodyWidth, storedCompare);

            if (IsUsableCompareWidth(storedCompare))
                return ClampCompare(storedCompare!.Value);

            return RightPanelCompareDefault;
        }

        return ResolveNormalWidth(storedNormal);
    }
}
