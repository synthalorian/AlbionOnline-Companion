using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified HealthUpdate event for Linux port.
/// Extracts damage/healing data from Photon parameters.
/// </summary>
public class HealthUpdateEvent
{
    public long CauserId { get; }
    public long AffectedObjectId { get; }
    public double HealthChange { get; }
    public double NewHealthValue { get; }
    public bool HasNewHealthValue { get; }
    public GameTimeStamp TimeStamp { get; }
    public byte EffectType { get; }
    public byte EffectOrigin { get; }
    public short CausingSpellIndex { get; }

    public bool IsDamage => HealthChange < 0;
    public bool IsHealing => HealthChange > 0;
    public double DamageAmount => IsDamage ? Math.Abs(HealthChange) : 0;
    public double HealingAmount => IsHealing ? HealthChange : 0;

    public HealthUpdateEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var affectedObjectId))
            AffectedObjectId = affectedObjectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var timestamp))
            TimeStamp = new GameTimeStamp(timestamp.ObjectToLong() ?? 0);

        if (parameters.TryGetValue(2, out var healthChange))
            HealthChange = healthChange.ObjectToDouble();

        if (parameters.TryGetValue(3, out var newHealthValue))
        {
            NewHealthValue = newHealthValue.ObjectToDouble();
            HasNewHealthValue = true;
        }

        if (parameters.TryGetValue(4, out var effectType))
            EffectType = effectType as byte? ?? 0;

        if (parameters.TryGetValue(5, out var effectOrigin))
            EffectOrigin = effectOrigin as byte? ?? 0;

        if (parameters.TryGetValue(6, out var causerId))
            CauserId = causerId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(7, out var causingSpellType))
            CausingSpellIndex = causingSpellType.ObjectToShort();
    }
}
