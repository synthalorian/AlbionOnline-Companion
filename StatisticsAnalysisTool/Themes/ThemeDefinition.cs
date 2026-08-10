using System.Collections.Generic;

namespace StatisticsAnalysisTool.Themes;

public class ThemeDefinition
{
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsDark { get; init; } = true;

    // Core colors
    public string Background { get; init; } = "#FF1E1E2E";
    public string Surface { get; init; } = "#FF313244";
    public string SurfaceAlt { get; init; } = "#FF181825";
    public string Border { get; init; } = "#FF45475A";
    public string Foreground { get; init; } = "#FFCDD6F4";
    public string ForegroundDim { get; init; } = "#FF6C7086";

    // Accent colors
    public string Accent { get; init; } = "#FFF5C2E7";       // Primary accent (pink)
    public string AccentSecondary { get; init; } = "#FF89B4FA"; // Blue
    public string Success { get; init; } = "#FFA6E3A1";      // Green
    public string Warning { get; init; } = "#FFF9E2AF";      // Yellow
    public string Error { get; init; } = "#FFF38BA8";        // Red
    public string Info { get; init; } = "#FF94E2D5";         // Teal
    public string Orange { get; init; } = "#FFFAB387";       // Orange
    public string Purple { get; init; } = "#FFCBA6F7";       // Purple

    // Button colors
    public string ButtonBackground { get; init; } = "#FF313244";
    public string ButtonHover { get; init; } = "#FF45475A";
    public string ButtonForeground { get; init; } = "#FFCDD6F4";

    // Input colors
    public string InputBackground { get; init; } = "#FF313244";
    public string InputBorder { get; init; } = "#FF45475A";
    public string InputForeground { get; init; } = "#FFCDD6F4";

    // Data visualization
    public string ChartLine1 { get; init; } = "#FFF5C2E7";
    public string ChartLine2 { get; init; } = "#FF89B4FA";
    public string ChartLine3 { get; init; } = "#FFA6E3A1";
    public string ChartLine4 { get; init; } = "#FFF9E2AF";
    public string ChartLine5 { get; init; } = "#FFF38BA8";

    public string FullDisplayName => $"{Emoji} {DisplayName}";
}
