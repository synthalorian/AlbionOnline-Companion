using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

public class DashboardReSpecHandler : EventPacketHandler<UpdateReSpecPointsEvent>
{
    private readonly DashboardEventHandler _dashboardHandler;

    public DashboardReSpecHandler(DashboardViewModel viewModel, DashboardEventHandler dashboardHandler)
        : base((int)EventCodes.UpdateReSpecPoints)
    {
        _dashboardHandler = dashboardHandler;
    }

    protected override Task OnActionAsync(UpdateReSpecPointsEvent value)
    {
        _dashboardHandler.OnReSpecGained(value.GainedReSpec.DoubleValue);
        return Task.CompletedTask;
    }
}
