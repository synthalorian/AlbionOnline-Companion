using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles batch HealthUpdates events (multiple health changes in one packet).
/// Processes each update individually through the damage meter logic.
/// </summary>
public class DamageMeterBatchHandler : EventPacketHandler<HealthUpdatesEvent>
{
    private readonly DamageMeterViewModel _viewModel;

    public DamageMeterBatchHandler(DamageMeterViewModel viewModel)
        : base((int)EventCodes.HealthUpdates)
    {
        _viewModel = viewModel;
    }

    protected override Task OnActionAsync(HealthUpdatesEvent value)
    {
        if (!_viewModel.IsDamageMeterActive)
            return Task.CompletedTask;

        foreach (var update in value.Updates)
        {
            try
            {
                DamageMeterEventHandler.ProcessHealthUpdate(_viewModel, update);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "DamageMeterBatchHandler error processing update");
            }
        }

        return Task.CompletedTask;
    }
}
