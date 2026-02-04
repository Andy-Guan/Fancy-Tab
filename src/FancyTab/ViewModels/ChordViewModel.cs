using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyTab.Models;

namespace FancyTab.ViewModels;

/// <summary>
/// 和弦图视图模型
/// </summary>
public partial class ChordViewModel : ObservableObject
{
    [ObservableProperty]
    private Chord? _selectedChord;

    [ObservableProperty]
    private List<Chord> _commonChords = Chord.CommonChords.Values.ToList();

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private List<Chord> _filteredChords = new();

    public ChordViewModel()
    {
        FilteredChords = CommonChords;
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredChords = CommonChords;
        }
        else
        {
            FilteredChords = CommonChords
                .Where(c => c.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    [RelayCommand]
    private void SelectChord(Chord chord)
    {
        SelectedChord = chord.Clone();
    }
}
