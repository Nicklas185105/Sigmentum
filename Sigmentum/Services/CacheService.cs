using Sigmentum.Models;

namespace Sigmentum.Services;

public static class CacheService
{
    public static List<Signal> LatestSignals { get; set; } = [];
    public static DataCacheService TwelveDataCache { get; set; } = new();
    public static DataCacheService BinanceDataCache { get; set; } = new();
}
