using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified MightAndFavorReceived event for Linux port.
/// </summary>
public class MightAndFavorReceivedEvent
{
    public long ObjectId { get; }
    public double Might { get; }
    public double Favor { get; }

    public MightAndFavorReceivedEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var might))
            Might = might.ObjectToDouble();

        if (parameters.TryGetValue(2, out var favor))
            Favor = favor.ObjectToDouble();
    }
}
