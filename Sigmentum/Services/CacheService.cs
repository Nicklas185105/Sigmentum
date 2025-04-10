using Sigmentum.Models;

namespace Sigmentum.Services;

public static class CacheService
{
    public static DateTime LastEvaluation { get; set; } = DateTime.MinValue;
    public static List<ScanResult> BinanceScanResults { get; set; } = [];
    public static List<ScanResult> TwelveDataScanResults { get; set; } = [];
    public static List<Signal> LatestSignals { get; set; } = [];
    public static DataCacheService TwelveDataCache { get; set; } = new();
    public static DataCacheService BinanceDataCache { get; set; } = new();
}
