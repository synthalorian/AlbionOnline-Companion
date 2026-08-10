using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StatisticsAnalysisTool.Network;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Statistics Analysis Tool - Linux";

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isTracking;

    [ObservableProperty]
    private string _selectedTab = "Dashboard";

    public ObservableCollection<string> Tabs { get; } = new()
    {
        "Dashboard",
        "Damage Meter",
        "Dungeon Tracker",
        "Loot Logger",
        "Crafting Calculator",
        "Map History",
        "Player Info",
        "Settings"
    };

    private NetworkManager? _networkManager;

    public MainViewModel()
    {
        Log.Information("MainViewModel initialized");
    }

    [RelayCommand]
    private async Task ToggleTracking()
    {
        if (IsTracking)
        {
            await StopTracking();
        }
        else
        {
            await StartTracking();
        }
    }

    private async Task StartTracking()
    {
        try
        {
            StatusText = "Starting packet capture...";
            Log.Information("Starting tracking");

            _networkManager = new NetworkManager();
            _networkManager.StatusChanged += (s, msg) => StatusText = msg;
            
            await Task.Run(() => _networkManager.Start());

            IsTracking = true;
            StatusText = "Tracking active - capturing packets";
            Log.Information("Tracking started successfully");
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            Log.Error(ex, "Failed to start tracking");
        }
    }

    private async Task StopTracking()
    {
        try
        {
            StatusText = "Stopping...";
            Log.Information("Stopping tracking");

            if (_networkManager != null)
            {
                await Task.Run(() => _networkManager.Stop());
                _networkManager.Dispose();
                _networkManager = null;
            }

            IsTracking = false;
            StatusText = "Ready";
            Log.Information("Tracking stopped");
        }
        catch (Exception ex)
        {
            StatusText = $"Error stopping: {ex.Message}";
            Log.Error(ex, "Error stopping tracking");
        }
    }
}
