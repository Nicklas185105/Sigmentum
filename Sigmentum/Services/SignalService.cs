using Microsoft.EntityFrameworkCore;
using Sigmentum.Infrastructure.Persistence.DbContext;
using Sigmentum.Infrastructure.Persistence.Entities;

namespace Sigmentum.Services;

public class SignalService(ILogger<SignalService> logger, IServiceProvider serviceProvider)
{
    public int TotalWins { get; private set; }
    public int TotalLosses { get; private set; }
    public int WinRate => TotalWins + TotalLosses == 0 ? 0 : (int)((TotalWins / (double)(TotalWins + TotalLosses)) * 100);
    public static DateTime? LastScanTime => CacheService.LastScanTimestamp;
    public static DateTime? LastEvaluationTime => CacheService.LastEvaluationTimestamp;

    public List<SignalEntity> LiveSignals { get; private set; } = [];

    public async Task Initialize()
    {
        try
        {
            TotalWins = LoadTotalWins();
            TotalLosses = LoadTotalLosses();
            LiveSignals = await LoadRecentLiveSignalsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize SignalService");
        }
    }

    private Task<List<SignalEntity>> LoadRecentLiveSignalsAsync()
    {
        // Simulated data: Get latest signals from cache
        var signals = CacheService.LatestSignals
            .OrderByDescending(s => s.TriggeredAt)
            .Take(20)
            .ToList();

        return Task.FromResult(signals);
    }
    
    private int LoadTotalWins()
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SigmentumDbContext>();
        
        var wins = db.EvaluationResults.Where(x => x.Result.Equals("Win"));
        return wins.Count();
    }
    
    private int LoadTotalLosses()
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SigmentumDbContext>();
        
        var losses = db.EvaluationResults.Where(x => x.Result.Equals("Loss"));
        return losses.Count();
    }

    public Task<List<SignalEntity>> GetPendingSignals()
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SigmentumDbContext>();
        
        // Example: assume pending signals are unevaluated
        return Task.FromResult(db.Signals
            .Where(s => s.IsPending)
            .OrderByDescending(s => s.TriggeredAt)
            .Include(s => s.Symbol)
            .ToList());
    }

    public void RefreshSignals()
    {
        LiveSignals = CacheService.LatestSignals
            .OrderByDescending(s => s.TriggeredAt)
            .Take(20)
            .ToList();
    }
}