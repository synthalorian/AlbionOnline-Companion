using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// UpdateMoney event — fires when silver changes.
/// Values are FixPoint (long / 10000).
/// </summary>
public class UpdateMoneyEvent
{
    public long ObjectId { get; }
    public FixPoint CurrentSilver { get; }
    public FixPoint GainedSilver { get; }

    public UpdateMoneyEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var currentSilver))
            CurrentSilver = FixPoint.FromInternalValue(currentSilver.ObjectToLong() ?? 0);

        if (parameters.TryGetValue(2, out var gainedSilver))
            GainedSilver = FixPoint.FromInternalValue(gainedSilver.ObjectToLong() ?? 0);
    }
}
