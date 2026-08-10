using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _totalGainedFameInSession = "0";

    [ObservableProperty]
    private string _famePerHour = "0 /h";

    [ObservableProperty]
    private string _totalGainedSilverInSession = "0";

    [ObservableProperty]
    private string _silverPerHour = "0 /h";

    [ObservableProperty]
    private string _totalGainedReSpecPointsInSession = "0";

    [ObservableProperty]
    private string _reSpecPointsPerHour = "0 /h";

    [ObservableProperty]
    private string _totalGainedMightInSession = "0";

    [ObservableProperty]
    private string _mightPerHour = "0 /h";

    [ObservableProperty]
    private string _totalGainedFavorInSession = "0";

    [ObservableProperty]
    private string _favorPerHour = "0 /h";

    [ObservableProperty]
    private bool _isTrackingActive;

    [ObservableProperty]
    private bool _isTrackingResetByMapChangeActive;

    [ObservableProperty]
    private string _killsDeathsText = "KILLS/DEATHS";

    [ObservableProperty]
    private bool _killDeathStatsVisible = true;

    [ObservableProperty]
    private string _lootedChestsText = "LOOTED CHESTS";

    [ObservableProperty]
    private bool _lootedChestsStatsVisible = true;

    [ObservableProperty]
    private string _reSpecText = "RESPEC";

    [ObservableProperty]
    private bool _reSpecStatsVisible = true;

    [ObservableProperty]
    private string _repairCostsText = "REPAIR COSTS";

    [ObservableProperty]
    private bool _repairCostsStatsVisible = true;

    [ObservableProperty]
    private string _activityChartText = "ACTIVITY CHART";

    [ObservableProperty]
    private bool _activityChartVisible = true;

    [ObservableProperty]
    private ObservableCollection<string> _dashboardChartRanges = new();

    [ObservableProperty]
    private string _selectedDashboardChartRange = "24h";

    public DashboardViewModel()
    {
        DashboardChartRanges.Add("1h");
        DashboardChartRanges.Add("6h");
        DashboardChartRanges.Add("12h");
        DashboardChartRanges.Add("24h");
        DashboardChartRanges.Add("7d");
    }

    [RelayCommand]
    private void ResetTrackingCounter()
    {
        // TODO: Implement tracking counter reset
    }

    [RelayCommand]
    private void ToggleKillDeathStats()
    {
        KillDeathStatsVisible = !KillDeathStatsVisible;
    }

    [RelayCommand]
    private void ToggleLootedChestsStats()
    {
        LootedChestsStatsVisible = !LootedChestsStatsVisible;
    }

    [RelayCommand]
    private void ToggleReSpecStats()
    {
        ReSpecStatsVisible = !ReSpecStatsVisible;
    }

    [RelayCommand]
    private void ToggleRepairCostsStats()
    {
        RepairCostsStatsVisible = !RepairCostsStatsVisible;
    }

    [RelayCommand]
    private void ToggleActivityChart()
    {
        ActivityChartVisible = !ActivityChartVisible;
    }
}
