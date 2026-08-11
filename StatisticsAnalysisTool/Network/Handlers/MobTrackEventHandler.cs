using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Linq;
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
            // Param 2 sometimes contains a numeric field (e.g. 255) rather than
            // a real name — discard purely numeric "names" so fallbacks kick in
            var name = value.Name;
            if (!string.IsNullOrEmpty(name) && name.All(char.IsDigit))
                name = string.Empty;

            _entityTracker.AddMob(
                value.ObjectId,
                value.MobId,
                name,
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
