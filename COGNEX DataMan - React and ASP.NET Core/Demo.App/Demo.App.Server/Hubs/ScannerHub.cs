using Demo.App.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface IScannerHub
{
    Task Received(string message);

    Task Discovered(Connector connector);
}

internal class ScannerHub : Hub<IScannerHub>
{
    public ScannerHub(Worker worker)
    {
        worker.SendScannerMessageAsync = SendScannerMessageAsync;
        worker.SendDiscoveredConnectorAsync = SendDiscoveredConnectorAsync;
    }

    private async Task SendDiscoveredConnectorAsync(Connector connector)
    {
        await Clients.All.Discovered(connector);
    }

    private async Task SendScannerMessageAsync(string message)
    {
        await Clients.All.Received(message);
    }
}
