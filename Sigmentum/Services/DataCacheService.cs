using Sigmentum.Interfaces;
using Sigmentum.Models;

namespace Sigmentum.Services;

public class DataCacheService
{
    private readonly Dictionary<string, CachedData> _cache = new();

    public class CachedData(List<Candle> data, DateTime timestamp, bool fromCache)
    {
        public List<Candle> Data { get; } = data;
        public DateTime RetrievedAtUtc { get; } = timestamp;
        public bool FromCache { get; } = fromCache;

        public Candle? LatestCandle => Data.LastOrDefault();
    }

    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

    public async Task<CachedData?> GetDataAsync(string symbol, string interval, IFetcher? fetcher, int limit = 100, bool forceRefresh = false)
    {
        var cacheKey = $"{symbol}-{interval}";

        if (!forceRefresh && _cache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.UtcNow - cached.RetrievedAtUtc < _cacheDuration)
            {
                return new CachedData(cached.Data, cached.RetrievedAtUtc, true);
            }
        }

        if (fetcher == null) return null;

        var freshData = await fetcher.GetHistoricalDataAsync(symbol, interval, limit);
        if (freshData == null) return null;

        var time = DateTime.UtcNow;
        var newCache = new CachedData(freshData, time, false);
        _cache[cacheKey] = newCache;
        return newCache;
    }

    public void Invalidate(string symbol, string interval)
    {
        var cacheKey = $"{symbol}-{interval}";
        _cache.Remove(cacheKey);
    }
} 
