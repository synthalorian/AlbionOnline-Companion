using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles UpdateMoney events for dashboard silver tracking.
/// </summary>
public class DashboardSilverHandler : EventPacketHandler<UpdateMoneyEvent>
{
    private readonly DashboardEventHandler _dashboardHandler;

    public DashboardSilverHandler(DashboardViewModel viewModel, DashboardEventHandler dashboardHandler)
        : base((int)EventCodes.UpdateMoney)
    {
        _dashboardHandler = dashboardHandler;
    }

    protected override Task OnActionAsync(UpdateMoneyEvent value)
    {
        _dashboardHandler.OnSilverGained(value.GainedSilver);
        return Task.CompletedTask;
    }
}
