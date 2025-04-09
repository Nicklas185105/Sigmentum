using Sigmentum.Models;

namespace Sigmentum.Services;

public class SmartSignalStrategy
{
    private const string CRYPTO_FILE = "Data/crypto-symbols.txt";
    private const string STOCK_FILE = "Data/stock-symbols.txt";

    public SmartSignalStrategy()
    {
        if (!File.Exists(CRYPTO_FILE))
        {
            Directory.CreateDirectory("Data");
            File.WriteAllLines(CRYPTO_FILE, [
                "BTCUSDT", "ETHUSDT", "BNBUSDT", "SOLUSDT", "XRPUSDT", "ADAUSDT",
                "DOGEUSDT", "MATICUSDT", "AVAXUSDT", "TRXUSDT", "LINKUSDT", "LTCUSDT",
                "DOTUSDT", "ATOMUSDT", "XLMUSDT", "APTUSDT", "ARBUSDT"
            ]);
        }

        if (File.Exists(STOCK_FILE)) return;
        Directory.CreateDirectory("Data");
        File.WriteAllLines(STOCK_FILE, [
            "NVDA", "TSLA", "AMZN", "MSFT", "META", "AMD", "GOOGL", "NFLX"
        ]);
    }

    public Signal? Evaluate(List<Candle> candles, string symbol)
    {
        if (candles.Count < 50) return null;

        var closePrices = candles.Select(c => c.Close).ToList();
        var rsi = Indicators.CalculateRsi(closePrices, 14);
        var ema9 = Indicators.CalculateEma(closePrices, 9);
        var ema21 = Indicators.CalculateEma(closePrices, 21);
        var volume = candles.Select(c => c.Volume).ToList();

        var idx = candles.Count - 1;

        var isRsiCrossUp = rsi[idx - 1] < 30 && rsi[idx] >= 30;
        var isEmaCrossover = ema9[idx - 1] < ema21[idx - 1] && ema9[idx] > ema21[idx];
        var isVolumeSpike = volume[idx] > volume.Average() * 1.5;

        switch (isRsiCrossUp)
        {
            case true when isEmaCrossover && isVolumeSpike:
                return new Signal
                {
                    Symbol = symbol,
                    Type = SignalType.Buy,
                    Reason = "RSI cross-up + EMA crossover + Volume spike"
                };
            case true:
                return new Signal
                {
                    Symbol = symbol,
                    Type = SignalType.Buy,
                    Reason = "RSI cross-up"
                };
            default:
            {
                if (isEmaCrossover)
                {
                    return new Signal
                    {
                        Symbol = symbol,
                        Type = SignalType.Buy,
                        Reason = "EMA crossover"
                    };
                }

                if (isVolumeSpike)
                {
                    return new Signal
                    {
                        Symbol = symbol,
                        Type = SignalType.Buy,
                        Reason = "Volume spike"
                    };
                }

                break;
            }
        }

        return null;
    }

    public IEnumerable<string> ExpandedSymbols =>
        File.ReadAllLines(CRYPTO_FILE).Concat(File.ReadAllLines(STOCK_FILE));

    public bool IsCrypto(string symbol) => symbol.EndsWith("USDT");
    public bool IsStock(string symbol) => !IsCrypto(symbol);
}