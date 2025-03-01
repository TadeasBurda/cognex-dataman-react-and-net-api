namespace Demo.App.Server.Endpoints;

internal static class LoggingEndpoints
{
    internal static WebApplication AddLoggingEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/api/logging",
            (bool enable, Worker worker) =>
            {
                worker.SetScannerLogging(enable);
            }
        );
        return app;
    }
}
