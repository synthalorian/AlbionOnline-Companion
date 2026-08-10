using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Tracks all entities (players, mobs) currently in range.
/// This is the foundation for damage meter, party tracking, kill feed, etc.
/// </summary>
public class EntityTracker
{
    private readonly ConcurrentDictionary<long, TrackedEntity> _entities = new();
    private readonly TimeSpan _staleTimeout = TimeSpan.FromMinutes(5);

    public static EntityTracker Instance { get; } = new();

    private EntityTracker() { }

    public int EntityCount => _entities.Count;
    public long LocalPlayerId { get; private set; }
    public string LocalPlayerName { get; private set; } = string.Empty;

    /// <summary>
    /// Register a new player entity.
    /// </summary>
    public void AddPlayer(long objectId, string name, string guild = "", string alliance = "", int[]? equipment = null)
    {
        var entity = _entities.GetOrAdd(objectId, _ => new TrackedEntity
        {
            ObjectId = objectId,
            Type = EntityType.Player
        });

        entity.Name = name;
        entity.Guild = guild;
        entity.Alliance = alliance;
        entity.Equipment = equipment ?? Array.Empty<int>();
        entity.LastSeen = DateTime.UtcNow;

        // First player with a name is likely the local player
        if (LocalPlayerId == 0 && !string.IsNullOrEmpty(name))
        {
            LocalPlayerId = objectId;
            LocalPlayerName = name;
            Log.Information("Local player detected: {Name} (ID: {Id})", name, objectId);
        }
    }

    /// <summary>
    /// Register a new mob entity.
    /// </summary>
    public void AddMob(long objectId, int mobId, string name, int tier, double health, double maxHealth)
    {
        var entity = _entities.GetOrAdd(objectId, _ => new TrackedEntity
        {
            ObjectId = objectId,
            Type = EntityType.Mob
        });

        entity.MobId = mobId;
        entity.Name = name;
        entity.Tier = tier;
        entity.Health = health;
        entity.MaxHealth = maxHealth;
        entity.LastSeen = DateTime.UtcNow;
    }

    /// <summary>
    /// Update entity health from HealthUpdate events.
    /// </summary>
    public void UpdateHealth(long objectId, double newHealth)
    {
        if (_entities.TryGetValue(objectId, out var entity))
        {
            entity.Health = newHealth;
            entity.LastSeen = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Remove entity (died, left range, etc).
    /// </summary>
    public void RemoveEntity(long objectId)
    {
        _entities.TryRemove(objectId, out _);
    }

    /// <summary>
    /// Get entity by ID.
    /// </summary>
    public TrackedEntity? GetEntity(long objectId)
    {
        return _entities.TryGetValue(objectId, out var entity) ? entity : null;
    }

    /// <summary>
    /// Get entity name, with fallback.
    /// </summary>
    public string GetName(long objectId)
    {
        if (_entities.TryGetValue(objectId, out var entity))
            return entity.Name;
        return $"Unknown_{objectId}";
    }

    /// <summary>
    /// Get all tracked players.
    /// </summary>
    public List<TrackedEntity> GetPlayers()
    {
        return _entities.Values.Where(e => e.Type == EntityType.Player).ToList();
    }

    /// <summary>
    /// Get all tracked mobs.
    /// </summary>
    public List<TrackedEntity> GetMobs()
    {
        return _entities.Values.Where(e => e.Type == EntityType.Mob).ToList();
    }

    /// <summary>
    /// Check if an entity is a player.
    /// </summary>
    public bool IsPlayer(long objectId)
    {
        return _entities.TryGetValue(objectId, out var entity) && entity.Type == EntityType.Player;
    }

    /// <summary>
    /// Check if an entity is the local player.
    /// </summary>
    public bool IsLocalPlayer(long objectId)
    {
        return objectId == LocalPlayerId;
    }

    /// <summary>
    /// Remove stale entities not seen recently.
    /// </summary>
    public void PruneStale()
    {
        var cutoff = DateTime.UtcNow - _staleTimeout;
        var staleKeys = _entities
            .Where(kvp => kvp.Value.LastSeen < cutoff && kvp.Key != LocalPlayerId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in staleKeys)
        {
            _entities.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Clear all tracked entities (on zone change, etc).
    /// </summary>
    public void Clear()
    {
        var localId = LocalPlayerId;
        var localName = LocalPlayerName;
        _entities.Clear();
        LocalPlayerId = localId;
        LocalPlayerName = localName;
        Log.Debug("EntityTracker cleared ({LocalPlayer} preserved)", localName);
    }
}

public class TrackedEntity
{
    public long ObjectId { get; set; }
    public EntityType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Guild { get; set; } = string.Empty;
    public string Alliance { get; set; } = string.Empty;
    public int[] Equipment { get; set; } = Array.Empty<int>();
    public int MobId { get; set; }
    public int Tier { get; set; }
    public double Health { get; set; }
    public double MaxHealth { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public bool IsAlive => Health > 0;
    public double HealthPercent => MaxHealth > 0 ? Health / MaxHealth * 100 : 0;
}

public enum EntityType
{
    Player,
    Mob,
    Mount,
    Building,
    Resource,
    Chest,
    Unknown
}
