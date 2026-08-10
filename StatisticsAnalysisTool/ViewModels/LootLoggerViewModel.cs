using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class LootLoggerViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LootLogEntry> _lootEntries = new();

    [ObservableProperty]
    private ObservableCollection<LootLogEntry> _filteredLootEntries = new();

    [ObservableProperty]
    private string _notificationFilterSummary = "Filter: All";

    [ObservableProperty]
    private string _trackingSummary = "Tracking: All";

    [ObservableProperty]
    private bool _isTrackingPartyLootOnly = true;

    [ObservableProperty]
    private bool _isTrackingSilver = true;

    [ObservableProperty]
    private bool _isTrackingFame = true;

    [ObservableProperty]
    private bool _isTrackingMobLoot = true;

    [ObservableProperty]
    private bool _isTrackingKill = true;

    [ObservableProperty]
    private string _totalLootValue = "0";

    [ObservableProperty]
    private string _lootValuePerHour = "0 /h";

    [ObservableProperty]
    private bool _isLootComparatorInfoPopupVisible;

    [ObservableProperty]
    private string _chestLogText = string.Empty;

    [ObservableProperty]
    private string _lootComparatorLogCountSummary = "0 chest files | 0 loot log files";

    [ObservableProperty]
    private bool _isCompareButtonEnabled = true;

    [ObservableProperty]
    private bool _isAllButtonsEnabled = true;

    [ObservableProperty]
    private ObservableCollection<string> _lootComparatorSaves = new();

    [ObservableProperty]
    private string? _selectedLootComparatorSave;

    [ObservableProperty]
    private bool _canLoadLootComparatorSave;

    [ObservableProperty]
    private bool _canDeleteLootComparatorSave;

    [ObservableProperty]
    private bool _canSaveLootComparator;

    public LootLoggerViewModel()
    {
        LootEntries.CollectionChanged += (s, e) => ApplyFilter();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredLootEntries.Clear();
        foreach (var entry in LootEntries)
        {
            if (string.IsNullOrWhiteSpace(SearchText) ||
                entry.ItemName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                entry.PlayerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                FilteredLootEntries.Add(entry);
            }
        }
    }

    [RelayCommand]
    private void ResetTrackingNotifications()
    {
        LootEntries.Clear();
        FilteredLootEntries.Clear();
    }

    [RelayCommand]
    private void ExportToCsv()
    {
        // TODO: Implement CSV export
    }

    [RelayCommand]
    private void ExportToJson()
    {
        // TODO: Implement JSON export
    }

    [RelayCommand]
    private void LoadChestLogText()
    {
        // TODO: Implement load chest log text
    }

    [RelayCommand]
    private void UploadChestFiles()
    {
        // TODO: Implement upload chest files
    }

    [RelayCommand]
    private void AddLootLogFiles()
    {
        // TODO: Implement add loot log files
    }

    [RelayCommand]
    private void CompareLogs()
    {
        // TODO: Implement compare logs
    }

    [RelayCommand]
    private void LoadLootComparatorSave()
    {
        // TODO: Implement load save
    }

    [RelayCommand]
    private void DeleteLootComparatorSave()
    {
        // TODO: Implement delete save
    }

    [RelayCommand]
    private void SaveLootComparator()
    {
        // TODO: Implement save
    }

    [RelayCommand]
    private void ClearChestLogs()
    {
        // TODO: Implement clear chest logs
    }

    [RelayCommand]
    private void ClearAllLogs()
    {
        LootEntries.Clear();
        FilteredLootEntries.Clear();
    }

    [RelayCommand]
    private void ToggleLootComparatorInfoPopup()
    {
        IsLootComparatorInfoPopupVisible = !IsLootComparatorInfoPopupVisible;
    }
}

public class LootLogEntry
{
    public string ItemName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int EstimatedValue { get; set; }
    public DateTime Timestamp { get; set; }
    public string LootType { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
}
