using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified NewMob event for Linux port.
/// </summary>
public class NewMobEvent
{
    public long ObjectId { get; }
    public int MobId { get; }
    public string Name { get; } = string.Empty;
    public int Tier { get; }
    public double Health { get; }
    public double MaxHealth { get; }

    public NewMobEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var mobId))
            MobId = mobId.ObjectToInt();

        if (parameters.TryGetValue(2, out var name))
            Name = name.ObjectToString();

        if (parameters.TryGetValue(3, out var tier))
            Tier = tier.ObjectToInt();

        if (parameters.TryGetValue(4, out var health))
            Health = health.ObjectToDouble();

        if (parameters.TryGetValue(5, out var maxHealth))
            MaxHealth = maxHealth.ObjectToDouble();
    }
}
