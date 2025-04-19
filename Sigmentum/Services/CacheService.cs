using Sigmentum.Infrastructure.Persistence.Entities;
using Sigmentum.Models;

namespace Sigmentum.Services;

public static class CacheService
{
    public static DateTime LastEvaluationTimestamp { get; set; } = DateTime.MinValue;
    public static DateTime LastScanTimestamp { get; set; } = DateTime.MinValue;
    public static List<ScanResult> BinanceScanResults { get; set; } = [];
    public static List<ScanResult> TwelveDataScanResults { get; set; } = [];
    public static List<SignalEntity> LatestSignals { get; set; } = [];
    public static DataCacheService TwelveDataCache { get; set; } = new();
    public static DataCacheService BinanceDataCache { get; set; } = new();
}
