namespace Demo.App.Server.Models;

public record SerSystemConnector : Connector
{
    public required string PortName { get; init; }

    public required int Baudrate { get; init; }
}
