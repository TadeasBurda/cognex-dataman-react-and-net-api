using System.Diagnostics;
using System.Reflection;
using Cognex.DataMan.SDK;
using Demo.App.Server.Services;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace Demo.App.Server;

internal enum ScannerLoggerState
{
    Disabled,
    Enabled,
}

[SignalRHub]
public class DataHub(WorkerService workerService) : Hub
{
    private readonly WorkerService _workerService = workerService;

    public async Task SetLoggingEnabled(bool isEnabled)
    {
        _workerService.SetScannerLogging(isEnabled);
        await Clients.All.SendAsync("LoggingStatusChanged", isEnabled);
    }
}

// Worker Service
public class WorkerService(ILogger<WorkerService> logger, IHubContext<DataHub> hubContext)
    : BackgroundService
{
    private readonly ILogger<WorkerService> _logger = logger;
    private readonly IHubContext<DataHub> _hubContext = hubContext;

    private ISystemConnector? _connector;
    private ScannerLogger? _scannerLogger;

    private void Log(string message)
    {
        if (_scannerLogger == null)
        {
            _logger.LogWarning("ScannerLogger is not initialized");
            return;
        }

        var stackTrace = new StackTrace();
        var function = stackTrace.GetFrame(1)?.GetMethod()?.Name;

        if (function == null)
        {
            _logger.LogWarning("Failed to get function name");
            return;
        }

        _scannerLogger.Log(function, message);
    }

    public void SetScannerLogging(bool isEnabled)
    {
        if (_connector == null)
        {
            _logger.LogWarning("Connector is not initialized");
            return;
        }

        if (_scannerLogger == null)
        {
            _logger.LogWarning("ScannerLogger is not initialized");
            return;
        }

        if (_connector.Logger == null)
        {
            _logger.LogWarning("Connector.Logger is not initialized");
            return;
        }

        _connector.Logger.Enabled = _scannerLogger.Enabled = isEnabled;

        _logger.LogInformation(
            "Scanner logging is {LoggingStatus}",
            isEnabled ? ScannerLoggerState.Enabled : ScannerLoggerState.Disabled
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken); // Simulate incoming event every 5 sec

            if (_scannerLogger != null && _scannerLogger.Enabled)
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
