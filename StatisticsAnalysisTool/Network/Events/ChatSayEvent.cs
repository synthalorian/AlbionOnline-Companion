using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// ChatSay event (code 74) — local /say chat (players near you).
/// </summary>
public class ChatSayEvent
{
    public string SenderName { get; } = string.Empty;
    public string Message { get; } = string.Empty;

    public ChatSayEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var sender))
            SenderName = sender.ObjectToString();

        if (parameters.TryGetValue(1, out var message))
            Message = message.ObjectToString();
    }
}
