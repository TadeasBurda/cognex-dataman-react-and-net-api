using System.Text;

namespace Demo.App.Server.Services;

internal enum LogTrafficState
{
    Read,
    Written,
}

public class ScannerLogger(ILogger<ScannerLogger> logger) : Cognex.DataMan.SDK.ILogger
{
    private static int _nextSessionId = 0;

    private readonly ILogger<ScannerLogger> _logger = logger;

    public bool Enabled { get; set; }

    public Func<string, Task>? ReceivedAsync { get; set; }

    public int GetNextUniqueSessionId()
    {
        return Interlocked.Increment(ref _nextSessionId);
    }

    public void Log(string function, string message)
    {
        SendMessage(
            string.Format("{0}: {1} [{2}]\r\n", function, message, DateTime.Now.ToLongTimeString())
        );
    }

    public void LogTraffic(int sessionId, bool isRead, byte[] buffer, int offset, int count)
    {
        SendMessage(
            string.Format(
                "Traffic: {0} {1} bytes at {2} [session #{3}]: {4}{5}\r\n",
                isRead ? LogTrafficState.Read : LogTrafficState.Written,
                count,
                DateTime.Now.ToLongTimeString(),
                sessionId,
                GetBytesAsPrintable(buffer, offset, Math.Min(50, count)),
                count > 50 ? "..." : string.Empty
            )
        );
    }

    private static string GetBytesAsPrintable(byte[] buffer, int offset, int count)
    {
        if (buffer == null || count < 1 || offset + count > buffer.Length)
            return string.Empty;

        var stringBuilder = new StringBuilder(count * 6);
        for (var i = offset; i < buffer.Length && i < offset + count; ++i)
        {
            if (buffer[i] < (byte)' ' || buffer[i] >= 127)
            {
                stringBuilder.Append(string.Format("<0x{0:X2}>", buffer[i]));
            }
            else
            {
                stringBuilder.Append((char)buffer[i]);
            }
        }

        return stringBuilder.ToString();
    }

    private void SendMessage(string message)
    {
        if (Enabled)
        {
            ReceivedAsync?.Invoke(message);

            _logger.LogDebug("Sended message: {Message}", message);
        }
    }
}
