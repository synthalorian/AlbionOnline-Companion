using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// UpdateReSpecPoints event — fires when respec points change.
/// Values are FixPoint (long / 10000).
/// </summary>
public class UpdateReSpecPointsEvent
{
    public long ObjectId { get; }
    public FixPoint GainedReSpec { get; }
    public FixPoint PaidSilver { get; }

    public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(2, out var gainedReSpec))
            GainedReSpec = FixPoint.FromInternalValue(gainedReSpec.ObjectToLong() ?? 0);

        if (parameters.TryGetValue(3, out var paidSilver))
            PaidSilver = FixPoint.FromInternalValue(paidSilver.ObjectToLong() ?? 0);
    }
}
