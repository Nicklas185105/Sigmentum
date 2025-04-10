using Sigmentum.Models;
using Sigmentum.Providers;
using Sigmentum.Services;

namespace Sigmentum.Background;

public class TwelvePollingService(ILogger<TwelvePollingService> logger, TwelveDataFetcher twelveFetcher, IConfiguration config) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(20);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scanLogger = new ScanLogger();
        
        if (!config.GetValue<bool>("Sigmentum:EnableStockScanning"))
        {
            CacheService.TwelveDataScanResults = SymbolProvider.StockSymbols.Select(symbol => new ScanResult
            {
                TimestampUtc = DateTime.UtcNow,
                Symbol = symbol,
                Result = "Stock scanning is disabled"
            }).ToList();
            scanLogger.LogScans();
            return;
        }
        
        var evaluationLogger = new EvaluationLogger();
        var outputService = new CsvWriter();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Fetching TwelveData data at {Time}", DateTimeOffset.Now);
                var results = new List<Signal>();
                var scanLog = new List<ScanResult>();
                var timestamp = DateTime.UtcNow;

                foreach (var symbol in SymbolProvider.StockSymbols)
                {
                    try
                    {
                        var candles = await CacheService.TwelveDataCache.GetDataAsync(symbol, "1h", twelveFetcher);
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
                CacheService.TwelveDataScanResults = scanLog;
                scanLogger.LogScans();

                if (results.Count != 0)
                {
                    outputService.SaveSignalsToCsv(results);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching TwelveData data");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}