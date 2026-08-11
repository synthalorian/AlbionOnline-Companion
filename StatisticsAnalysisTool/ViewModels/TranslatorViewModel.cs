using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public partial class TranslatorViewModel : ViewModelBase
{
    private readonly TranslationService _translationService = TranslationService.Instance;

    [ObservableProperty] private ObservableCollection<ChatMessageEntry> _messages = new();
    [ObservableProperty] private ObservableCollection<ChatMessageEntry> _filteredMessages = new();
    [ObservableProperty] private bool _translationEnabled = true;
    [ObservableProperty] private string _selectedTargetLanguage = "English";
    [ObservableProperty] private ObservableCollection<string> _targetLanguages = new();
    [ObservableProperty] private string _selectedChannelFilter = "All";
    [ObservableProperty] private ObservableCollection<string> _channelFilters = new();
    [ObservableProperty] private string _statusText = "Listening for chat...";
    [ObservableProperty] private int _messageCount;
    [ObservableProperty] private int _translatedCount;
    [ObservableProperty] private bool _autoScroll = true;

    // Channel visibility toggles
    [ObservableProperty] private bool _showSay = true;
    [ObservableProperty] private bool _showWhisper = true;
    [ObservableProperty] private bool _showParty = true;
    [ObservableProperty] private bool _showGuild = true;
    [ObservableProperty] private bool _showAlliance = true;
    [ObservableProperty] private bool _showFaction = true;
    [ObservableProperty] private bool _showTrade = true;
    [ObservableProperty] private bool _showLFG = true;
    [ObservableProperty] private bool _showRecruitment = true;
    [ObservableProperty] private bool _showGlobal = true;

    public TranslatorViewModel()
    {
        // Language options
        TargetLanguages.Add("English");
        TargetLanguages.Add("Spanish");
        TargetLanguages.Add("Portuguese");
        TargetLanguages.Add("French");
        TargetLanguages.Add("German");
        TargetLanguages.Add("Russian");
        TargetLanguages.Add("Korean");
        TargetLanguages.Add("Chinese");
        TargetLanguages.Add("Japanese");
        TargetLanguages.Add("Arabic");
        TargetLanguages.Add("Turkish");
        TargetLanguages.Add("Polish");

        // Channel filter options
        ChannelFilters.Add("All");
        ChannelFilters.Add("Say");
        ChannelFilters.Add("Whisper");
        ChannelFilters.Add("Party");
        ChannelFilters.Add("Guild");
        ChannelFilters.Add("Alliance");
        ChannelFilters.Add("Faction");
        ChannelFilters.Add("Trade");
        ChannelFilters.Add("LFG");
        ChannelFilters.Add("Recruitment");
        ChannelFilters.Add("Global");
    }

    /// <summary>
    /// Add a chat message from any channel.
    /// </summary>
    public void AddMessage(string sender, string message, ChatChannelType channel, string channelName = "")
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var entry = new ChatMessageEntry
        {
            Timestamp = DateTime.Now,
            SenderName = sender,
            OriginalText = message,
            Channel = channel,
            ChannelName = channelName.Length > 0 ? channelName : channel.ToString(),
        };

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Messages.Insert(0, entry);
            MessageCount = Messages.Count;

            // Keep max 500 messages
            while (Messages.Count > 500)
                Messages.RemoveAt(Messages.Count - 1);

            ApplyFilter();
        });

        // Translate in background
        if (TranslationEnabled)
        {
            _ = TranslateMessageAsync(entry);
        }
    }

    private async Task TranslateMessageAsync(ChatMessageEntry entry)
    {
        try
        {
            var targetCode = GetLanguageCode(SelectedTargetLanguage);
            _translationService.TargetLanguage = targetCode;

            var result = await _translationService.TranslateAsync(entry.OriginalText);

            if (!result.Error && result.TranslatedText != entry.OriginalText)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    entry.TranslatedText = result.TranslatedText;
                    entry.DetectedLanguage = result.DetectedLanguage ?? "unknown";
                    entry.IsTranslated = true;
                    TranslatedCount++;
                });
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Translation failed for: {Text}", entry.OriginalText);
        }
    }

    partial void OnSelectedChannelFilterChanged(string value) => ApplyFilter();
    partial void OnShowSayChanged(bool value) => ApplyFilter();
    partial void OnShowWhisperChanged(bool value) => ApplyFilter();
    partial void OnShowPartyChanged(bool value) => ApplyFilter();
    partial void OnShowGuildChanged(bool value) => ApplyFilter();
    partial void OnShowAllianceChanged(bool value) => ApplyFilter();
    partial void OnShowFactionChanged(bool value) => ApplyFilter();
    partial void OnShowTradeChanged(bool value) => ApplyFilter();
    partial void OnShowLFGChanged(bool value) => ApplyFilter();
    partial void OnShowRecruitmentChanged(bool value) => ApplyFilter();
    partial void OnShowGlobalChanged(bool value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredMessages.Clear();

        foreach (var msg in Messages)
        {
            // Channel filter dropdown — match by channel type name
            if (SelectedChannelFilter != "All")
            {
                var filterMatch = SelectedChannelFilter switch
                {
                    "Say" => msg.Channel == ChatChannelType.Say,
                    "Whisper" => msg.Channel == ChatChannelType.Whisper,
                    "Party" => msg.Channel == ChatChannelType.Party,
                    "Guild" => msg.Channel == ChatChannelType.Guild,
                    "Alliance" => msg.Channel == ChatChannelType.Alliance,
                    "Faction" => msg.Channel == ChatChannelType.Faction,
                    "Trade" => msg.Channel == ChatChannelType.Trade,
                    "LFG" => msg.Channel == ChatChannelType.LFG,
                    "Recruitment" => msg.Channel == ChatChannelType.Recruitment,
                    "Global" => msg.Channel == ChatChannelType.Global,
                    _ => true
                };

                if (!filterMatch)
                    continue;
            }

            // Individual channel toggles
            var visible = msg.Channel switch
            {
                ChatChannelType.Say => ShowSay,
                ChatChannelType.Whisper => ShowWhisper,
                ChatChannelType.Party => ShowParty,
                ChatChannelType.Guild => ShowGuild,
                ChatChannelType.Alliance => ShowAlliance,
                ChatChannelType.Faction => ShowFaction,
                ChatChannelType.Trade => ShowTrade,
                ChatChannelType.LFG => ShowLFG,
                ChatChannelType.Recruitment => ShowRecruitment,
                ChatChannelType.Global => ShowGlobal,
                _ => true
            };

            if (visible)
                FilteredMessages.Add(msg);
        }
    }

    [RelayCommand]
    private void ClearMessages()
    {
        Messages.Clear();
        FilteredMessages.Clear();
        MessageCount = 0;
        TranslatedCount = 0;
    }

    [RelayCommand]
    private void ToggleTranslation()
    {
        TranslationEnabled = !TranslationEnabled;
        StatusText = TranslationEnabled ? "Translation ON" : "Translation OFF";
    }

    private static string GetLanguageCode(string language)
    {
        return language switch
        {
            "Spanish" => "es",
            "Portuguese" => "pt",
            "French" => "fr",
            "German" => "de",
            "Russian" => "ru",
            "Korean" => "ko",
            "Chinese" => "zh",
            "Japanese" => "ja",
            "Arabic" => "ar",
            "Turkish" => "tr",
            "Polish" => "pl",
            _ => "en"
        };
    }
}

public class ChatMessageEntry : ObservableObject
{
    public DateTime Timestamp { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public ChatChannelType Channel { get; set; }
    public string ChannelName { get; set; } = string.Empty;

    private string _translatedText = string.Empty;
    public string TranslatedText
    {
        get => _translatedText;
        set => SetProperty(ref _translatedText, value);
    }

    private string _detectedLanguage = string.Empty;
    public string DetectedLanguage
    {
        get => _detectedLanguage;
        set => SetProperty(ref _detectedLanguage, value);
    }

    private bool _isTranslated;
    public bool IsTranslated
    {
        get => _isTranslated;
        set => SetProperty(ref _isTranslated, value);
    }

    public string TimestampDisplay => Timestamp.ToString("HH:mm:ss");
    public string DisplayText => IsTranslated ? TranslatedText : OriginalText;
    public string LanguageTag => IsTranslated ? $"[{DetectedLanguage}→en]" : "";

    public string ChannelColor => Channel switch
    {
        ChatChannelType.Say => "#FF89B4FA",        // Blue
        ChatChannelType.Whisper => "#FFF5C2E7",    // Pink
        ChatChannelType.Party => "#FFA6E3A1",      // Green
        ChatChannelType.Guild => "#FFF9E2AF",      // Yellow
        ChatChannelType.Alliance => "#FF94E2D5",   // Teal
        ChatChannelType.Faction => "#FFF38BA8",    // Red
        ChatChannelType.Trade => "#FFFAB387",      // Orange
        ChatChannelType.LFG => "#FFCBA6F7",        // Purple
        ChatChannelType.Recruitment => "#FF89DCEB",// Sky
        ChatChannelType.Global => "#FFCDD6F4",     // Light
        _ => "#FF6C7086"                            // Gray
    };
}
