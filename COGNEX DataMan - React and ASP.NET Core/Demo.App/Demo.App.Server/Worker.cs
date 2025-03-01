using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace Demo.App.Server;

[SignalRHub]
public class DataHub(WorkerService workerService) : Hub
{
    private readonly WorkerService _workerService = workerService;

    public async Task SetLoggingEnabled(bool isEnabled)
    {
        _workerService.SetLoggingEnabled(isEnabled);
        await Clients.All.SendAsync("LoggingStatusChanged", isEnabled);
    }
}

// Worker Service
public class WorkerService(IHubContext<DataHub> hubContext) : BackgroundService
{
    private readonly IHubContext<DataHub> _hubContext = hubContext;

    private bool _loggingEnabled = false;

    public void SetLoggingEnabled(bool isEnabled)
    {
        _loggingEnabled = isEnabled;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken); // Simulate incoming event every 5 sec

            if (_loggingEnabled)
            {
                await _hubContext.Clients.All.SendAsync(
                    "Logs",
                    $"New Event at {DateTime.UtcNow}",
                    cancellationToken: stoppingToken
                );
            }
        }
    }
}
