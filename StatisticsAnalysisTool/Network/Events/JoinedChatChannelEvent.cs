using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// JoinedChatChannel event (code 207) — tells us which channel IDs map to which chat types.
/// This is how we know channel 3517 = Guild, etc.
/// </summary>
public class JoinedChatChannelEvent
{
    public long ChannelId { get; }
    public long ChatIndex { get; }
    public string ChannelName { get; } = string.Empty;

    public JoinedChatChannelEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var channelId))
            ChannelId = channelId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var chatIndex))
            ChatIndex = chatIndex.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(2, out var name))
            ChannelName = name.ObjectToString();
    }
}
