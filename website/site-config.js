/**
 * Public site configuration (editable placeholders for Marshall).
 * Keep buy URL in sync with docs/development/commercial.md when the payment product is live.
 */
window.RegexCraftSite = {
  version: "1.2.0",
  distRepo: "https://github.com/MarshallMoorman/RegexCraft-Releases",
  distLatest: "https://github.com/MarshallMoorman/RegexCraft-Releases/releases/latest",
  /** Direct asset pattern: {latest}/download/RegexCraft-{rid}.zip */
  asset: function (rid) {
    return (
      "https://github.com/MarshallMoorman/RegexCraft-Releases/releases/latest/download/RegexCraft-" +
      rid +
      ".zip"
    );
  },
  /** Replace with Gumroad / Lemon Squeezy / Stripe Payment Link when ready. */
  buyUrl: "https://regexcraft.com/pricing.html#buy",
  businessPrice: "$49",
  eulaUrl: "eula.html",
  pricingUrl: "pricing.html",
};
