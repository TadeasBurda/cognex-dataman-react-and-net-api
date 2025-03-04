namespace Demo.App.Server.Models;

public sealed record EthSystemConnectorRequest
{
    public required string IpAddress { get; init; }

    public required int Port { get; init; }

    public required string Password { get; init; }

    public required bool RunKeepAliveThread { get; init; }

    public required bool AutoReconnect { get; init; }
}
