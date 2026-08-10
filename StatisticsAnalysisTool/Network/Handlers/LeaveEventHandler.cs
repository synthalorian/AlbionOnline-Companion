using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles entity leave events — removes them from tracking.
/// </summary>
public class LeaveEventHandler : EventPacketHandler<LeaveEvent>
{
    private readonly EntityTracker _entityTracker;

    public LeaveEventHandler()
        : base((int)EventCodes.Leave)
    {
        _entityTracker = EntityTracker.Instance;
    }

    protected override Task OnActionAsync(LeaveEvent value)
    {
        try
        {
            _entityTracker.RemoveEntity(value.ObjectId);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "LeaveEventHandler error");
        }

        return Task.CompletedTask;
    }
}
