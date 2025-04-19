namespace Sigmentum.Models;

public class Candle
{
    public DateTime Time { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; init; }
    public decimal Volume { get; set; }
}