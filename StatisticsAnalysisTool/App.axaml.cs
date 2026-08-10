using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Themes;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;

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

        base.OnFrameworkInitializationCompleted();
    }
}
