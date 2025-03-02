using Demo.App.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface IScannerHub
{
    Task Received(string message);

    Task List(Connector[] connectors);
}

internal class ScannerHub : Hub<IScannerHub>
{
    public ScannerHub(Worker worker)
    {
        worker.SendScannerMessageAsync = SendScannerMessageAsync;
        worker.SendListConnectorsAsync = SendListConnectorsAsync;
    }

    private async Task SendListConnectorsAsync(Connector[] connectors)
    {
        await Clients.All.List(connectors);
    }

    private async Task SendScannerMessageAsync(string message)
    {
        await Clients.All.Received(message);
    }
}
