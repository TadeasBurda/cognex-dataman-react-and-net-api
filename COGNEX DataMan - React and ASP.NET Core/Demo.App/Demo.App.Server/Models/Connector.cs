namespace Demo.App.Server.Models;

public abstract record Connector
{
    public required string Name { get; init; }

    public required string SerialNumber { get; init; }
}
