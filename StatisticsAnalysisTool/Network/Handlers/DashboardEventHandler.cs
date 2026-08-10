using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles stat update events and updates the DashboardViewModel.
/// </summary>
public class DashboardEventHandler : 
    EventPacketHandler<UpdateFameEvent>,
    IPacketHandler
{
    private readonly DashboardViewModel _viewModel;
    private double _sessionFame;
    private double _sessionSilver;
    private double _sessionReSpec;
    private double _sessionMight;
    private double _sessionFavor;
    private DateTime _sessionStart = DateTime.UtcNow;

    public DashboardEventHandler(DashboardViewModel viewModel) 
        : base((int)EventCodes.UpdateFame)
    {
        _viewModel = viewModel;
    }

    protected override Task OnActionAsync(UpdateFameEvent value)
    {
        try
        {
            _sessionFame += value.GainedFame;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _viewModel.TotalGainedFameInSession = FormatNumber(_sessionFame);
                _viewModel.FamePerHour = CalculatePerHour(_sessionFame) + " /h";
            });
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "DashboardEventHandler fame error");
        }

        return Task.CompletedTask;
    }

    public void OnSilverGained(double silver)
    {
        _sessionSilver += silver;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.TotalGainedSilverInSession = FormatNumber(_sessionSilver);
            _viewModel.SilverPerHour = CalculatePerHour(_sessionSilver) + " /h";
        });
    }

    public void OnReSpecGained(double respec)
    {
        _sessionReSpec += respec;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.TotalGainedReSpecPointsInSession = FormatNumber(_sessionReSpec);
            _viewModel.ReSpecPointsPerHour = CalculatePerHour(_sessionReSpec) + " /h";
        });
    }

    public void OnMightFavorGained(double might, double favor)
    {
        _sessionMight += might;
        _sessionFavor += favor;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.TotalGainedMightInSession = FormatNumber(_sessionMight);
            _viewModel.MightPerHour = CalculatePerHour(_sessionMight) + " /h";
            _viewModel.TotalGainedFavorInSession = FormatNumber(_sessionFavor);
            _viewModel.FavorPerHour = CalculatePerHour(_sessionFavor) + " /h";
        });
    }

    private string CalculatePerHour(double value)
    {
        var hours = (DateTime.UtcNow - _sessionStart).TotalHours;
        if (hours < 0.001) hours = 0.001;
        return FormatNumber(value / hours);
    }

    private static string FormatNumber(double value)
    {
        return value switch
        {
            >= 1_000_000 => $"{value / 1_000_000:F1}M",
            >= 1_000 => $"{value / 1_000:F1}K",
            _ => $"{value:F0}"
        };
    }

    public void ResetSession()
    {
        _sessionFame = 0;
        _sessionSilver = 0;
        _sessionReSpec = 0;
        _sessionMight = 0;
        _sessionFavor = 0;
        _sessionStart = DateTime.UtcNow;
    }

}
