using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified GrabbedLoot event for Linux port.
/// </summary>
public class GrabbedLootEvent
{
    public long ObjectId { get; }
    public int ItemId { get; }
    public int Amount { get; }
    public string LooterName { get; } = string.Empty;

    public GrabbedLootEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var itemId))
            ItemId = itemId.ObjectToInt();

        if (parameters.TryGetValue(2, out var amount))
            Amount = amount.ObjectToInt();

        if (parameters.TryGetValue(3, out var looterName))
            LooterName = looterName.ObjectToString();
    }
}
