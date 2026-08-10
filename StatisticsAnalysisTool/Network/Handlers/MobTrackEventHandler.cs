using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Tracks mob spawns from NewMob events.
/// </summary>
public class MobTrackEventHandler : EventPacketHandler<NewMobEvent>
{
    private readonly EntityTracker _entityTracker;

    public MobTrackEventHandler()
        : base((int)EventCodes.NewMob)
    {
        _entityTracker = EntityTracker.Instance;
    }

    protected override Task OnActionAsync(NewMobEvent value)
    {
        try
        {
            _entityTracker.AddMob(
                value.ObjectId,
                value.MobId,
                value.Name,
                value.Tier,
                value.Health,
                value.MaxHealth);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "MobTrackEventHandler error");
        }

        return Task.CompletedTask;
    }
}
