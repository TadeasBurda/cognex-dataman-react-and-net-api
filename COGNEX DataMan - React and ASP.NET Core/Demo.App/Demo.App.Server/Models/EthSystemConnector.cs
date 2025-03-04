namespace Demo.App.Server.Models;

public record EthSystemConnector : Connector
{
    public required string IpAddress { get; init; }

    public required int Port { get; init; }
}
