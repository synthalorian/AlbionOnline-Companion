using StatisticsAnalysisTool.PhotonPackageParser;
using System.Globalization;

namespace StatisticsAnalysisTool.Network;

internal sealed class AlbionParser : PhotonParser
{
    private readonly HandlersCollection _handlers = new();

    public void AddHandler<TPacket>(PacketHandler<TPacket> handler)
    {
        _handlers.Add(handler);
    }

    private static int _eventCount;

    protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
    {
        short eventCode = ParseEventCode(parameters);

        _eventCount++;
        if (_eventCount <= 50 || _eventCount % 100 == 0)
        {
            Console.WriteLine($"AlbionParser: Event #{_eventCount}: code={eventCode} params={parameters.Count}");
        }

        if (eventCode <= -1)
        {
            return;
        }

        var eventPacket = new EventPacket(eventCode, parameters);

        _ = _handlers.HandleAsync(eventPacket);
    }

    protected override void OnRequest(byte operationCodeByte, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);

        if (operationCode <= -1)
        {
            return;
        }

        var requestPacket = new RequestPacket(operationCode, parameters);

        _ = _handlers.HandleAsync(requestPacket);
    }

    private static int _responseCount;

    protected override void OnResponse(byte operationCodeByte, short returnCode, string debugMessage, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);

        _responseCount++;
        if (_responseCount <= 50 || _responseCount % 100 == 0)
        {
            Console.WriteLine($"AlbionParser: Response #{_responseCount}: opCode={operationCode} returnCode={returnCode} params={parameters.Count}");
        }

        if (operationCode <= -1)
        {
            return;
        }

        var responsePacket = new ResponsePacket(operationCode, parameters);

        _ = _handlers.HandleAsync(responsePacket);
    }

    private static short ParseOperationCode(Dictionary<byte, object> parameters)
    {
        return ParsePhotonCode(parameters, 253);
    }

    private static short ParseEventCode(Dictionary<byte, object> parameters)
    {
        return ParsePhotonCode(parameters, 252);
    }

    private static short ParsePhotonCode(Dictionary<byte, object> parameters, byte parameterKey)
    {
        if (!parameters.TryGetValue(parameterKey, out object value))
        {
            return -1;
        }

        try
        {
            return checked((short) Convert.ToInt32(value, CultureInfo.InvariantCulture));
        }
        catch
        {
            return -1;
        }
    }
}
