using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
using Demo.App.Server.Hubs;
using Demo.App.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Drawing;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

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
        var ethSystemConnector = new EthSystemConnector(address, port)
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
        var serSystemConnector = new SerSystemConnector(portName, baudrate);

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

            // Subscribe to events that are signalled when the system is connected / disconnected.
            _dataManSystem.SystemConnected += (_, _) =>
            {
                SendSystemConnectedAsync?.Invoke();
            };
            _dataManSystem.SystemDisconnected += (_, _) =>
            {
                SendSystemDisconnectedAsync?.Invoke();
            };
            _dataManSystem.SystemWentOnline += (_, _) =>
            {
                SendSystemWentOnlineAsync?.Invoke();
            };
            _dataManSystem.SystemWentOffline += (_, _) =>
            {
                SendSystemWentOfflineAsync?.Invoke();
            };
            _dataManSystem.KeepAliveResponseMissed += (_, _) =>
            {
                SendKeepAliveResponseMissedAsync?.Invoke();
            };
            _dataManSystem.BinaryDataTransferProgress += (_, args) =>
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
            };
            _dataManSystem.OffProtocolByteReceived += (_, args) =>
            {
                Log("OffProtocolByteReceived", string.Format("{0}", (char)args.Byte));
            };
            _dataManSystem.AutomaticResponseArrived += (_, args) =>
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
            };

            // Subscribe to events that are signalled when the device sends auto-responses.
            ResultTypes requested_result_types =
                ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
            _resultCollector = new ResultCollector(_dataManSystem, requested_result_types);
            _resultCollector.ComplexResultCompleted += (_, e) =>
            {
                List<Image> images = new List<Image>();
                List<string> image_graphics = new List<string>();
                string read_result = null;
                int result_id = -1;
                ResultTypes collected_results = ResultTypes.None;

                // Take a reference or copy values from the locked result info object. This is done
                // so that the lock is used only for a short period of time.
                lock (_currentResultInfoSyncLock)
                {
                    foreach (var simple_result in e.SimpleResults)
                    {
                        collected_results |= simple_result.Id.Type;

                        switch (simple_result.Id.Type)
                        {
                            case ResultTypes.Image:
                                Image image = ImageArrivedEventArgs.GetImageFromImageBytes(
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

                AddListItem(
                    string.Format(
                        "Complex result arrived: resultId = {0}, read result = {1}",
                        result_id,
                        read_result
                    )
                );
                Log("Complex result contains", string.Format("{0}", collected_results.ToString()));

                if (images.Count > 0)
                {
                    Image first_image = images[0];

                    Size image_size = Gui.FitImageInControl(first_image.Size, picResultImage.Size);
                    Image fitted_image = Gui.ResizeImageToBitmap(first_image, image_size);

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

                    if (picResultImage.Image != null)
                    {
                        var image = picResultImage.Image;
                        picResultImage.Image = null;
                        image.Dispose();
                    }

                    picResultImage.Image = fitted_image;
                    picResultImage.Invalidate();
                }

                if (read_result != null)
                    lbReadString.Text = read_result;
            };
            _resultCollector.SimpleResultDropped += (_, e) =>
            {
                AddListItem(string.Format("Partial result dropped: {0}, id={1}", e.Id.Type.ToString(), e.Id.Id));
            };

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        _scannerLogger = scope.ServiceProvider.GetRequiredService<ScannerLogger>();
        InitializeScannerLogger(_scannerLogger);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
