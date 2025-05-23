﻿using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using Sigmentum.Models;
using Sigmentum.Services;
using Sigmentum.Infrastructure.Persistence.Entities;
using Sigmentum.Infrastructure.Persistence.DbContext;

namespace Sigmentum.Background;

public class BinancePollingService(
    ILogger<BinancePollingService> logger,
    BinanceDataFetcher binanceFetcher,
    IServiceProvider serviceProvider, // For scoped DbContext
    IConfiguration config
) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // var evaluationLogger = new EvaluationLogger();
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SigmentumDbContext>();
            
            var symbols = db.Symbols.Where(x => !x.IsStock).ToList();
            
            #if DEBUG
            const bool isDebug = true;
            #else
            const bool isDebug = false;
            #endif

            try
            {
                logger.LogDebug("Fetching Binance data at {Time}", DateTimeOffset.Now);
                var results = new List<SignalEntity>();
                var scanLog = new List<ScanResult>();
                var timestamp = DateTimeOffset.Now.DateTime;
                CacheService.LastScanTimestamp = timestamp;

                foreach (var symbol in symbols)
                {
                    try
                    {
                        var candles = await CacheService.BinanceDataCache.GetDataAsync(symbol.Symbol, "1m", binanceFetcher);
                        if (candles == null) continue;

                        var signal = SmartSignalStrategy.Evaluate(candles.Data, symbol.Symbol);
                        if (signal != null)
                        {
                            var dedupHours = config.GetValue<int>("Sigmentum:SignalDeduplicationHours");
                            var windowStart = timestamp.AddHours(-dedupHours); // adjustable timeframe
                            var alreadyExists = await db.Signals.AnyAsync(s =>
                                s.Symbol == symbol &&
                                s.Exchange == "Binance" &&
                                s.SignalType == signal.Type.ToString() &&
                                s.TriggeredAt >= windowStart &&
                                s.TriggeredAt <= timestamp, cancellationToken: stoppingToken);
                            
                            scanLog.Add(new ScanResult
                            {
                                TimestampUtc = timestamp,
                                Symbol = symbol.Symbol,
                                Type = signal.Type.ToString(),
                                Reason = signal.Reason,
                                Result = "Signal Generated"
                            });
                            
                            if (!alreadyExists)
                            {
                                // Save to DB
                                var entity = new SignalEntity
                                {
                                    Symbol = symbol,
                                    SymbolId = symbol.Id,
                                    Exchange = "Binance",
                                    SignalType = signal.Type.ToString(),
                                    SignalValue = signal.RsiValue,
                                    TriggeredAt = timestamp,
                                    Indicator = signal.Reason,
                                    IsPending = true,
                                    EntryPrice = (decimal)signal.ClosePrice!,
                                    StrategyVersion = signal.StrategyVersion,
                                    IsTest = isDebug
                                };
                                results.Add(entity);
                                db.Signals.Add(entity);

                                using (LogContext.PushProperty("Symbol", symbol.Symbol))
                                {
                                    Log.ForContext("LogToDb", true)
                                        .Information("Generated signal for {Symbol} | Type: {Type} | Value: {Value} | TriggeredAt: {TriggeredAt}",
                                            symbol.Symbol, signal.Type, signal.RsiValue, timestamp);
                                }
                            }
                            else
                            {
                                logger.LogDebug("Duplicate signal skipped for {Symbol} at {Time}", symbol.Symbol, timestamp);
                            }
                        }
                        else
                        {
                            scanLog.Add(new ScanResult
                            {
                                TimestampUtc = timestamp,
                                Symbol = symbol.Symbol,
                                Type = "N/A",
                                Reason = "No Signal",
                                Result = "No Signal Generated"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Scan failed for {Symbol}", symbol.Symbol);
                        scanLog.Add(new ScanResult
                        {
                            TimestampUtc = timestamp,
                            Symbol = symbol.Symbol,
                            Result = ex.Message
                        });
                    }
                }

                if (results.Count != 0)
                {
                    await db.SaveChangesAsync(stoppingToken); // ✅ Save all signals
                }

                CacheService.LatestSignals = results;
                CacheService.BinanceScanResults = scanLog;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching Binance data");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
