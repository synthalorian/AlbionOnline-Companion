using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class DungeonTrackerViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<DungeonEntry> _dungeons = new();

    [ObservableProperty]
    private ObservableCollection<string> _statTimeTypes = new();

    [ObservableProperty]
    private string _selectedStatTimeType = "All Time";

    [ObservableProperty]
    private ObservableCollection<string> _dungeonStatsTypes = new();

    [ObservableProperty]
    private string _selectedDungeonStatsType = "Overview";

    [ObservableProperty]
    private string _dungeonCloseTimer = "--:--";

    [ObservableProperty]
    private int _totalDungeons;

    [ObservableProperty]
    private int _totalFame;

    [ObservableProperty]
    private int _totalSilver;

    [ObservableProperty]
    private double _averageFamePerDungeon;

    public DungeonTrackerViewModel()
    {
        StatTimeTypes.Add("Today");
        StatTimeTypes.Add("This Week");
        StatTimeTypes.Add("This Month");
        StatTimeTypes.Add("All Time");

        DungeonStatsTypes.Add("Overview");
        DungeonStatsTypes.Add("By Type");
        DungeonStatsTypes.Add("By Tier");
    }

    [RelayCommand]
    private void DeleteSelectedDungeons()
    {
        // TODO: Implement delete selected
    }

    [RelayCommand]
    private void DeleteZeroFameDungeons()
    {
        // TODO: Implement delete zero fame
    }

    [RelayCommand]
    private void ResetDungeonTracking()
    {
        Dungeons.Clear();
        TotalDungeons = 0;
        TotalFame = 0;
        TotalSilver = 0;
        AverageFamePerDungeon = 0;
    }

    [RelayCommand]
    private void ResetTodaysDungeons()
    {
        // TODO: Implement reset today's dungeons
    }
}

public class DungeonEntry
{
    public string Name { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Fame { get; set; }
    public int Silver { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime EnteredAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
