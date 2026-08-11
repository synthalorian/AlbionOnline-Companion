using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Themes;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Apply saved theme
        var settings = SettingsService.Instance.Settings;
        ThemeManager.Apply(settings.Theme);

        // Set saved username for local player detection
        if (!string.IsNullOrEmpty(settings.PlayerUsername))
        {
            Network.EntityTracker.Instance.SetSavedUsername(settings.PlayerUsername);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }

        // Load item + cluster databases in background
        _ = Task.Run(async () =>
        {
            await ItemDatabase.Instance.LoadAsync();
            Log.Information("Item database ready: {Count} items", ItemDatabase.Instance.ItemCount);
        });
        _ = Task.Run(async () =>
        {
            await ClusterDatabase.Instance.LoadAsync();
        });

        base.OnFrameworkInitializationCompleted();
    }
}
