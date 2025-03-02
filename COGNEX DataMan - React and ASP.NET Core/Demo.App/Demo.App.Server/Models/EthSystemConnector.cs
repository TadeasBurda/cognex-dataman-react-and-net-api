namespace Demo.App.Server.Models;

internal class EthSystemConnector : Connector
{
    internal required string IpAddress { get; init; }

    internal required int Port { get; init; }
}
