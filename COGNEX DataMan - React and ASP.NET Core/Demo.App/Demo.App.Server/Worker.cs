using System.Diagnostics;
using System.Net;
using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using Cognex.DataMan.SDK.Utils;
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

    private bool _autoReconnect = false;
    private bool _autoconnect = false;

    private ISystemConnector? _systemConnector;
    private ScannerLogger? _scannerLogger;
    private DataManSystem? _dataManSystem;
    private ResultCollector? _resultCollector;

    internal Func<string, Task>? SendLogMessageAsync { get; set; }

    public void Connect(
        bool autoReconnect,
        IPAddress address,
        int port,
        string password,
        bool runKeepAliveThread
    )
    {
        _autoReconnect = autoReconnect;
        _autoconnect = false;

        try
        {
            var ethSystemConnector = new EthSystemConnector(address, port)
            {
                UserName = "admin",
                Password = password,
            };

            _systemConnector = ethSystemConnector;

            _dataManSystem = new DataManSystem(_systemConnector) { DefaultTimeout = 5000 };

            // Subscribe to events that are signalled when the system is connected / disconnected.
            _dataManSystem.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
            _dataManSystem.SystemDisconnected += new SystemDisconnectedHandler(
                OnSystemDisconnected
            );
            _dataManSystem.SystemWentOnline += new SystemWentOnlineHandler(OnSystemWentOnline);
            _dataManSystem.SystemWentOffline += new SystemWentOfflineHandler(OnSystemWentOffline);
            _dataManSystem.KeepAliveResponseMissed += new KeepAliveResponseMissedHandler(
                OnKeepAliveResponseMissed
            );
            _dataManSystem.BinaryDataTransferProgress += new BinaryDataTransferProgressHandler(
                OnBinaryDataTransferProgress
            );
            _dataManSystem.OffProtocolByteReceived += new OffProtocolByteReceivedHandler(
                OffProtocolByteReceived
            );
            _dataManSystem.AutomaticResponseArrived += new AutomaticResponseArrivedHandler(
                AutomaticResponseArrived
            );

            // Subscribe to events that are signalled when the device sends auto-responses.
            ResultTypes requested_result_types =
                ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
            _resultCollector = new ResultCollector(_dataManSystem, requested_result_types);
            _resultCollector.ComplexResultCompleted += Results_ComplexResultCompleted;
            _resultCollector.SimpleResultDropped += Results_SimpleResultDropped;

            _dataManSystem.SetKeepAliveOptions(runKeepAliveThread, 3000, 1000);

            _dataManSystem.Connect();

            try
            {
                _dataManSystem.SetResultTypes(requested_result_types);
            }
            catch { }
        }
        catch (Exception ex)
        {
            CleanupConnection();

            AddListItem("Failed to connect: " + ex.ToString());
        }

        _autoconnect = true;
        RefreshGui();
    }

    private void RefreshGui()
    {
        throw new NotImplementedException();
    }

    private void AddListItem(string v)
    {
        throw new NotImplementedException();
    }

    private void CleanupConnection()
    {
        throw new NotImplementedException();
    }

    private void Results_SimpleResultDropped(object sender, SimpleResult e)
    {
        throw new NotImplementedException();
    }

    private void Results_ComplexResultCompleted(object sender, ComplexResult e)
    {
        throw new NotImplementedException();
    }

    private void AutomaticResponseArrived(object sender, AutomaticResponseArrivedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OffProtocolByteReceived(object sender, OffProtocolByteReceivedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnBinaryDataTransferProgress(
        object sender,
        BinaryDataTransferProgressEventArgs args
    )
    {
        throw new NotImplementedException();
    }

    private void OnKeepAliveResponseMissed(object sender, EventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnSystemWentOffline(object sender, EventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnSystemWentOnline(object sender, EventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnSystemDisconnected(object sender, EventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnSystemConnected(object sender, EventArgs args)
    {
        throw new NotImplementedException();
    }

    public void Connect(bool autoReconnect, string portName, int baudrate, bool runKeepAliveThread)
    {
        _autoReconnect = autoReconnect;
        _autoconnect = false;

        try
        {
            var serSystemConnector = new SerSystemConnector(portName, baudrate);

            _systemConnector = serSystemConnector;

            _dataManSystem = new DataManSystem(_systemConnector) { DefaultTimeout = 5000 };

            // Subscribe to events that are signalled when the system is connected / disconnected.
            _dataManSystem.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
            _dataManSystem.SystemDisconnected += new SystemDisconnectedHandler(
                OnSystemDisconnected
            );
            _dataManSystem.SystemWentOnline += new SystemWentOnlineHandler(OnSystemWentOnline);
            _dataManSystem.SystemWentOffline += new SystemWentOfflineHandler(OnSystemWentOffline);
            _dataManSystem.KeepAliveResponseMissed += new KeepAliveResponseMissedHandler(
                OnKeepAliveResponseMissed
            );
            _dataManSystem.BinaryDataTransferProgress += new BinaryDataTransferProgressHandler(
                OnBinaryDataTransferProgress
            );
            _dataManSystem.OffProtocolByteReceived += new OffProtocolByteReceivedHandler(
                OffProtocolByteReceived
            );
            _dataManSystem.AutomaticResponseArrived += new AutomaticResponseArrivedHandler(
                AutomaticResponseArrived
            );

            // Subscribe to events that are signalled when the device sends auto-responses.
            ResultTypes requested_result_types =
                ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
            _resultCollector = new ResultCollector(_dataManSystem, requested_result_types);
            _resultCollector.ComplexResultCompleted += Results_ComplexResultCompleted;
            _resultCollector.SimpleResultDropped += Results_SimpleResultDropped;

            _dataManSystem.SetKeepAliveOptions(runKeepAliveThread, 3000, 1000);

            _dataManSystem.Connect();

            try
            {
                _dataManSystem.SetResultTypes(requested_result_types);
            }
            catch { }
        }
        catch (Exception ex)
        {
            CleanupConnection();

            AddListItem("Failed to connect: " + ex.ToString());
        }

        _autoconnect = true;
        RefreshGui();
    }

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
        if (_systemConnector == null)
        {
            _logger.LogWarning("Connector is not initialized");
            return;
        }

        if (_scannerLogger == null)
        {
            _logger.LogWarning("ScannerLogger is not initialized");
            return;
        }

        _systemConnector.Logger ??= _scannerLogger;
        _systemConnector.Logger.Enabled = _scannerLogger.Enabled = isEnabled;

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
