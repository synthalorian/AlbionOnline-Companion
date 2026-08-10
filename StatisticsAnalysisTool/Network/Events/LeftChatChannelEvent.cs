using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// LeftChatChannel event (code 208) — player left a chat channel.
/// </summary>
public class LeftChatChannelEvent
{
    public long ChannelId { get; }

    public LeftChatChannelEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var channelId))
            ChannelId = channelId.ObjectToLong() ?? 0;
    }
}
