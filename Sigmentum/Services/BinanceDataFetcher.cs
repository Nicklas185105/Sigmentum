using System.Text.Json;
using Sigmentum.Interfaces;
using Sigmentum.Models;

namespace Sigmentum.Services;

public class BinanceDataFetcher : IFetcher
{
    private readonly HttpClient _http = new();

    public async Task<List<Candle>?> GetHistoricalDataAsync(string symbol, string interval, int limit)
    {
        var url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
        var responseStream = await _http.GetStreamAsync(url);
        var klines = await JsonSerializer.DeserializeAsync<List<List<JsonElement>>>(responseStream);

        return klines?.Select(k => new Candle
        {
            Time = DateTimeOffset.FromUnixTimeMilliseconds(k[0].GetInt64()).UtcDateTime,
            Open = double.Parse(k[1].GetString()!),
            High = double.Parse(k[2].GetString()!),
            Low = double.Parse(k[3].GetString()!),
            Close = double.Parse(k[4].GetString()!),
            Volume = double.Parse(k[5].GetString()!)
        }).ToList();
    }

}