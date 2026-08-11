using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Tracks the current party roster by name. Names come from PartyJoined
/// (full roster) and PartyPlayerJoined/Left (deltas). Kept separate from
/// EntityTracker because party membership is account-level (names + GUIDs),
/// not in-world entity state.
/// </summary>
public class PartyTracker
{
    private readonly HashSet<string> _members = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public static PartyTracker Instance { get; } = new();

    private PartyTracker() { }

    public IReadOnlyCollection<string> Members
    {
        get
        {
            lock (_lock)
                return new List<string>(_members);
        }
    }

    public int MemberCount
    {
        get
        {
            lock (_lock)
                return _members.Count;
        }
    }

    public bool IsPartyMember(string name)
    {
        lock (_lock)
            return _members.Contains(name);
    }

    public void SetRoster(IEnumerable<string> names)
    {
        lock (_lock)
        {
            _members.Clear();
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    _members.Add(name);
            }
        }
    }

    public void AddMember(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;
        lock (_lock)
            _members.Add(name);
    }

    public void RemoveMember(string name)
    {
        lock (_lock)
            _members.Remove(name);
    }

    public void Clear()
    {
        lock (_lock)
            _members.Clear();
    }
}
