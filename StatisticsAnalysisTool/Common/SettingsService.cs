using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Common;

/// <summary>
/// JSON-based settings persistence for Linux.
/// Stores config in ~/.config/StatisticsAnalysisTool/
/// </summary>
public class SettingsService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "StatisticsAnalysisTool");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettings Settings { get; set; } = new();

    public static SettingsService Instance { get; } = new();

    private SettingsService()
    {
        Load();
    }

    public void Load()
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);

            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                Log.Information("Settings loaded from {Path}", ConfigPath);
            }
            else
            {
                Settings = new AppSettings();
                Save(); // Create default config
                Log.Information("Default settings created at {Path}", ConfigPath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings, using defaults");
            Settings = new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            File.WriteAllText(ConfigPath, json);
            Log.Debug("Settings saved to {Path}", ConfigPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save settings");
        }
    }
}

public class AppSettings
{
    // General
    public string Language { get; set; } = "en-US";
    public string GameServer { get; set; } = "auto"; // auto, americas, asia, europe
    public string Theme { get; set; } = "Dark";

    // Tracking
    public bool AutoStartTracking { get; set; }
    public bool ResetOnMapChange { get; set; } = true;
    public string PacketFilter { get; set; } = string.Empty;

    // Damage Meter
    public bool DamageMeterEnabled { get; set; } = true;
    public bool ResetDamageOnMapChange { get; set; } = true;
    public bool ResetDamageBeforeCombat { get; set; }
    public bool OnlyDamageToPlayers { get; set; }

    // Dungeon Tracker
    public bool DungeonTrackerEnabled { get; set; } = true;
    public bool ShowDungeonCloseTimer { get; set; } = true;

    // Loot Logger
    public bool LootLoggerEnabled { get; set; } = true;
    public bool TrackPartyLootOnly { get; set; } = true;
    public bool TrackSilver { get; set; } = true;
    public bool TrackFame { get; set; } = true;
    public bool TrackMobLoot { get; set; } = true;
    public bool TrackKills { get; set; } = true;

    // Notifications
    public bool ShowNotifications { get; set; } = true;
    public bool PlaySounds { get; set; } = true;
    public bool NotifyOnRareLoot { get; set; } = true;
    public bool NotifyOnDeath { get; set; } = true;

    // UI
    public bool AlwaysOnTop { get; set; }
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
    public double WindowX { get; set; } = -1;
    public double WindowY { get; set; } = -1;
    public string SelectedTab { get; set; } = "Dashboard";

    // Paths
    public string GameLogPath { get; set; } = string.Empty;
    public string ScreenshotPath { get; set; } = string.Empty;
}
