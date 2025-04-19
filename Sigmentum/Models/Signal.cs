namespace Sigmentum.Models;

public enum SignalType { Buy, Sell }

public class Signal
{
    public string Symbol { get; set; } = string.Empty;
    public SignalType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Value { get; set; }

    public decimal? RsiValue { get; set; }      // New: for inspection & ML
    public decimal? Ema9 { get; set; }          // New
    public decimal? Ema21 { get; set; }         // New
    public decimal? Volume { get; set; }        // New
    public decimal? ClosePrice { get; set; }    // New: helpful for backtesting
    
    public string StrategyVersion { get; set; } = string.Empty;
}