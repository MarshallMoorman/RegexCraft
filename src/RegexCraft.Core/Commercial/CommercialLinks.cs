namespace RegexCraft.Core.Commercial;

/// <summary>
/// Public commercial URLs. No license keys — honor system.
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
    /// Stripe sandbox Payment Link for the business license ($49 one-time).
    /// Replace with the live Payment Link after Stripe account approval.
    /// </summary>
    public const string BuyLicenseUrl =
        "https://buy.stripe.com/test_00w5kFgOHc4ucQnc8u3oA00";

    /// <summary>Suggested one-time business price shown in UI (display only).</summary>
    public const string SuggestedBusinessPrice = "$49";

    public const string LicenseSummary =
        "Free for personal use. Business / commercial use requires a paid license (honor system — no keys).";
}
