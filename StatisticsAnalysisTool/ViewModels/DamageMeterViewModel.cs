using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class DamageMeterViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isDamageMeterActive = true;

    [ObservableProperty]
    private string _damageMeterActivationToggleIcon = "⏻";

    [ObservableProperty]
    private string _damageMeterActivationToggleColor = "#FFCDD6F4";

    [ObservableProperty]
    private bool _isDamageMeterResetByMapChangeActive;

    [ObservableProperty]
    private bool _isDamageMeterResetBeforeCombatActive;

    [ObservableProperty]
    private bool _isSnapshotAfterMapChangeActive;

    [ObservableProperty]
    private bool _shortDamageMeterToClipboard;

    [ObservableProperty]
    private bool _onlyDamageToPlayersCounts;

    [ObservableProperty]
    private ObservableCollection<DamageMeterEntry> _damageMeterEntries = new();

    [ObservableProperty]
    private ObservableCollection<DamageMeterSnapshot> _damageMeterSnapshots = new();

    [ObservableProperty]
    private DamageMeterSnapshot? _selectedSnapshot;

    [ObservableProperty]
    private string _selectedSortOption = "Damage";

    [ObservableProperty]
    private ObservableCollection<string> _sortOptions = new();

    [ObservableProperty]
    private bool _isInfoPopupVisible;

    // YOUR STATS tab — local player's own combat numbers
    [ObservableProperty]
    private bool _hasYourStats;

    [ObservableProperty]
    private string _yourName = string.Empty;

    [ObservableProperty]
    private string _yourRank = string.Empty;

    [ObservableProperty]
    private string _yourDamage = "0";

    [ObservableProperty]
    private string _yourDps = "0";

    [ObservableProperty]
    private string _yourHealing = "0";

    [ObservableProperty]
    private string _yourHps = "0";

    public DamageMeterViewModel()
    {
        SortOptions.Add("Damage");
        SortOptions.Add("DPS");
        SortOptions.Add("Healing");
        SortOptions.Add("HPS");
    }

    // Sort dropdown changed: re-format and re-rank existing entries
    partial void OnSelectedSortOptionChanged(string value)
    {
        Network.Handlers.DamageMeterEventHandler.RefreshView(this);
    }

    [RelayCommand]
    private void ToggleDamageMeter()
    {
        IsDamageMeterActive = !IsDamageMeterActive;
        DamageMeterActivationToggleColor = IsDamageMeterActive ? "#FF89B4FA" : "#FFCDD6F4";
    }

    [RelayCommand]
    private void ResetDamageMeter()
    {
        DamageMeterEntries.Clear();
    }

    [RelayCommand]
    private void TakeSnapshot()
    {
        // TODO: Implement snapshot logic
    }

    [RelayCommand]
    private void DeleteSelectedSnapshot()
    {
        if (SelectedSnapshot != null)
        {
            DamageMeterSnapshots.Remove(SelectedSnapshot);
        }
    }

    [RelayCommand]
    private void DeleteAllSnapshots()
    {
        DamageMeterSnapshots.Clear();
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        // TODO: Implement clipboard copy
    }

    [RelayCommand]
    private void ToggleInfoPopup()
    {
        IsInfoPopupVisible = !IsInfoPopupVisible;
    }
}

public class DamageMeterEntry
{
    public int Rank { get; set; }
    public long CauserId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string ValueString { get; set; } = string.Empty;
    public double Damage { get; set; }
    public double Dps { get; set; }
    public double Healing { get; set; }
    public double Hps { get; set; }
}

public class DamageMeterSnapshot
{
    public string TimestampString { get; set; } = string.Empty;
    public ObservableCollection<DamageMeterEntry> Entries { get; set; } = new();
}
