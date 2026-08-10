using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified PartySilverGained event for Linux port.
/// </summary>
public class PartySilverGainedEvent
{
    public long ObjectId { get; }
    public double Silver { get; }

    public PartySilverGainedEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var silver))
            Silver = silver.ObjectToDouble();
    }
}
