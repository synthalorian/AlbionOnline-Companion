using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

/// <summary>
/// Player search and info via Albion Online's official gameinfo API.
/// </summary>
public class AlbionPlayerService
{
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://gameinfo.albiononline.com/api/gameinfo/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AlbionPlayerService Instance { get; } = new();

    private AlbionPlayerService() { }

    /// <summary>
    /// Search for players by name.
    /// </summary>
    public async Task<List<PlayerSearchResult>> SearchPlayersAsync(string name, CancellationToken ct = default)
    {
        try
        {
            var url = $"search?q={Uri.EscapeDataString(name)}";
            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<SearchResponse>(json, JsonOptions);

            return result?.Players ?? new List<PlayerSearchResult>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Player search failed for: {Name}", name);
            return new List<PlayerSearchResult>();
        }
    }

    /// <summary>
    /// Get detailed player info by ID.
    /// </summary>
    public async Task<PlayerInfo?> GetPlayerAsync(string playerId, CancellationToken ct = default)
    {
        try
        {
            var url = $"players/{Uri.EscapeDataString(playerId)}";
            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<PlayerInfo>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get player: {Id}", playerId);
            return null;
        }
    }

    /// <summary>
    /// Get player's recent kills.
    /// </summary>
    public async Task<List<KillEvent>> GetPlayerKillsAsync(string playerId, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            var url = $"players/{Uri.EscapeDataString(playerId)}/kills?limit={limit}";
            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<KillEvent>>(json, JsonOptions) ?? new List<KillEvent>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get kills for player: {Id}", playerId);
            return new List<KillEvent>();
        }
    }

    /// <summary>
    /// Get player's recent deaths.
    /// </summary>
    public async Task<List<KillEvent>> GetPlayerDeathsAsync(string playerId, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            var url = $"players/{Uri.EscapeDataString(playerId)}/deaths?limit={limit}";
            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<KillEvent>>(json, JsonOptions) ?? new List<KillEvent>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get deaths for player: {Id}", playerId);
            return new List<KillEvent>();
        }
    }
}

public class SearchResponse
{
    [JsonPropertyName("players")]
    public List<PlayerSearchResult>? Players { get; set; }

    [JsonPropertyName("guilds")]
    public List<GuildSearchResult>? Guilds { get; set; }
}

public class PlayerSearchResult
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("GuildName")]
    public string? GuildName { get; set; }

    [JsonPropertyName("AllianceName")]
    public string? AllianceName { get; set; }

    [JsonPropertyName("KillFame")]
    public long KillFame { get; set; }

    [JsonPropertyName("DeathFame")]
    public long DeathFame { get; set; }

    [JsonPropertyName("FameRatio")]
    public double FameRatio { get; set; }
}

public class GuildSearchResult
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("AllianceName")]
    public string? AllianceName { get; set; }

    [JsonPropertyName("KillFame")]
    public long KillFame { get; set; }

    [JsonPropertyName("DeathFame")]
    public long DeathFame { get; set; }
}

public class PlayerInfo
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("GuildName")]
    public string? GuildName { get; set; }

    [JsonPropertyName("AllianceName")]
    public string? AllianceName { get; set; }

    [JsonPropertyName("Avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("AvatarRing")]
    public string? AvatarRing { get; set; }

    [JsonPropertyName("KillFame")]
    public long KillFame { get; set; }

    [JsonPropertyName("DeathFame")]
    public long DeathFame { get; set; }

    [JsonPropertyName("FameRatio")]
    public double FameRatio { get; set; }

    [JsonPropertyName("LifetimeStatistics")]
    public LifetimeStatistics? Stats { get; set; }
}

public class LifetimeStatistics
{
    [JsonPropertyName("PvE")]
    public PvEStats? PvE { get; set; }

    [JsonPropertyName("Gathering")]
    public GatheringStats? Gathering { get; set; }

    [JsonPropertyName("Crafting")]
    public CraftingStats? Crafting { get; set; }
}

public class PvEStats
{
    [JsonPropertyName("Total")]
    public long Total { get; set; }
}

public class GatheringStats
{
    [JsonPropertyName("Fiber")]
    public ResourceStats? Fiber { get; set; }

    [JsonPropertyName("Hide")]
    public ResourceStats? Hide { get; set; }

    [JsonPropertyName("Ore")]
    public ResourceStats? Ore { get; set; }

    [JsonPropertyName("Stone")]
    public ResourceStats? Stone { get; set; }

    [JsonPropertyName("Wood")]
    public ResourceStats? Wood { get; set; }
}

public class ResourceStats
{
    [JsonPropertyName("Total")]
    public long Total { get; set; }
}

public class CraftingStats
{
    [JsonPropertyName("Total")]
    public long Total { get; set; }
}

public class KillEvent
{
    [JsonPropertyName("EventId")]
    public long EventId { get; set; }

    [JsonPropertyName("TimeStamp")]
    public DateTime TimeStamp { get; set; }

    [JsonPropertyName("TotalVictimKillFame")]
    public long TotalVictimKillFame { get; set; }

    [JsonPropertyName("Killer")]
    public KillParticipant? Killer { get; set; }

    [JsonPropertyName("Victim")]
    public KillParticipant? Victim { get; set; }
}

public class KillParticipant
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("GuildName")]
    public string? GuildName { get; set; }

    [JsonPropertyName("AverageItemPower")]
    public double AverageItemPower { get; set; }
}
