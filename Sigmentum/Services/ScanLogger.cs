using System.Globalization;
using Sigmentum.Models;

namespace Sigmentum.Services;

public class ScanLogger
{
    private const string SCAN_LOG_FILE = "Data/last_scan_results.csv";

    public void LogScans(List<ScanResult> results)
    {
        var lines = new List<string> { "TimestampUtc,Symbol,Type,Reason,Result" };
        lines.AddRange(results.Select(result => string.Join(",", result.TimestampUtc.ToString("o", CultureInfo.InvariantCulture), result.Symbol, result.Type ?? "-", result.Reason ?? "No signal", result.Result)));

        Directory.CreateDirectory(Path.GetDirectoryName(SCAN_LOG_FILE)!);
        File.WriteAllLines(SCAN_LOG_FILE, lines);
    }

    public List<ScanResult> LoadScanResults()
    {
        if (!File.Exists(SCAN_LOG_FILE)) return [];

        var lines = File.ReadAllLines(SCAN_LOG_FILE).Skip(1);

        return (from line in lines
            select line.Split(',')
            into parts
            where parts.Length >= 5
            select new ScanResult
            {
                TimestampUtc = DateTime.Parse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                Symbol = parts[1],
                Type = parts[2] == "-" ? null : parts[2],
                Reason = parts[3] == "No signal" ? null : parts[3],
                Result = parts[4]
            }).ToList();
    }
}