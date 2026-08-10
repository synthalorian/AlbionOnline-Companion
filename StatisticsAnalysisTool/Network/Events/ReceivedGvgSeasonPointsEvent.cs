using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified ReceivedGvgSeasonPoints event for Linux port.
/// </summary>
public class ReceivedGvgSeasonPointsEvent
{
    public long ObjectId { get; }
    public double Points { get; }

    public ReceivedGvgSeasonPointsEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var points))
            Points = points.ObjectToDouble();
    }
}
