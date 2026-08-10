using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.ViewModels;

public partial class GatheringViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _totalFiber = "0";

    [ObservableProperty]
    private string _totalHide = "0";

    [ObservableProperty]
    private string _totalOre = "0";

    [ObservableProperty]
    private string _totalStone = "0";

    [ObservableProperty]
    private string _totalWood = "0";

    [ObservableProperty]
    private string _fiberPerHour = "0 /h";

    [ObservableProperty]
    private string _hidePerHour = "0 /h";

    [ObservableProperty]
    private string _orePerHour = "0 /h";

    [ObservableProperty]
    private string _stonePerHour = "0 /h";

    [ObservableProperty]
    private string _woodPerHour = "0 /h";

    [ObservableProperty]
    private string _totalGathered = "0";

    [ObservableProperty]
    private string _gatheringSessionTime = "0:00";

    [ObservableProperty]
    private ObservableCollection<GatheringEntry> _recentGathering = new();

    private double _fiberCount, _hideCount, _oreCount, _stoneCount, _woodCount;
    private DateTime _sessionStart = DateTime.UtcNow;

    public void AddGatheredResource(string resourceType, int amount, string resourceName, int tier)
    {
        switch (resourceType.ToLower())
        {
            case "fiber": _fiberCount += amount; break;
            case "hide": _hideCount += amount; break;
            case "ore": _oreCount += amount; break;
            case "stone": _stoneCount += amount; break;
            case "wood": _woodCount += amount; break;
        }

        var total = _fiberCount + _hideCount + _oreCount + _stoneCount + _woodCount;
        var hours = Math.Max(0.001, (DateTime.UtcNow - _sessionStart).TotalHours);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            TotalFiber = FormatNumber(_fiberCount);
            TotalHide = FormatNumber(_hideCount);
            TotalOre = FormatNumber(_oreCount);
            TotalStone = FormatNumber(_stoneCount);
            TotalWood = FormatNumber(_woodCount);
            TotalGathered = FormatNumber(total);

            FiberPerHour = FormatNumber(_fiberCount / hours) + " /h";
            HidePerHour = FormatNumber(_hideCount / hours) + " /h";
            OrePerHour = FormatNumber(_oreCount / hours) + " /h";
            StonePerHour = FormatNumber(_stoneCount / hours) + " /h";
            WoodPerHour = FormatNumber(_woodCount / hours) + " /h";

            GatheringSessionTime = (DateTime.UtcNow - _sessionStart).ToString(@"h\:mm");

            RecentGathering.Insert(0, new GatheringEntry
            {
                ResourceName = resourceName,
                ResourceType = resourceType,
                Amount = amount,
                Tier = $"T{tier}",
                Timestamp = DateTime.Now
            });

            // Keep only last 100 entries
            while (RecentGathering.Count > 100)
                RecentGathering.RemoveAt(RecentGathering.Count - 1);
        });
    }

    [RelayCommand]
    private void ResetGathering()
    {
        _fiberCount = _hideCount = _oreCount = _stoneCount = _woodCount = 0;
        _sessionStart = DateTime.UtcNow;

        TotalFiber = TotalHide = TotalOre = TotalStone = TotalWood = "0";
        TotalGathered = "0";
        GatheringSessionTime = "0:00";
        RecentGathering.Clear();
    }

    private static string FormatNumber(double value)
    {
        return value switch
        {
            >= 1_000_000 => $"{value / 1_000_000:F1}M",
            >= 1_000 => $"{value / 1_000:F1}K",
            _ => $"{value:F0}"
        };
    }
}

public class GatheringEntry
{
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Tier { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
