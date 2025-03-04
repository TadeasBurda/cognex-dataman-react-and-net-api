namespace Demo.App.Server.Models;

public sealed record SerSystemConnectorRequest
{
    public required string PortName { get; init; }

    public required int Baudrate { get; init; }

    public required bool RunKeepAliveThread { get; init; }

    public required bool AutoReconnect { get; init; }
}
