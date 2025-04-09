using Sigmentum.Background;

namespace Sigmentum.Endpoints;

public static class EvaluationMetricsEndpoint
{
    public static void MapEvaluationMetrics(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/evaluation-metrics", () =>
        {
            const string path = "Data/evaluated_signals.csv";
            var metrics = new Metrics();

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path).Skip(1);

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length < 7) continue;

                    var symbol = parts[0];
                    var outcome = parts[5];

                    metrics.Total++;

                    if (outcome == "Win")
                    {
                        metrics.Wins++;
                        if (!metrics.BestPerformers.ContainsKey(symbol))
                            metrics.BestPerformers[symbol] = 0;
                        metrics.BestPerformers[symbol]++;
                    }
                    else if (outcome == "Loss")
                    {
                        metrics.Losses++;
                    }
                }
            }

            metrics.WinRate = metrics.Total > 0 ? (double)metrics.Wins / metrics.Total : 0;
            metrics.LastEvaluated = SignalPollingService.LastEvaluatedUtc;

            return Results.Ok(metrics);
        });
    }

    public class Metrics
    {
        public int Total { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
        public Dictionary<string, int> BestPerformers { get; set; } = new();
        public DateTime LastEvaluated { get; set; }
    }
}