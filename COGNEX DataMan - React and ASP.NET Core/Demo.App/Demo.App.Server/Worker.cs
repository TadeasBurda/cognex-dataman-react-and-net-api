// TODO: Add dispose method to cleanup resources

using Cognex.DataMan.SDK.Discovery;
using Demo.App.Server.Models;
using Demo.App.Server.Services;

namespace Demo.App.Server;

internal enum ScannerLoggerState
{
    Disabled,
    Enabled,
}

internal class Worker(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private readonly EthSystemDiscoverer _ethSystemDiscoverer = new();
    private readonly SerSystemDiscoverer _serSystemDiscoverer = new();

    internal Func<Connector, Task>? SendDiscoveredConnectorAsync { get; set; }

    internal Scanner? Scanner { get; private set;  }

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

        var scanner = scope.ServiceProvider.GetRequiredService<Scanner>();
        Scanner = scanner;

        _ethSystemDiscoverer.SystemDiscovered += (e) =>
        {
            SendDiscoveredConnectorAsync?.Invoke(
                new EthSystemConnector()
                {
                    IpAddress = e.IPAddress.ToString(),
                    Name = e.Name,
                    Port = e.Port,
                    SerialNumber = e.SerialNumber,
                }
            );
        };
        _serSystemDiscoverer.SystemDiscovered += (e) =>
        {
            SendDiscoveredConnectorAsync?.Invoke(
                new SerSystemConnector()
                {
                    Baudrate = e.Baudrate,
                    Name = e.Name,
                    PortName = e.PortName,
                    SerialNumber = e.SerialNumber,
                }
            );
        };

        _ethSystemDiscoverer.Discover();
        _serSystemDiscoverer.Discover();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
