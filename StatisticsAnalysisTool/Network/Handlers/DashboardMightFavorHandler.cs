using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles MightAndFavorReceived events for dashboard.
/// </summary>
public class DashboardMightFavorHandler : EventPacketHandler<MightAndFavorReceivedEvent>
{
    private readonly DashboardEventHandler _dashboardHandler;

    public DashboardMightFavorHandler(DashboardViewModel viewModel, DashboardEventHandler dashboardHandler)
        : base((int)EventCodes.MightAndFavorReceivedEvent)
    {
        _dashboardHandler = dashboardHandler;
    }

    protected override Task OnActionAsync(MightAndFavorReceivedEvent value)
    {
        _dashboardHandler.OnMightFavorGained(value.Might, value.Favor);
        return Task.CompletedTask;
    }
}
