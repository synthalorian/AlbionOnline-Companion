using Serilog;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Tracks player names from NewCharacter events.
/// Feeds name lookups to other handlers.
/// </summary>
public class PlayerNameEventHandler : EventPacketHandler<NewCharacterEvent>
{
    private static readonly Dictionary<long, string> _playerNames = new();
    private static long _localPlayerId;
    private static string _localPlayerName = string.Empty;

    public static long LocalPlayerId => _localPlayerId;
    public static string LocalPlayerName => _localPlayerName;

    public PlayerNameEventHandler()
        : base((int)EventCodes.NewCharacter)
    {
    }

    protected override Task OnActionAsync(NewCharacterEvent value)
    {
        try
        {
            _playerNames[value.ObjectId] = value.Name;

            // First character seen is likely the local player
            if (_localPlayerId == 0 && !string.IsNullOrEmpty(value.Name))
            {
                _localPlayerId = value.ObjectId;
                _localPlayerName = value.Name;
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "PlayerNameEventHandler error");
        }

        return Task.CompletedTask;
    }

    public static string GetName(long objectId)
    {
        return _playerNames.TryGetValue(objectId, out var name) ? name : $"Player_{objectId}";
    }

    public static void Reset()
    {
        _playerNames.Clear();
        _localPlayerId = 0;
        _localPlayerName = string.Empty;
    }
}
