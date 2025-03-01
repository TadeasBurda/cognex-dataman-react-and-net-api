using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface IScannerHub
{
    Task Received(string message);
}

internal class ScannerHub : Hub<IScannerHub>
{
    public ScannerHub(Worker worker)
    {
        worker.SendScannerMessageAsync = SendScannerMessageAsync;
    }
    private async Task SendScannerMessageAsync(string message)
    {
        await Clients.All.Received(message);
    }
}
