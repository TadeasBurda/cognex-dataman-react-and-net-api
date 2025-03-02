namespace Demo.App.Server.Models;

internal class SerSystemConnector : Connector
{
    public required string PortName { get; init; }

    public required int Baudrate { get; init; }
}
