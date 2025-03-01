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
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly ILogger<ScannerLogger> _logger = logger;
    private readonly Queue<string> _messages = [];

    public bool Enabled { get; set; }

    public int GetNextUniqueSessionId()
    {
        return Interlocked.Increment(ref _nextSessionId);
    }

    public void Log(string function, string message)
    {
        EnqueueMessage(
            string.Format("{0}: {1} [{2}]\r\n", function, message, DateTime.Now.ToLongTimeString())
        );
    }

    public void LogTraffic(int sessionId, bool isRead, byte[] buffer, int offset, int count)
    {
        EnqueueMessage(
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

    private void EnqueueMessage(string message)
    {
        _semaphoreSlim.Wait();
        try
        {
            if (Enabled)
            {
                _messages.Enqueue(message);

                _logger.LogDebug("Enqueued message: {Message}", message);
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
