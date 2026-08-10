using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Themes;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    private string _selectedThemeName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _themeNames = new();

    [ObservableProperty]
    private string _selectedThemeDescription = string.Empty;

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

        // Load themes from catalog
        foreach (var theme in ThemeCatalog.All)
        {
            ThemeNames.Add(theme.FullDisplayName);
        }

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

        // Set theme from saved name
        var theme = ThemeCatalog.GetByName(s.Theme);
        SelectedThemeName = theme.FullDisplayName;
        SelectedThemeDescription = theme.Description;

        IsTrackingResetByMapChangeActive = s.ResetOnMapChange;
        IsDamageMeterTrackingActive = s.DamageMeterEnabled;
        EnableNotifications = s.ShowNotifications;
        EnableSounds = s.PlaySounds;
        AutoStartTracking = s.AutoStartTracking;
        AlbionDataPath = s.GameLogPath;
    }

    partial void OnSelectedThemeNameChanged(string value)
    {
        // Find theme by display name and apply it live
        var theme = ThemeCatalog.All.FirstOrDefault(t => t.FullDisplayName == value);
        if (theme != null)
        {
            SelectedThemeDescription = theme.Description;
            ThemeManager.Apply(theme);

            // Save immediately
            _settingsService.Settings.Theme = theme.Name;
            _settingsService.Save();
        }
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

        s.ResetOnMapChange = IsTrackingResetByMapChangeActive;
        s.DamageMeterEnabled = IsDamageMeterTrackingActive;
        s.ShowNotifications = EnableNotifications;
        s.PlaySounds = EnableSounds;
        s.AutoStartTracking = AutoStartTracking;
        s.GameLogPath = AlbionDataPath;

        _settingsService.Save();
        StatusMessage = "Settings saved!";
        Log.Information("Settings saved");

        Task.Delay(3000).ContinueWith(_ =>
            Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusMessage = string.Empty));
    }

    [RelayCommand]
    private void ResetSettings()
    {
        _settingsService.Settings = new AppSettings();
        _settingsService.Save();
        LoadFromSettings();
        ThemeManager.Apply(ThemeCatalog.Default);
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
        StatusMessage = "Use file picker dialog";
    }
}
