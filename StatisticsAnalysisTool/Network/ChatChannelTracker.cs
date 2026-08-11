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
            // Verified 2026-08-11 (second pass):
            // 21 = general English help/chat ("what is raging storm?", destiny board questions)
            { 21, (ChatChannelType.Global, "Global") },

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
    /// CRITICAL: ChatMessage events address channels by the RUNTIME channel id,
    /// which is the JoinedChatChannel event's chatIndex param — NOT the channelId
    /// param (that one is a small channel-TYPE enum). Keying by channelId meant
    /// joined channels never matched ChatMessage lookups and everything showed
    /// as Unknown (verified in live capture 2026-08-11).
    /// </summary>
    public void JoinChannel(long channelId, long chatIndex, string channelName = "")
    {
        var channelType = MapChatIndex(channelId);

        // Fallback: if the type enum is unrecognized, derive the type from the
        // channel name Albion sends (e.g. "LFG", "Trade", "Faction - Caerleon").
        if (channelType == ChatChannelType.Unknown)
            channelType = MapChannelName(channelName);

        // If we already have a verified static entry for this runtime id, keep
        // its type unless the join event gives us a better one.
        if (_channels.TryGetValue(chatIndex, out var existing) &&
            existing.Type != ChatChannelType.Unknown &&
            channelType == ChatChannelType.Unknown)
        {
            channelType = existing.Type;
        }

        var info = new ChatChannelInfo
        {
            ChannelId = chatIndex, // runtime id — this is what ChatMessage uses
            ChatIndex = channelId, // type enum (kept for diagnostics)
            Name = !string.IsNullOrEmpty(channelName) ? channelName : channelType.ToString(),
            Type = channelType
        };

        _channels[chatIndex] = info;
        Log.Information("Chat channel joined: runtime:{RuntimeId} → {Type} ({Name}, typeEnum:{TypeEnum})",
            chatIndex, channelType, channelName, channelId);
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

    /// <summary>
    /// Map the JoinedChatChannel channelId param — this is a channel-TYPE enum,
    /// not a runtime id. Verified 2026-08-11 by correlating join events with
    /// live message content: typeEnum 8 joined runtime id 2 (Trade),
    /// 2 → 18 (Recruitment), 3 → 19 (LFG). 24/25 have high dynamic runtime ids
    /// (Guild/Alliance). 26/27 inferred by sequence — pending live verification.
    /// </summary>
    private static ChatChannelType MapChatIndex(long chatIndex)
    {
        return chatIndex switch
        {
            2 => ChatChannelType.Recruitment,  // verified: joined runtime 18
            3 => ChatChannelType.LFG,          // verified: joined runtime 19
            5 => ChatChannelType.Global,       // verified: joined runtime 21
            7 => ChatChannelType.Faction,      // inferred (joined runtime 22, unverified)
            8 => ChatChannelType.Trade,        // verified: joined runtime 2
            24 => ChatChannelType.Guild,       // verified pattern: high dynamic runtime id
            25 => ChatChannelType.Alliance,    // verified pattern: high dynamic runtime id
            26 => ChatChannelType.Party,       // inferred by 24/25 sequence — needs live verify
            // verified 2026-08-11: zone-local channel; runtime id is DYNAMIC per
            // cluster (436, 182, 94, 307, 57, 471, 1479 seen across zones)
            27 => ChatChannelType.Say,
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
