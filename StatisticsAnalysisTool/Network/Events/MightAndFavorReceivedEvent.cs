using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// MightAndFavorReceived event — fires when might/favor is gained.
/// Values are FixPoint (long / 10000).
/// </summary>
public class MightAndFavorReceivedEvent
{
    public long ObjectId { get; }
    public FixPoint Might { get; }
    public FixPoint Favor { get; }

    public MightAndFavorReceivedEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var might))
            Might = FixPoint.FromInternalValue(might.ObjectToLong() ?? 0);

        if (parameters.TryGetValue(2, out var favor))
            Favor = FixPoint.FromInternalValue(favor.ObjectToLong() ?? 0);
    }
}
