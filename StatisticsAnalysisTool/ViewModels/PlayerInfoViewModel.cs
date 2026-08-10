using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class PlayerInfoViewModel : ViewModelBase
{
    private readonly AlbionPlayerService _playerService = AlbionPlayerService.Instance;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _playerName = string.Empty;

    [ObservableProperty]
    private string _guildName = string.Empty;

    [ObservableProperty]
    private string _allianceName = string.Empty;

    [ObservableProperty]
    private string _killFame = "0";

    [ObservableProperty]
    private string _deathFame = "0";

    [ObservableProperty]
    private string _fameRatio = "0";

    [ObservableProperty]
    private string _gatheringFame = "0";

    [ObservableProperty]
    private string _craftingFame = "0";

    [ObservableProperty]
    private string _totalFame = "0";

    [ObservableProperty]
    private string _pveFame = "0";

    [ObservableProperty]
    private ObservableCollection<PlayerEquipmentItem> _equipment = new();

    [ObservableProperty]
    private ObservableCollection<PlayerStatEntry> _recentStats = new();

    [ObservableProperty]
    private ObservableCollection<PlayerSearchResult> _searchResults = new();

    [ObservableProperty]
    private string _statusText = "Enter a player name to search";

    [ObservableProperty]
    private bool _isPlayerFound;

    [ObservableProperty]
    private bool _showSearchResults;

    [RelayCommand]
    private async Task SearchPlayer()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        IsSearching = true;
        StatusText = $"Searching for \"{SearchText}\"...";
        IsPlayerFound = false;
        ShowSearchResults = false;

        try
        {
            var results = await _playerService.SearchPlayersAsync(SearchText);

            if (results.Count == 0)
            {
                StatusText = $"No players found matching \"{SearchText}\"";
                return;
            }

            if (results.Count == 1)
            {
                // Exact match — load player directly
                await LoadPlayerAsync(results[0].Id);
            }
            else
            {
                // Multiple results — show picker
                SearchResults.Clear();
                foreach (var r in results.Take(20))
                {
                    SearchResults.Add(r);
                }
                ShowSearchResults = true;
                StatusText = $"{results.Count} players found — select one";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            Log.Error(ex, "Player search failed");
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectSearchResult(PlayerSearchResult? result)
    {
        if (result == null) return;
        ShowSearchResults = false;
        await LoadPlayerAsync(result.Id);
    }

    private async Task LoadPlayerAsync(string playerId)
    {
        IsSearching = true;
        StatusText = "Loading player data...";

        try
        {
            var player = await _playerService.GetPlayerAsync(playerId);
            if (player == null)
            {
                StatusText = "Failed to load player data";
                return;
            }

            PlayerName = player.Name;
            GuildName = player.GuildName ?? "No Guild";
            AllianceName = player.AllianceName ?? "No Alliance";
            KillFame = FormatNumber(player.KillFame);
            DeathFame = FormatNumber(player.DeathFame);
            FameRatio = $"{player.FameRatio:F2}";

            if (player.Stats != null)
            {
                PveFame = FormatNumber(player.Stats.PvE?.Total ?? 0);

                var gathering = player.Stats.Gathering;
                if (gathering != null)
                {
                    var totalGathering = (gathering.Fiber?.Total ?? 0) +
                                        (gathering.Hide?.Total ?? 0) +
                                        (gathering.Ore?.Total ?? 0) +
                                        (gathering.Stone?.Total ?? 0) +
                                        (gathering.Wood?.Total ?? 0);
                    GatheringFame = FormatNumber(totalGathering);
                }

                CraftingFame = FormatNumber(player.Stats.Crafting?.Total ?? 0);
            }

            TotalFame = FormatNumber(player.KillFame + player.DeathFame);

            // Load recent kills
            var kills = await _playerService.GetPlayerKillsAsync(playerId, 10);
            RecentStats.Clear();
            foreach (var kill in kills)
            {
                RecentStats.Add(new PlayerStatEntry
                {
                    Date = kill.TimeStamp.ToString("MMM dd HH:mm"),
                    Activity = $"⚔️ Killed {kill.Victim?.Name ?? "Unknown"}",
                    Fame = FormatNumber(kill.TotalVictimKillFame),
                    Details = $"IP: {kill.Killer?.AverageItemPower:F0}"
                });
            }

            IsPlayerFound = true;
            StatusText = $"Loaded: {player.Name}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            Log.Error(ex, "Failed to load player");
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        PlayerName = string.Empty;
        GuildName = string.Empty;
        AllianceName = string.Empty;
        Equipment.Clear();
        RecentStats.Clear();
        SearchResults.Clear();
        IsPlayerFound = false;
        ShowSearchResults = false;
        StatusText = "Enter a player name to search";
    }

    private static string FormatNumber(long value)
    {
        return value switch
        {
            >= 1_000_000 => $"{value / 1_000_000.0:F1}M",
            >= 1_000 => $"{value / 1_000.0:F1}K",
            _ => value.ToString("N0")
        };
    }
}

public class PlayerEquipmentItem
{
    public string Slot { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

public class PlayerStatEntry
{
    public string Date { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public string Fame { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
