namespace Sigmentum.Models;

public class ScanResult
{
    public DateTime TimestampUtc { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string? Type { get; set; } = null;
    public string? Reason { get; set; } = null;
    public string Result { get; set; } = "Scanned"; // or "Signal Generated"
}
