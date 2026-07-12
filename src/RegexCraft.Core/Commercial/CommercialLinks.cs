namespace RegexCraft.Core.Commercial;

/// <summary>
/// Public commercial URLs and placeholders. No license keys — honor system.
/// Update <see cref="BuyLicenseUrl"/> when the payment product is live.
/// </summary>
public static class CommercialLinks
{
    public const string WebsiteUrl = "https://regexcraft.com";
    public const string DownloadUrl = "https://regexcraft.com/download.html";
    public const string PricingUrl = "https://regexcraft.com/pricing.html";
    public const string EulaUrl = "https://regexcraft.com/eula.html";
    public const string DocsUrl = "https://regexcraft.com/docs.html";

    /// <summary>
    /// Public dist repo for portable binaries (Actions publishes releases here).
    /// </summary>
    public const string DistRepoUrl = "https://github.com/MarshallMoorman/RegexCraft-Releases";

    /// <summary>Latest release page on the public dist repo.</summary>
    public const string DistLatestReleaseUrl =
        "https://github.com/MarshallMoorman/RegexCraft-Releases/releases/latest";

    /// <summary>
    /// Checkout / buy URL. Replace with Gumroad, Lemon Squeezy, or Stripe Payment Link
    /// when Marshall creates the product (see docs/development/commercial.md).
    /// </summary>
    public const string BuyLicenseUrl = "https://regexcraft.com/pricing.html#buy";

    /// <summary>Suggested one-time business price shown in UI (display only).</summary>
    public const string SuggestedBusinessPrice = "$49";

    public const string LicenseSummary =
        "Free for personal use. Business / commercial use requires a paid license (honor system — no keys).";
}
