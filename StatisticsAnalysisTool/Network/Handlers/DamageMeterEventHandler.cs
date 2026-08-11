using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles damage/healing events and updates the DamageMeterViewModel.
/// Uses EntityTracker for name resolution and CombatTracker for session management.
/// </summary>
public class DamageMeterEventHandler : EventPacketHandler<HealthUpdateEvent>
{
    private readonly DamageMeterViewModel _viewModel;
    private readonly EntityTracker _entityTracker;
    private readonly CombatTracker _combatTracker;
    private static readonly Dictionary<long, DamageMeterEntry> _entries = new();
    private static DateTime _sessionStart = DateTime.UtcNow;

    public DamageMeterEventHandler(DamageMeterViewModel viewModel)
        : base((int)EventCodes.HealthUpdate)
    {
        _viewModel = viewModel;
        _entityTracker = EntityTracker.Instance;
        _combatTracker = CombatTracker.Instance;
    }

    protected override Task OnActionAsync(HealthUpdateEvent value)
    {
        if (!_viewModel.IsDamageMeterActive)
            return Task.CompletedTask;

        // Feed to combat tracker
        _combatTracker.ProcessHealthUpdate(value);

        // Diagnostic: log first 5 health updates so live repros are verifiable
        if (_diagCount < 5)
        {
            _diagCount++;
            Log.Information("HealthUpdate: causer={Causer} affected={Affected} change={Change} dmg={Dmg} heal={Heal}",
                value.CauserId, value.AffectedObjectId, value.HealthChange, value.DamageAmount, value.HealingAmount);
        }

        // Process for damage meter
        ProcessHealthUpdate(_viewModel, value, _entityTracker);
        return Task.CompletedTask;
    }

    private static int _diagCount;

    // Dedupe: Albion sends each health update twice — once as single
    // HealthUpdate (event 6) and once inside the HealthUpdates batch (event 7).
    private static readonly HashSet<string> _recentUpdates = new();
    private static readonly object _dedupeLock = new();

    private static bool IsDuplicate(HealthUpdateEvent value)
    {
        var key = $"{value.CauserId}:{value.AffectedObjectId}:{value.TimeStamp.Value}:{value.HealthChange}";
        lock (_dedupeLock)
        {
            if (!_recentUpdates.Add(key))
                return true;

            if (_recentUpdates.Count > 500)
                _recentUpdates.Clear();

            return false;
        }
    }

    /// <summary>
    /// Static processing method shared with batch handler.
    /// </summary>
    public static void ProcessHealthUpdate(
        DamageMeterViewModel viewModel,
        HealthUpdateEvent value,
        EntityTracker? entityTracker = null)
    {
        try
        {
            if (value.CauserId <= 0 || value.AffectedObjectId <= 0)
                return;

            if (value.CauserId == value.AffectedObjectId)
                return;

            if (IsDuplicate(value))
                return;

            // Update entity health
            entityTracker?.UpdateHealth(value.AffectedObjectId, value.NewHealthValue);

            var entry = GetOrCreateEntry(value.CauserId, entityTracker);

            // Names arrive via NewCharacter, sometimes AFTER the first damage
            // tick — keep retrying until the placeholder resolves to a real name
            if (entityTracker != null &&
                (entry.PlayerName.StartsWith("Unknown_") || entry.PlayerName.StartsWith("Player_")))
            {
                var resolved = entityTracker.GetName(value.CauserId);
                if (!resolved.StartsWith("Unknown_"))
                    entry.PlayerName = resolved;
            }

            if (value.IsDamage)
            {
                entry.Damage += value.DamageAmount;
                entry.Dps = CalculateRate(entry.Damage);
            }
            else if (value.IsHealing)
            {
                entry.Healing += value.HealingAmount;
                entry.Hps = CalculateRate(entry.Healing);
            }

            entry.ValueString = FormatValue(viewModel.SelectedSortOption, entry);
            UpdateRankings(viewModel);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "DamageMeterEventHandler error");
        }
    }

    private static DamageMeterEntry GetOrCreateEntry(long causerId, EntityTracker? entityTracker)
    {
        if (!_entries.TryGetValue(causerId, out var entry))
        {
            var name = entityTracker?.GetName(causerId) ?? $"Player_{causerId}";

            entry = new DamageMeterEntry
            {
                CauserId = causerId,
                PlayerName = name,
                Damage = 0,
                Dps = 0,
                Healing = 0,
                Hps = 0
            };
            _entries[causerId] = entry;
        }
        return entry;
    }

    private static double CalculateRate(double total)
    {
        var elapsed = (DateTime.UtcNow - _sessionStart).TotalSeconds;
        return elapsed > 0 ? total / elapsed : 0;
    }

    private static string FormatValue(string sortOption, DamageMeterEntry entry)
    {
        return sortOption switch
        {
            "Damage" => FormatNumber(entry.Damage),
            "DPS" => FormatNumber(entry.Dps),
            "Healing" => FormatNumber(entry.Healing),
            "HPS" => FormatNumber(entry.Hps),
            _ => FormatNumber(entry.Damage)
        };
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

    private static void UpdateRankings(DamageMeterViewModel viewModel)
    {
        var sorted = viewModel.SelectedSortOption switch
        {
            "Damage" => _entries.Values.OrderByDescending(e => e.Damage),
            "DPS" => _entries.Values.OrderByDescending(e => e.Dps),
            "Healing" => _entries.Values.OrderByDescending(e => e.Healing),
            "HPS" => _entries.Values.OrderByDescending(e => e.Hps),
            _ => _entries.Values.OrderByDescending(e => e.Damage)
        };

        int rank = 1;
        foreach (var entry in sorted)
        {
            entry.Rank = rank++;
        }

        // YOUR STATS: local player's own entry (LocalPlayerId from JoinResponse)
        var localId = EntityTracker.Instance.LocalPlayerId;
        var yourEntry = localId > 0 && _entries.TryGetValue(localId, out var you) ? you : null;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            viewModel.DamageMeterEntries.Clear();
            foreach (var entry in sorted.Take(20))
            {
                viewModel.DamageMeterEntries.Add(entry);
            }

            if (yourEntry != null)
            {
                viewModel.HasYourStats = true;
                viewModel.YourName = yourEntry.PlayerName;
                viewModel.YourRank = $"#{yourEntry.Rank} of {_entries.Count}";
                viewModel.YourDamage = FormatNumber(yourEntry.Damage);
                viewModel.YourDps = FormatNumber(yourEntry.Dps);
                viewModel.YourHealing = FormatNumber(yourEntry.Healing);
                viewModel.YourHps = FormatNumber(yourEntry.Hps);
            }
            else
            {
                viewModel.HasYourStats = false;
            }
        });
    }

    public static void ResetEntries()
    {
        _entries.Clear();
        _sessionStart = DateTime.UtcNow;
    }

    /// <summary>
    /// Recompute every entry's display value for the current sort option and
    /// re-rank the list. Called when the user changes the sort dropdown.
    /// </summary>
    public static void RefreshView(DamageMeterViewModel viewModel)
    {
        foreach (var entry in _entries.Values)
            entry.ValueString = FormatValue(viewModel.SelectedSortOption, entry);

        UpdateRankings(viewModel);
    }
}
