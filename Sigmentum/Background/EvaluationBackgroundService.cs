using Sigmentum.Services;

namespace Sigmentum.Background;

public class EvaluationBackgroundService(ILogger<EvaluationBackgroundService> logger, IConfiguration configuration)
    : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);
    private readonly string? _apiKey = configuration.GetValue<string>("SignalScanner:StockApiKey");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Running automatic signal evaluation at: {Time}", DateTimeOffset.Now);
                var evaluator = new EvaluationService(new BinanceDataFetcher(), new TwelveDataFetcher(_apiKey));
                await evaluator.EvaluatePendingSignalsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during automatic evaluation");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}