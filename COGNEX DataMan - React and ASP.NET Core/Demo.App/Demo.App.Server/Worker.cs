// TODO: Add dispose method to cleanup resources

using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using Cognex.DataMan.SDK.Utils;
using Demo.App.Server.Hubs;
using Demo.App.Server.Models;
using Demo.App.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Drawing;
using System.Net;
using System.Xml;

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
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<Worker> _logger = logger;
    private readonly IHubContext<LoggingHub> _loggingHubContext = hubContext;
    private readonly EthSystemDiscoverer _ethSystemDiscoverer = new();
    private readonly SerSystemDiscoverer _serSystemDiscoverer = new();

    private bool _autoReconnect = false;
    private bool _autoconnect = false;

    private ISystemConnector? _systemConnector;
    private ScannerLogger? _scannerLogger;
    private DataManSystem? _dataManSystem;
    private ResultCollector? _resultCollector;

    internal Func<string, Task>? SendLogMessageAsync { get; set; }
    internal Func<string, Task>? SendConnectLogMessageAsync { get; set; }
    internal Func<string, Task>? SendScannerMessageAsync { get; set; }
    internal Func<Connector, Task>? SendDiscoveredConnectorAsync { get; set; }
    internal Func<System.Drawing.Image, Task>? SendImageAsync { get; set; }
    internal Func<Task>? SendSystemConnectedAsync { get; set; }
    internal Func<Task>? SendSystemDisconnectedAsync { get; set; }
    internal Func<Task>? SendSystemWentOnlineAsync { get; set; }
    internal Func<Task>? SendSystemWentOfflineAsync { get; set; }
    internal Func<Task>? SendKeepAliveResponseMissedAsync { get; set; }

    internal void Connect(
        bool autoReconnect,
        IPAddress address,
        int port,
        string password,
        bool runKeepAliveThread
    )
    {
        var ethSystemConnector = new Cognex.DataMan.SDK.EthSystemConnector(address, port)
        {
            UserName = "admin",
            Password = password,
        };

        ConnectInternal(autoReconnect, ethSystemConnector, runKeepAliveThread);
    }

    internal void Connect(
        bool autoReconnect,
        string portName,
        int baudrate,
        bool runKeepAliveThread
    )
    {
        var serSystemConnector = new Cognex.DataMan.SDK.SerSystemConnector(portName, baudrate);

        ConnectInternal(autoReconnect, serSystemConnector, runKeepAliveThread);
    }

    private void ConnectInternal(
        bool autoReconnect,
        ISystemConnector systemConnector,
        bool runKeepAliveThread
    )
    {
        _autoReconnect = autoReconnect;
        _autoconnect = false;

        try
        {
            _systemConnector = systemConnector;

            _dataManSystem = new DataManSystem(_systemConnector) { DefaultTimeout = 5000 };

            _dataManSystem.SystemConnected += OnSystemConnected;
            _dataManSystem.SystemDisconnected += OnSystemDisconnected;
            _dataManSystem.SystemWentOnline += OnSystemWentOnline;
            _dataManSystem.SystemWentOffline += OnSystemWentOffline;
            _dataManSystem.KeepAliveResponseMissed += OnKeepAliveResponseMissed;
            _dataManSystem.BinaryDataTransferProgress += OnBinaryDataTransferProgress;
            _dataManSystem.OffProtocolByteReceived += OffProtocolByteReceived;
            _dataManSystem.AutomaticResponseArrived += AutomaticResponseArrived;

            // Subscribe to events that are signalled when the device sends auto-responses.
            ResultTypes requested_result_types =
                ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
            _resultCollector = new ResultCollector(_dataManSystem, requested_result_types);
            _resultCollector.ComplexResultCompleted += OnComplexResultCompleted;
            _resultCollector.SimpleResultDropped += OnSimpleResultDropped;

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

            SendConnectLogMessageAsync?.Invoke("Failed to connect: " + ex.ToString());
        }

        _autoconnect = true;
    }

    private void CleanupConnection()
    {
        if (_dataManSystem != null)
        {
            _dataManSystem.SystemConnected -= OnSystemConnected;
            _dataManSystem.SystemDisconnected -= OnSystemDisconnected;
            _dataManSystem.SystemWentOnline -= OnSystemWentOnline;
            _dataManSystem.SystemWentOffline -= OnSystemWentOffline;
            _dataManSystem.KeepAliveResponseMissed -= OnKeepAliveResponseMissed;
            _dataManSystem.BinaryDataTransferProgress -= OnBinaryDataTransferProgress;
            _dataManSystem.OffProtocolByteReceived -= OffProtocolByteReceived;
            _dataManSystem.AutomaticResponseArrived -= AutomaticResponseArrived;
        }

        _systemConnector = null;
        _dataManSystem = null;
    }

    private void OnComplexResultCompleted(object _, ComplexResult e)
    {
        var images = new List<System.Drawing.Image>();
        var image_graphics = new List<string>();
        string? read_result = null;
        var result_id = -1;
        var collected_results = ResultTypes.None;

        // Take a reference or copy values from the locked result info object. This is done
        // so that the lock is used only for a short period of time.
        _semaphoreSlim.Wait();
        try
        {
            foreach (var simple_result in e.SimpleResults)
            {
                collected_results |= simple_result.Id.Type;

                switch (simple_result.Id.Type)
                {
                    case ResultTypes.Image:
                        var image = ImageArrivedEventArgs.GetImageFromImageBytes(
                            simple_result.Data
                        );
                        if (image != null)
                            images.Add(image);
                        break;

                    case ResultTypes.ImageGraphics:
                        image_graphics.Add(simple_result.GetDataAsString());
                        break;

                    case ResultTypes.ReadXml:
                        read_result = GetReadStringFromResultXml(simple_result.GetDataAsString());
                        result_id = simple_result.Id.Id;
                        break;

                    case ResultTypes.ReadString:
                        read_result = simple_result.GetDataAsString();
                        result_id = simple_result.Id.Id;
                        break;
                }
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        SendConnectLogMessageAsync?.Invoke(
            string.Format(
                "Complex result arrived: resultId = {0}, read result = {1}",
                result_id,
                e
            )
        );
        Log("Complex result contains", string.Format("{0}", collected_results.ToString()));

        if (images.Count > 0)
        {
            var first_image = images[0];

            Size image_size = Gui.FitImageInControl(first_image.Size, new Size(400, 400));
            var fitted_image = Gui.ResizeImageToBitmap(first_image, image_size);

            if (image_graphics.Count > 0)
            {
                using (Graphics g = Graphics.FromImage(fitted_image))
                {
                    foreach (var graphics in image_graphics)
                    {
                        ResultGraphics rg = GraphicsResultParser.Parse(
                            graphics,
                            new Rectangle(0, 0, image_size.Width, image_size.Height)
                        );
                        ResultGraphicsRenderer.PaintResults(g, rg);
                    }
                }
            }

            SendImageAsync?.Invoke(fitted_image);
        }

        if (read_result != null)
            SendScannerMessageAsync?.Invoke(read_result);
    }

    private string GetReadStringFromResultXml(string resultXml)
    {
        try
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(resultXml);

            XmlNode full_string_node = doc.SelectSingleNode("result/general/full_string");

            if (full_string_node != null && _dataManSystem != null && _dataManSystem.State == ConnectionState.Connected)
            {
                XmlAttribute encoding = full_string_node.Attributes["encoding"];
                if (encoding != null && encoding.InnerText == "base64")
                {
                    if (!string.IsNullOrEmpty(full_string_node.InnerText))
                    {
                        byte[] code = Convert.FromBase64String(full_string_node.InnerText);
                        return _dataManSystem.Encoding.GetString(code, 0, code.Length);
                    }
                    else
                    {
                        return "";
                    }
                }

                return full_string_node.InnerText;
            }
        }
        catch
        {
        }

        return "";
    }

    private void OnSimpleResultDropped(object _, SimpleResult e)
    {
        SendConnectLogMessageAsync?.Invoke(
            string.Format("Partial result dropped: {0}, id={1}", e.Id.Type.ToString(), e.Id.Id)
        );
    }

    private void AutomaticResponseArrived(object _, AutomaticResponseArrivedEventArgs args)
    {
        Log(
            "AutomaticResponseArrived",
            string.Format(
                "Type={0}, Id={1}, Data={2} bytes",
                args.DataType.ToString(),
                args.ResponseId,
                args.Data != null ? args.Data.Length : 0
            )
        );
    }

    private void OffProtocolByteReceived(object _, OffProtocolByteReceivedEventArgs args)
    {
        Log("OffProtocolByteReceived", string.Format("{0}", (char)args.Byte));
    }

    private void OnBinaryDataTransferProgress(object _, BinaryDataTransferProgressEventArgs args)
    {
        Log(
            "OnBinaryDataTransferProgress",
            string.Format(
                "{0}: {1}% of {2} bytes (Type={3}, Id={4})",
                args.Direction == TransferDirection.Incoming ? "Receiving" : "Sending",
                args.TotalDataSize > 0
                    ? (int)(100 * (args.BytesTransferred / (double)args.TotalDataSize))
                    : -1,
                args.TotalDataSize,
                args.ResultType.ToString(),
                args.ResponseId
            )
        );
    }

    private void OnKeepAliveResponseMissed(object _, EventArgs __)
    {
        SendKeepAliveResponseMissedAsync?.Invoke();
    }

    private void OnSystemWentOffline(object _, EventArgs __)
    {
        SendSystemWentOfflineAsync?.Invoke();
    }

    private void OnSystemWentOnline(object _, EventArgs __)
    {
        SendSystemWentOnlineAsync?.Invoke();
    }

    private void OnSystemDisconnected(object _, EventArgs __)
    {
        SendSystemDisconnectedAsync?.Invoke();
    }

    private void OnSystemConnected(object _, EventArgs __)
    {
        SendSystemConnectedAsync?.Invoke();
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

    private void Log(string function, string message)
    {
        if (_scannerLogger == null)
        {
            _logger.LogWarning("ScannerLogger is not initialized");
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

    internal void Disconnect()
    {
        if (_dataManSystem == null || _dataManSystem.State != ConnectionState.Connected)
            return;

        _autoconnect = false;
        _dataManSystem.Disconnect();

        CleanupConnection();

        _resultCollector?.ClearCachedResults();
        _resultCollector = null;
    }

    internal void Refresh()
    {
        if (_ethSystemDiscoverer.IsDiscoveryInProgress || _serSystemDiscoverer.IsDiscoveryInProgress)
            return;

        _ethSystemDiscoverer.Discover();
        _serSystemDiscoverer.Discover();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        _scannerLogger = scope.ServiceProvider.GetRequiredService<ScannerLogger>();
        InitializeScannerLogger(_scannerLogger);

        _ethSystemDiscoverer.SystemDiscovered += (e) =>
        {
            SendDiscoveredConnectorAsync?.Invoke(new Demo.App.Server.Models.EthSystemConnector()
            {
                IpAddress = e.IPAddress.ToString(),
                Name = e.Name,
                Port = e.Port,
                SerialNumber = e.SerialNumber
            });
        };
        _serSystemDiscoverer.SystemDiscovered += (e) =>
        {
            SendDiscoveredConnectorAsync?.Invoke(new Demo.App.Server.Models.SerSystemConnector()
            {
                Baudrate = e.Baudrate,
                Name = e.Name,
                PortName = e.PortName,
                SerialNumber = e.SerialNumber
            });
        };

        _ethSystemDiscoverer.Discover();
        _serSystemDiscoverer.Discover();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
