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
    private string _title = "Albion Online Companion";

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isTracking;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private string _selectedTab = "Dashboard";

    [ObservableProperty]
    private bool _showPrivilegeDialog;

    [ObservableProperty]
    private string _privilegeStatusText = Network.PrivilegeEscalation.GetStatusMessage();

    [ObservableProperty]
    private string _privilegeInstructions = Network.PrivilegeEscalation.GetSetupInstructions();

    public ObservableCollection<string> Tabs { get; } = new()
    {
        "Dashboard",
        "Damage Meter",
        "Kill Feed",
        "Chat",
        "Dungeon Tracker",
        "Loot Logger",
        "Gathering",
        "Map History",
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
    private readonly GatheringViewModel _gatheringViewModel = new();
    private readonly MapHistoryViewModel _mapHistoryViewModel = new();
    private readonly KillFeedViewModel _killFeedViewModel = new();
    private readonly ChatViewModel _chatViewModel = new();
    private readonly SettingsViewModel _settingsViewModel = new();

    private NetworkManager? _networkManager;

    public ChatViewModel ChatVM => _chatViewModel;

    public MainViewModel()
    {
        Log.Information("MainViewModel initialized");
        CurrentView = _dashboardViewModel;

        // Honor the "Auto-start tracking" setting so packet capture
        // starts immediately without a manual Start Tracking click.
        if (Common.SettingsService.Instance.Settings.AutoStartTracking)
        {
            Log.Information("Auto-start tracking enabled — starting packet capture");
            _ = StartTracking();
        }
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
            "Gathering" => _gatheringViewModel,
            "Map History" => _mapHistoryViewModel,
            "Kill Feed" => _killFeedViewModel,
            "Chat" => _chatViewModel,
            "Settings" => _settingsViewModel,
            _ => _dashboardViewModel
        };
    }

    [RelayCommand]
    private void DismissPrivilegeDialog()
    {
        ShowPrivilegeDialog = false;
        StatusText = "Ready (packet capture unavailable)";
    }

    [RelayCommand]
    private void RestartWithPkexec()
    {
        try
        {
            // Find the app DLL or binary
            var appDir = AppContext.BaseDirectory;
            var dllPath = System.IO.Path.Combine(appDir, "AlbionOnlineCompanion.dll");
            var binPath = System.IO.Path.Combine(appDir, "AlbionOnlineCompanion");

            string exePath;
            string args;

            if (System.IO.File.Exists(binPath))
            {
                // Self-contained binary
                exePath = binPath;
                args = "";
            }
            else if (System.IO.File.Exists(dllPath))
            {
                // Framework-dependent: use dotnet
                exePath = "dotnet";
                args = $"\"{dllPath}\"";
            }
            else
            {
                // Dev mode: use dotnet run
                exePath = "dotnet";
                args = "run -c Release";
                appDir = System.IO.Directory.GetCurrentDirectory();
            }

            Log.Information("Escalating via pkexec: {Exe} {Args} (cwd: {Dir})", exePath, args, appDir);

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pkexec",
                UseShellExecute = false,
                WorkingDirectory = appDir
            };

            startInfo.ArgumentList.Add("--disable-internal-agent");
            startInfo.ArgumentList.Add("env");
            startInfo.ArgumentList.Add($"WAYLAND_DISPLAY={Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") ?? "wayland-0"}");
            startInfo.ArgumentList.Add($"XDG_RUNTIME_DIR={Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? "/run/user/1000"}");
            startInfo.ArgumentList.Add($"DISPLAY={Environment.GetEnvironmentVariable("DISPLAY") ?? ":0"}");
            startInfo.ArgumentList.Add(exePath);
            if (!string.IsNullOrEmpty(args))
            {
                foreach (var arg in args.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    startInfo.ArgumentList.Add(arg.Trim('"'));
                }
            }

            System.Diagnostics.Process.Start(startInfo);
            StatusText = "Elevated instance starting...";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to escalate: {ex.Message}";
            Log.Error(ex, "pkexec escalation failed");
        }
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
            // Check if we have packet capture privileges
            if (!Network.PrivilegeEscalation.TestRawSocketAccess())
            {
                if (Network.PrivilegeEscalation.CanEscalate())
                {
                    StatusText = "⚠️ Root required — restart with elevated privileges";
                    ShowPrivilegeDialog = true;
                    Log.Warning("Packet capture requires root privileges");
                    return;
                }
                else
                {
                    StatusText = "❌ Cannot capture packets — no root access";
                    ShowPrivilegeDialog = true;
                    Log.Error("No packet capture privileges and cannot escalate");
                    return;
                }
            }

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
                _lootLoggerViewModel,
                _chatViewModel);

            await Task.Run(() => _networkManager.Start());

            IsTracking = true;
            StatusText = "Tracking active — capturing packets";
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
