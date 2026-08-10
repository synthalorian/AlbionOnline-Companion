using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// ChatWhisper event (code 75) — private /whisper messages.
/// </summary>
public class ChatWhisperEvent
{
    public string SenderName { get; } = string.Empty;
    public string Message { get; } = string.Empty;

    public ChatWhisperEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var sender))
            SenderName = sender.ObjectToString();

        if (parameters.TryGetValue(1, out var message))
            Message = message.ObjectToString();
    }
}
