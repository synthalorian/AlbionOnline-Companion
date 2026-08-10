using Avalonia;
using Serilog;
using System;
using System.IO;

namespace StatisticsAnalysisTool;

sealed class Program
{
    public static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AlbionOnlineCompanion");

    [STAThread]
    public static void Main(string[] args)
    {
        Directory.CreateDirectory(AppDataDir);
        Directory.CreateDirectory(Path.Combine(AppDataDir, "logs"));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppDataDir, "logs", "sat-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        Log.Information("Albion Online Companion starting...");

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application crashed");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>();

        // On Linux, prefer Wayland if available, fall back to X11
        if (OperatingSystem.IsLinux())
        {
            var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            if (!string.IsNullOrEmpty(waylandDisplay))
            {
                builder = builder.UseWayland();
                Log.Information("Using Wayland platform");
            }
            else
            {
                builder = builder.UseX11();
                Log.Information("Using X11 platform");
            }
        }
        else
        {
            builder = builder.UsePlatformDetect();
        }

#if DEBUG
        builder = builder.WithDeveloperTools();
#endif

        return builder
            .UseSkia()
            .UseHarfBuzz()
            .WithInterFont()
            .LogToTrace();
    }
}
