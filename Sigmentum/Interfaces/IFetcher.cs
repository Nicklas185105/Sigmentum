using Sigmentum.Models;

namespace Sigmentum.Interfaces;

public interface IFetcher
{
    public Task<List<Candle>?> GetHistoricalDataAsync(string symbol, string interval, int limit);
}