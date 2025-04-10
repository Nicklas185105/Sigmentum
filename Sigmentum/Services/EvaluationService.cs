using System.Globalization;
using Sigmentum.Interfaces;

namespace Sigmentum.Services;

public class EvaluationService(ILogger<EvaluationService> logger, IConfiguration config)
{
    private const string PENDING_PATH = "Data/pending_signals.csv";
    private const string EVALUATED_PATH = "Data/evaluated_signals.csv";

    public async Task EvaluatePendingSignalsAsync()
    {
        if (!File.Exists(PENDING_PATH)) throw new FileNotFoundException(PENDING_PATH);

        var lines = (await File.ReadAllLinesAsync(PENDING_PATH)).Skip(1).ToList();
        var remainingLines = new List<string> { "Symbol,Type,EntryPrice,TargetPrice,TimeoutUtc,Status" };
        var evaluatedLines = new List<string> { "Symbol,Type,EntryPrice,TargetPrice,FinalPrice,Outcome,TimeoutUtc" };

        foreach (var line in lines)
        {
            var parts = line.Split(',');

            var symbol = parts[0];
            if (SmartSignalStrategy.IsStock(symbol) &&
                !config.GetValue<bool>("Sigmentum:EnableStockScanning")) continue;
            var type = parts[1];
            var entry = double.Parse(parts[2], CultureInfo.InvariantCulture);
            var target = double.Parse(parts[3], CultureInfo.InvariantCulture);
            var timeout = DateTime.Parse(parts[4], null, DateTimeStyles.AdjustToUniversal);
            var status = parts[5];

            if (status != "Pending") continue;

            if (DateTime.UtcNow < timeout)
            {
                remainingLines.Add(line);
                continue;
            }

            var currentPrice = await GetCurrentPriceAsync(symbol);

            var outcome = type == "Buy"
                ? (currentPrice >= target ? "Win" : "Loss")
                : (currentPrice <= target ? "Win" : "Loss");

            logger.LogInformation("Evaluated {Symbol} | Type: {Type} | Entry: {Entry} | Target: {Target} | Current: {Current} | Outcome: {Outcome}",
                symbol, type, entry, target, currentPrice, outcome);

            evaluatedLines.Add($"{symbol},{type},{entry},{target},{currentPrice.ToString(CultureInfo.InvariantCulture)},{outcome},{timeout:O}");
        }

        await File.WriteAllLinesAsync(PENDING_PATH, remainingLines);

        if (File.Exists(EVALUATED_PATH))
        {
            await File.AppendAllLinesAsync(EVALUATED_PATH, evaluatedLines.Skip(1));
        }
        else
        {
            await File.WriteAllLinesAsync(EVALUATED_PATH, evaluatedLines);
        }
    }

    private async Task<double> GetCurrentPriceAsync(string symbol)
    {
        var cache = symbol.EndsWith("USDT") ? CacheService.BinanceDataCache : CacheService.TwelveDataCache;
        var candles = await cache.GetDataAsync(symbol, "1h", null);
        return candles?.Last().Close ?? 0;
    }
}
