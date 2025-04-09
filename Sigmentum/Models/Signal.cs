namespace Sigmentum.Models;

public enum SignalType { Buy, Sell }

public class Signal
{
    public string? Symbol { get; set; }
    public SignalType Type { get; set; }
    public string? Reason { get; set; }
}