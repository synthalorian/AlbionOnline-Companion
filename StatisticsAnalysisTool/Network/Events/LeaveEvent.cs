using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Entity left range or was removed.
/// </summary>
public class LeaveEvent
{
    public long ObjectId { get; }

    public LeaveEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;
    }
}
