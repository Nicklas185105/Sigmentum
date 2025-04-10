using System.Globalization;

namespace Sigmentum.Services;

public class EvaluationService(BinanceDataFetcher binanceFetcher, TwelveDataFetcher twelveFetcher)
{
    private const string PENDING_PATH = "Data/pending_signals.csv";
    private const string EVALUATED_PATH = "Data/evaluated_signals.csv";

    public async Task EvaluatePendingSignalsAsync()
    {
        if (!File.Exists(PENDING_PATH)) return;

        var lines = (await File.ReadAllLinesAsync(PENDING_PATH)).Skip(1).ToList();
        var remainingLines = new List<string> { "Symbol,Type,EntryPrice,TargetPrice,TimeoutUtc,Status" };
        var evaluatedLines = new List<string> { "Symbol,Type,EntryPrice,TargetPrice,FinalPrice,Outcome,TimeoutUtc" };

        foreach (var line in lines)
        {
            var parts = line.Split(',');

            var symbol = parts[0];
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

            double currentPrice;

            if (symbol.EndsWith("USDT")) // Crypto
            {
                var candle = await CacheService.BinanceDataCache.GetDataAsync(symbol, "1h", binanceFetcher);
                currentPrice = candle?.Last().Close ?? 0;
            }
            else // Stock
            {
                var candle = await CacheService.TwelveDataCache.GetDataAsync(symbol, "1h", twelveFetcher);
                currentPrice = candle?.Last().Close ?? 0;
            }

            var outcome = type == "Buy"
                ? (currentPrice >= target ? "Win" : "Loss")
                : (currentPrice <= target ? "Win" : "Loss");

            evaluatedLines.Add($"{symbol},{type},{entry},{target},{currentPrice.ToString(CultureInfo.InvariantCulture)},{outcome},{timeout:O}");
        }

        // Save updated pending file
        await File.WriteAllLinesAsync(PENDING_PATH, remainingLines);

        // Append to evaluated file
        if (File.Exists(EVALUATED_PATH))
        {
            await File.AppendAllLinesAsync(EVALUATED_PATH, evaluatedLines.Skip(1)); // skip header if file exists
        }
        else
        {
            await File.WriteAllLinesAsync(EVALUATED_PATH, evaluatedLines);
        }
    }
}
