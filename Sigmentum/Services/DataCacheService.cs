using Sigmentum.Interfaces;
using Sigmentum.Models;

namespace Sigmentum.Services;

public class DataCacheService
{
    private readonly Dictionary<string, CachedData> _cache = new();

    private record CachedData(List<Candle>? Data, DateTime Timestamp);

    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

    public async Task<List<Candle>?> GetDataAsync(string symbol, string interval, IFetcher fetcher, int limit = 100, bool forceRefresh = false)
    {
        var cacheKey = $"{symbol}-{interval}";

        if (!forceRefresh && _cache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.UtcNow - cached.Timestamp < _cacheDuration)
            {
                return cached.Data;
            }
        }

        var freshData = await fetcher.GetHistoricalDataAsync(symbol, interval, limit);
        if (freshData != null)
        {
            _cache[cacheKey] = new CachedData(freshData, DateTime.UtcNow);
        }
        return freshData;
    }
}