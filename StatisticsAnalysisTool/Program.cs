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
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(
                args, Avalonia.Controls.ShutdownMode.OnLastWindowClose);
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

        // On Linux, use X11 (Xwayland): Avalonia's Wayland backend never calls
        // xdg_toplevel.set_app_id, so KWin can't match our desktop file and the
        // window gets the default Wayland icon. The X11 backend sets WM_CLASS
        // (WmClass below), which KWin matches via StartupWMClass → icon works.
        if (OperatingSystem.IsLinux())
        {
            builder = builder.UseX11()
                .With(new Avalonia.X11PlatformOptions { WmClass = "AlbionOnlineCompanion" });
            Log.Information("Using X11 platform (WM_CLASS=AlbionOnlineCompanion for icon matching)");
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
