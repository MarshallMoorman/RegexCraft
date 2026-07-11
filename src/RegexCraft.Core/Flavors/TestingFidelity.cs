namespace RegexCraft.Core.Flavors;

/// <summary>
/// How closely live Test / Replace / Split / GREP match the real flavor dialect.
/// </summary>
public enum TestingFidelity
{
    /// <summary>Native engine for this flavor — results should match production closely.</summary>
    Full = 0,

    /// <summary>Native or near-native engine with known minor dialect gaps.</summary>
    High = 1,

    /// <summary>Closest available engine; results may differ from the real flavor.</summary>
    Approximate = 2,

    /// <summary>No dedicated test engine; testing falls back with a strong warning.</summary>
    CodegenOnly = 3,
}

public static class TestingFidelityExtensions
{
    public static string DisplayName(this TestingFidelity fidelity) => fidelity switch
    {
        TestingFidelity.Full => "Full",
        TestingFidelity.High => "High",
        TestingFidelity.Approximate => "Approximate",
        TestingFidelity.CodegenOnly => "Codegen only",
        _ => fidelity.ToString(),
    };

    public static bool IsApproximateOrWeaker(this TestingFidelity fidelity) =>
        fidelity is TestingFidelity.Approximate or TestingFidelity.CodegenOnly;
}
