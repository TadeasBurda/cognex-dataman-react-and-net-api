using Cognex.DataMan.SDK.Discovery;
using Demo.App.Server.Models;
using Demo.App.Server.Services;

namespace Demo.App.Server;

internal sealed class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    : BackgroundService
{
    #region Fields

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<Worker> _logger = logger;

    private readonly EthSystemDiscoverer _ethSystemDiscoverer = new();
    private readonly SerSystemDiscoverer _serSystemDiscoverer = new();

    #endregion

    #region Events

    internal Func<Connector, Task>? SendDiscoveredConnectorAsync { get; set; }

    #endregion

    #region Properties

    internal Scanner? Scanner { get; private set; }

    #endregion

    public override void Dispose()
    {
        base.Dispose();

        _logger.LogInformation("Disposing EthSystemDiscoverer and SerSystemDiscoverer.");
        _ethSystemDiscoverer.Dispose();
        _serSystemDiscoverer.Dispose();

        if (Scanner != null)
        {
            _logger.LogInformation("Disposing Scanner.");
            Scanner.Dispose();
        }
    }

    internal void Refresh()
    {
        if (
            _ethSystemDiscoverer.IsDiscoveryInProgress || _serSystemDiscoverer.IsDiscoveryInProgress
        )
        {
            _logger.LogWarning("Discovery is already in progress.");
            return;
        }

        _logger.LogInformation(
            "Starting discovery for EthSystemDiscoverer and SerSystemDiscoverer."
        );
        _ethSystemDiscoverer.Discover();
        _serSystemDiscoverer.Discover();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        Scanner = scope.ServiceProvider.GetRequiredService<Scanner>();
        _logger.LogInformation("Scanner service initialized.");

        InitializeEthSystemDiscoverer();
        InitializeSerSystemDiscoverer();

        try
        {
            _logger.LogInformation("Worker running, waiting for cancellation.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred.");
        }
    }

    #region Helpers

    private void InitializeEthSystemDiscoverer()
    {
        _ethSystemDiscoverer.SystemDiscovered += async (e) =>
        {
            _logger.LogInformation(
                "EthSystem discovered: {Name} ({IPAddress})",
                e.Name,
                e.IPAddress
            );

            if (SendDiscoveredConnectorAsync != null)
            {
                await SendDiscoveredConnectorAsync(
                    new EthSystemConnector
                    {
                        IpAddress = e.IPAddress.ToString(),
                        Name = e.Name,
                        Port = e.Port,
                        SerialNumber = e.SerialNumber,
                    }
                );
            }
        };
        _ethSystemDiscoverer.Discover();
    }

    private void InitializeSerSystemDiscoverer()
    {
        _serSystemDiscoverer.SystemDiscovered += async (e) =>
        {
            _logger.LogInformation("SerSystem discovered: {Name} ({PortName})", e.Name, e.PortName);

            if (SendDiscoveredConnectorAsync != null)
            {
                await SendDiscoveredConnectorAsync(
                    new SerSystemConnector
                    {
                        Baudrate = e.Baudrate,
                        Name = e.Name,
                        PortName = e.PortName,
                        SerialNumber = e.SerialNumber,
                    }
                );
            }
        };
        _serSystemDiscoverer.Discover();
    }

    #endregion
}
