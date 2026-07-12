using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RegexCraft.Core.Compare;
using RegexCraft.Core.Flavors;

namespace RegexCraft.App.ViewModels;

/// <summary>Selectable flavor row for the Compare panel.</summary>
public partial class CompareFlavorChoiceViewModel : ViewModelBase
{
    public CompareFlavorChoiceViewModel(FlavorDefinition flavor, bool isSelected = false)
    {
        Flavor = flavor;
        DisplayName = flavor.DisplayName;
        EngineId = flavor.EngineId;
        FidelityLabel = flavor.Fidelity.DisplayName();
        IsSelected = isSelected;
    }

    public FlavorDefinition Flavor { get; }
    public string DisplayName { get; }
    public string EngineId { get; }
    public string FidelityLabel { get; }
    public string Subtitle => $"{EngineId} · {FidelityLabel}";

    [ObservableProperty]
    private bool _isSelected;
}

/// <summary>One result card in the Compare panel.</summary>
public sealed class CompareCardViewModel
{
    public CompareCardViewModel(FlavorCompareResult result)
    {
        Result = result;
        Header = result.HeaderLine;
        ValidityLabel = result.ValidityLabel;
        IsValid = result.IsValid;
        ErrorText = result.ErrorMessage ?? string.Empty;
        MatchCountLabel = result.MatchCountLabel;
        DurationLabel = result.DurationLabel;
        FidelityBadge = result.FidelityBadge;
        FidelityNote = result.FidelityNote ?? string.Empty;
        ShowFidelityNote = !string.IsNullOrWhiteSpace(result.FidelityNote)
            && result.Fidelity != TestingFidelity.Full;

        foreach (var m in result.Matches)
        {
            MatchLines.Add(m.SummaryLine);
            foreach (var g in m.GroupSummaries)
                MatchLines.Add("  " + g);
        }

        foreach (var n in result.KeyNotes)
            KeyNotes.Add(n);

        HasMatches = MatchLines.Count > 0;
        HasNotes = KeyNotes.Count > 0;
        EmptyMatchesMessage = IsValid
            ? (result.MatchCount == 0 ? "No matches" : string.Empty)
            : "—";
    }

    public FlavorCompareResult Result { get; }
    public string Header { get; }
    public string ValidityLabel { get; }
    public bool IsValid { get; }
    public string ErrorText { get; }
    public string MatchCountLabel { get; }
    public string DurationLabel { get; }
    public string FidelityBadge { get; }
    public string FidelityNote { get; }
    public bool ShowFidelityNote { get; }
    public bool HasMatches { get; }
    public bool HasNotes { get; }
    public string EmptyMatchesMessage { get; }
    public ObservableCollection<string> MatchLines { get; } = new();
    public ObservableCollection<string> KeyNotes { get; } = new();
}
