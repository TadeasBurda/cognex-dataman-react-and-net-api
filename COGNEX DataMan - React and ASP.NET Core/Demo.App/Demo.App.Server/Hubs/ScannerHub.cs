using Demo.App.Server.Models;
using Demo.App.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface IScannerHub
{
    Task Received(string message);

    Task Discovered(Connector connector);
}

internal class ScannerHub : Hub<IScannerHub>
{
    public ScannerHub(Worker worker, Scanner scanner)
    {
        worker.SendDiscoveredConnectorAsync = SendDiscoveredConnectorAsync;

        scanner.SendScannerMessageAsync = SendScannerMessageAsync;
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
