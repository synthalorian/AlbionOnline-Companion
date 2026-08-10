using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified Died event for Linux port.
/// </summary>
public class DiedEvent
{
    public long ObjectId { get; }
    public long KillerId { get; }
    public string KillerName { get; } = string.Empty;

    public DiedEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var killerId))
            KillerId = killerId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(2, out var killerName))
            KillerName = killerName.ObjectToString();
    }
}
