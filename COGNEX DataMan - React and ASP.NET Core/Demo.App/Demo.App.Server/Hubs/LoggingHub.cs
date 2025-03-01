using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface ILoggingHub
{
    Task Logs(string message);

    Task ConnectLog(string message);
}

internal class LoggingHub : Hub<ILoggingHub>
{
    public LoggingHub(Worker worker)
    {
        worker.SendLogMessageAsync = SendLogMessageAsync;
        worker.SendSystemConnectedAsync = SendSystemConnectedAsync;
        worker.SendSystemDisconnectedAsync = SendSystemDisconnectedAsync;
        worker.SendSystemWentOnlineAsync = SendSystemWentOnlineAsync;
        worker.SendSystemWentOfflineAsync = SendSystemWentOfflineAsync;
        worker.SendKeepAliveResponseMissedAsync = SendKeepAliveResponseMissedAsync;
    }

    private async Task SendKeepAliveResponseMissedAsync()
    {
        await Clients.All.ConnectLog("Keep-alive response missed");
    }

    private async Task SendSystemWentOfflineAsync()
    {
        await Clients.All.ConnectLog("System went offline");
    }

    private async Task SendSystemWentOnlineAsync()
    {
        await Clients.All.ConnectLog("System went online");
    }

    private async Task SendSystemDisconnectedAsync()
    {
        await Clients.All.ConnectLog("System disconnected");
    }

    private async Task SendSystemConnectedAsync()
    {
        await Clients.All.ConnectLog("System connected");
    }

    private async Task SendLogMessageAsync(string message)
    {
        await Clients.All.Logs(message);
    }
}
