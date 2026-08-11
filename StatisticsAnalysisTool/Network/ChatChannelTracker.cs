using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Tracks chat channel IDs → channel type mapping.
/// Uses hardcoded Albion channel IDs + JoinedChatChannel events.
/// </summary>
public class ChatChannelTracker
{
    private readonly ConcurrentDictionary<long, ChatChannelInfo> _channels = new();

    public static ChatChannelTracker Instance { get; } = new();

    private ChatChannelTracker()
    {
        // Pre-populate known Albion channel IDs
        InitializeKnownChannels();
    }

    public int ChannelCount => _channels.Count;

    private void InitializeKnownChannels()
    {
        // Known Albion Online channel IDs (from albion-network-lib and community research)
        var knownChannels = new Dictionary<long, (ChatChannelType Type, string Name)>
        {
            // Global channels
            { 0, (ChatChannelType.Say, "Local") },
            { 1, (ChatChannelType.Global, "Global") },
            { 2, (ChatChannelType.Trade, "Trade") },
            // Verified from live capture 2026-08: 18 = Recruitment ("RECLUTA" spam),
            // 19 = LFG ("busco party/team"). Old guesses 3/4 were wrong.
            { 18, (ChatChannelType.Recruitment, "Recruitment") },
            { 19, (ChatChannelType.LFG, "LFG") },
            
            // Faction channels (city-specific)
            { 1856, (ChatChannelType.Faction, "Martlock") },
            { 1857, (ChatChannelType.Faction, "Bridgewatch") },
            { 1858, (ChatChannelType.Faction, "Lymhurst") },
            { 1859, (ChatChannelType.Faction, "Fort Sterling") },
            { 1860, (ChatChannelType.Faction, "Caerleon") },
            { 1868, (ChatChannelType.Faction, "Thetford") },
            
            // Guild channel (dynamic, but common ID)
            { 3517, (ChatChannelType.Guild, "Guild") },
        };

        foreach (var kvp in knownChannels)
        {
            _channels[kvp.Key] = new ChatChannelInfo
            {
                ChannelId = kvp.Key,
                Type = kvp.Value.Type,
                Name = kvp.Value.Name
            };
        }

        Log.Information("ChatChannelTracker initialized with {Count} known channels", _channels.Count);
    }

    /// <summary>
    /// Register a joined channel.
    /// </summary>
    public void JoinChannel(long channelId, long chatIndex, string channelName = "")
    {
        var channelType = MapChatIndex(chatIndex);

        // Fallback: if the chatIndex is unrecognized, derive the type from the
        // channel name Albion sends (e.g. "LFG", "Trade", "Faction - Caerleon").
        if (channelType == ChatChannelType.Unknown)
            channelType = MapChannelName(channelName);

        var info = new ChatChannelInfo
        {
            ChannelId = channelId,
            ChatIndex = chatIndex,
            Name = channelName,
            Type = channelType
        };

        _channels[channelId] = info;
        Log.Information("Chat channel joined: {Id} → {Type} ({Name}, index:{Index})",
            channelId, channelType, channelName, chatIndex);
    }

    /// <summary>
    /// Map a channel name string to a type. Handles plain names ("LFG") and
    /// composite names ("Faction - Caerleon") case-insensitively.
    /// </summary>
    private static ChatChannelType MapChannelName(string channelName)
    {
        if (string.IsNullOrWhiteSpace(channelName))
            return ChatChannelType.Unknown;

        var name = channelName.Trim().ToLowerInvariant();

        if (name.Contains("lfg") || name.Contains("looking"))
            return ChatChannelType.LFG;
        if (name.Contains("recruit"))
            return ChatChannelType.Recruitment;
        if (name.Contains("trade"))
            return ChatChannelType.Trade;
        if (name.Contains("faction"))
            return ChatChannelType.Faction;
        if (name.Contains("guild"))
            return ChatChannelType.Guild;
        if (name.Contains("alliance"))
            return ChatChannelType.Alliance;
        if (name.Contains("party") || name.Contains("group"))
            return ChatChannelType.Party;
        if (name.Contains("global") || name.Contains("english") || name.Contains("international"))
            return ChatChannelType.Global;
        if (name.Contains("say") || name.Contains("local"))
            return ChatChannelType.Say;
        if (name.Contains("whisper"))
            return ChatChannelType.Whisper;

        return ChatChannelType.Unknown;
    }

    /// <summary>
    /// Remove a left channel.
    /// </summary>
    public void LeaveChannel(long channelId)
    {
        _channels.TryRemove(channelId, out _);
    }

    /// <summary>
    /// Get the channel type for a channel ID.
    /// </summary>
    public ChatChannelType GetChannelType(long channelId)
    {
        if (_channels.TryGetValue(channelId, out var info))
            return info.Type;

        return ChatChannelType.Unknown;
    }

    /// <summary>
    /// Get channel info.
    /// </summary>
    public ChatChannelInfo? GetChannelInfo(long channelId)
    {
        return _channels.TryGetValue(channelId, out var info) ? info : null;
    }

    /// <summary>
    /// Get display name for a channel.
    /// </summary>
    public string GetChannelName(long channelId)
    {
        if (_channels.TryGetValue(channelId, out var info) && !string.IsNullOrEmpty(info.Name))
            return info.Name;

        return GetChannelType(channelId).ToString();
    }

    public void Clear()
    {
        _channels.Clear();
        InitializeKnownChannels();
    }

    private static ChatChannelType MapChatIndex(long chatIndex)
    {
        return chatIndex switch
        {
            27 => ChatChannelType.Say,
            24 => ChatChannelType.Guild,
            25 => ChatChannelType.Alliance,
            26 => ChatChannelType.Party,
            28 => ChatChannelType.Trade,
            29 => ChatChannelType.Faction,
            30 => ChatChannelType.LFG,
            31 => ChatChannelType.Recruitment,
            _ => ChatChannelType.Unknown
        };
    }
}

public class ChatChannelInfo
{
    public long ChannelId { get; set; }
    public long ChatIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public ChatChannelType Type { get; set; }
}

public enum ChatChannelType
{
    Say,
    Whisper,
    Party,
    Guild,
    Alliance,
    Faction,
    Trade,
    LFG,
    Recruitment,
    Global,
    Unknown
}
