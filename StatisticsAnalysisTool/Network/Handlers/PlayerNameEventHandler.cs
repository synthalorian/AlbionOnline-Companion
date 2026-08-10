using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Tracks player names from NewCharacter events.
/// Feeds the EntityTracker for name resolution across all handlers.
/// </summary>
public class PlayerNameEventHandler : EventPacketHandler<NewCharacterEvent>
{
    private readonly EntityTracker _entityTracker;

    public PlayerNameEventHandler()
        : base((int)EventCodes.NewCharacter)
    {
        _entityTracker = EntityTracker.Instance;
    }

    protected override Task OnActionAsync(NewCharacterEvent value)
    {
        try
        {
            _entityTracker.AddPlayer(
                value.ObjectId,
                value.Name,
                value.Guild,
                value.Alliance,
                value.Equipment);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "PlayerNameEventHandler error");
        }

        return Task.CompletedTask;
    }
}
