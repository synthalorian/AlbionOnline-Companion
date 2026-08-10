using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class CraftingCalculatorViewModel : ViewModelBase
{
    private readonly AlbionDataService _dataService = AlbionDataService.Instance;
    private readonly ItemDatabase _itemDb = ItemDatabase.Instance;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CraftableItem> _craftableItems = new();

    [ObservableProperty]
    private ObservableCollection<CraftableItem> _filteredItems = new();

    [ObservableProperty]
    private CraftableItem? _selectedItem;

    [ObservableProperty]
    private int _craftQuantity = 1;

    [ObservableProperty]
    private string _selectedCity = "Caerleon";

    [ObservableProperty]
    private ObservableCollection<string> _cities = new();

    [ObservableProperty]
    private bool _useFocus;

    [ObservableProperty]
    private string _resourceReturnRate = "15.2%";

    [ObservableProperty]
    private string _craftingFee = "0";

    [ObservableProperty]
    private string _totalResourceCost = "0";

    [ObservableProperty]
    private string _totalCraftingFee = "0";

    [ObservableProperty]
    private string _totalCost = "0";

    [ObservableProperty]
    private string _estimatedSellPrice = "0";

    [ObservableProperty]
    private string _profit = "0";

    [ObservableProperty]
    private string _profitMargin = "0%";

    [ObservableProperty]
    private bool _isCalculating;

    [ObservableProperty]
    private bool _isLoadingItems;

    [ObservableProperty]
    private string _itemLoadStatus = "Loading items...";

    [ObservableProperty]
    private ObservableCollection<CraftingResource> _requiredResources = new();

    public CraftingCalculatorViewModel()
    {
        Cities.Add("Caerleon");
        Cities.Add("Bridgewatch");
        Cities.Add("Lymhurst");
        Cities.Add("Martlock");
        Cities.Add("Thetford");
        Cities.Add("Fort Sterling");
        Cities.Add("Brecilien");

        CraftableItems.CollectionChanged += (s, e) => ApplyFilter();

        // Load items in background
        _ = LoadItemsAsync();
    }

    private async Task LoadItemsAsync()
    {
        IsLoadingItems = true;
        ItemLoadStatus = "Loading item database...";

        try
        {
            await _itemDb.LoadAsync();

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                CraftableItems.Clear();

                foreach (var item in _itemDb.Search("", 500))
                {
                    CraftableItems.Add(new CraftableItem
                    {
                        Name = item.Name,
                        UniqueName = item.UniqueName,
                        Tier = item.TierDisplay,
                        Category = item.Category,
                    });
                }

                ApplyFilter();
                ItemLoadStatus = $"{_itemDb.ItemCount} items loaded";
                IsLoadingItems = false;
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load items");
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                ItemLoadStatus = "Failed to load items";
                IsLoadingItems = false;
            });
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredItems.Clear();
        foreach (var item in CraftableItems)
        {
            if (string.IsNullOrWhiteSpace(SearchText) ||
                item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                item.UniqueName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                FilteredItems.Add(item);
            }
        }
    }

    [RelayCommand]
    private async Task CalculateProfit()
    {
        if (SelectedItem == null)
            return;

        IsCalculating = true;

        try
        {
            // Fetch real market prices from Albion Online Data Project
            var prices = await _dataService.GetPricesAsync(
                new[] { SelectedItem.UniqueName },
                SelectedCity);

            var price = prices.FirstOrDefault();

            if (price != null)
            {
                EstimatedSellPrice = FormatSilver(price.SellPriceMin);
                
                // Simplified calculation — real one needs resource costs
                var sellPrice = price.SellPriceMin;
                var estimatedCost = (long)(sellPrice * 0.7); // Rough 70% cost estimate
                var profit = sellPrice - estimatedCost;
                var margin = sellPrice > 0 ? (double)profit / sellPrice * 100 : 0;

                TotalResourceCost = FormatSilver(estimatedCost);
                TotalCraftingFee = FormatSilver((long)(estimatedCost * 0.1));
                TotalCost = FormatSilver(estimatedCost);
                Profit = FormatSilver(profit);
                ProfitMargin = $"{margin:F1}%";
            }
            else
            {
                EstimatedSellPrice = "No data";
                Profit = "Unknown";
                ProfitMargin = "N/A";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to calculate profit");
            EstimatedSellPrice = "Error";
        }
        finally
        {
            IsCalculating = false;
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedItem = null;
        RequiredResources.Clear();
        TotalResourceCost = "0";
        TotalCraftingFee = "0";
        TotalCost = "0";
        EstimatedSellPrice = "0";
        Profit = "0";
        ProfitMargin = "0%";
    }

    private static string FormatSilver(long value)
    {
        return value switch
        {
            >= 1_000_000 => $"{value / 1_000_000.0:F2}M",
            >= 1_000 => $"{value / 1_000.0:F1}K",
            _ => value.ToString("N0")
        };
    }
}

public class CraftableItem
{
    public string Name { get; set; } = string.Empty;
    public string UniqueName { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public int BaseResourceCost { get; set; }
}

public class CraftingResource
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string UnitPrice { get; set; } = string.Empty;
    public string TotalPrice { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}
