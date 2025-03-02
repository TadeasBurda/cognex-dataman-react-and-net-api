using System.Net;
using System.Runtime.Versioning;

namespace Demo.App.Server.Endpoints;

internal sealed record EthSystemConnectorRequest
{
    internal required string IpAddress { get; init; }
    internal required int Port { get; init; }
    internal required string Password { get; init; }
    internal required bool RunKeepAliveThread { get; init; }
    internal required bool AutoReconnect { get; init; }
}

internal sealed record SerSystemConnectorRequest
{
    internal required string PortName { get; init; }
    internal required int Baudrate { get; init; }
    internal required bool RunKeepAliveThread { get; init; }
    internal required bool AutoReconnect { get; init; }
}

internal static class ScannerEndpoints
{
    [SupportedOSPlatform("windows")]
    internal static WebApplication AddScannerEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/api/scanner/logging",
            (bool enable, Worker worker) =>
            {
                if (worker.Scanner == null)
                {
                    return Results.Problem("Scanner is not initialized.");
                }
                worker.Scanner.SetScannerLogging(enable);
                return Results.Ok();
            }
        );
        app.MapPost(
            "/api/scanner/live-display",
            (bool enable, Worker worker) =>
            {
                if (worker.Scanner == null)
                {
                    return Results.Problem("Scanner is not initialized.");
                }
                worker.Scanner.SetLiveDisplay(enable);
                return Results.Ok();
            }
        );
        app.MapPost(
            "/api/scanner/trigger",
            (bool on, Worker worker) =>
            {
                if (worker.Scanner == null)
                {
                    return Results.Problem("Scanner is not initialized.");
                }
                if (on)
                {
                    worker.Scanner.TriggerOn();
                }
                else
                {
                    worker.Scanner.TriggerOff();
                }
                return Results.Ok();
            }
        );
        app.MapPost(
            "/api/scanner/connect/eth",
            (EthSystemConnectorRequest body, Worker worker) =>
            {
                if (worker.Scanner == null)
                {
                    return Results.Problem("Scanner is not initialized.");
                }
                worker.Scanner.Connect(
                    autoReconnect: body.AutoReconnect,
                    address: IPAddress.Parse(body.IpAddress),
                    port: body.Port,
                    password: body.Password,
                    runKeepAliveThread: body.RunKeepAliveThread
                );
                return Results.Ok();
            }
        );
        app.MapPost(
            "/api/scanner/connect/ser",
            (SerSystemConnectorRequest body, Worker worker) =>
            {
                if (worker.Scanner == null)
                {
                    return Results.Problem("Scanner is not initialized.");
                }
                worker.Scanner.Connect(
                    autoReconnect: body.AutoReconnect,
                    portName: body.PortName,
                    baudrate: body.Baudrate,
                    runKeepAliveThread: body.RunKeepAliveThread
                );
                return Results.Ok();
            }
        );
        app.MapPost(
            "/api/scanner/disconnect",
            (Worker worker) =>
            {
                if (worker.Scanner == null)
                {
                    return Results.Problem("Scanner is not initialized.");
                }
                worker.Scanner.Disconnect();
                return Results.Ok();
            }
        );
        return app;
    }
}
