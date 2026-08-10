using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// ChatMessage event (code 73) — all channel-based chat (guild, alliance, trade, LFG, faction, etc).
/// The ChannelId determines which chat channel this message belongs to.
/// </summary>
public class ChatMessageEvent
{
    public long ChannelId { get; }
    public string SenderName { get; } = string.Empty;
    public string Message { get; } = string.Empty;

    public ChatMessageEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var channelId))
            ChannelId = channelId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var sender))
            SenderName = sender.ObjectToString();

        if (parameters.TryGetValue(2, out var message))
            Message = message.ObjectToString();
    }
}
