using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles loot events and updates the LootLoggerViewModel.
/// </summary>
public class LootLoggerEventHandler : 
    EventPacketHandler<NewLootEvent>,
    IPacketHandler
{
    private readonly LootLoggerViewModel _viewModel;
    private double _totalValue;
    private DateTime _sessionStart = DateTime.UtcNow;

    public LootLoggerEventHandler(LootLoggerViewModel viewModel) 
        : base((int)EventCodes.NewEquipmentItem)
    {
        _viewModel = viewModel;
    }

    protected override Task OnActionAsync(NewLootEvent value)
    {
        try
        {
            _totalValue += value.EstimatedMarketValue;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var loot = new LootLogEntry
                {
                    ItemName = $"Item_{value.ItemId}",
                    PlayerName = value.CrafterName,
                    Quantity = value.Amount,
                    EstimatedValue = (int)value.EstimatedMarketValue,
                    Timestamp = DateTime.Now,
                    LootType = "Equipment",
                    Tier = "T4"
                };

                _viewModel.LootEntries.Insert(0, loot);
                _viewModel.TotalLootValue = FormatNumber(_totalValue);
                _viewModel.LootValuePerHour = CalculatePerHour(_totalValue) + " /h";
            });
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "LootLoggerEventHandler error");
        }

        return Task.CompletedTask;
    }

    private string CalculatePerHour(double value)
    {
        var hours = (DateTime.UtcNow - _sessionStart).TotalHours;
        if (hours < 0.001) hours = 0.001;
        return FormatNumber(value / hours);
    }

    private static string FormatNumber(double value)
    {
        return value switch
        {
            >= 1_000_000 => $"{value / 1_000_000:F1}M",
            >= 1_000 => $"{value / 1_000:F1}K",
            _ => $"{value:F0}"
        };
    }

}
