using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.ComponentModel;

namespace StatisticsAnalysisTool.Views;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;

    public MainWindow()
    {
        InitializeComponent();
        
        // Load icon from embedded resources
        try
        {
            var assets = AssetLoader.Open(new Uri("avares://AlbionOnlineCompanion/Assets/app-icon-256.png"));
            Icon = new WindowIcon(new Bitmap(assets));
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to load window icon");
        }
        _settingsService = SettingsService.Instance;

        // Restore window state
        var s = _settingsService.Settings;
        if (s.WindowWidth > 0 && s.WindowHeight > 0)
        {
            Width = s.WindowWidth;
            Height = s.WindowHeight;
        }
        if (s.WindowX >= 0 && s.WindowY >= 0)
        {
            Position = new Avalonia.PixelPoint((int)s.WindowX, (int)s.WindowY);
        }
        Opacity = s.WindowOpacity;
        Topmost = s.AlwaysOnTop;
        FontSize = 14 * s.FontSizeScale;

        // Save on close
        Closing += OnWindowClosing;
        PropertyChanged += OnWindowPropertyChanged;
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        SaveWindowState();
    }

    private void OnWindowPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        // Debounce: only save on actual changes to bounds
        if (e.Property.Name == nameof(Bounds))
        {
            SaveWindowState();
        }
    }

    private void SaveWindowState()
    {
        try
        {
            var s = _settingsService.Settings;
            s.WindowWidth = Width;
            s.WindowHeight = Height;
            s.WindowX = Position.X;
            s.WindowY = Position.Y;
            s.WindowOpacity = Opacity;
            s.AlwaysOnTop = Topmost;
            _settingsService.Save();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to save window state");
        }
    }
}
