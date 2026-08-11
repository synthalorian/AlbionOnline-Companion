using Serilog;
using StatisticsAnalysisTool.Common;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Events;

/// <summary>
/// JoinedChatChannel event (code 207) — tells us which runtime channel IDs map
/// to which chat types. Param 0 is a channel-TYPE enum (2=Recruitment, 3=LFG,
/// 5=Global, 8=Trade, 24=Guild, 25=Alliance...), param 1 is the RUNTIME channel
/// id that ChatMessage events actually use (2, 18, 19, 21, 34125...).
/// </summary>
public class JoinedChatChannelEvent
{
    public long ChannelId { get; }
    public long ChatIndex { get; }
    public string ChannelName { get; } = string.Empty;

    private static int _diagCount;

    public JoinedChatChannelEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out var channelId))
            ChannelId = channelId.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(1, out var chatIndex))
            ChatIndex = chatIndex.ObjectToLong() ?? 0;

        if (parameters.TryGetValue(2, out var name))
            ChannelName = name.ObjectToString();

        // Ground-truth instrumentation: dump every raw param (first 10 joins) so
        // we can find where Albion hides the channel name, if it sends one at all.
        if (_diagCount < 10)
        {
            _diagCount++;
            var dump = string.Join(" ", parameters
                .OrderBy(kv => kv.Key)
                .Select(kv => $"[{kv.Key}:{DescribeValue(kv.Value)}]"));
            Log.Information("JoinedChatChannel RAW: {Params}", dump);
        }
    }

    private static string DescribeValue(object? value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            System.Array arr => $"({arr.GetType().GetElementType()?.Name}[{arr.Length}])",
            _ => value.ToString() ?? "?"
        };
    }
}
