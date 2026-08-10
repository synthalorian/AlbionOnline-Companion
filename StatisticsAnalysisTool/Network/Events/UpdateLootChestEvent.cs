using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified UpdateLootChest event for Linux port.
/// </summary>
public class UpdateLootChestEvent
{
    public long ObjectId { get; }
    public string PlayerGuid { get; } = string.Empty;
    public bool IsFreeForAll { get; }

    public UpdateLootChestEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(3, out var playerGuid))
            PlayerGuid = playerGuid.ObjectToString();

        if (parameters.TryGetValue(7, out var freeForAll))
            IsFreeForAll = freeForAll.ObjectToBool();
    }
}
