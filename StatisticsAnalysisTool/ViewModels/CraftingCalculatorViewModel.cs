using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class CraftingCalculatorViewModel : ViewModelBase
{
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
                item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
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
            // TODO: Implement crafting calculation
            await Task.Delay(500); // Placeholder

            TotalResourceCost = "10,000";
            TotalCraftingFee = "1,500";
            TotalCost = "11,500";
            EstimatedSellPrice = "15,000";
            Profit = "3,500";
            ProfitMargin = "30.4%";
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
}

public class CraftableItem
{
    public string Name { get; set; } = string.Empty;
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
