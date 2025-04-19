using System.Globalization;
using System.Text.Json;
using Sigmentum.Interfaces;
using Sigmentum.Models;
using Sigmentum.Providers;

namespace Sigmentum.Services;

public class TwelveDataFetcher(IConfiguration config) : IFetcher
{
    private readonly HttpClient _http = new();
    private string? _apiKey = config.GetValue<string>("Sigmentum:StockApiKey");

    public async Task<List<Candle>?> GetHistoricalDataAsync(string symbol, string interval, int limit)
    {
        if (_apiKey == null)
        {
            var key = config.GetValue<string>("Sigmentum:StockApiKey");
            if (key != null)
            {
                _apiKey = key;
            }
            else
            {
                throw new Exception("No API key provided");
            }
        }
        
        var url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval={interval}&outputsize={limit}&apikey={_apiKey}";
        var response = await _http.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        var candles = new List<Candle>();
        if (json.RootElement.TryGetProperty("values", out var values))
        {
            candles.AddRange(values.EnumerateArray()
            .Select(item => new Candle
            {
                Time = DateTime.Parse(item.GetProperty("datetime").GetString()!, CultureInfo.InvariantCulture),
                Open = decimal.Parse(item.GetProperty("open").GetString()!, CultureInfo.InvariantCulture),
                High = decimal.Parse(item.GetProperty("high").GetString()!, CultureInfo.InvariantCulture),
                Low = decimal.Parse(item.GetProperty("low").GetString()!, CultureInfo.InvariantCulture),
                Close = decimal.Parse(item.GetProperty("close").GetString()!, CultureInfo.InvariantCulture),
                Volume = decimal.Parse(item.GetProperty("volume").GetString()!, CultureInfo.InvariantCulture)
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
    
    public async Task FetchAllSymbolsAndCacheAsync()
    {
        foreach (var symbol in SymbolProvider.StockSymbols) // or however you store your symbols
        {
            await CacheService.TwelveDataCache.GetDataAsync(symbol, "1h", this);
        }
    }
}