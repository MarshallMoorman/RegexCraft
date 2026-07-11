using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegexCraft.Core.Models;

namespace RegexCraft.App.ViewModels;

public partial class MatchItemViewModel : ViewModelBase
{
    public MatchItemViewModel(int index, MatchResult match, Action<string>? copyText = null, Action<int, int>? selectRange = null)
    {
        Index = index;
        Start = match.Index;
        Length = match.Length;
        Value = match.Value;
        _copyText = copyText;
        _selectRange = selectRange;

        foreach (var g in match.Groups.Where(g => g.Number > 0))
        {
            Groups.Add(new GroupItemViewModel(g, copyText, selectRange));
        }
    }

    private readonly Action<string>? _copyText;
    private readonly Action<int, int>? _selectRange;

    public int Index { get; }
    public int Start { get; }
    public int Length { get; }
    public string Value { get; }
    public string Summary => $"[{Index}] {Start}+{Length}: {Truncate(Value, 80)}";

    public ObservableCollection<GroupItemViewModel> Groups { get; } = new();

    [ObservableProperty]
    private bool _isExpanded = true;

    [RelayCommand]
    private void CopyMatch() => _copyText?.Invoke(Value);

    [RelayCommand]
    private void SelectMatch() => _selectRange?.Invoke(Start, Length);

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

public partial class GroupItemViewModel : ViewModelBase
{
    public GroupItemViewModel(GroupResult group, Action<string>? copyText = null, Action<int, int>? selectRange = null)
    {
        Number = group.Number;
        Name = group.Name;
        Success = group.Success;
        Start = group.Index;
        Length = group.Length;
        Value = group.Value;
        DisplayName = group.Name != group.Number.ToString()
            ? $"G{group.Number}/{group.Name}"
            : $"G{group.Number}";
        DisplayValue = group.Success ? group.Value : "(no match)";
        RangeText = group.Success ? $"{group.Index}+{group.Length}" : "—";
        _copyText = copyText;
        _selectRange = selectRange;
    }

    private readonly Action<string>? _copyText;
    private readonly Action<int, int>? _selectRange;

    public int Number { get; }
    public string Name { get; }
    public bool Success { get; }
    public int Start { get; }
    public int Length { get; }
    public string Value { get; }
    public string DisplayName { get; }
    public string DisplayValue { get; }
    public string RangeText { get; }

    [RelayCommand]
    private void CopyGroup()
    {
        if (Success)
            _copyText?.Invoke(Value);
    }

    [RelayCommand]
    private void SelectGroup()
    {
        if (Success && Start >= 0 && Length >= 0)
            _selectRange?.Invoke(Start, Length);
    }
}
