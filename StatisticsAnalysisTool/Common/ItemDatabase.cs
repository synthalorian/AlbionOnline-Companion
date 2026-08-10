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
/// Item database that maps Albion item unique names to display info.
/// Uses the Albion Online Data Project for item info.
/// </summary>
public class ItemDatabase
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private readonly Dictionary<string, ItemInfo> _items = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLoaded;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public static ItemDatabase Instance { get; } = new();

    private ItemDatabase() { }

    public bool IsLoaded => _isLoaded;
    public int ItemCount => _items.Count;

    /// <summary>
    /// Load item data from the AODP item list endpoint.
    /// </summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (_isLoaded) return;

        await _loadLock.WaitAsync(ct);
        try
        {
            if (_isLoaded) return;

            Log.Information("Loading item database...");

            // Try loading from the AODP GitHub items JSON
            var urls = new[]
            {
                "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/formatted/items.json",
                "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/items.json",
            };

            foreach (var url in urls)
            {
                try
                {
                    var response = await _http.GetAsync(url, ct);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync(ct);
                    
                    // Try parsing as array of items
                    try
                    {
                        var items = JsonSerializer.Deserialize<List<AodpItem>>(json);
                        if (items != null && items.Count > 0)
                        {
                            foreach (var item in items)
                            {
                                if (!string.IsNullOrEmpty(item.UniqueName))
                                {
                                    _items[item.UniqueName] = new ItemInfo
                                    {
                                        UniqueName = item.UniqueName,
                                        Name = item.LocalizedNames?.EnUs ?? item.UniqueName,
                                        Description = item.LocalizedDescriptions?.EnUs ?? string.Empty,
                                        Tier = ParseTier(item.UniqueName),
                                        Category = item.ShopCategory ?? string.Empty,
                                        SubCategory = item.ShopSubCategory ?? string.Empty,
                                    };
                                }
                            }
                            _isLoaded = true;
                            Log.Information("Loaded {Count} items from {Url}", _items.Count, url);
                            return;
                        }
                    }
                    catch { /* try next format */ }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Failed to load items from {Url}", url);
                }
            }

            // Fallback: create minimal database with common items
            LoadFallbackItems();
            _isLoaded = true;
            Log.Information("Loaded {Count} fallback items", _items.Count);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    /// <summary>
    /// Look up an item by its unique name (e.g., "T4_BAG", "T6_SWORD_BROAD").
    /// </summary>
    public ItemInfo? GetItem(string uniqueName)
    {
        // Strip enchantment level (@1, @2, etc.)
        var cleanName = uniqueName.Split('@')[0];
        return _items.TryGetValue(cleanName, out var item) ? item : null;
    }

    /// <summary>
    /// Get display name for an item, falling back to the unique name.
    /// </summary>
    public string GetDisplayName(string uniqueName)
    {
        var item = GetItem(uniqueName);
        return item?.Name ?? FormatUniqueName(uniqueName);
    }

    /// <summary>
    /// Search items by name.
    /// </summary>
    public List<ItemInfo> Search(string query, int maxResults = 50)
    {
        return _items.Values
            .Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        i.UniqueName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(maxResults)
            .ToList();
    }

    private static int ParseTier(string uniqueName)
    {
        if (uniqueName.StartsWith("T") && uniqueName.Length > 1 && char.IsDigit(uniqueName[1]))
        {
            return uniqueName[1] - '0';
        }
        return 0;
    }

    private static string FormatUniqueName(string uniqueName)
    {
        // "T4_SWORD_BROAD@1" → "T4 Sword Broad (Enchanted 1)"
        var parts = uniqueName.Split('@');
        var name = parts[0].Replace("_", " ");
        if (parts.Length > 1)
        {
            name += $" (Enchanted {parts[1]})";
        }
        return name;
    }

    private void LoadFallbackItems()
    {
        // Common items for basic functionality
        var fallbackItems = new[]
        {
            ("T4_BAG", "Adept's Bag", "Equipment"),
            ("T5_BAG", "Expert's Bag", "Equipment"),
            ("T6_BAG", "Master's Bag", "Equipment"),
            ("T7_BAG", "Grandmaster's Bag", "Equipment"),
            ("T8_BAG", "Elder's Bag", "Equipment"),
            ("T4_SWORD_BROAD", "Adept's Broadsword", "Weapon"),
            ("T5_SWORD_BROAD", "Expert's Broadsword", "Weapon"),
            ("T6_SWORD_BROAD", "Master's Broadsword", "Weapon"),
            ("T7_SWORD_BROAD", "Grandmaster's Broadsword", "Weapon"),
            ("T8_SWORD_BROAD", "Elder's Broadsword", "Weapon"),
            ("T4_ARMOR_PLATE_SET1", "Adept's Soldier Armor", "Armor"),
            ("T5_ARMOR_PLATE_SET1", "Expert's Soldier Armor", "Armor"),
            ("T6_ARMOR_PLATE_SET1", "Master's Soldier Armor", "Armor"),
            ("T7_ARMOR_PLATE_SET1", "Grandmaster's Soldier Armor", "Armor"),
            ("T8_ARMOR_PLATE_SET1", "Elder's Soldier Armor", "Armor"),
            ("T4_HEAD_PLATE_SET1", "Adept's Soldier Helmet", "Armor"),
            ("T5_HEAD_PLATE_SET1", "Expert's Soldier Helmet", "Armor"),
            ("T6_HEAD_PLATE_SET1", "Master's Soldier Helmet", "Armor"),
            ("T7_HEAD_PLATE_SET1", "Grandmaster's Soldier Helmet", "Armor"),
            ("T8_HEAD_PLATE_SET1", "Elder's Soldier Helmet", "Armor"),
            ("T4_SHOES_PLATE_SET1", "Adept's Soldier Boots", "Armor"),
            ("T5_SHOES_PLATE_SET1", "Expert's Soldier Boots", "Armor"),
            ("T6_SHOES_PLATE_SET1", "Master's Soldier Boots", "Armor"),
            ("T7_SHOES_PLATE_SET1", "Grandmaster's Soldier Boots", "Armor"),
            ("T8_SHOES_PLATE_SET1", "Elder's Soldier Boots", "Armor"),
            ("T4_CAPE", "Adept's Cape", "Accessory"),
            ("T5_CAPE", "Expert's Cape", "Accessory"),
            ("T6_CAPE", "Master's Cape", "Accessory"),
            ("T7_CAPE", "Grandmaster's Cape", "Accessory"),
            ("T8_CAPE", "Elder's Cape", "Accessory"),
        };

        foreach (var (uniqueName, name, category) in fallbackItems)
        {
            _items[uniqueName] = new ItemInfo
            {
                UniqueName = uniqueName,
                Name = name,
                Tier = ParseTier(uniqueName),
                Category = category,
            };
        }
    }
}

public class ItemInfo
{
    public string UniqueName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Tier { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string TierDisplay => Tier > 0 ? $"T{Tier}" : "";
}

// AODP JSON format
internal class AodpItem
{
    [JsonPropertyName("UniqueName")]
    public string? UniqueName { get; set; }

    [JsonPropertyName("LocalizedNames")]
    public AodpLocalized? LocalizedNames { get; set; }

    [JsonPropertyName("LocalizedDescriptions")]
    public AodpLocalized? LocalizedDescriptions { get; set; }

    [JsonPropertyName("ShopCategory")]
    public string? ShopCategory { get; set; }

    [JsonPropertyName("ShopSubCategory")]
    public string? ShopSubCategory { get; set; }
}

internal class AodpLocalized
{
    [JsonPropertyName("EN-US")]
    public string? EnUs { get; set; }
}
