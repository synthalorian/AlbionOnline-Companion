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
/// </summary>
public class DamageMeterEventHandler : EventPacketHandler<HealthUpdateEvent>
{
    private readonly DamageMeterViewModel _viewModel;
    private readonly Dictionary<long, string> _playerNames = new();
    private static readonly Dictionary<long, DamageMeterEntry> _entries = new();
    private static DateTime _combatStart = DateTime.UtcNow;
    private long _localPlayerId;

    public DamageMeterEventHandler(DamageMeterViewModel viewModel)
        : base((int)EventCodes.HealthUpdate)
    {
        _viewModel = viewModel;
    }

    public void SetLocalPlayerId(long playerId)
    {
        _localPlayerId = playerId;
    }

    public void RegisterPlayerName(long objectId, string name)
    {
        _playerNames[objectId] = name;
    }

    protected override Task OnActionAsync(HealthUpdateEvent value)
    {
        if (!_viewModel.IsDamageMeterActive)
            return Task.CompletedTask;

        ProcessHealthUpdate(_viewModel, value, _playerNames);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Static processing method that can be called from batch handler too.
    /// </summary>
    public static void ProcessHealthUpdate(
        DamageMeterViewModel viewModel,
        HealthUpdateEvent value,
        Dictionary<long, string>? playerNames = null)
    {
        try
        {
            // Only track damage caused by players
            if (value.CauserId <= 0 || value.AffectedObjectId <= 0)
                return;

            // Skip self-damage
            if (value.CauserId == value.AffectedObjectId)
                return;

            var entry = GetOrCreateEntry(value.CauserId, playerNames);

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

    private static DamageMeterEntry GetOrCreateEntry(long causerId, Dictionary<long, string>? playerNames)
    {
        if (!_entries.TryGetValue(causerId, out var entry))
        {
            var name = playerNames?.TryGetValue(causerId, out var n) == true
                ? n
                : $"Player_{causerId}";

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
        var elapsed = (DateTime.UtcNow - _combatStart).TotalSeconds;
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
                _viewModelRef?.DamageMeterEntries.Add(entry);
            }
        });
    }

    // Keep a static ref for the static UpdateRankings
    private static DamageMeterViewModel? _viewModelRef;

    public static void SetViewModelRef(DamageMeterViewModel vm)
    {
        _viewModelRef = vm;
    }

    /// <summary>
    /// Reset all tracked damage data.
    /// </summary>
    public static void ResetEntries()
    {
        _entries.Clear();
        _combatStart = DateTime.UtcNow;
    }
}
