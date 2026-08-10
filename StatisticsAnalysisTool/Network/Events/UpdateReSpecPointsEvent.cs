using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified UpdateReSpecPoints event for Linux port.
/// </summary>
public class UpdateReSpecPointsEvent
{
    public long ObjectId { get; }
    public double GainedReSpec { get; }
    public double PaidSilver { get; }

    public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(2, out var gainedReSpec))
            GainedReSpec = gainedReSpec.ObjectToDouble();

        if (parameters.TryGetValue(3, out var paidSilver))
            PaidSilver = paidSilver.ObjectToDouble();
    }
}
