using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// UpdateFame event — fires when player gains fame.
/// Values are FixPoint (long / 10000).
/// </summary>
public class UpdateFameEvent
{
    public long ObjectId { get; }
    public FixPoint TotalPlayerFame { get; }
    public FixPoint FameWithZoneMultiplier { get; }
    public FixPoint ZoneFame { get; }
    public FixPoint Multiplier { get; } = FixPoint.FromFloatingPointValue(1);
    public bool IsPremiumBonus { get; }
    public FixPoint SatchelFame { get; }
    public double BonusFactor { get; } = 1;
    public double PremiumFame { get; }
    public double TotalGainedFame { get; }

    public UpdateFameEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out var objectId))
                ObjectId = objectId.ObjectToLong() ?? 0;

            if (parameters.TryGetValue(1, out var totalFame))
                TotalPlayerFame = FixPoint.FromInternalValue(totalFame.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(2, out var fameZone))
                FameWithZoneMultiplier = FixPoint.FromInternalValue(fameZone.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(3, out var zoneFame))
                ZoneFame = FixPoint.FromInternalValue(zoneFame.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(4, out var multiplier))
                Multiplier = FixPoint.FromInternalValue(multiplier.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(5, out var premium))
                IsPremiumBonus = premium.ObjectToBool();

            if (parameters.TryGetValue(10, out var satchel))
                SatchelFame = FixPoint.FromInternalValue(satchel.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(17, out var bonusFactor))
            {
                BonusFactor = 1 + (bonusFactor as float? ?? 0);
                if (BonusFactor > 2) BonusFactor = 1; // Sanity check
            }

            // Calculate total gained (matching original logic)
            double fameWithZoneAndPremium = 0;
            if (FameWithZoneMultiplier.DoubleValue > 0)
            {
                fameWithZoneAndPremium = IsPremiumBonus
                    ? FameWithZoneMultiplier.DoubleValue * 1.5f
                    : FameWithZoneMultiplier.DoubleValue;
            }

            if (fameWithZoneAndPremium > 0 && FameWithZoneMultiplier.DoubleValue > 0)
            {
                PremiumFame = fameWithZoneAndPremium - FameWithZoneMultiplier.DoubleValue;
            }

            TotalGainedFame = (FameWithZoneMultiplier.DoubleValue + PremiumFame + SatchelFame.DoubleValue) * BonusFactor;
        }
        catch { /* Silently handle parse errors */ }
    }
}
