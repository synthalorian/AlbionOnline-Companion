using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified NewRandomDungeonExit event for Linux port.
/// </summary>
public class NewRandomDungeonExitEvent
{
    public int ObjectId { get; }
    public WorldPosition? SourceExitPosition { get; }
    public string SourceClusterIndex { get; } = string.Empty;
    public int Level { get; } = -1;
    public bool IsAlreadyEntered { get; }

    public NewRandomDungeonExitEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToInt();

        if (parameters.TryGetValue(1, out var x) && parameters.TryGetValue(2, out var y))
            SourceExitPosition = new WorldPosition(x.ObjectToInt(), y.ObjectToInt());

        if (parameters.TryGetValue(3, out var clusterIndex))
            SourceClusterIndex = clusterIndex.ObjectToString();

        if (parameters.TryGetValue(4, out var level))
            Level = level.ObjectToInt();

        if (parameters.TryGetValue(5, out var alreadyEntered))
            IsAlreadyEntered = alreadyEntered.ObjectToBool();
    }
}
