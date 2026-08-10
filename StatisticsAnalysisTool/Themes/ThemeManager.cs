using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Serilog;
using System;

namespace StatisticsAnalysisTool.Themes;

/// <summary>
/// Applies ThemeDefinitions to the running Avalonia app by swapping
/// resource brushes at the Application level.
/// </summary>
public static class ThemeManager
{
    private static ThemeDefinition _current = ThemeCatalog.Default;

    public static ThemeDefinition Current => _current;
    public static event EventHandler<ThemeDefinition>? ThemeChanged;

    public static void Apply(string themeName)
    {
        var theme = ThemeCatalog.GetByName(themeName);
        Apply(theme);
    }

    public static void Apply(ThemeDefinition theme)
    {
        _current = theme;

        if (Application.Current == null) return;

        var app = Application.Current;

        // Set the base theme variant (dark/light)
        app.RequestedThemeVariant = theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;

        // Override core brushes at the app resource level
        SetBrush(app, "ThemeBackground", theme.Background);
        SetBrush(app, "ThemeSurface", theme.Surface);
        SetBrush(app, "ThemeSurfaceAlt", theme.SurfaceAlt);
        SetBrush(app, "ThemeBorder", theme.Border);
        SetBrush(app, "ThemeForeground", theme.Foreground);
        SetBrush(app, "ThemeForegroundDim", theme.ForegroundDim);

        SetBrush(app, "ThemeAccent", theme.Accent);
        SetBrush(app, "ThemeAccentSecondary", theme.AccentSecondary);
        SetBrush(app, "ThemeSuccess", theme.Success);
        SetBrush(app, "ThemeWarning", theme.Warning);
        SetBrush(app, "ThemeError", theme.Error);
        SetBrush(app, "ThemeInfo", theme.Info);
        SetBrush(app, "ThemeOrange", theme.Orange);
        SetBrush(app, "ThemePurple", theme.Purple);

        SetBrush(app, "ThemeButtonBackground", theme.ButtonBackground);
        SetBrush(app, "ThemeButtonHover", theme.ButtonHover);
        SetBrush(app, "ThemeButtonForeground", theme.ButtonForeground);

        SetBrush(app, "ThemeInputBackground", theme.InputBackground);
        SetBrush(app, "ThemeInputBorder", theme.InputBorder);
        SetBrush(app, "ThemeInputForeground", theme.InputForeground);

        SetBrush(app, "ThemeChartLine1", theme.ChartLine1);
        SetBrush(app, "ThemeChartLine2", theme.ChartLine2);
        SetBrush(app, "ThemeChartLine3", theme.ChartLine3);
        SetBrush(app, "ThemeChartLine4", theme.ChartLine4);
        SetBrush(app, "ThemeChartLine5", theme.ChartLine5);

        Log.Information("Theme applied: {Theme} ({DisplayName})", theme.Name, theme.DisplayName);
        ThemeChanged?.Invoke(null, theme);
    }

    private static void SetBrush(Application app, string key, string hexColor)
    {
        var color = Color.Parse(hexColor);
        app.Resources[key] = new SolidColorBrush(color);
    }

    /// <summary>
    /// Helper for views to bind to dynamic theme brushes.
    /// Usage: {DynamicResource ThemeAccent}
    /// </summary>
    public static void RegisterDefaultResources(Application app)
    {
        Apply(_current);
    }
}
