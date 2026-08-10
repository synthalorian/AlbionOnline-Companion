using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles stat update events and updates the DashboardViewModel.
/// Uses FixPoint values from the game protocol.
/// Separates fame into combat/gathering/crafting categories.
/// </summary>
public class DashboardEventHandler : EventPacketHandler<UpdateFameEvent>
{
    private readonly DashboardViewModel _viewModel;
    private readonly EntityTracker _entityTracker;

    // Session totals
    private double _sessionFame;
    private double _sessionSilver;
    private double _sessionReSpec;
    private double _sessionMight;
    private double _sessionFavor;
    private double _combatFame;
    private double _gatheringFame;
    private double _craftingFame;
    private DateTime _sessionStart = DateTime.UtcNow;

    // Last known totals (for delta calculation)
    private double _lastTotalFame;
    private double _lastTotalSilver;
    private bool _hasBaseline;
    private bool _hasSilverBaseline;

    public DashboardEventHandler(DashboardViewModel viewModel)
        : base((int)EventCodes.UpdateFame)
    {
        _viewModel = viewModel;
        _entityTracker = EntityTracker.Instance;
    }

    protected override Task OnActionAsync(UpdateFameEvent value)
    {
        try
        {
            var totalFame = value.TotalPlayerFame.DoubleValue;
            var gainedFame = value.TotalGainedFame;

            // Set baseline on first event
            if (!_hasBaseline && totalFame > 0)
            {
                _lastTotalFame = totalFame;
                _hasBaseline = true;

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    _viewModel.TotalFame = FormatNumber(totalFame);
                    _viewModel.PlayerName = _entityTracker.LocalPlayerName;
                });
                return Task.CompletedTask;
            }

            // Use TotalGainedFame from the event (already calculated correctly)
            if (gainedFame > 0)
            {
                _sessionFame += gainedFame;

                // Categorize fame
                // If SatchelFame > 0, it's crafting; if ZoneFame > 0, it's combat/gathering
                if (value.SatchelFame.DoubleValue > 0)
                {
                    _craftingFame += gainedFame;
                }
                else
                {
                    // Default to combat for now — gathering events have different signatures
                    _combatFame += gainedFame;
                }

                var hours = Math.Max(0.001, (DateTime.UtcNow - _sessionStart).TotalHours);

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    _viewModel.TotalFame = FormatNumber(totalFame);
                    _viewModel.SessionFame = FormatNumber(_sessionFame);
                    _viewModel.FamePerHour = FormatNumber(_sessionFame / hours) + " /h";
                    _viewModel.CombatFame = FormatNumber(_combatFame);
                    _viewModel.CombatFamePerHour = FormatNumber(_combatFame / hours) + " /h";
                    _viewModel.GatheringFame = FormatNumber(_gatheringFame);
                    _viewModel.GatheringFamePerHour = FormatNumber(_gatheringFame / hours) + " /h";
                    _viewModel.CraftingFame = FormatNumber(_craftingFame);
                    _viewModel.CraftingFamePerHour = FormatNumber(_craftingFame / hours) + " /h";
                    _viewModel.PlayerName = _entityTracker.LocalPlayerName;
                    _viewModel.UpdateSessionDuration();
                });
            }

            _lastTotalFame = totalFame;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "DashboardEventHandler fame error");
        }

        return Task.CompletedTask;
    }

    public void OnSilverGained(double gainedSilver)
    {
        if (gainedSilver <= 0) return;

        _sessionSilver += gainedSilver;
        var hours = Math.Max(0.001, (DateTime.UtcNow - _sessionStart).TotalHours);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.SessionSilver = FormatNumber(_sessionSilver);
            _viewModel.SilverPerHour = FormatNumber(_sessionSilver / hours) + " /h";
        });
    }

    public void OnSilverTotalUpdated(double totalSilver)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.TotalSilver = FormatNumber(totalSilver);
        });
    }

    public void OnReSpecGained(double gainedReSpec)
    {
        if (gainedReSpec <= 0) return;

        _sessionReSpec += gainedReSpec;
        var hours = Math.Max(0.001, (DateTime.UtcNow - _sessionStart).TotalHours);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.SessionReSpec = FormatNumber(_sessionReSpec);
            _viewModel.ReSpecPerHour = FormatNumber(_sessionReSpec / hours) + " /h";
        });
    }

    public void OnMightFavorGained(double might, double favor)
    {
        _sessionMight += might;
        _sessionFavor += favor;
        var hours = Math.Max(0.001, (DateTime.UtcNow - _sessionStart).TotalHours);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.SessionMight = FormatNumber(_sessionMight);
            _viewModel.MightPerHour = FormatNumber(_sessionMight / hours) + " /h";
            _viewModel.SessionFavor = FormatNumber(_sessionFavor);
            _viewModel.FavorPerHour = FormatNumber(_sessionFavor / hours) + " /h";
        });
    }

    public void AddGatheringFame(double fame)
    {
        _gatheringFame += fame;
        var hours = Math.Max(0.001, (DateTime.UtcNow - _sessionStart).TotalHours);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.GatheringFame = FormatNumber(_gatheringFame);
            _viewModel.GatheringFamePerHour = FormatNumber(_gatheringFame / hours) + " /h";
        });
    }

    private static string FormatNumber(double value)
    {
        return value switch
        {
            >= 1_000_000_000 => $"{value / 1_000_000_000:F2}B",
            >= 1_000_000 => $"{value / 1_000_000:F1}M",
            >= 1_000 => $"{value / 1_000:F1}K",
            _ => $"{value:F0}"
        };
    }

    /// <summary>
    /// Set the silver baseline from JoinResponse.
    /// </summary>
    public void SetSilverBaseline(double totalSilver)
    {
        _lastTotalSilver = totalSilver;
        _hasSilverBaseline = true;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.TotalSilver = FormatNumber(totalSilver);
        });
    }

    /// <summary>
    /// Set the fame baseline from JoinResponse or first UpdateFame event.
    /// </summary>
    public void SetFameBaseline(double totalFame)
    {
        _lastTotalFame = totalFame;
        _hasBaseline = true;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.TotalFame = FormatNumber(totalFame);
        });
    }

    public void ResetSession()
    {
        _sessionFame = _sessionSilver = _sessionReSpec = _sessionMight = _sessionFavor = 0;
        _combatFame = _gatheringFame = _craftingFame = 0;
        _sessionStart = DateTime.UtcNow;
        _hasBaseline = false;
    }
}
