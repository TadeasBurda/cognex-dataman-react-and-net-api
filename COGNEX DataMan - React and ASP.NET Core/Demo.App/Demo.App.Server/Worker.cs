using Cognex.DataMan.SDK.Discovery;
using Demo.App.Server.Models;
using Demo.App.Server.Services;

namespace Demo.App.Server;

internal sealed class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
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

        _ethSystemDiscoverer.Dispose();
        _serSystemDiscoverer.Dispose();

        Scanner?.Dispose();
    }

    internal void Refresh()
    {
        if (
            _ethSystemDiscoverer.IsDiscoveryInProgress || _serSystemDiscoverer.IsDiscoveryInProgress
        )
            return;

        _ethSystemDiscoverer.Discover();
        _serSystemDiscoverer.Discover();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        Scanner = scope.ServiceProvider.GetRequiredService<Scanner>();

        InitializeEthSystemDiscoverer();
        InitializeSerSystemDiscoverer();

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Task was canceled, no action needed
        }
    }

    #region Helpers

    private void InitializeEthSystemDiscoverer()
    {
        _ethSystemDiscoverer.SystemDiscovered += async (e) =>
        {
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
