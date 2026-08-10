using Serilog;
using StatisticsAnalysisTool.Abstractions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.PacketProviders;

/// <summary>
/// Linux-native packet provider using raw sockets.
/// Requires CAP_NET_RAW or root privileges.
/// </summary>
public class LinuxSocketPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly List<Socket> _socketsV4 = [];
    private readonly List<Socket> _socketsV6 = [];
    private readonly List<Task> _receiveTasks = [];
    private volatile bool _stopReceiving;
    private CancellationTokenSource? _cts;

    public static readonly HashSet<ushort> PhotonUdpPorts = [5055, 5056, 5058];

    public override bool IsRunning => _socketsV4.Any(s => s is { IsBound: true }) || _socketsV6.Any(s => s is { IsBound: true });

    public LinuxSocketPacketProvider(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
    }

    public override void Start()
    {
        _stopReceiving = false;
        _cts = new CancellationTokenSource();

        var v4 = GetLocalUnicastAddresses(AddressFamily.InterNetwork).ToList();
        var v6 = GetLocalUnicastAddresses(AddressFamily.InterNetworkV6).ToList();

        if (v4.Count == 0 && v6.Count == 0)
        {
            Log.Warning("LinuxSocket: no local unicast addresses found");
            return;
        }

        foreach (var ip in v4)
        {
            CreateRawSocketIPv4(ip);
        }

        foreach (var ip in v6)
        {
            CreateRawSocketIPv6(ip);
        }

        foreach (var s in _socketsV4.Concat(_socketsV6))
        {
            var buffer = new byte[65535];
            _receiveTasks.Add(Task.Run(() => ReceiveLoopAsync(s, buffer, _cts.Token)));
        }

        Log.Information("LinuxSocket: capture started on {V4Count} IPv4 + {V6Count} IPv6 sockets",
            _socketsV4.Count, _socketsV6.Count);
    }

    public override void Stop()
    {
        _stopReceiving = true;
        _cts?.Cancel();

        foreach (var s in _socketsV4.Concat(_socketsV6))
        {
            SafeClose(s);
        }
        _socketsV4.Clear();
        _socketsV6.Clear();

        try
        {
            Task.WaitAll(_receiveTasks.ToArray(), TimeSpan.FromSeconds(2));
        }
        catch { /* ignore */ }

        _receiveTasks.Clear();
        _cts?.Dispose();
        _cts = null;

        Log.Information("LinuxSocket: capture stopped");
    }

    private void CreateRawSocketIPv4(IPAddress ip)
    {
        try
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(ip, 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            // Linux: no IOControl needed - raw sockets receive all IP packets by default
            _socketsV4.Add(socket);
            Log.Debug("LinuxSocket: IPv4 raw socket bound to {IP}", ip);
        }
        catch (SocketException ex)
        {
            Log.Warning(ex, "LinuxSocket: IPv4 bind failed on {IP} ({Error}) - root/CAP_NET_RAW required?", ip, ex.SocketErrorCode);
        }
    }

    private void CreateRawSocketIPv6(IPAddress ip)
    {
        try
        {
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(ip, 0));

            _socketsV6.Add(socket);
            Log.Debug("LinuxSocket: IPv6 raw socket bound to {IP}", ip);
        }
        catch (SocketException ex)
        {
            Log.Warning(ex, "LinuxSocket: IPv6 bind failed on {IP} ({Error})", ip, ex.SocketErrorCode);
        }
    }

    private async Task ReceiveLoopAsync(Socket socket, byte[] buffer, CancellationToken ct)
    {
        while (!_stopReceiving && !ct.IsCancellationRequested)
        {
            try
            {
                int bytes = await socket.ReceiveAsync(buffer, SocketFlags.None, ct).ConfigureAwait(false);
                if (bytes > 0)
                {
                    ProcessFrame(buffer.AsSpan(0, bytes), socket.AddressFamily);
                }
            }
            catch (SocketException) when (_stopReceiving || ct.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "LinuxSocket: receive error");
            }
        }
    }

    private void ProcessFrame(ReadOnlySpan<byte> frame, AddressFamily af)
    {
        if (af == AddressFamily.InterNetwork)
        {
            ProcessIPv4(frame);
        }
        else if (af == AddressFamily.InterNetworkV6)
        {
            ProcessIPv6(frame);
        }
    }

    private void ProcessIPv4(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 20) return;

        byte verIhl = frame[0];
        int version = verIhl >> 4;
        if (version != 4) return;

        int ihl = (verIhl & 0x0F) * 4;
        if (ihl < 20 || frame.Length < ihl) return;

        // Skip fragmented packets
        ushort flagsFrag = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(6, 2));
        bool hasMoreFragments = (flagsFrag & 0x2000) != 0;
        int fragOffset = (flagsFrag & 0x1FFF) * 8;
        if (hasMoreFragments || fragOffset != 0) return;

        byte proto = frame[9];
        if (proto != 17) return; // UDP only

        var sourceIp = new IPAddress(frame.Slice(12, 4).ToArray()).ToString();

        if (frame.Length < ihl + 8) return;

        var udp = frame[ihl..];
        ushort srcPort = BinaryPrimitives.ReadUInt16BigEndian(udp);
        ushort dstPort = BinaryPrimitives.ReadUInt16BigEndian(udp[2..]);
        ushort udpLen = BinaryPrimitives.ReadUInt16BigEndian(udp[4..]);

        if (!PhotonUdpPorts.Contains(srcPort) && !PhotonUdpPorts.Contains(dstPort))
        {
            // Check if payload looks like Photon protocol
            int payloadOffset = ihl + 8;
            if (payloadOffset >= frame.Length) return;

            var payloadAll = frame[payloadOffset..];
            if (!LooksLikePhoton(payloadAll)) return;

            int maxPayload = frame.Length - payloadOffset;
            int payloadLen = Math.Min(maxPayload, Math.Max(0, udpLen - 8));
            if (payloadLen <= 0) return;

            Deliver(frame.Slice(payloadOffset, payloadLen), sourceIp);
            return;
        }

        int po = ihl + 8;
        int max = frame.Length - po;
        int len = Math.Min(max, Math.Max(0, udpLen - 8));
        if (len <= 0) return;

        Deliver(frame.Slice(po, len), sourceIp);
    }

    private void ProcessIPv6(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 40) return;

        int version = (frame[0] >> 4) & 0x0F;
        if (version != 6) return;

        byte nextHeader = frame[6];
        if (nextHeader != 17) return; // UDP only

        var sourceIp = new IPAddress(frame.Slice(8, 16).ToArray()).ToString();

        if (frame.Length < 48) return;

        var udp = frame[40..];
        ushort srcPort = BinaryPrimitives.ReadUInt16BigEndian(udp);
        ushort dstPort = BinaryPrimitives.ReadUInt16BigEndian(udp[2..]);
        ushort udpLen = BinaryPrimitives.ReadUInt16BigEndian(udp[4..]);

        int payloadOffset = 48;
        if (udpLen == 0 || frame.Length < payloadOffset) return;

        int maxPayload = frame.Length - payloadOffset;
        int payloadLenCalc = Math.Min(maxPayload, Math.Max(0, udpLen - 8));
        if (payloadLenCalc <= 0) return;

        var payload = frame.Slice(payloadOffset, payloadLenCalc);

        if (!PhotonUdpPorts.Contains(srcPort) && !PhotonUdpPorts.Contains(dstPort))
        {
            if (!LooksLikePhoton(payload)) return;
        }

        Deliver(payload, sourceIp);
    }

    private void Deliver(ReadOnlySpan<byte> payload, string sourceIp)
    {
        if (payload.Length == 0) return;

        try
        {
            _photonReceiver.ReceivePacket(payload);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "LinuxSocket: PhotonReceiver.ReceivePacket failed");
        }
    }

    private static bool LooksLikePhoton(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 3) return false;
        byte b0 = payload[0];
        return b0 is 0xF1 or 0xF2 or 0xFE;
    }

    private static IEnumerable<IPAddress> GetLocalUnicastAddresses(AddressFamily family)
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Select(ua => ua.Address)
                .Where(a => a.AddressFamily == family)
                .Distinct();
        }
        catch
        {
            return [];
        }
    }

    private static void SafeClose(Socket s)
    {
        try { if (s.Connected) s.Shutdown(SocketShutdown.Both); } catch { }
        try { s.Close(); } catch { }
        try { s.Dispose(); } catch { }
    }
}
