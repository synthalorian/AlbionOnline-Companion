using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StatisticsAnalysisTool.Network;
using System;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.ViewModels;

public partial class KillFeedViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<KillFeedEntry> _killFeed = new();

    [ObservableProperty]
    private string _totalKills = "0";

    [ObservableProperty]
    private string _totalDeaths = "0";

    [ObservableProperty]
    private string _kdRatio = "0.00";

    [ObservableProperty]
    private string _totalKillFame = "0";

    [ObservableProperty]
    private bool _showOnlyPlayerKills = true;

    private int _kills;
    private int _deaths;
    private long _killFame;

    public void AddKill(string killerName, string victimName, long fame, string killerGuild = "", string victimGuild = "")
    {
        _kills++;
        _killFame += fame;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            KillFeed.Insert(0, new KillFeedEntry
            {
                KillerName = killerName,
                VictimName = victimName,
                KillerGuild = killerGuild,
                VictimGuild = victimGuild,
                Fame = fame,
                Timestamp = DateTime.Now,
                IsLocalPlayerKill = killerName == EntityTracker.Instance.LocalPlayerName
            });

            TotalKills = _kills.ToString();
            TotalKillFame = FormatNumber(_killFame);
            UpdateKdRatio();

            while (KillFeed.Count > 200)
                KillFeed.RemoveAt(KillFeed.Count - 1);
        });
    }

    public void AddDeath(string killerName, long fame)
    {
        _deaths++;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            TotalDeaths = _deaths.ToString();
            UpdateKdRatio();
        });
    }

    private void UpdateKdRatio()
    {
        KdRatio = _deaths > 0 ? $"{(double)_kills / _deaths:F2}" : $"{_kills:F2}";
    }

    [RelayCommand]
    private void ClearFeed()
    {
        KillFeed.Clear();
        _kills = 0;
        _deaths = 0;
        _killFame = 0;
        TotalKills = "0";
        TotalDeaths = "0";
        KdRatio = "0.00";
        TotalKillFame = "0";
    }

    private static string FormatNumber(long value)
    {
        return value switch
        {
            >= 1_000_000 => $"{value / 1_000_000.0:F1}M",
            >= 1_000 => $"{value / 1_000.0:F1}K",
            _ => value.ToString("N0")
        };
    }
}

public class KillFeedEntry
{
    public string KillerName { get; set; } = string.Empty;
    public string VictimName { get; set; } = string.Empty;
    public string KillerGuild { get; set; } = string.Empty;
    public string VictimGuild { get; set; } = string.Empty;
    public long Fame { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsLocalPlayerKill { get; set; }
    public string FameDisplay => Fame switch
    {
        >= 1_000_000 => $"{Fame / 1_000_000.0:F1}M",
        >= 1_000 => $"{Fame / 1_000.0:F1}K",
        _ => Fame.ToString("N0")
    };
}
