using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server.Hubs;

public interface ILoggingHub
{
    Task Logs(string message);
}

internal class LoggingHub : Hub<ILoggingHub>
{
    public LoggingHub(Worker worker)
    {
        worker.SendLogMessageAsync = SendLogMessageAsync;
    }

    private async Task SendLogMessageAsync(string message)
    {
        await Clients.All.Logs(message);
    }
}
