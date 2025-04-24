using Sigmentum.Services;

namespace Sigmentum.Endpoints;

public static class ScanResultsEndpoint
{
    public static void MapScanResults(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/scan-results", () =>
        {
            var results = CacheService.TwelveDataScanResults.Concat(CacheService.BinanceScanResults).ToList();
            return Results.Ok(results);
        });
    }
}
