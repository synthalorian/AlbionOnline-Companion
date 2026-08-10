using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified InCombatStateUpdate event for Linux port.
/// </summary>
public class InCombatStateUpdateEvent
{
    public long ObjectId { get; }
    public bool PlayerHitsEnemy { get; }
    public bool EnemyHitsPlayer { get; }
    public bool IsInCombat => PlayerHitsEnemy || EnemyHitsPlayer;

    public InCombatStateUpdateEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var playerHitsEnemy))
            PlayerHitsEnemy = playerHitsEnemy.ObjectToBool();

        if (parameters.TryGetValue(2, out var enemyHitsPlayer))
            EnemyHitsPlayer = enemyHitsPlayer.ObjectToBool();
    }
}
