using Sigmentum.Models;
using Sigmentum.Providers;
using Sigmentum.Services;

namespace Sigmentum.Background;

public class BinancePollingService(ILogger<BinancePollingService> logger, BinanceDataFetcher binanceFetcher) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var strategy = new SmartSignalStrategy();
        var evaluationLogger = new EvaluationLogger();
        var outputService = new CsvWriter();
        var scanLogger = new ScanLogger();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Fetching Binance data at {Time}", DateTimeOffset.Now);
                var results = new List<Signal>();
                var scanLog = new List<ScanResult>();
                var timestamp = DateTime.UtcNow;

                foreach (var symbol in SymbolProvider.CryptoSymbols)
                {
                    try
                    {
                        var candles = await CacheService.BinanceDataCache.GetDataAsync(symbol, "1h", binanceFetcher);
                        if (candles == null) continue;
                        var signal = SmartSignalStrategy.Evaluate(candles, symbol);
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
                                Type = "N/A",
                                Reason = "No Signal",
                                Result = "No Signal Generated"
                            });
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
                
                CacheService.LatestSignals = results;
                CacheService.BinanceScanResults = scanLog;
                scanLogger.LogScans();

                if (results.Count != 0)
                {
                    outputService.SaveSignalsToCsv(results);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching Binance data");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}