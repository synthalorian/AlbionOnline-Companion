using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace StatisticsAnalysisTool.Common;

public class BoolToTrackingTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "Stop Tracking" : "Start Tracking";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToTrackingColorConverter : IValueConverter
{
    private static readonly IBrush StopBrush = new SolidColorBrush(Color.Parse("#F38BA8"));
    private static readonly IBrush StartBrush = new SolidColorBrush(Color.Parse("#A6E3A1"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? StopBrush : StartBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
