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
    private readonly Dictionary<long, DamageMeterEntry> _entries = new();
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

        try
        {
            // Only track damage caused by players
            if (value.CauserId <= 0 || value.AffectedObjectId <= 0)
                return Task.CompletedTask;

            // Skip self-damage
            if (value.CauserId == value.AffectedObjectId)
                return Task.CompletedTask;

            var entry = GetOrCreateEntry(value.CauserId);

            if (value.IsDamage)
            {
                entry.Damage += value.DamageAmount;
                entry.Dps = CalculateDps(entry);
            }
            else if (value.IsHealing)
            {
                entry.Healing += value.HealingAmount;
                entry.Hps = CalculateHps(entry);
            }

            entry.ValueString = FormatValue(entry);
            UpdateRankings();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "DamageMeterEventHandler error");
        }

        return Task.CompletedTask;
    }

    private DamageMeterEntry GetOrCreateEntry(long causerId)
    {
        if (!_entries.TryGetValue(causerId, out var entry))
        {
            entry = new DamageMeterEntry
            {
                PlayerName = _playerNames.TryGetValue(causerId, out var name) ? name : $"Player_{causerId}",
                Damage = 0,
                Dps = 0,
                Healing = 0,
                Hps = 0
            };
            _entries[causerId] = entry;
        }
        return entry;
    }

    private double CalculateDps(DamageMeterEntry entry)
    {
        // Simplified DPS calculation - would need time window in real implementation
        return entry.Damage / Math.Max(1, _viewModel.DamageMeterEntries.Count);
    }

    private double CalculateHps(DamageMeterEntry entry)
    {
        return entry.Healing / Math.Max(1, _viewModel.DamageMeterEntries.Count);
    }

    private string FormatValue(DamageMeterEntry entry)
    {
        return _viewModel.SelectedSortOption switch
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

    private void UpdateRankings()
    {
        var sorted = _viewModel.SelectedSortOption switch
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

        // Update the observable collection on UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.DamageMeterEntries.Clear();
            foreach (var entry in sorted.Take(20)) // Top 20
            {
                _viewModel.DamageMeterEntries.Add(entry);
            }
        });
    }
}
