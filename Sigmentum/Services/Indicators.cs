namespace Sigmentum.Services;

public static class Indicators
{
    public static List<decimal> CalculateRsi(List<decimal> prices, int period)
    {
        var result = new List<decimal>();
        decimal gain = 0, loss = 0;
        for (var i = 1; i <= period; i++)
        {
            var diff = prices[i] - prices[i - 1];
            if (diff >= 0) gain += diff;
            else loss -= diff;
        }

        gain /= period;
        loss /= period;
        result.Add(100 - 100 / (1 + gain / loss));

        for (var i = period + 1; i < prices.Count; i++)
        {
            var diff = prices[i] - prices[i - 1];
            if (diff >= 0)
            {
                gain = (gain * (period - 1) + diff) / period;
                loss = (loss * (period - 1)) / period;
            }
            else
            {
                gain = (gain * (period - 1)) / period;
                loss = (loss * (period - 1) - diff) / period;
            }

            result.Add(100 - 100 / (1 + gain / (loss == 0 ? 0.01m : loss)));
        }

        return Enumerable.Repeat(50.0m, prices.Count - result.Count).Concat(result).ToList();
    }

    public static List<decimal> CalculateEma(List<decimal> prices, int period)
    {
        var ema = new List<decimal>();
        var multiplier = 2.0m / (period + 1);
        ema.Add(prices.Take(period).Average());

        for (var i = period; i < prices.Count; i++)
        {
            var value = (prices[i] - ema.Last()) * multiplier + ema.Last();
            ema.Add(value);
        }

        return Enumerable.Repeat(prices[0], prices.Count - ema.Count).Concat(ema).ToList();
    }
}
