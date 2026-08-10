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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }

        // Load item database in background
        _ = Task.Run(async () =>
        {
            await ItemDatabase.Instance.LoadAsync();
            Log.Information("Item database ready: {Count} items", ItemDatabase.Instance.ItemCount);
        });

        base.OnFrameworkInitializationCompleted();
    }
}
