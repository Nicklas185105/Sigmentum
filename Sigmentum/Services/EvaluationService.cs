using System.Globalization;

namespace Sigmentum.Services;

public class EvaluationService
{
    private readonly BinanceDataFetcher _binanceFetcher;
    private readonly TwelveDataFetcher _twelveFetcher;
    private const string PendingPath = "Data/pending_signals.csv";
    private const string EvaluatedPath = "Data/evaluated_signals.csv";

    public EvaluationService(BinanceDataFetcher binanceFetcher, TwelveDataFetcher twelveFetcher)
    {
        _binanceFetcher = binanceFetcher;
        _twelveFetcher = twelveFetcher;
    }

    public async Task EvaluatePendingSignalsAsync()
    {
        if (!File.Exists(PendingPath)) return;

        var lines = File.ReadAllLines(PendingPath).Skip(1).ToList();
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
                var candles = await _binanceFetcher.GetHistoricalDataAsync(symbol, "1h", 1);
                currentPrice = candles.Last().Close;
            }
            else // Stock
            {
                var candles = await _twelveFetcher.GetHistoricalDataAsync(symbol, "1h", 2); // grab last two daily closes
                currentPrice = candles.Last().Close;
            }

            string outcome = type == "Buy"
                ? (currentPrice >= target ? "Win" : "Loss")
                : (currentPrice <= target ? "Win" : "Loss");

            evaluatedLines.Add($"{symbol},{type},{entry},{target},{currentPrice.ToString(CultureInfo.InvariantCulture)},{outcome},{timeout:O}");
        }

        // Save updated pending file
        File.WriteAllLines(PendingPath, remainingLines);

        // Append to evaluated file
        if (File.Exists(EvaluatedPath))
        {
            File.AppendAllLines(EvaluatedPath, evaluatedLines.Skip(1)); // skip header if file exists
        }
        else
        {
            File.WriteAllLines(EvaluatedPath, evaluatedLines);
        }
    }
}
