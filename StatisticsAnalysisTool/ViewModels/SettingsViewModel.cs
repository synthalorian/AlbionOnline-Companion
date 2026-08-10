using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
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

    public SettingsViewModel()
    {
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
    }

    [RelayCommand]
    private void BrowseAlbionDataPath()
    {
        // TODO: Implement folder browser using StorageProvider
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // TODO: Implement save settings
    }

    [RelayCommand]
    private void ResetSettings()
    {
        // TODO: Implement reset settings
    }

    [RelayCommand]
    private void OpenLogsFolder()
    {
        // TODO: Implement open logs folder
    }

    [RelayCommand]
    private void ClearCache()
    {
        // TODO: Implement clear cache
    }
}
