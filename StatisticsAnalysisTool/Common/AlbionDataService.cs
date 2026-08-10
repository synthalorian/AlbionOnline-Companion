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
/// Fetches game data from the Albion Online Data Project API.
/// No game installation required — works purely over HTTP.
/// </summary>
public class AlbionDataService
{
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://www.albion-online-data.com/api/v2/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Cache
    private readonly Dictionary<string, CachedPriceData> _priceCache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public static AlbionDataService Instance { get; } = new();

    private AlbionDataService() { }

    /// <summary>
    /// Get current market prices for items at a specific city.
    /// </summary>
    public async Task<List<ItemPrice>> GetPricesAsync(
        IEnumerable<string> itemIds,
        string location = "Caerleon",
        int quality = 1,
        CancellationToken ct = default)
    {
        var itemList = itemIds.ToList();
        if (itemList.Count == 0) return new List<ItemPrice>();

        // Check cache first
        var cacheKey = $"{string.Join(",", itemList)}:{location}:{quality}";
        await _cacheLock.WaitAsync(ct);
        try
        {
            if (_priceCache.TryGetValue(cacheKey, out var cached) && 
                DateTime.UtcNow - cached.FetchedAt < _cacheExpiry)
            {
                return cached.Prices;
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        try
        {
            var ids = string.Join(",", itemList.Take(100)); // API limit
            var url = $"stats/prices/{Uri.EscapeDataString(ids)}?locations={Uri.EscapeDataString(location)}&qualities={quality}";

            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var prices = JsonSerializer.Deserialize<List<ItemPrice>>(json, JsonOptions) ?? new List<ItemPrice>();

            // Cache the result
            await _cacheLock.WaitAsync(ct);
            try
            {
                _priceCache[cacheKey] = new CachedPriceData
                {
                    Prices = prices,
                    FetchedAt = DateTime.UtcNow
                };
            }
            finally
            {
                _cacheLock.Release();
            }

            Log.Debug("Fetched {Count} prices for {Location}", prices.Count, location);
            return prices;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to fetch prices for {Location}", location);
            return new List<ItemPrice>();
        }
    }

    /// <summary>
    /// Get price history for an item.
    /// </summary>
    public async Task<List<PriceHistoryEntry>> GetPriceHistoryAsync(
        string itemId,
        string location = "Caerleon",
        int quality = 1,
        string timeScale = "24",
        CancellationToken ct = default)
    {
        try
        {
            var url = $"stats/history/{Uri.EscapeDataString(itemId)}?locations={Uri.EscapeDataString(location)}&qualities={quality}&time-scale={timeScale}";

            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var history = JsonSerializer.Deserialize<List<PriceHistoryResponse>>(json, JsonOptions);

            return history?.FirstOrDefault()?.Data ?? new List<PriceHistoryEntry>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to fetch price history for {ItemId}", itemId);
            return new List<PriceHistoryEntry>();
        }
    }

    /// <summary>
    /// Get gold prices (premium currency).
    /// </summary>
    public async Task<List<GoldPrice>> GetGoldPricesAsync(int count = 24, CancellationToken ct = default)
    {
        try
        {
            var url = $"stats/gold?count={count}";
            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<GoldPrice>>(json, JsonOptions) ?? new List<GoldPrice>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to fetch gold prices");
            return new List<GoldPrice>();
        }
    }

    private class CachedPriceData
    {
        public List<ItemPrice> Prices { get; set; } = new();
        public DateTime FetchedAt { get; set; }
    }
}

public class ItemPrice
{
    [JsonPropertyName("item_id")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("quality")]
    public int Quality { get; set; }

    [JsonPropertyName("sell_price_min")]
    public long SellPriceMin { get; set; }

    [JsonPropertyName("sell_price_min_date")]
    public DateTime SellPriceMinDate { get; set; }

    [JsonPropertyName("sell_price_max")]
    public long SellPriceMax { get; set; }

    [JsonPropertyName("sell_price_max_date")]
    public DateTime SellPriceMaxDate { get; set; }

    [JsonPropertyName("buy_price_min")]
    public long BuyPriceMin { get; set; }

    [JsonPropertyName("buy_price_min_date")]
    public DateTime BuyPriceMinDate { get; set; }

    [JsonPropertyName("buy_price_max")]
    public long BuyPriceMax { get; set; }

    [JsonPropertyName("buy_price_max_date")]
    public DateTime BuyPriceMaxDate { get; set; }
}

public class PriceHistoryResponse
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("item_id")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("quality")]
    public int Quality { get; set; }

    [JsonPropertyName("data")]
    public List<PriceHistoryEntry> Data { get; set; } = new();
}

public class PriceHistoryEntry
{
    [JsonPropertyName("item_count")]
    public int ItemCount { get; set; }

    [JsonPropertyName("avg_price")]
    public double AvgPrice { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class GoldPrice
{
    [JsonPropertyName("price")]
    public long Price { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
