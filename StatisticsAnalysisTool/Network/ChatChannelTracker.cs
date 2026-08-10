using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Tracks chat channel IDs → channel type mapping.
/// Populated by JoinedChatChannel events, used by ChatMessage handler.
/// </summary>
public class ChatChannelTracker
{
    private readonly ConcurrentDictionary<long, ChatChannelInfo> _channels = new();

    public static ChatChannelTracker Instance { get; } = new();

    private ChatChannelTracker() { }

    public int ChannelCount => _channels.Count;

    /// <summary>
    /// Register a joined channel.
    /// </summary>
    public void JoinChannel(long channelId, long chatIndex, string channelName = "")
    {
        var channelType = MapChatIndex(chatIndex);
        var info = new ChatChannelInfo
        {
            ChannelId = channelId,
            ChatIndex = chatIndex,
            Name = channelName,
            Type = channelType
        };

        _channels[channelId] = info;
        Log.Debug("Chat channel joined: {Id} → {Type} ({Name}, index:{Index})",
            channelId, channelType, channelName, chatIndex);
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

        // Fallback to known channel IDs
        return MapKnownChannelId(channelId);
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
    }

    private static ChatChannelType MapChatIndex(long chatIndex)
    {
        return chatIndex switch
        {
            27 => ChatChannelType.Say,
            24 => ChatChannelType.Guild,
            29 => ChatChannelType.Faction,
            25 => ChatChannelType.Alliance,
            26 => ChatChannelType.Party,
            28 => ChatChannelType.Trade,
            30 => ChatChannelType.LFG,
            31 => ChatChannelType.Recruitment,
            _ => ChatChannelType.Unknown
        };
    }

    private static ChatChannelType MapKnownChannelId(long channelId)
    {
        return channelId switch
        {
            0 => ChatChannelType.Say,
            3517 => ChatChannelType.Guild,
            1868 => ChatChannelType.Faction,  // Thetford
            1856 => ChatChannelType.Faction,  // Martlock
            1857 => ChatChannelType.Faction,  // Bridgewatch
            1858 => ChatChannelType.Faction,  // Lymhurst
            1859 => ChatChannelType.Faction,  // Fort Sterling
            1860 => ChatChannelType.Faction,  // Caerleon
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
