using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
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
    private double _farmingFame;
    private DateTime _sessionStart = DateTime.UtcNow;

    // Dedupe: fame events arrive doubled (single + batch delivery)
    private readonly HashSet<string> _recentFame = new();
    private DateTime _lastFamePrune = DateTime.UtcNow;

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
                // Dedupe doubled delivery (same event via two packet paths)
                var dedupeKey = $"{value.ObjectId}:{totalFame}:{gainedFame}";
                if (!_recentFame.Add(dedupeKey))
                {
                    _lastTotalFame = totalFame;
                    return Task.CompletedTask;
                }
                if (_recentFame.Count > 200 || (DateTime.UtcNow - _lastFamePrune).TotalSeconds > 30)
                {
                    _recentFame.Clear();
                    _recentFame.Add(dedupeKey);
                    _lastFamePrune = DateTime.UtcNow;
                }

                _sessionFame += gainedFame;

                // Ground-truth logging while we figure out how to distinguish
                // combat vs gathering fame
                var entityType = _entityTracker.GetEntity(value.ObjectId)?.Type.ToString() ?? "untracked";
                Log.Information("UpdateFame: objectId={Id} ({Type}) gained={Gained} zoneFame={Zone} satchel={Satchel} premium={Premium}",
                    value.ObjectId, entityType, gainedFame, value.ZoneFame.DoubleValue,
                    value.SatchelFame.DoubleValue, value.IsPremiumBonus);

                // Categorize fame (verified from live capture 2026-08):
                // - Mob kills arrive with ZoneFame > 0 → combat
                // - SatchelFame is the Satchel of Insight bonus riding along
                //   with any fame type — NOT a category of its own
                // - Island zones + no zone fame → crop/animal farming
                // - Everything else with no zone fame → gathering/crafting ticks
                if (value.ZoneFame.DoubleValue > 0)
                {
                    _combatFame += gainedFame;
                }
                else if (ClusterTracker.Instance.CurrentCluster.StartsWith("@ISLAND@", StringComparison.OrdinalIgnoreCase))
                {
                    _farmingFame += gainedFame;
                }
                else
                {
                    _gatheringFame += gainedFame;
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
                    _viewModel.FarmingFame = FormatNumber(_farmingFame);
                    _viewModel.FarmingFamePerHour = FormatNumber(_farmingFame / hours) + " /h";
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

    public void OnReSpecTotalUpdated(double totalReSpec)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.TotalReSpecPoints = FormatNumber(totalReSpec);
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
        _combatFame = _gatheringFame = _craftingFame = _farmingFame = 0;
        _sessionStart = DateTime.UtcNow;
        _hasBaseline = false;
    }
}
