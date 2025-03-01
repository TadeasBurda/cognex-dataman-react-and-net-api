using System.Diagnostics;
using Cognex.DataMan.SDK;
using Demo.App.Server.Hubs;
using Demo.App.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace Demo.App.Server;

internal enum ScannerLoggerState
{
    Disabled,
    Enabled,
}

internal class Worker(
    IServiceProvider serviceProvider,
    ILogger<Worker> logger,
    IHubContext<LoggingHub> hubContext
) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<Worker> _logger = logger;
    private readonly IHubContext<LoggingHub> _loggingHubContext = hubContext;

    private ISystemConnector? _connector;
    private ScannerLogger? _scannerLogger;

    internal Func<string, Task>? SendLogMessageAsync { get; set; }

    private void InitializeScannerLogger(ScannerLogger scannerLogger)
    {
        scannerLogger.ReceivedAsync = async message =>
        {
            if (SendLogMessageAsync == null)
            {
                _logger.LogWarning("{Func} is not initialized", nameof(SendLogMessageAsync));
                return;
            }

            await SendLogMessageAsync(message);
        };
    }

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
        using var scope = _serviceProvider.CreateScope();

        _scannerLogger = scope.ServiceProvider.GetRequiredService<ScannerLogger>();
        InitializeScannerLogger(_scannerLogger);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
