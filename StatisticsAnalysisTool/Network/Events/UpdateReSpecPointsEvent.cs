using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// UpdateReSpecPoints event — fires when respec points change.
/// Values are FixPoint (long / 10000).
/// </summary>
public class UpdateReSpecPointsEvent
{
    public long ObjectId { get; }
    public FixPoint TotalReSpec { get; }
    public FixPoint GainedReSpec { get; }
    public FixPoint PaidSilver { get; }

    public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters)
    {
        // Param 0: array [?, totalRespecInternal, ...] per protocol dump in
        // EventCodes.cs: map[0:[0 55814284204 0 0 0] 1:1 2:9948534 3:10000000]
        if (parameters.TryGetValue(0, out var objectId))
        {
            if (objectId is Array arr && arr.Length > 1)
            {
                ObjectId = arr.GetValue(0).ObjectToLong() ?? 0;
                TotalReSpec = FixPoint.FromInternalValue(arr.GetValue(1).ObjectToLong() ?? 0);
            }
            else
            {
                ObjectId = objectId.ObjectToLong() ?? 0;
            }
        }

        if (parameters.TryGetValue(2, out var gainedReSpec))
            GainedReSpec = FixPoint.FromInternalValue(gainedReSpec.ObjectToLong() ?? 0);

        if (parameters.TryGetValue(3, out var paidSilver))
            PaidSilver = FixPoint.FromInternalValue(paidSilver.ObjectToLong() ?? 0);
    }
}
