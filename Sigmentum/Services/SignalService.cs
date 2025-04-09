using Sigmentum.Models;

namespace Sigmentum.Services;

public class SignalService(IndicatorService indicators)
{
    public Signal? Evaluate(List<Candle>? candles, string symbol)
    {
        var rsi = indicators.CalculateRsi(candles, 14);
        var sma = indicators.CalculateSma(candles, 50);
        if (candles == null) return null;
        var lastPrice = candles.Last().Close;

        if (rsi < 30 && sma != null && lastPrice > sma.Last())
        {
            return new Signal
            {
                Symbol = symbol,
                Type = SignalType.Buy,
                Reason = $"RSI {rsi:F1} < 30 and price {lastPrice} > SMA50"
            };
        }

        return null;
    }
}