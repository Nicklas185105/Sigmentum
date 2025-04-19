using Sigmentum.Models;

namespace Sigmentum.Services;

public class SmartSignalStrategy
{
    public static Signal? Evaluate(List<Candle> candles, string symbol)
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
        var isVolumeSpike = volume[idx] > volume.Average() * 1.5m;

        var result = new Signal
        {
            Symbol = symbol,
            Type = SignalType.Buy,
            RsiValue = rsi[idx],
            Ema9 = ema9[idx],
            Ema21 = ema21[idx],
            Volume = volume[idx],
            ClosePrice = closePrices[idx],
            StrategyVersion = "v1.0"
        };
        
        switch (isRsiCrossUp)
        {
            case true when isEmaCrossover && isVolumeSpike:
                result.Reason = "RSI cross-up + EMA crossover + Volume spike";
                result.Value = (decimal)result.RsiValue;
                return result;
            case true:
                result.Reason = "RSI cross-up";
                result.Value = (decimal)result.RsiValue;
                return result;
            default:
            {
                if (isEmaCrossover)
                {
                    result.Reason = "EMA crossover";
                    result.Value = (decimal)result.Ema9;
                    return result;
                }

                if (isVolumeSpike)
                {
                    result.Reason = "Volume spike";
                    result.Value = (decimal)result.Volume;
                    return result;
                }

                break;
            }
        }

        return null;
    }

    public static bool IsCrypto(string symbol) => symbol.EndsWith("USDT");
    public static bool IsStock(string symbol) => !IsCrypto(symbol);
}