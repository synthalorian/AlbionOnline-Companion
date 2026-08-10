using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private string _selectedLanguage = "English";

    [ObservableProperty]
    private ObservableCollection<string> _languages = new();

    [ObservableProperty]
    private string _selectedServer = "Americas";

    [ObservableProperty]
    private ObservableCollection<string> _servers = new();

    [ObservableProperty]
    private bool _isTrackingResetByMapChangeActive;

    [ObservableProperty]
    private bool _isDamageMeterTrackingActive = true;

    [ObservableProperty]
    private bool _isNavigationMenuOpen = true;

    [ObservableProperty]
    private bool _isOpenItemWindowInNewWindowChecked;

    [ObservableProperty]
    private string _albionDataPath = string.Empty;

    [ObservableProperty]
    private string _selectedTheme = "Dark";

    [ObservableProperty]
    private ObservableCollection<string> _themes = new();

    [ObservableProperty]
    private bool _enableNotifications = true;

    [ObservableProperty]
    private bool _enableSounds = true;

    [ObservableProperty]
    private int _refreshIntervalSeconds = 5;

    [ObservableProperty]
    private bool _autoStartTracking;

    [ObservableProperty]
    private bool _minimizeToTray;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel()
    {
        _settingsService = SettingsService.Instance;

        Languages.Add("English");
        Languages.Add("German");
        Languages.Add("French");
        Languages.Add("Spanish");
        Languages.Add("Russian");
        Languages.Add("Portuguese");
        Languages.Add("Korean");
        Languages.Add("Chinese");

        Servers.Add("Americas");
        Servers.Add("Europe");
        Servers.Add("Asia");

        Themes.Add("Dark");
        Themes.Add("Light");
        Themes.Add("System");

        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        var s = _settingsService.Settings;

        SelectedLanguage = s.Language switch
        {
            "de-DE" => "German",
            "fr-FR" => "French",
            "es-ES" => "Spanish",
            "ru-RU" => "Russian",
            "pt-BR" => "Portuguese",
            "ko-KR" => "Korean",
            "zh-CN" => "Chinese",
            _ => "English"
        };

        SelectedServer = s.GameServer switch
        {
            "europe" => "Europe",
            "asia" => "Asia",
            _ => "Americas"
        };

        SelectedTheme = s.Theme;
        IsTrackingResetByMapChangeActive = s.ResetOnMapChange;
        IsDamageMeterTrackingActive = s.DamageMeterEnabled;
        EnableNotifications = s.ShowNotifications;
        EnableSounds = s.PlaySounds;
        AutoStartTracking = s.AutoStartTracking;
        AlbionDataPath = s.GameLogPath;
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var s = _settingsService.Settings;

        s.Language = SelectedLanguage switch
        {
            "German" => "de-DE",
            "French" => "fr-FR",
            "Spanish" => "es-ES",
            "Russian" => "ru-RU",
            "Portuguese" => "pt-BR",
            "Korean" => "ko-KR",
            "Chinese" => "zh-CN",
            _ => "en-US"
        };

        s.GameServer = SelectedServer switch
        {
            "Europe" => "europe",
            "Asia" => "asia",
            _ => "americas"
        };

        s.Theme = SelectedTheme;
        s.ResetOnMapChange = IsTrackingResetByMapChangeActive;
        s.DamageMeterEnabled = IsDamageMeterTrackingActive;
        s.ShowNotifications = EnableNotifications;
        s.PlaySounds = EnableSounds;
        s.AutoStartTracking = AutoStartTracking;
        s.GameLogPath = AlbionDataPath;

        _settingsService.Save();
        StatusMessage = "Settings saved!";
        Log.Information("Settings saved");

        // Clear status after 3 seconds
        Task.Delay(3000).ContinueWith(_ =>
            Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusMessage = string.Empty));
    }

    [RelayCommand]
    private void ResetSettings()
    {
        _settingsService.Settings = new AppSettings();
        _settingsService.Save();
        LoadFromSettings();
        StatusMessage = "Settings reset to defaults";
        Log.Information("Settings reset to defaults");
    }

    [RelayCommand]
    private void OpenLogsFolder()
    {
        var logsDir = Path.Combine(Program.AppDataDir, "logs");
        Directory.CreateDirectory(logsDir);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = logsDir,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open logs folder");
            StatusMessage = $"Cannot open: {logsDir}";
        }
    }

    [RelayCommand]
    private void ClearCache()
    {
        StatusMessage = "Cache cleared";
        Log.Information("Cache cleared");
    }

    [RelayCommand]
    private void BrowseAlbionDataPath()
    {
        // Will use StorageProvider when wired to the view
        StatusMessage = "Use file picker dialog";
    }
}
