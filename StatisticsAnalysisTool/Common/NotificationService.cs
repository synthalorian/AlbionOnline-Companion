using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

/// <summary>
/// Cross-platform notification and sound alert service.
/// Uses system notifications + WAV file playback.
/// </summary>
public class NotificationService
{
    private readonly SettingsService _settings;
    private readonly Queue<Notification> _queue = new();
    private bool _isProcessing;

    public static NotificationService Instance { get; } = new();

    private NotificationService()
    {
        _settings = SettingsService.Instance;
    }

    /// <summary>
    /// Show a notification and optionally play a sound.
    /// </summary>
    public void Notify(string title, string message, NotificationType type = NotificationType.Info, string? soundFile = null)
    {
        if (!_settings.Settings.ShowNotifications)
            return;

        _queue.Enqueue(new Notification
        {
            Title = title,
            Message = message,
            Type = type,
            SoundFile = soundFile,
            Timestamp = DateTime.Now
        });

        if (!_isProcessing)
        {
            _ = ProcessQueueAsync();
        }
    }

    public void NotifyRareLoot(string itemName, long estimatedValue)
    {
        if (!_settings.Settings.NotifyOnRareLoot) return;
        Notify("🎉 Rare Loot!", $"{itemName} (~{FormatSilver(estimatedValue)})", NotificationType.Success, "alert1.wav");
    }

    public void NotifyDeath(string killerName)
    {
        if (!_settings.Settings.NotifyOnDeath) return;
        Notify("💀 You Died!", $"Killed by {killerName}", NotificationType.Error, "deathalert1.wav");
    }

    public void NotifyDungeonClosed(TimeSpan duration, int fame)
    {
        Notify("🏰 Dungeon Complete", $"Duration: {duration:hh\\:mm\\:ss} | Fame: {fame:N0}", NotificationType.Info, "dungeon_closed.wav");
    }

    public void NotifyKill(string victimName)
    {
        Notify("⚔️ Kill!", $"You killed {victimName}", NotificationType.Success, "alert2.wav");
    }

    private async Task ProcessQueueAsync()
    {
        _isProcessing = true;

        while (_queue.Count > 0)
        {
            var notification = _queue.Dequeue();

            try
            {
                // Log the notification
                Log.Information("[{Type}] {Title}: {Message}", 
                    notification.Type, notification.Title, notification.Message);

                // Play sound if enabled
                if (_settings.Settings.PlaySounds && !string.IsNullOrEmpty(notification.SoundFile))
                {
                    await PlaySoundAsync(notification.SoundFile);
                }

                // Small delay between notifications
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Notification error");
            }
        }

        _isProcessing = false;
    }

    private async Task PlaySoundAsync(string soundFile)
    {
        try
        {
            var soundPath = Path.Combine(
                AppContext.BaseDirectory, "Sounds", soundFile);

            if (!File.Exists(soundPath))
            {
                Log.Debug("Sound file not found: {Path}", soundPath);
                return;
            }

            // Use aplay on Linux (ALSA)
            if (OperatingSystem.IsLinux())
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "aplay",
                        Arguments = $"\"{soundPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                process.Start();
                await process.WaitForExitAsync();
            }
            // On Windows/macOS, Avalonia or system APIs can handle it
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to play sound: {File}", soundFile);
        }
    }

    private static string FormatSilver(long value)
    {
        return value switch
        {
            >= 1_000_000 => $"{value / 1_000_000.0:F1}M",
            >= 1_000 => $"{value / 1_000.0:F1}K",
            _ => value.ToString("N0")
        };
    }
}

public class Notification
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? SoundFile { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
