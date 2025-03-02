using System.Drawing;
using System.Net;
using System.Runtime.Versioning;
using System.Xml;
using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;

namespace Demo.App.Server.Services;

internal enum ScannerLoggerState
{
    Disabled,
    Enabled,
}

internal sealed class Scanner : IDisposable
{
    #region Fields

    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly ILogger<Scanner> _logger;
    private readonly ScannerLogger? _scannerLogger;

    private bool _autoReconnect = false;
    private bool _autoconnect = false;
    private bool _liveDisplay = false;

    private ISystemConnector? _systemConnector;
    private DataManSystem? _dataManSystem;
    private ResultCollector? _resultCollector;

    #endregion

    #region Events

    internal Func<string, Task>? SendLogMessageAsync { get; set; }
    internal Func<string, Task>? SendConnectLogMessageAsync { get; set; }
    internal Func<string, Task>? SendScannerMessageAsync { get; set; }
    internal Func<Image, Task>? SendImageAsync { get; set; }
    internal Func<Task>? SendSystemConnectedAsync { get; set; }
    internal Func<Task>? SendSystemDisconnectedAsync { get; set; }
    internal Func<Task>? SendSystemWentOnlineAsync { get; set; }
    internal Func<Task>? SendSystemWentOfflineAsync { get; set; }
    internal Func<Task>? SendKeepAliveResponseMissedAsync { get; set; }

    #endregion

    #region Constructors

    public Scanner(ILogger<Scanner> logger, ScannerLogger scannerLogger)
    {
        _logger = logger;
        _scannerLogger = scannerLogger;

        _scannerLogger.ReceivedAsync = async message =>
        {
            await SendLogMessageAsync?.Invoke(message);
        };

        _logger.LogInformation("Scanner instance created.");
    }

    #endregion

    public void Dispose()
    {
        _logger.LogInformation("Disposing Scanner instance.");
    }

    internal void Disconnect()
    {
        _logger.LogInformation("Disconnecting...");

        if (_dataManSystem == null || _dataManSystem.State != ConnectionState.Connected)
        {
            _logger.LogWarning("DataManSystem is not connected.");
            return;
        }

        _autoconnect = false;
        _dataManSystem.Disconnect();

        CleanupConnection();

        _resultCollector?.ClearCachedResults();
        _resultCollector = null;

        _logger.LogInformation("Disconnected successfully.");
    }

    internal void TriggerOn()
    {
        _logger.LogInformation("Triggering ON...");

        try
        {
            _dataManSystem?.SendCommand("TRIGGER ON");
            _logger.LogInformation("TRIGGER ON command sent successfully.");
        }
        catch (Exception ex)
        {
            Log(nameof(TriggerOn), "Failed to send TRIGGER ON command: " + ex.ToString());
        }
    }

    internal void TriggerOff()
    {
        _logger.LogInformation("Triggering OFF...");

        try
        {
            _dataManSystem?.SendCommand("TRIGGER OFF");
            _logger.LogInformation("TRIGGER OFF command sent successfully.");
        }
        catch (Exception ex)
        {
            Log(nameof(TriggerOff), "Failed to send TRIGGER OFF command: " + ex.ToString());
        }
    }

    [SupportedOSPlatform("windows")]
    internal void SetLiveDisplay(bool isEnabled)
    {
        _logger.LogInformation("Setting live display to {IsEnabled}.", isEnabled);

        if (_dataManSystem == null)
        {
            _logger.LogWarning("DataManSystem is not initialized");
            return;
        }

        _liveDisplay = isEnabled;

        try
        {
            if (_liveDisplay)
            {
                _dataManSystem.SendCommand("SET LIVEIMG.MODE 2");
                _dataManSystem.BeginGetLiveImage(
                    ImageFormat.jpeg,
                    ImageSize.Sixteenth,
                    ImageQuality.Medium,
                    OnLiveImageArrived,
                    null
                );
                _logger.LogInformation("Live display enabled.");
            }
            else
            {
                _dataManSystem.SendCommand("SET LIVEIMG.MODE 0");
                _logger.LogInformation("Live display disabled.");
            }
        }
        catch (Exception ex)
        {
            Log(nameof(SetLiveDisplay), "Failed to set live image mode: " + ex.ToString());
        }
    }

    internal void SetScannerLogging(bool isEnabled)
    {
        _logger.LogInformation("Setting scanner logging to {IsEnabled}.", isEnabled);

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

    #region Connection

    internal void Connect(
        bool autoReconnect,
        IPAddress address,
        int port,
        string password,
        bool runKeepAliveThread
    )
    {
        _logger.LogInformation(
            "Connecting to {Address}:{Port} with autoReconnect={AutoReconnect}.",
            address,
            port,
            autoReconnect
        );

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
        _logger.LogInformation(
            "Connecting to {PortName} with baudrate={Baudrate} and autoReconnect={AutoReconnect}.",
            portName,
            baudrate,
            autoReconnect
        );

        var serSystemConnector = new Cognex.DataMan.SDK.SerSystemConnector(portName, baudrate);

        ConnectInternal(autoReconnect, serSystemConnector, runKeepAliveThread);
    }

    private void ConnectInternal(
        bool autoReconnect,
        ISystemConnector systemConnector,
        bool runKeepAliveThread
    )
    {
        _logger.LogInformation(
            "Internal connect with autoReconnect={AutoReconnect}.",
            autoReconnect
        );

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
            _dataManSystem.OffProtocolByteReceived += OnOffProtocolByteReceived;
            _dataManSystem.AutomaticResponseArrived += OnAutomaticResponseArrived;

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

            _logger.LogInformation("Connected successfully.");
        }
        catch (Exception ex)
        {
            CleanupConnection();

            SendConnectLogMessageAsync?.Invoke("Failed to connect: " + ex.ToString());
            _logger.LogError(ex, "Failed to connect");
        }

        _autoconnect = true;
    }

    #endregion

    #region Events Handlers

    private void OnAutomaticResponseArrived(object _, AutomaticResponseArrivedEventArgs args)
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

    private void OnOffProtocolByteReceived(object _, OffProtocolByteReceivedEventArgs args)
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

    [SupportedOSPlatform("windows")]
    private void OnComplexResultCompleted(object _, ComplexResult e)
    {
        var images = new List<Image>();
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
            string.Format("Complex result arrived: resultId = {0}, read result = {1}", result_id, e)
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

    private void OnSimpleResultDropped(object _, SimpleResult e)
    {
        SendConnectLogMessageAsync?.Invoke(
            string.Format("Partial result dropped: {0}, id={1}", e.Id.Type.ToString(), e.Id.Id)
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
        SendScannerMessageAsync?.Invoke(string.Empty);
    }

    private void OnSystemConnected(object _, EventArgs __)
    {
        SendSystemConnectedAsync?.Invoke();
    }

    #endregion

    #region Helpers

    private void CleanupConnection()
    {
        _logger.LogInformation("Cleaning up connection...");

        if (_dataManSystem != null)
        {
            _dataManSystem.SystemConnected -= OnSystemConnected;
            _dataManSystem.SystemDisconnected -= OnSystemDisconnected;
            _dataManSystem.SystemWentOnline -= OnSystemWentOnline;
            _dataManSystem.SystemWentOffline -= OnSystemWentOffline;
            _dataManSystem.KeepAliveResponseMissed -= OnKeepAliveResponseMissed;
            _dataManSystem.BinaryDataTransferProgress -= OnBinaryDataTransferProgress;
            _dataManSystem.OffProtocolByteReceived -= OnOffProtocolByteReceived;
            _dataManSystem.AutomaticResponseArrived -= OnAutomaticResponseArrived;
        }

        _systemConnector = null;
        _dataManSystem = null;

        _logger.LogInformation("Connection cleaned up.");
    }

    private string GetReadStringFromResultXml(string resultXml)
    {
        _logger.LogInformation("Parsing read string from result XML.");

        try
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(resultXml);

            XmlNode full_string_node = doc.SelectSingleNode("result/general/full_string");

            if (
                full_string_node != null
                && _dataManSystem != null
                && _dataManSystem.State == ConnectionState.Connected
            )
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse read string from result XML");
        }

        return "";
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

    [SupportedOSPlatform("windows")]
    private void OnLiveImageArrived(IAsyncResult result)
    {
        _logger.LogInformation("Live image arrived.");

        if (_dataManSystem == null)
        {
            _logger.LogWarning("DataManSystem is not initialized");
            return;
        }

        try
        {
            Image image = _dataManSystem.EndGetLiveImage(result);
            Size image_size = Gui.FitImageInControl(image.Size, new Size(400, 400));
            Image fitted_image = Gui.ResizeImageToBitmap(image, image_size);
            SendImageAsync?.Invoke(fitted_image);

            if (_liveDisplay)
            {
                _dataManSystem.BeginGetLiveImage(
                    ImageFormat.jpeg,
                    ImageSize.Sixteenth,
                    ImageQuality.Medium,
                    OnLiveImageArrived,
                    null
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process live image");
        }
    }

    #endregion
}
