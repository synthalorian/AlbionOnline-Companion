using PacketDotNet;
using Serilog;
using SharpPcap;
using SharpPcap.LibPcap;
using StatisticsAnalysisTool.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.PacketProviders;

/// <summary>
/// libpcap-based packet provider using SharpPcap.
/// Captures Ethernet frames and extracts UDP payloads for Photon parsing.
/// Same approach as the working Albion Online Translator.
/// </summary>
public class LibpcapPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly List<LibPcapLiveDevice> _devices = new();
    private CancellationTokenSource? _cts;
    private volatile bool _isRunning;

    private static readonly HashSet<ushort> PhotonUdpPorts = [5055, 5056, 5058, 4535];
    private static readonly string PacketFilter = "udp and (port 5055 or port 5056 or port 5058 or port 4535)";

    private int _packetsReceived;
    private int _packetsDelivered;

    public override bool IsRunning => _isRunning;

    public LibpcapPacketProvider(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
    }

    public override void Start()
    {
        if (_isRunning) return;

        _cts = new CancellationTokenSource();
        _packetsReceived = 0;
        _packetsDelivered = 0;

        try
        {
            var devices = LibPcapLiveDeviceList.Instance;
            if (devices.Count == 0)
            {
                Log.Warning("Libpcap: no devices found");
                return;
            }

            foreach (var device in devices)
            {
                // Skip loopback (flags & 0x1) and down interfaces
                var flags = device.Interface.Flags;
                if ((flags & 0x1) != 0) // Loopback
                    continue;
                if ((flags & 0x2) == 0) // Not Up
                    continue;

                try
                {
                    device.Open(DeviceModes.Promiscuous, 100);
                    device.Filter = PacketFilter;
                    device.OnPacketArrival += OnPacketArrival;
                    device.StartCapture();
                    _devices.Add(device);

                    Log.Information("Libpcap: capturing on {Name} ({Description})",
                        device.Interface.Name, device.Interface.FriendlyName);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Libpcap: failed to open {Name}", device.Interface.Name);
                }
            }

            if (_devices.Count == 0)
            {
                Log.Warning("Libpcap: no devices opened");
                return;
            }

            _isRunning = true;
            Log.Information("Libpcap: capture started on {Count} device(s), filter: {Filter}",
                _devices.Count, PacketFilter);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Libpcap: failed to start capture");
            throw;
        }
    }

    public override void Stop()
    {
        _isRunning = false;
        _cts?.Cancel();

        foreach (var device in _devices)
        {
            try
            {
                device.OnPacketArrival -= OnPacketArrival;
                device.StopCapture();
                device.Close();
            }
            catch { }
        }
        _devices.Clear();

        Log.Information("Libpcap: capture stopped. Received: {Received}, Delivered: {Delivered}",
            _packetsReceived, _packetsDelivered);
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var rawPacket = e.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            _packetsReceived++;

            var udpPacket = packet.Extract<UdpPacket>();
            if (udpPacket == null) return;

            if (!PhotonUdpPorts.Contains(udpPacket.SourcePort) && !PhotonUdpPorts.Contains(udpPacket.DestinationPort))
                return;

            var ipPacket = packet.Extract<IPPacket>();
            var sourceIp = ipPacket?.SourceAddress?.ToString() ?? "unknown";

            var payload = udpPacket.PayloadData;
            if (payload == null || payload.Length == 0) return;

            _packetsDelivered++;

            if (_packetsDelivered <= 10 || _packetsDelivered % 100 == 0)
            {
                Log.Information("Libpcap: Packet #{Count} from {IP}:{SrcPort} → {DstPort}, {Len} bytes, first: 0x{B0:X2} {B1:X2} {B2:X2} {B3:X2}",
                    _packetsDelivered, sourceIp, udpPacket.SourcePort, udpPacket.DestinationPort,
                    payload.Length,
                    payload.Length > 0 ? payload[0] : 0,
                    payload.Length > 1 ? payload[1] : 0,
                    payload.Length > 2 ? payload[2] : 0,
                    payload.Length > 3 ? payload[3] : 0);
            }

            _photonReceiver.ReceivePacket(payload);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Libpcap: packet processing error");
        }
    }
}
