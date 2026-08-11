using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// PartyJoined event (code 231) — full party roster when YOU join a party.
/// Known layout (from albion-network-lib dumps):
///   0: party object id, 3: own GUID, 4: member GUID array,
///   5: member NAME array (string[]), 6-10: per-member stat arrays.
/// NOTE: carries names + character GUIDs, not in-world ObjectIds — ObjectIds
/// for roster members resolve via NewCharacter / PartyPlayerJoined.
/// </summary>
public class PartyJoinedEvent
{
    public long PartyObjectId { get; }
    public string[] MemberNames { get; } = Array.Empty<string>();

    private static int _diagCount;

    public PartyJoinedEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var id))
            PartyObjectId = id.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(5, out var names))
            MemberNames = PartyEventParams.AsStringArray(names);

        PartyEventParams.DumpOnce(ref _diagCount, "PartyJoined", parameters);
    }
}

/// <summary>
/// PartyPlayerJoined event (code 233) — someone joined your party (also fires
/// once per existing member when YOU join or re-zone).
/// Verified layout 2026-08-11: 0: PARTY id (constant across members),
/// 1: member GUID (byte[16]), 2: member NAME (string), 3-9: member stats.
/// No member ObjectId is carried — names resolve to entities via NewCharacter.
/// </summary>
public class PartyPlayerJoinedEvent
{
    public long ObjectId { get; }
    public string Name { get; } = string.Empty;

    private static int _diagCount;

    public PartyPlayerJoinedEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var id))
            ObjectId = id.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(2, out var name))
            Name = name.ObjectToString();

        PartyEventParams.DumpOnce(ref _diagCount, "PartyPlayerJoined", parameters);
    }
}

/// <summary>
/// PartyPlayerLeft event (code 235) — someone left/was kicked.
/// Known layout: 0: ObjectId, 1: GUID.
/// </summary>
public class PartyPlayerLeftEvent
{
    public long ObjectId { get; }

    public PartyPlayerLeftEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var id))
            ObjectId = id.ObjectToLong() ?? 0;
    }
}

/// <summary>
/// PartyDisbanded event (code 232).
/// </summary>
public class PartyDisbandedEvent
{
    public PartyDisbandedEvent(Dictionary<byte, object> parameters)
    {
        // No fields needed — handler just clears party state.
    }
}

/// <summary>
/// Shared parsing/instrumentation helpers for party events.
/// </summary>
internal static class PartyEventParams
{
    /// <summary>
    /// Normalize a Photon-typed array (string[], object[], etc.) to string[].
    /// Photon deserializer materializes TYPED arrays — never assume object[].
    /// </summary>
    public static string[] AsStringArray(object? value)
    {
        if (value is not Array arr)
            return Array.Empty<string>();

        var result = new string[arr.Length];
        for (int i = 0; i < arr.Length; i++)
            result[i] = arr.GetValue(i)?.ToString() ?? string.Empty;
        return result;
    }

    /// <summary>
    /// Information-level dump of the full raw param set (first 5 occurrences per
    /// event type) so live repros verify our assumed layouts from ground truth.
    /// </summary>
    public static void DumpOnce(ref int counter, string eventName, Dictionary<byte, object> parameters)
    {
        if (counter >= 5)
            return;

        counter++;
        var dump = string.Join(" ", parameters
            .OrderBy(kv => kv.Key)
            .Select(kv => $"[{kv.Key}:{Describe(kv.Value)}]"));
        Log.Information("{Event} RAW: {Params}", eventName, dump);
    }

    private static string Describe(object? value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            Array arr when arr.GetType().GetElementType() == typeof(string)
                => "[" + string.Join(",", arr.Cast<object>().Select(o => o?.ToString())) + "]",
            Array arr => $"({arr.GetType().GetElementType()?.Name}[{arr.Length}])",
            _ => value.ToString() ?? "?"
        };
    }
}
