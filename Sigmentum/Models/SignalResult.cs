namespace Sigmentum.Models;

public class SignalResult
{
    public string Symbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "RSI", "EMA"
    public decimal Value { get; set; }
    public DateTime Timestamp { get; set; }
}