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

        // Process for damage meter
        ProcessHealthUpdate(_viewModel, value, _entityTracker);
        return Task.CompletedTask;
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

            // Update entity health
            entityTracker?.UpdateHealth(value.AffectedObjectId, value.NewHealthValue);

            var entry = GetOrCreateEntry(value.CauserId, entityTracker);

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

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            viewModel.DamageMeterEntries.Clear();
            foreach (var entry in sorted.Take(20))
            {
                viewModel.DamageMeterEntries.Add(entry);
            }
        });
    }

    public static void ResetEntries()
    {
        _entries.Clear();
        _sessionStart = DateTime.UtcNow;
    }
}
