using Microsoft.EntityFrameworkCore;
using Serilog;
using Sigmentum.Infrastructure.Persistence.DbContext;
using Sigmentum.Infrastructure.Persistence.Entities;

namespace Sigmentum.Services;

public class EvaluationService(
    ILogger<EvaluationService> logger,
    IConfiguration config,
    IServiceProvider serviceProvider)
{
    private const decimal TARGET_PERCENTAGE = 0.02m; // 2% price target

    public async Task EvaluatePendingSignalsAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SigmentumDbContext>();
        
        #if DEBUG
        const bool isDebug = true;
        #else
        const bool isDebug = false;
        #endif

        var pendingSignals = await db.Signals
            .Where(s => s.IsPending && s.IsTest == isDebug).Include(signalEntity => signalEntity.Symbol)
            .ToListAsync();

        var anyEvaluated = false;

        foreach (var signal in pendingSignals)
        {
            if (signal.Symbol is { IsStock: true } &&
                !config.GetValue<bool>("Sigmentum:EnableStockScanning")) continue;

            // Timeout (simple version — could later use TimeoutUtc column if added)
            var timeout = signal.TriggeredAt.AddHours(1);
            if (DateTime.UtcNow < timeout) continue;

            if (signal.Symbol != null)
            {
            var currentPrice = await GetCurrentPriceAsync(signal.Symbol);
                if (currentPrice == 0) continue;

                var targetPrice = signal.EntryPrice * (1 + TARGET_PERCENTAGE);
                var isWin = signal.SignalType == "Buy"
                    ? currentPrice >= targetPrice
                    : currentPrice <= targetPrice;

                var result = isWin ? "Win" : "Loss";

                // Save evaluation result
                var evaluation = new EvaluationResultEntity
                {
                    Symbol = signal.Symbol.Symbol,
                    Exchange = signal.Exchange,
                    SignalType = signal.SignalType,
                    Result = result,
                    EvaluatedAt = DateTime.UtcNow,
                    IsTest = isDebug
                };
                db.EvaluationResults.Add(evaluation);

                if (result == "Win")
                    signal.Symbol.WinCount++;
                else
                    signal.Symbol.LossCount++;
                db.Symbols.Update(signal.Symbol);

                // Mark signal as processed
                signal.IsPending = false;

                Log.ForContext("LogToDb", true)
                    .Information("Evaluated {Symbol} | Type: {Type} | Entry: {Entry} | Target: {Target} | Current: {Current} | Outcome: {Outcome}",
                        signal.Symbol.Symbol, signal.SignalType, signal.EntryPrice, targetPrice, currentPrice, result);
            }

            anyEvaluated = true;
        }
        
        if (anyEvaluated)
            await db.SaveChangesAsync();
    }

    private static async Task<decimal> GetCurrentPriceAsync(SymbolEntity symbol)
    {
        var cache = !symbol.IsStock ? CacheService.BinanceDataCache : CacheService.TwelveDataCache;
        var candles = await cache.GetDataAsync(symbol.Symbol, !symbol.IsStock ? "1m" : "30min", null);
        return candles?.LatestCandle?.Close ?? 0;
    }
}
