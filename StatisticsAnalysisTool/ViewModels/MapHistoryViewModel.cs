using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StatisticsAnalysisTool.Network;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.ViewModels;

public partial class MapHistoryViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<MapVisitEntry> _visits = new();

    [ObservableProperty]
    private string _currentZone = "Unknown";

    [ObservableProperty]
    private string _timeInCurrentZone = "0:00";

    [ObservableProperty]
    private string _totalZonesVisited = "0";

    [ObservableProperty]
    private string _totalTimeExploring = "0:00";

    [ObservableProperty]
    private string _mostVisitedZone = "None";

    public MapHistoryViewModel()
    {
        // Subscribe to cluster changes
        ClusterTracker.Instance.ClusterChanged += OnClusterChanged;
    }

    private void OnClusterChanged(object? sender, ClusterChangedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            CurrentZone = e.NewClusterName.Length > 0 ? e.NewClusterName : e.NewCluster;

            Visits.Insert(0, new MapVisitEntry
            {
                ZoneName = CurrentZone,
                ClusterId = e.NewCluster,
                EnteredAt = DateTime.Now,
                PreviousZone = e.PreviousCluster,
                TimeInPrevious = e.TimeInPrevious
            });

            TotalZonesVisited = Visits.Count.ToString();

            var totalTime = Visits.Sum(v => v.TimeInPrevious.TotalMinutes);
            TotalTimeExploring = $"{(int)(totalTime / 60)}:{(int)(totalTime % 60):D2}";

            var mostVisited = Visits
                .GroupBy(v => v.ZoneName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            MostVisitedZone = mostVisited?.Key ?? "None";
        });
    }

    [RelayCommand]
    private void ClearHistory()
    {
        Visits.Clear();
        TotalZonesVisited = "0";
        TotalTimeExploring = "0:00";
        MostVisitedZone = "None";
        ClusterTracker.Instance.ClearHistory();
    }
}

public class MapVisitEntry
{
    public string ZoneName { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public DateTime EnteredAt { get; set; }
    public string PreviousZone { get; set; } = string.Empty;
    public TimeSpan TimeInPrevious { get; set; }
    public string TimeInPreviousDisplay => $"{(int)TimeInPrevious.TotalMinutes}m {TimeInPrevious.Seconds}s";
}
