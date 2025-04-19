using Sigmentum.Models;

namespace Sigmentum.Services;

public class IndicatorService
{
    public decimal CalculateRsi(List<Candle>? candles, int period)
    {
        var gains = new List<decimal>();
        var losses = new List<decimal>();

        if (candles != null)
            for (var i = 1; i < candles.Count; i++)
            {
                var change = candles[i].Close - candles[i - 1].Close;
                if (change >= 0)
                    gains.Add(change);
                else
                    losses.Add(Math.Abs(change));
            }

        var avgGain = gains.TakeLast(period).DefaultIfEmpty(0).Average();
        var avgLoss = losses.TakeLast(period).DefaultIfEmpty(0).Average();

        if (avgLoss == 0) return 100;
        var rs = avgGain / avgLoss;
        return 100 - 100 / (1 + rs);
    }

    public List<decimal>? CalculateSma(List<Candle>? candles, int period)
    {
        return candles?.Select((c, i) =>
        {
            if (i < period - 1) return 0;
            return candles.Skip(i - period + 1).Take(period).Average(x => x.Close);
        }).ToList();
    }
}