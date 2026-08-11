using StatisticsAnalysisTool.Common;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified HealthUpdates event for Linux port (batch health updates).
/// </summary>
public class HealthUpdatesEvent
{
    public List<HealthUpdateEvent> Updates { get; } = new();

    public HealthUpdatesEvent(Dictionary<byte, object> parameters)
    {
        // HealthUpdates contains arrays of health update data.
        // NOTE: the Photon deserializer materializes TYPED arrays (int[], long[],
        // float[]) — `is object[]` fails for those (no covariance for value types),
        // which silently killed the whole damage meter. Normalize via Array.
        var ids = AsObjectArray(parameters.TryGetValue(0, out var a0) ? a0 : null);
        if (ids == null)
            return;

        var timestamps = AsObjectArray(parameters.TryGetValue(1, out var a1) ? a1 : null);
        var healthChanges = AsObjectArray(parameters.TryGetValue(2, out var a2) ? a2 : null);
        var newHealthValues = AsObjectArray(parameters.TryGetValue(3, out var a3) ? a3 : null);
        var effectTypes = AsObjectArray(parameters.TryGetValue(4, out var a4) ? a4 : null);
        var effectOrigins = AsObjectArray(parameters.TryGetValue(5, out var a5) ? a5 : null);
        var causerIds = AsObjectArray(parameters.TryGetValue(6, out var a6) ? a6 : null);
        var spellIndexes = AsObjectArray(parameters.TryGetValue(7, out var a7) ? a7 : null);

        for (int i = 0; i < ids.Length; i++)
        {
            var updateParams = new Dictionary<byte, object>
            {
                [0] = ids[i]
            };
            if (timestamps != null && i < timestamps.Length) updateParams[1] = timestamps[i];
            if (healthChanges != null && i < healthChanges.Length) updateParams[2] = healthChanges[i];
            if (newHealthValues != null && i < newHealthValues.Length) updateParams[3] = newHealthValues[i];
            if (effectTypes != null && i < effectTypes.Length) updateParams[4] = effectTypes[i];
            if (effectOrigins != null && i < effectOrigins.Length) updateParams[5] = effectOrigins[i];
            if (causerIds != null && i < causerIds.Length) updateParams[6] = causerIds[i];
            if (spellIndexes != null && i < spellIndexes.Length) updateParams[7] = spellIndexes[i];

            Updates.Add(new HealthUpdateEvent(updateParams));
        }
    }

    /// <summary>
    /// Normalize any array (typed or object[]) to object[].
    /// </summary>
    private static object[]? AsObjectArray(object? value)
    {
        switch (value)
        {
            case null:
                return null;
            case object[] objectArray:
                return objectArray;
            case System.Array array:
                var result = new object[array.Length];
                for (int i = 0; i < array.Length; i++)
                    result[i] = array.GetValue(i)!;
                return result;
            default:
                return null;
        }
    }
}
