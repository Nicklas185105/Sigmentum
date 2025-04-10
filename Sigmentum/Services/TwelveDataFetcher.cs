using System.Globalization;
using System.Text.Json;
using Sigmentum.Interfaces;
using Sigmentum.Models;

namespace Sigmentum.Services;

public class TwelveDataFetcher(string? apiKey) : IFetcher
{
    private readonly HttpClient _http = new();

    public async Task<List<Candle>?> GetHistoricalDataAsync(string symbol, string interval, int limit)
    {
        if (apiKey == null)
        {
            throw new Exception("No API key provided");
        }
        
        var url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval={interval}&outputsize={limit}&apikey={apiKey}";
        var response = await _http.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        var candles = new List<Candle>();
        if (json.RootElement.TryGetProperty("values", out var values))
        {
            candles.AddRange(values.EnumerateArray()
            .Select(item => new Candle
            {
                Time = DateTime.Parse(item.GetProperty("datetime").GetString()!, CultureInfo.InvariantCulture),
                Open = double.Parse(item.GetProperty("open").GetString()!, CultureInfo.InvariantCulture),
                High = double.Parse(item.GetProperty("high").GetString()!, CultureInfo.InvariantCulture),
                Low = double.Parse(item.GetProperty("low").GetString()!, CultureInfo.InvariantCulture),
                Close = double.Parse(item.GetProperty("close").GetString()!, CultureInfo.InvariantCulture),
                Volume = double.Parse(item.GetProperty("volume").GetString()!, CultureInfo.InvariantCulture)
            }));
        }
        
        if (json.RootElement.TryGetProperty("status", out var status) && status.GetString() != "ok")
        {
            throw new Exception($"Error fetching data: {json.RootElement.GetProperty("message").GetString()}");
        }
        if (candles.Count == 0)
        {
            throw new Exception("No data returned");
        }

        candles.Reverse(); // Twelve Data returns newest first
        return candles;
    }
}