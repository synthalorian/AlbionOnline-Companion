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
        // HealthUpdates contains arrays of health update data
        if (parameters.TryGetValue(0, out var affectedIds) && affectedIds is object[] ids)
        {
            var healthChanges = parameters.TryGetValue(2, out var hc) && hc is object[] hca ? hca : null;
            var timestamps = parameters.TryGetValue(1, out var ts) && ts is object[] tsa ? tsa : null;
            var causerIds = parameters.TryGetValue(6, out var ci) && ci is object[] cia ? cia : null;

            for (int i = 0; i < ids.Length; i++)
            {
                var updateParams = new Dictionary<byte, object>();
                updateParams[0] = ids[i];
                if (timestamps != null && i < timestamps.Length) updateParams[1] = timestamps[i];
                if (healthChanges != null && i < healthChanges.Length) updateParams[2] = healthChanges[i];
                if (causerIds != null && i < causerIds.Length) updateParams[6] = causerIds[i];

                Updates.Add(new HealthUpdateEvent(updateParams));
            }
        }
    }
}
