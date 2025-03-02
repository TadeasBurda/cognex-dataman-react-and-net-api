using System.Net;

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
    internal static WebApplication AddScannerEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/api/scanner/logging",
            (bool enable, Worker worker) =>
            {
                worker.SetScannerLogging(enable);
            }
        );
        app.MapPost(
            "/api/scanner/live-display",
            (bool enable, Worker worker) =>
            {
                worker.SetLiveDisplay(enable);
            }
        );
        app.MapPost(
            "/api/scanner/trigger",
            (bool on, Worker worker) =>
            {
                if (on)
                {
                    worker.TriggerOn();
                }
                else
                {
                    worker.TriggerOff();
                }
            }
        );
        app.MapPost(
            "/api/scanner/connect/eth",
            (EthSystemConnectorRequest body, Worker worker) =>
            {
                worker.Connect(
                    autoReconnect: body.AutoReconnect,
                    address: IPAddress.Parse(body.IpAddress),
                    port: body.Port,
                    password: body.Password,
                    runKeepAliveThread: body.RunKeepAliveThread
                );
            }
        );
        app.MapPost(
            "/api/scanner/connect/ser",
            (SerSystemConnectorRequest body, Worker worker) =>
            {
                worker.Connect(
                    autoReconnect: body.AutoReconnect,
                    portName: body.PortName,
                    baudrate: body.Baudrate,
                    runKeepAliveThread: body.RunKeepAliveThread
                );
            }
        );
        app.MapPost(
            "/api/scanner/disconnect",
            (Worker worker) =>
            {
                worker.Disconnect();
            }
        );
        return app;
    }
}
