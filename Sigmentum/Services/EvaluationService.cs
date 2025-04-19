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

        var pendingSignals = await db.Signals
            .Where(s => s.IsPending)
            .ToListAsync();

        var anyEvaluated = false;

        foreach (var signal in pendingSignals)
        {
            if (SmartSignalStrategy.IsStock(signal.Symbol) &&
                !config.GetValue<bool>("Sigmentum:EnableStockScanning")) continue;

            // Timeout (simple version — could later use TimeoutUtc column if added)
            var timeout = signal.TriggeredAt.AddHours(1);
            if (DateTime.UtcNow < timeout) continue;

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
                Symbol = signal.Symbol,
                Exchange = signal.Exchange,
                SignalType = signal.SignalType,
                Result = result,
                EvaluatedAt = DateTime.UtcNow
            };
            db.EvaluationResults.Add(evaluation);

            // Mark signal as processed
            signal.IsPending = false;

            Log.ForContext("LogToDb", true)
                .Information("Evaluated {Symbol} | Type: {Type} | Entry: {Entry} | Target: {Target} | Current: {Current} | Outcome: {Outcome}",
                signal.Symbol, signal.SignalType, signal.EntryPrice, targetPrice, currentPrice, result);
            
            anyEvaluated = true;
        }
        
        if (anyEvaluated)
            await db.SaveChangesAsync();
    }

    private async Task<decimal> GetCurrentPriceAsync(string symbol)
    {
        var cache = symbol.EndsWith("USDT") ? CacheService.BinanceDataCache : CacheService.TwelveDataCache;
        var candles = await cache.GetDataAsync(symbol, symbol.EndsWith("USDT") ? "1m" : "30min", null);
        return candles?.LatestCandle?.Close ?? 0;
    }
}
