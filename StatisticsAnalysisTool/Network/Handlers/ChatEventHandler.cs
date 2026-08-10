using Serilog;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handlers;

/// <summary>
/// Handles all chat events and feeds them to the TranslatorViewModel.
/// Supports: ChatMessage (73), ChatSay (74), ChatWhisper (75),
/// JoinedChatChannel (207), LeftChatChannel (208).
/// </summary>
public class ChatMessageEventHandler : EventPacketHandler<ChatMessageEvent>
{
    private readonly TranslatorViewModel _viewModel;
    private readonly ChatChannelTracker _channelTracker;

    public ChatMessageEventHandler(TranslatorViewModel viewModel)
        : base((int)EventCodes.ChatMessage)
    {
        _viewModel = viewModel;
        _channelTracker = ChatChannelTracker.Instance;
    }

    protected override Task OnActionAsync(ChatMessageEvent value)
    {
        try
        {
            var channelType = _channelTracker.GetChannelType(value.ChannelId);
            var channelName = _channelTracker.GetChannelName(value.ChannelId);

            _viewModel.AddMessage(value.SenderName, value.Message, channelType, channelName);
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "ChatMessageEventHandler error");
        }

        return Task.CompletedTask;
    }
}

public class ChatSayEventHandler : EventPacketHandler<ChatSayEvent>
{
    private readonly TranslatorViewModel _viewModel;

    public ChatSayEventHandler(TranslatorViewModel viewModel)
        : base((int)EventCodes.ChatSay)
    {
        _viewModel = viewModel;
    }

    protected override Task OnActionAsync(ChatSayEvent value)
    {
        try
        {
            _viewModel.AddMessage(value.SenderName, value.Message, ChatChannelType.Say, "Local");
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "ChatSayEventHandler error");
        }

        return Task.CompletedTask;
    }
}

public class ChatWhisperEventHandler : EventPacketHandler<ChatWhisperEvent>
{
    private readonly TranslatorViewModel _viewModel;

    public ChatWhisperEventHandler(TranslatorViewModel viewModel)
        : base((int)EventCodes.ChatWhisper)
    {
        _viewModel = viewModel;
    }

    protected override Task OnActionAsync(ChatWhisperEvent value)
    {
        try
        {
            _viewModel.AddMessage(value.SenderName, value.Message, ChatChannelType.Whisper, "Whisper");
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "ChatWhisperEventHandler error");
        }

        return Task.CompletedTask;
    }
}

public class JoinedChatChannelEventHandler : EventPacketHandler<JoinedChatChannelEvent>
{
    private readonly ChatChannelTracker _channelTracker;

    public JoinedChatChannelEventHandler()
        : base((int)EventCodes.JoinedChatChannel)
    {
        _channelTracker = ChatChannelTracker.Instance;
    }

    protected override Task OnActionAsync(JoinedChatChannelEvent value)
    {
        try
        {
            _channelTracker.JoinChannel(value.ChannelId, value.ChatIndex, value.ChannelName);
            Log.Information("Joined chat channel: {Id} → index:{Index} ({Name})",
                value.ChannelId, value.ChatIndex, value.ChannelName);
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "JoinedChatChannelEventHandler error");
        }

        return Task.CompletedTask;
    }
}

public class LeftChatChannelEventHandler : EventPacketHandler<LeftChatChannelEvent>
{
    private readonly ChatChannelTracker _channelTracker;

    public LeftChatChannelEventHandler()
        : base((int)EventCodes.LeftChatChannel)
    {
        _channelTracker = ChatChannelTracker.Instance;
    }

    protected override Task OnActionAsync(LeftChatChannelEvent value)
    {
        try
        {
            _channelTracker.LeaveChannel(value.ChannelId);
        }
        catch (System.Exception ex)
        {
            Log.Debug(ex, "LeftChatChannelEventHandler error");
        }

        return Task.CompletedTask;
    }
}
