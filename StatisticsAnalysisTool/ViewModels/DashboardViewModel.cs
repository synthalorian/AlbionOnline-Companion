using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    // Player info
    [ObservableProperty] private string _playerName = "Unknown";
    [ObservableProperty] private string _guildName = "";
    [ObservableProperty] private string _currentZone = "Unknown";

    // Total stats (from game)
    [ObservableProperty] private string _totalFame = "0";
    [ObservableProperty] private string _totalSilver = "0";
    [ObservableProperty] private string _totalReSpecPoints = "0";

    // Session stats (gained since tracking started)
    [ObservableProperty] private string _sessionFame = "0";
    [ObservableProperty] private string _famePerHour = "0 /h";
    [ObservableProperty] private string _sessionSilver = "0";
    [ObservableProperty] private string _silverPerHour = "0 /h";
    [ObservableProperty] private string _sessionReSpec = "0";
    [ObservableProperty] private string _reSpecPerHour = "0 /h";
    [ObservableProperty] private string _sessionMight = "0";
    [ObservableProperty] private string _mightPerHour = "0 /h";
    [ObservableProperty] private string _sessionFavor = "0";
    [ObservableProperty] private string _favorPerHour = "0 /h";

    // Combat fame (from kills/mobs)
    [ObservableProperty] private string _combatFame = "0";
    [ObservableProperty] private string _combatFamePerHour = "0 /h";

    // Gathering fame (from harvesting)
    [ObservableProperty] private string _gatheringFame = "0";
    [ObservableProperty] private string _gatheringFamePerHour = "0 /h";

    // Crafting fame
    [ObservableProperty] private string _craftingFame = "0";
    [ObservableProperty] private string _craftingFamePerHour = "0 /h";

    // Farming fame (island crops/animals)
    [ObservableProperty] private string _farmingFame = "0";
    [ObservableProperty] private string _farmingFamePerHour = "0 /h";

    // Kill/Death
    [ObservableProperty] private string _killsDeathsText = "0 / 0";
    [ObservableProperty] private string _kdRatio = "0.00";
    [ObservableProperty] private bool _killDeathStatsVisible = true;

    // Chests
    [ObservableProperty] private string _lootedChestsText = "0";
    [ObservableProperty] private bool _lootedChestsStatsVisible = true;

    // Visibility toggles
    [ObservableProperty] private bool _reSpecStatsVisible = true;
    [ObservableProperty] private bool _repairCostsStatsVisible = true;
    [ObservableProperty] private bool _activityChartVisible = true;

    // Chart
    [ObservableProperty] private ObservableCollection<string> _dashboardChartRanges = new();
    [ObservableProperty] private string _selectedDashboardChartRange = "24h";

    // Session timer
    [ObservableProperty] private string _sessionDuration = "0:00";

    private DateTime _sessionStart = DateTime.UtcNow;

    public DashboardViewModel()
    {
        DashboardChartRanges.Add("1h");
        DashboardChartRanges.Add("6h");
        DashboardChartRanges.Add("12h");
        DashboardChartRanges.Add("24h");
        DashboardChartRanges.Add("7d");
    }

    public void UpdateSessionDuration()
    {
        var elapsed = DateTime.UtcNow - _sessionStart;
        SessionDuration = $"{(int)elapsed.TotalHours}:{elapsed.Minutes:D2}";
    }

    [RelayCommand]
    private void ResetTrackingCounter()
    {
        _sessionStart = DateTime.UtcNow;
        SessionFame = "0";
        FamePerHour = "0 /h";
        SessionSilver = "0";
        SilverPerHour = "0 /h";
        SessionReSpec = "0";
        ReSpecPerHour = "0 /h";
        SessionMight = "0";
        MightPerHour = "0 /h";
        SessionFavor = "0";
        FavorPerHour = "0 /h";
        CombatFame = "0";
        CombatFamePerHour = "0 /h";
        GatheringFame = "0";
        GatheringFamePerHour = "0 /h";
        CraftingFame = "0";
        CraftingFamePerHour = "0 /h";
        FarmingFame = "0";
        FarmingFamePerHour = "0 /h";
        KillsDeathsText = "0 / 0";
        KdRatio = "0.00";
        LootedChestsText = "0";
        SessionDuration = "0:00";
    }

    [RelayCommand] private void ToggleKillDeathStats() => KillDeathStatsVisible = !KillDeathStatsVisible;
    [RelayCommand] private void ToggleLootedChestsStats() => LootedChestsStatsVisible = !LootedChestsStatsVisible;
    [RelayCommand] private void ToggleReSpecStats() => ReSpecStatsVisible = !ReSpecStatsVisible;
    [RelayCommand] private void ToggleRepairCostsStats() => RepairCostsStatsVisible = !RepairCostsStatsVisible;
    [RelayCommand] private void ToggleActivityChart() => ActivityChartVisible = !ActivityChartVisible;
}
