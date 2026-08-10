using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// Simplified NewCharacter event for Linux port.
/// </summary>
public class NewCharacterEvent
{
    public long ObjectId { get; }
    public string Name { get; } = string.Empty;
    public string Guild { get; } = string.Empty;
    public string Alliance { get; } = string.Empty;
    public int[] Equipment { get; } = Array.Empty<int>();
    public int[] Spells { get; } = Array.Empty<int>();

    public NewCharacterEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var objectId))
            ObjectId = objectId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var name))
            Name = name.ObjectToString();

        if (parameters.TryGetValue(2, out var guild))
            Guild = guild.ObjectToString();

        if (parameters.TryGetValue(3, out var alliance))
            Alliance = alliance.ObjectToString();

        if (parameters.TryGetValue(8, out var equipment) && equipment is object[] equipArray)
        {
            Equipment = new int[equipArray.Length];
            for (int i = 0; i < equipArray.Length; i++)
                Equipment[i] = equipArray[i].ObjectToInt();
        }

        if (parameters.TryGetValue(9, out var spells) && spells is object[] spellArray)
        {
            Spells = new int[spellArray.Length];
            for (int i = 0; i < spellArray.Length; i++)
                Spells[i] = spellArray[i].ObjectToInt();
        }
    }
}
