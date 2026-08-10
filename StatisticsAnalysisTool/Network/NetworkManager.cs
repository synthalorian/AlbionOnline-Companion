using Serilog;
using StatisticsAnalysisTool.Abstractions;
using StatisticsAnalysisTool.Network.Handlers;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Manages network packet capture and Photon protocol parsing for Albion Online.
/// Linux-compatible version using raw sockets.
/// </summary>
public class NetworkManager : IDisposable
{
    private readonly ReceiverBuilder _receiverBuilder;
    private PacketProvider? _packetProvider;
    private bool _isRunning;

    // Event handlers
    private DamageMeterEventHandler? _damageMeterHandler;
    private DamageMeterBatchHandler? _damageMeterBatchHandler;
    private DungeonTrackerEventHandler? _dungeonTrackerHandler;
    private LootLoggerEventHandler? _lootLoggerHandler;
    private DashboardEventHandler? _dashboardHandler;
    private DashboardSilverHandler? _dashboardSilverHandler;
    private DashboardReSpecHandler? _dashboardReSpecHandler;
    private DashboardMightFavorHandler? _dashboardMightFavorHandler;
    private KillDeathEventHandler? _killDeathHandler;
    private PlayerNameEventHandler? _playerNameHandler;
    private MobTrackEventHandler? _mobTrackHandler;
    private LeaveEventHandler? _leaveHandler;

    public bool IsRunning => _isRunning;
    public event EventHandler<string>? StatusChanged;

    public NetworkManager()
    {
        _receiverBuilder = ReceiverBuilder.Create();
    }

    /// <summary>
    /// Register ViewModels to receive game events.
    /// </summary>
    public void RegisterViewModels(
        DashboardViewModel dashboard,
        DamageMeterViewModel damageMeter,
        DungeonTrackerViewModel dungeonTracker,
        LootLoggerViewModel lootLogger)
    {
        _dashboardHandler = new DashboardEventHandler(dashboard);
        _damageMeterHandler = new DamageMeterEventHandler(damageMeter);
        _dungeonTrackerHandler = new DungeonTrackerEventHandler(dungeonTracker);
        _lootLoggerHandler = new LootLoggerEventHandler(lootLogger);

        // Register single-event handlers
        _receiverBuilder.AddEventHandler(_dashboardHandler);
        _receiverBuilder.AddEventHandler(_damageMeterHandler);
        _receiverBuilder.AddEventHandler(_dungeonTrackerHandler);
        _receiverBuilder.AddEventHandler(_lootLoggerHandler);

        // Register batch handlers
        _damageMeterBatchHandler = new DamageMeterBatchHandler(damageMeter);
        _receiverBuilder.AddEventHandler(_damageMeterBatchHandler);

        // Register dashboard sub-handlers
        _dashboardSilverHandler = new DashboardSilverHandler(dashboard, _dashboardHandler);
        _dashboardReSpecHandler = new DashboardReSpecHandler(dashboard, _dashboardHandler);
        _dashboardMightFavorHandler = new DashboardMightFavorHandler(dashboard, _dashboardHandler);
        _receiverBuilder.AddEventHandler(_dashboardSilverHandler);
        _receiverBuilder.AddEventHandler(_dashboardReSpecHandler);
        _receiverBuilder.AddEventHandler(_dashboardMightFavorHandler);

        // Register kill/death and player name handlers
        _killDeathHandler = new KillDeathEventHandler(dashboard);
        _playerNameHandler = new PlayerNameEventHandler();
        _receiverBuilder.AddEventHandler(_killDeathHandler);
        _receiverBuilder.AddEventHandler(_playerNameHandler);

        // Register entity tracking handlers
        _mobTrackHandler = new MobTrackEventHandler();
        _leaveHandler = new LeaveEventHandler();
        _receiverBuilder.AddEventHandler(_mobTrackHandler);
        _receiverBuilder.AddEventHandler(_leaveHandler);

        // Set static refs

        Log.Information("ViewModels registered with NetworkManager");
    }

    public void AddHandler<TPacket>(PacketHandler<TPacket> handler)
    {
        _receiverBuilder.AddHandler(handler);
    }

    public void AddEventHandler<TEvent>(EventPacketHandler<TEvent> handler)
    {
        _receiverBuilder.AddEventHandler(handler);
    }

    public void AddRequestHandler<TOperation>(RequestPacketHandler<TOperation> handler)
    {
        _receiverBuilder.AddRequestHandler(handler);
    }

    public void AddResponseHandler<TOperation>(ResponsePacketHandler<TOperation> handler)
    {
        _receiverBuilder.AddResponseHandler(handler);
    }

    public void Start()
    {
        if (_isRunning)
        {
            Log.Warning("NetworkManager already running");
            return;
        }

        try
        {
            var receiver = _receiverBuilder.Build();
            _packetProvider = new LinuxSocketPacketProvider(receiver);
            _packetProvider.Start();
            _isRunning = true;

            Log.Information("NetworkManager started");
            StatusChanged?.Invoke(this, "Tracking started");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start NetworkManager");
            StatusChanged?.Invoke(this, $"Error: {ex.Message}");
            throw;
        }
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        try
        {
            _packetProvider?.Stop();
            _packetProvider = null;
            _isRunning = false;

            Log.Information("NetworkManager stopped");
            StatusChanged?.Invoke(this, "Tracking stopped");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error stopping NetworkManager");
            StatusChanged?.Invoke(this, $"Error stopping: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
