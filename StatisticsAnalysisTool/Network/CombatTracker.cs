using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Tracks combat sessions — groups of damage/healing events that form a fight.
/// Provides DPS windows, fight summaries, and combat detection.
/// </summary>
public class CombatTracker
{
    private readonly List<CombatSession> _sessions = new();
    private CombatSession? _currentSession;
    private readonly TimeSpan _combatTimeout = TimeSpan.FromSeconds(5);

    public static CombatTracker Instance { get; } = new();

    private CombatTracker() { }

    public bool IsInCombat => _currentSession != null;
    public CombatSession? CurrentSession => _currentSession;
    public IReadOnlyList<CombatSession> Sessions => _sessions;
    public int TotalFights => _sessions.Count;

    public event EventHandler<CombatSession>? CombatStarted;
    public event EventHandler<CombatSession>? CombatEnded;

    /// <summary>
    /// Process a health update event — starts/ends combat sessions automatically.
    /// </summary>
    public void ProcessHealthUpdate(HealthUpdateEvent evt)
    {
        var now = DateTime.UtcNow;

        // Check if current combat has timed out
        if (_currentSession != null && now - _currentSession.LastEventAt > _combatTimeout)
        {
            EndCombat();
        }

        // Start new combat if needed
        if (_currentSession == null)
        {
            _currentSession = new CombatSession
            {
                StartedAt = now
            };

            CombatStarted?.Invoke(this, _currentSession);
            Log.Debug("Combat started");
        }

        _currentSession.LastEventAt = now;

        // Track damage
        if (evt.IsDamage)
        {
            _currentSession.TotalDamage += evt.DamageAmount;

            if (!_currentSession.DamageByPlayer.ContainsKey(evt.CauserId))
                _currentSession.DamageByPlayer[evt.CauserId] = 0;
            _currentSession.DamageByPlayer[evt.CauserId] += evt.DamageAmount;

            if (!_currentSession.DamageToTarget.ContainsKey(evt.AffectedObjectId))
                _currentSession.DamageToTarget[evt.AffectedObjectId] = 0;
            _currentSession.DamageToTarget[evt.AffectedObjectId] += evt.DamageAmount;
        }

        // Track healing
        if (evt.IsHealing)
        {
            _currentSession.TotalHealing += evt.HealingAmount;

            if (!_currentSession.HealingByPlayer.ContainsKey(evt.CauserId))
                _currentSession.HealingByPlayer[evt.CauserId] = 0;
            _currentSession.HealingByPlayer[evt.CauserId] += evt.HealingAmount;
        }

        _currentSession.EventCount++;
    }

    /// <summary>
    /// Force end the current combat session.
    /// </summary>
    public void EndCombat()
    {
        if (_currentSession == null) return;

        _currentSession.EndedAt = DateTime.UtcNow;
        _currentSession.Duration = _currentSession.EndedAt.Value - _currentSession.StartedAt;

        // Calculate DPS/HPS for the session
        var seconds = Math.Max(1, _currentSession.Duration.TotalSeconds);
        _currentSession.OverallDps = _currentSession.TotalDamage / seconds;
        _currentSession.OverallHps = _currentSession.TotalHealing / seconds;

        _sessions.Add(_currentSession);

        Log.Debug("Combat ended: {Duration}s, {Damage} dmg, {Healing} heal, {Events} events",
            _currentSession.Duration.TotalSeconds,
            _currentSession.TotalDamage,
            _currentSession.TotalHealing,
            _currentSession.EventCount);

        CombatEnded?.Invoke(this, _currentSession);
        _currentSession = null;
    }

    /// <summary>
    /// Get the top damager in the current session.
    /// </summary>
    public (long PlayerId, double Damage) GetTopDamager()
    {
        if (_currentSession == null || _currentSession.DamageByPlayer.Count == 0)
            return (0, 0);

        var top = _currentSession.DamageByPlayer.OrderByDescending(kvp => kvp.Value).First();
        return (top.Key, top.Value);
    }

    /// <summary>
    /// Get the top healer in the current session.
    /// </summary>
    public (long PlayerId, double Healing) GetTopHealer()
    {
        if (_currentSession == null || _currentSession.HealingByPlayer.Count == 0)
            return (0, 0);

        var top = _currentSession.HealingByPlayer.OrderByDescending(kvp => kvp.Value).First();
        return (top.Key, top.Value);
    }

    /// <summary>
    /// Reset all combat data.
    /// </summary>
    public void Reset()
    {
        _currentSession = null;
        _sessions.Clear();
    }
}

public class CombatSession
{
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime LastEventAt { get; set; }
    public TimeSpan Duration { get; set; }
    public double TotalDamage { get; set; }
    public double TotalHealing { get; set; }
    public double OverallDps { get; set; }
    public double OverallHps { get; set; }
    public int EventCount { get; set; }
    public Dictionary<long, double> DamageByPlayer { get; } = new();
    public Dictionary<long, double> DamageToTarget { get; } = new();
    public Dictionary<long, double> HealingByPlayer { get; } = new();
}
