using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// PartyJoined (231) — caches the full party roster by name. Names from this
/// event don't carry in-world ObjectIds, but they let us recognize party
/// members when they later spawn via NewCharacter or join via PartyPlayerJoined.
/// </summary>
public class PartyJoinedEventHandler : EventPacketHandler<PartyJoinedEvent>
{
    public PartyJoinedEventHandler()
        : base((int)EventCodes.PartyJoined)
    {
    }

    protected override Task OnActionAsync(PartyJoinedEvent value)
    {
        try
        {
            // Verified 2026-08-11: the real event often carries an EMPTY name
            // array — the roster actually arrives one member at a time via
            // PartyPlayerJoined. Never wipe the roster on an empty payload.
            if (value.MemberNames.Length > 0)
            {
                PartyTracker.Instance.SetRoster(value.MemberNames);
                Log.Information("Party joined: {Count} members ({Names})",
                    value.MemberNames.Length, string.Join(", ", value.MemberNames));
            }
            else
            {
                Log.Information("Party joined (empty roster payload — members arrive via PartyPlayerJoined)");
            }
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "PartyJoinedEventHandler error");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// PartyPlayerJoined (233) — carries the member NAME (param 2). Param 0 is the
/// PARTY id (constant across members — verified 2026-08-11: all roster members
/// reported ObjectId:27562), NOT the member's in-world ObjectId, so it cannot
/// feed EntityTracker. Member ObjectIds resolve via NewCharacter when they're
/// in range; here we only maintain the name roster.
/// </summary>
public class PartyPlayerJoinedEventHandler : EventPacketHandler<PartyPlayerJoinedEvent>
{
    public PartyPlayerJoinedEventHandler()
        : base((int)EventCodes.PartyPlayerJoined)
    {
    }

    protected override Task OnActionAsync(PartyPlayerJoinedEvent value)
    {
        try
        {
            if (!string.IsNullOrEmpty(value.Name))
            {
                PartyTracker.Instance.AddMember(value.Name);
                Log.Information("Party member joined: {Name}", value.Name);
            }
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "PartyPlayerJoinedEventHandler error");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// PartyPlayerLeft (235) / PartyDisbanded (232) — keep the roster current.
/// We deliberately do NOT remove the entity from EntityTracker: the player is
/// still in the world and may still appear in the damage meter / kill feed.
/// </summary>
public class PartyPlayerLeftEventHandler : EventPacketHandler<PartyPlayerLeftEvent>
{
    public PartyPlayerLeftEventHandler()
        : base((int)EventCodes.PartyPlayerLeft)
    {
    }

    protected override Task OnActionAsync(PartyPlayerLeftEvent value)
    {
        try
        {
            // Param 0 here is the PARTY id (same as PartyJoined/PartyPlayerJoined),
            // not the departing member — the event carries no member identifier
            // we can resolve, so the roster entry stays until the next roster
            // event or PartyDisbanded. Log for diagnostics only.
            Log.Information("PartyPlayerLeft event (party:{PartyId}) — member not identifiable from payload", value.ObjectId);
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "PartyPlayerLeftEventHandler error");
        }

        return Task.CompletedTask;
    }
}

public class PartyDisbandedEventHandler : EventPacketHandler<PartyDisbandedEvent>
{
    public PartyDisbandedEventHandler()
        : base((int)EventCodes.PartyDisbanded)
    {
    }

    protected override Task OnActionAsync(PartyDisbandedEvent value)
    {
        try
        {
            PartyTracker.Instance.Clear();
            Log.Information("Party disbanded");
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "PartyDisbandedEventHandler error");
        }

        return Task.CompletedTask;
    }
}
