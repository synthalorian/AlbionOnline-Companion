using Serilog;
using StatisticsAnalysisTool.Abstractions;
using StatisticsAnalysisTool.Network.PacketProviders;
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

    public bool IsRunning => _isRunning;
    public event EventHandler<string>? StatusChanged;

    public NetworkManager()
    {
        _receiverBuilder = ReceiverBuilder.Create();
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
