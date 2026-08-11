using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

/// <summary>
/// JoinResponse — the key packet that identifies the local player.
/// Contains username, UserObjectId, map, silver, gold, respec, guild, alliance.
/// </summary>
public class JoinResponse
{
    public long UserObjectId { get; }
    public string Username { get; } = string.Empty;
    public string MapIndex { get; } = string.Empty;
    public string GuildName { get; } = string.Empty;
    public string AllianceName { get; } = string.Empty;
    public FixPoint Silver { get; }
    public FixPoint Gold { get; }
    public FixPoint ReSpecPoints { get; }
    public FixPoint LearningPoints { get; }
    public double CurrentFocusPoints { get; }
    public double MaxCurrentFocusPoints { get; }
    public double Reputation { get; }
    public bool IsReSpecActive { get; }
    public string SourceClusterIndex { get; } = string.Empty;

    public JoinResponse(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out var objectId))
                UserObjectId = objectId.ObjectToLong() ?? 0;

            if (parameters.TryGetValue(2, out var username))
                Username = username.ObjectToString();

            if (parameters.TryGetValue(8, out var mapIndex))
                MapIndex = mapIndex.ObjectToString();

            if (parameters.TryGetValue(27, out var focus))
                CurrentFocusPoints = focus.ObjectToDouble();

            if (parameters.TryGetValue(28, out var maxFocus))
                MaxCurrentFocusPoints = maxFocus.ObjectToDouble();

            if (parameters.TryGetValue(33, out var silver))
                Silver = FixPoint.FromInternalValue(silver.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(34, out var gold))
                Gold = FixPoint.FromInternalValue(gold.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(37, out var learningPoints))
                LearningPoints = FixPoint.FromInternalValue(learningPoints.ObjectToLong() ?? 0);

            if (parameters.TryGetValue(41, out var reputation))
                Reputation = reputation.ObjectToDouble();

            // Param 43: respec array [?, totalInternal, ...] — arrives as a typed
            // array (long[]/int[]/object[] depending on serializer path), so
            // normalize via Array instead of a single-type pattern match
            if (parameters.TryGetValue(43, out var reSpecObj) && reSpecObj is Array reSpecArray && reSpecArray.Length > 1)
                ReSpecPoints = FixPoint.FromInternalValue(reSpecArray.GetValue(1).ObjectToLong() ?? 0);

            if (parameters.TryGetValue(58, out var guildName))
                GuildName = guildName.ObjectToString();

            if (parameters.TryGetValue(65, out var sourceCluster))
                SourceClusterIndex = sourceCluster.ObjectToString();

            if (parameters.TryGetValue(79, out var allianceName))
                AllianceName = allianceName.ObjectToString();

            if (parameters.TryGetValue(98, out var reSpecActive))
                IsReSpecActive = reSpecActive.ObjectToBool();
        }
        catch { /* Silently handle parse errors */ }
    }
}
