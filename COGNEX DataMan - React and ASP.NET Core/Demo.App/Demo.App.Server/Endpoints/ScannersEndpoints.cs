namespace Demo.App.Server.Endpoints;

internal static class ScannersEndpoints
{
    internal static WebApplication AddScannersEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/api/scanners/list/refresh",
            (Worker worker) =>
            {
                worker.Refresh();
            }
        );
        return app;
    }
}
