using Demo.App.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface ILoggingHub
{
    Task ReceivedLogs(string message);

    Task ConnectLogs(string message);
}

internal class LoggingHub : Hub<ILoggingHub>
{
    public LoggingHub(Scanner scanner)
    {
        scanner.SendLogMessageAsync = SendLogMessageAsync;
        scanner.SendConnectLogMessageAsync = SendConnectLogMessageAsync;
        scanner.SendSystemConnectedAsync = SendSystemConnectedAsync;
        scanner.SendSystemDisconnectedAsync = SendSystemDisconnectedAsync;
        scanner.SendSystemWentOnlineAsync = SendSystemWentOnlineAsync;
        scanner.SendSystemWentOfflineAsync = SendSystemWentOfflineAsync;
        scanner.SendKeepAliveResponseMissedAsync = SendKeepAliveResponseMissedAsync;
    }

    private async Task SendConnectLogMessageAsync(string arg)
    {
        await Clients.All.ConnectLogs(arg);
    }

    private async Task SendKeepAliveResponseMissedAsync()
    {
        await Clients.All.ConnectLogs("Keep-alive response missed");
    }

    private async Task SendSystemWentOfflineAsync()
    {
        await Clients.All.ConnectLogs("System went offline");
    }

    private async Task SendSystemWentOnlineAsync()
    {
        await Clients.All.ConnectLogs("System went online");
    }

    private async Task SendSystemDisconnectedAsync()
    {
        await Clients.All.ConnectLogs("System disconnected");
    }

    private async Task SendSystemConnectedAsync()
    {
        await Clients.All.ConnectLogs("System connected");
    }

    private async Task SendLogMessageAsync(string message)
    {
        await Clients.All.ReceivedLogs(message);
    }
}
