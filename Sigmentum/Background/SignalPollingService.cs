using Sigmentum.Models;
using Sigmentum.Services;

namespace Sigmentum.Background;

public class SignalPollingService(ILogger<SignalPollingService> logger, IConfiguration config) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);
    public static DateTime LastEvaluatedUtc { get; private set; } = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fetcher = new BinanceDataFetcher();
        var twelveFetcher = new TwelveDataFetcher(config.GetValue<string>("Sigmentum:StockApiKey"));
        var strategy = new SmartSignalStrategy();
        var evaluationLogger = new EvaluationLogger();
        var outputService = new CsvWriter();
        var scanLogger = new ScanLogger();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Polling for signals at: {Time}", DateTimeOffset.Now);
                var results = new List<Signal>();
                var scanLog = new List<ScanResult>();
                var timestamp = DateTime.UtcNow;

                foreach (var symbol in strategy.ExpandedSymbols)
                {
                    if (!config.GetValue<bool>("Sigmentum:EnableStockScanning") && strategy.IsStock(symbol))
                        continue;
                    try
                    {
                        var candles = strategy.IsCrypto(symbol)
                            ? await fetcher.GetHistoricalDataAsync(symbol, "1h", 100)
                            : await twelveFetcher.GetHistoricalDataAsync(symbol, "1h", 100);

                        if (candles != null)
                        {
                            var signal = strategy.Evaluate(candles, symbol);
                            if (signal != null)
                            {
                                results.Add(signal);
                                evaluationLogger.SavePendingSignal(signal, candles.Last().Close);

                                scanLog.Add(new ScanResult
                                {
                                    TimestampUtc = timestamp,
                                    Symbol = symbol,
                                    Type = signal.Type.ToString(),
                                    Reason = signal.Reason,
                                    Result = "Signal Generated"
                                });
                            }
                            else
                            {
                                scanLog.Add(new ScanResult
                                {
                                    TimestampUtc = timestamp,
                                    Symbol = symbol,
                                    Result = "No Signal"
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Scan failed for {Symbol}", symbol);
                        scanLog.Add(new ScanResult
                        {
                            TimestampUtc = timestamp,
                            Symbol = symbol,
                            Result = ex.Message
                        });
                    }
                }

                SignalCache.LatestSignals = results;
                scanLogger.LogScans(scanLog);

                if (results.Count != 0)
                {
                    outputService.SaveSignalsToCsv(results);
                }

                LastEvaluatedUtc = timestamp;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error polling for signals");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
