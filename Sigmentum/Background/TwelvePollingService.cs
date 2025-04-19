using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using Sigmentum.Models;
using Sigmentum.Providers;
using Sigmentum.Services;
using Sigmentum.Infrastructure.Persistence.Entities;
using Sigmentum.Infrastructure.Persistence.DbContext;

namespace Sigmentum.Background;

public class TwelvePollingService(
    ILogger<TwelvePollingService> logger,
    TwelveDataFetcher twelveFetcher,
    IConfiguration config,
    IServiceProvider serviceProvider
) : BackgroundService
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

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SigmentumDbContext>();

            try
            {
                logger.LogDebug("Fetching TwelveData data at {Time}", DateTimeOffset.Now);
                var results = new List<SignalEntity>();
                var scanLog = new List<ScanResult>();
                var timestamp = DateTimeOffset.Now.DateTime;
                CacheService.LastScanTimestamp = timestamp;

                foreach (var symbol in SymbolProvider.StockSymbols)
                {
                    try
                    {
                        var candles = await CacheService.TwelveDataCache.GetDataAsync(symbol, "30min", twelveFetcher);
                        if (candles == null) continue;

                        var signal = SmartSignalStrategy.Evaluate(candles.Data, symbol);
                        if (signal != null)
                        {
                            var dedupHours = config.GetValue<int>("Sigmentum:SignalDeduplicationHours");
                            var windowStart = timestamp.AddHours(-dedupHours); // adjustable timeframe
                            var alreadyExists = await db.Signals.AnyAsync(s =>
                                s.Symbol == symbol &&
                                s.Exchange == "TwelveData" &&
                                s.SignalType == signal.Type.ToString() &&
                                s.TriggeredAt >= windowStart &&
                                s.TriggeredAt <= timestamp, cancellationToken: stoppingToken);
                            
                            scanLog.Add(new ScanResult
                            {
                                TimestampUtc = timestamp,
                                Symbol = symbol,
                                Type = signal.Type.ToString(),
                                Reason = signal.Reason,
                                Result = "Signal Generated"
                            });
                            
                            if (!alreadyExists)
                            {
                                var entity = new SignalEntity
                                {
                                    Symbol = symbol,
                                    Exchange = "TwelveData",
                                    SignalType = signal.Type.ToString(),
                                    SignalValue = signal.Value,
                                    TriggeredAt = timestamp,
                                    Indicator = signal.Reason,
                                    IsPending = true,
                                    EntryPrice = (decimal)signal.ClosePrice!,
                                    StrategyVersion = signal.StrategyVersion
                                };
                                results.Add(entity);
                                db.Signals.Add(entity);
                                
                                using (LogContext.PushProperty("Symbol", symbol))
                                {
                                    Log.ForContext("LogToDb", true)
                                        .Information("Generated signal for {Symbol} | Type: {Type} | Value: {Value} | TriggeredAt: {TriggeredAt}",
                                            symbol, signal.Type, signal.RsiValue, timestamp);
                                }
                            }
                            else
                            {
                                logger.LogDebug("Duplicate signal skipped for {Symbol} at {Time}", symbol, timestamp);
                            }
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

                if (results.Count != 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }

                CacheService.LatestSignals = results;
                CacheService.TwelveDataScanResults = scanLog;
                scanLogger.LogScans();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching TwelveData data");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
