using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified UpdateMoney event for Linux port.
/// </summary>
public class UpdateMoneyEvent
{
    public long ObjectId { get; }
    public double CurrentSilver { get; }
    public double GainedSilver { get; }

    public UpdateMoneyEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var currentSilver))
            CurrentSilver = currentSilver.ObjectToDouble();

        if (parameters.TryGetValue(2, out var gainedSilver))
            GainedSilver = gainedSilver.ObjectToDouble();
    }
}
