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
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private string _selectedTab = "Dashboard";

    public ObservableCollection<string> Tabs { get; } = new()
    {
        "Dashboard",
        "Damage Meter",
        "Dungeon Tracker",
        "Loot Logger",
        "Crafting Calculator",
        "Player Info",
        "Settings"
    };

    private readonly DashboardViewModel _dashboardViewModel = new();
    private readonly DamageMeterViewModel _damageMeterViewModel = new();
    private readonly DungeonTrackerViewModel _dungeonTrackerViewModel = new();
    private readonly LootLoggerViewModel _lootLoggerViewModel = new();
    private readonly CraftingCalculatorViewModel _craftingCalculatorViewModel = new();
    private readonly PlayerInfoViewModel _playerInfoViewModel = new();
    private readonly SettingsViewModel _settingsViewModel = new();

    private NetworkManager? _networkManager;

    public MainViewModel()
    {
        Log.Information("MainViewModel initialized");
        CurrentView = _dashboardViewModel;
    }

    partial void OnSelectedTabChanged(string value)
    {
        CurrentView = value switch
        {
            "Dashboard" => _dashboardViewModel,
            "Damage Meter" => _damageMeterViewModel,
            "Dungeon Tracker" => _dungeonTrackerViewModel,
            "Loot Logger" => _lootLoggerViewModel,
            "Crafting Calculator" => _craftingCalculatorViewModel,
            "Player Info" => _playerInfoViewModel,
            "Settings" => _settingsViewModel,
            _ => _dashboardViewModel
        };
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
            _networkManager.StatusChanged += (s, msg) =>
                Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusText = msg);

            // Wire up event handlers to ViewModels
            _networkManager.RegisterViewModels(
                _dashboardViewModel,
                _damageMeterViewModel,
                _dungeonTrackerViewModel,
                _lootLoggerViewModel);

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
