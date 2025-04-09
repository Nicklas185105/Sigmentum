namespace Sigmentum.Models;

public class Candle
{
    public DateTime Time { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; init; }
    public double Volume { get; set; }
}