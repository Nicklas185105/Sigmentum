using Sigmentum.Services;

namespace Sigmentum.Endpoints;

public static class ScanResultsEndpoint
{
    public static void MapScanResults(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/scan-results", () =>
        {
            var logger = new ScanLogger();
            var results = logger.LoadScanResults();
            return Results.Ok(results);
        });
    }
}
