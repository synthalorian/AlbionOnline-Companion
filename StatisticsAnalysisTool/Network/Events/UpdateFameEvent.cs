using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified UpdateFame event for Linux port.
/// </summary>
public class UpdateFameEvent
{
    public long ObjectId { get; }
    public double TotalPlayerFame { get; }
    public double FameWithZoneMultiplier { get; }
    public double GroupSize { get; }
    public double Multiplier { get; }
    public bool IsPremiumBonus { get; }
    public double BonusFactor { get; }
    public double SatchelFame { get; }
    public double GainedFame => FameWithZoneMultiplier;

    public UpdateFameEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var totalFame))
            TotalPlayerFame = totalFame.ObjectToDouble();

        if (parameters.TryGetValue(2, out var fameWithZoneMultiplier))
            FameWithZoneMultiplier = fameWithZoneMultiplier.ObjectToDouble();

        if (parameters.TryGetValue(3, out var groupSize))
            GroupSize = groupSize.ObjectToDouble();

        if (parameters.TryGetValue(4, out var multiplier))
            Multiplier = multiplier.ObjectToDouble();

        if (parameters.TryGetValue(5, out var isPremiumBonus))
            IsPremiumBonus = isPremiumBonus.ObjectToBool();

        if (parameters.TryGetValue(6, out var bonusFactor))
            BonusFactor = bonusFactor.ObjectToDouble();

        if (parameters.TryGetValue(10, out var satchelFame))
            SatchelFame = satchelFame.ObjectToDouble();
    }
}
