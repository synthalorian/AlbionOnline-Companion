using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified NewShrine event for Linux port.
/// </summary>
public class NewShrineEvent
{
    public long ObjectId { get; }
    public int ShrineType { get; }
    public string Name { get; } = string.Empty;

    public NewShrineEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var shrineType))
            ShrineType = shrineType.ObjectToInt();

        if (parameters.TryGetValue(2, out var name))
            Name = name.ObjectToString();
    }
}
