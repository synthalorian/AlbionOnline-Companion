using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

/// <summary>
/// Maps Albion cluster indexes ("3003") to display names ("Caerleon").
/// Data from the Albion Online Data Project world.json.
/// </summary>
public class ClusterDatabase
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private readonly Dictionary<string, string> _clusters = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLoaded;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public static ClusterDatabase Instance { get; } = new();

    private ClusterDatabase() { }

    public bool IsLoaded => _isLoaded;
    public int ClusterCount => _clusters.Count;

    /// <summary>
    /// Load cluster index → name mapping from the AODP world JSON.
    /// </summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (_isLoaded) return;

        await _loadLock.WaitAsync(ct);
        try
        {
            if (_isLoaded) return;

            Log.Information("Loading cluster database...");

            const string url = "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/formatted/world.json";

            var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Cluster database download failed: {Status}", response.StatusCode);
                return;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var entries = JsonSerializer.Deserialize<List<AodpCluster>>(json);

            if (entries == null || entries.Count == 0)
            {
                Log.Warning("Cluster database parsed empty");
                return;
            }

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Index) && !string.IsNullOrEmpty(entry.UniqueName))
                    _clusters[entry.Index] = entry.UniqueName;
            }

            _isLoaded = true;
            Log.Information("Cluster database ready: {Count} clusters", _clusters.Count);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Cluster database load failed");
        }
        finally
        {
            _loadLock.Release();
        }
    }

    /// <summary>
    /// Resolve a cluster index to its display name.
    /// Returns the index itself when unknown or not loaded yet.
    /// </summary>
    public string GetName(string clusterIndex)
    {
        if (string.IsNullOrEmpty(clusterIndex))
            return "Unknown";

        // Player/guild islands arrive as "@ISLAND@<guid>"
        if (clusterIndex.StartsWith("@ISLAND@", StringComparison.OrdinalIgnoreCase))
            return "Island";

        return _clusters.TryGetValue(clusterIndex, out var name) ? name : clusterIndex;
    }

    private class AodpCluster
    {
        [JsonPropertyName("Index")]
        public string Index { get; set; } = string.Empty;

        [JsonPropertyName("UniqueName")]
        public string UniqueName { get; set; } = string.Empty;
    }
}
