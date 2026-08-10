using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Tracks kills and deaths for the dashboard.
/// </summary>
public class KillDeathEventHandler : EventPacketHandler<DiedEvent>
{
    private readonly DashboardViewModel _viewModel;
    private static int _kills;
    private static int _deaths;
    private long _localPlayerId;

    public KillDeathEventHandler(DashboardViewModel viewModel)
        : base((int)EventCodes.Died)
    {
        _viewModel = viewModel;
    }

    public void SetLocalPlayerId(long id) => _localPlayerId = id;

    protected override Task OnActionAsync(DiedEvent value)
    {
        try
        {
            if (value.ObjectId == _localPlayerId)
            {
                _deaths++;
            }
            else if (value.KillerId == _localPlayerId)
            {
                _kills++;
            }

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _viewModel.KillsDeathsText = $"{_kills} / {_deaths}";
            });
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "KillDeathEventHandler error");
        }

        return Task.CompletedTask;
    }

    public static void Reset()
    {
        _kills = 0;
        _deaths = 0;
    }
}
