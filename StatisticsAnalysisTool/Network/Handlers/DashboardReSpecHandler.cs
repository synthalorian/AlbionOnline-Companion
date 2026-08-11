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
        Serilog.Log.Information("UpdateReSpecPoints: gained={Gained} total={Total} paidSilver={Silver}",
            value.GainedReSpec.DoubleValue, value.TotalReSpec.DoubleValue, value.PaidSilver.DoubleValue);

        _dashboardHandler.OnReSpecGained(value.GainedReSpec.DoubleValue);
        if (value.TotalReSpec.InternalValue > 0)
            _dashboardHandler.OnReSpecTotalUpdated(value.TotalReSpec.DoubleValue);
        return Task.CompletedTask;
    }
}
