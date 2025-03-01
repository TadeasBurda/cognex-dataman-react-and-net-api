using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace Demo.App.Server.Hubs;

[SignalRHub]
internal class LoggingHub(Worker worker) : Hub
{
    private readonly Worker _worker = worker;

    public async Task SetLoggingEnabled(bool isEnabled)
    {
        _worker.SetScannerLogging(isEnabled);
        await Clients.All.SendAsync("LoggingStatusChanged", isEnabled);
    }
}
