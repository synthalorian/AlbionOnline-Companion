using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class PlayerInfoViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _playerName = string.Empty;

    [ObservableProperty]
    private string _guildName = string.Empty;

    [ObservableProperty]
    private string _allianceName = string.Empty;

    [ObservableProperty]
    private string _killFame = "0";

    [ObservableProperty]
    private string _deathFame = "0";

    [ObservableProperty]
    private string _fameRatio = "0";

    [ObservableProperty]
    private string _gatheringFame = "0";

    [ObservableProperty]
    private string _craftingFame = "0";

    [ObservableProperty]
    private string _totalFame = "0";

    [ObservableProperty]
    private ObservableCollection<PlayerEquipmentItem> _equipment = new();

    [ObservableProperty]
    private ObservableCollection<PlayerStatEntry> _recentStats = new();

    [ObservableProperty]
    private string _statusText = "Enter a player name to search";

    [ObservableProperty]
    private bool _isPlayerFound;

    [RelayCommand]
    private async Task SearchPlayer()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        IsSearching = true;
        StatusText = "Searching...";

        try
        {
            // TODO: Implement player search via API
            await Task.Delay(1000); // Placeholder

            PlayerName = SearchText;
            IsPlayerFound = true;
            StatusText = "Player found";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            IsPlayerFound = false;
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        PlayerName = string.Empty;
        GuildName = string.Empty;
        AllianceName = string.Empty;
        Equipment.Clear();
        RecentStats.Clear();
        IsPlayerFound = false;
        StatusText = "Enter a player name to search";
    }
}

public class PlayerEquipmentItem
{
    public string Slot { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

public class PlayerStatEntry
{
    public string Date { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public string Fame { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
