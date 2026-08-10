using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

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
        _dashboardHandler.OnSilverGained(value.GainedSilver.DoubleValue);
        _dashboardHandler.OnSilverTotalUpdated(value.CurrentSilver.DoubleValue);
        return Task.CompletedTask;
    }
}
