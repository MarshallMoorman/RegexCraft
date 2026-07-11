using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RegexCraft.Core.Models;

namespace RegexCraft.App.ViewModels;

public partial class MatchItemViewModel : ViewModelBase
{
    public MatchItemViewModel(int index, MatchResult match)
    {
        Index = index;
        Start = match.Index;
        Length = match.Length;
        Value = match.Value;

        foreach (var g in match.Groups.Where(g => g.Number > 0))
        {
            Groups.Add(new GroupItemViewModel(g));
        }
    }

    public int Index { get; }
    public int Start { get; }
    public int Length { get; }
    public string Value { get; }
    public string Summary => $"[{Index}] {Start}+{Length}: {Truncate(Value, 80)}";

    public ObservableCollection<GroupItemViewModel> Groups { get; } = new();

    [ObservableProperty]
    private bool _isExpanded = true;

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

public sealed class GroupItemViewModel
{
    public GroupItemViewModel(GroupResult group)
    {
        Number = group.Number;
        Name = group.Name;
        Success = group.Success;
        Start = group.Index;
        Length = group.Length;
        Value = group.Value;
        DisplayName = group.Name != group.Number.ToString()
            ? $"Group {group.Number} / {group.Name}"
            : $"Group {group.Number}";
        DisplayValue = group.Success ? group.Value : "(no match)";
        RangeText = group.Success ? $"{group.Index}+{group.Length}" : "—";
    }

    public int Number { get; }
    public string Name { get; }
    public bool Success { get; }
    public int Start { get; }
    public int Length { get; }
    public string Value { get; }
    public string DisplayName { get; }
    public string DisplayValue { get; }
    public string RangeText { get; }
}
