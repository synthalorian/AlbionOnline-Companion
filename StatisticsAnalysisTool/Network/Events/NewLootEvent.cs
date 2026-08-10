using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified NewLoot event for Linux port.
/// </summary>
public class NewLootEvent
{
    public long ObjectId { get; }
    public int ItemId { get; }
    public int Amount { get; }
    public double EstimatedMarketValue { get; }
    public string CrafterName { get; } = string.Empty;

    public NewLootEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var itemId))
            ItemId = itemId.ObjectToInt();

        if (parameters.TryGetValue(2, out var amount))
            Amount = amount.ObjectToInt();

        if (parameters.TryGetValue(4, out var marketValue))
            EstimatedMarketValue = marketValue.ObjectToDouble();

        if (parameters.TryGetValue(5, out var crafterName))
            CrafterName = crafterName.ObjectToString();
    }
}
