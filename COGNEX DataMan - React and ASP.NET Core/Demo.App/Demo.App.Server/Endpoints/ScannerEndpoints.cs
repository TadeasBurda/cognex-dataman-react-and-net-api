using System.Net;
using System.Runtime.Versioning;
using Demo.App.Server.Models;
using Demo.App.Server.Services;

namespace Demo.App.Server.Endpoints;

internal static class ScannerEndpoints
{
    [SupportedOSPlatform("windows")]
    internal static WebApplication AddScannerEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/api/scanner/logging",
            (bool enable, Scanner scanner) =>
            {
                scanner.SetScannerLogging(enable);
            }
        );
        app.MapPost(
            "/api/scanner/live-display",
            (bool enable, Scanner scanner) =>
            {
                scanner.SetLiveDisplay(enable);
            }
        );
        app.MapPost(
            "/api/scanner/trigger",
            (bool on, Scanner scanner) =>
            {
                if (on)
                {
                    scanner.TriggerOn();
                }
                else
                {
                    scanner.TriggerOff();
                }
            }
        );
        app.MapPost(
            "/api/scanner/connect/eth",
            (EthSystemConnectorRequest body, Scanner scanner) =>
            {
                scanner.Connect(
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
            (SerSystemConnectorRequest body, Scanner scanner) =>
            {
                scanner.Connect(
                    autoReconnect: body.AutoReconnect,
                    portName: body.PortName,
                    baudrate: body.Baudrate,
                    runKeepAliveThread: body.RunKeepAliveThread
                );
            }
        );
        app.MapPost(
            "/api/scanner/disconnect",
            (Scanner scanner) =>
            {
                scanner.Disconnect();
            }
        );
        return app;
    }
}
