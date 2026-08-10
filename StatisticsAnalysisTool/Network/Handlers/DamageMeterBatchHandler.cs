using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles batch HealthUpdates events.
/// </summary>
public class DamageMeterBatchHandler : EventPacketHandler<HealthUpdatesEvent>
{
    private readonly DamageMeterViewModel _viewModel;
    private readonly EntityTracker _entityTracker;
    private readonly CombatTracker _combatTracker;

    public DamageMeterBatchHandler(DamageMeterViewModel viewModel)
        : base((int)EventCodes.HealthUpdates)
    {
        _viewModel = viewModel;
        _entityTracker = EntityTracker.Instance;
        _combatTracker = CombatTracker.Instance;
    }

    protected override Task OnActionAsync(HealthUpdatesEvent value)
    {
        if (!_viewModel.IsDamageMeterActive)
            return Task.CompletedTask;

        foreach (var update in value.Updates)
        {
            try
            {
                _combatTracker.ProcessHealthUpdate(update);
                DamageMeterEventHandler.ProcessHealthUpdate(_viewModel, update, _entityTracker);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "DamageMeterBatchHandler error");
            }
        }

        return Task.CompletedTask;
    }
}
