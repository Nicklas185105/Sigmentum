﻿using Sigmentum.Services;

namespace Sigmentum.Background;

public class EvaluationBackgroundService(ILogger<EvaluationBackgroundService> logger, ILogger<EvaluationService> esLogger, EvaluationService evaluationService)
    : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("Running automatic signal evaluation at: {Time}", DateTimeOffset.Now);
                await evaluationService.EvaluatePendingSignalsAsync();
                CacheService.LastEvaluationTimestamp = DateTimeOffset.Now.DateTime;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during automatic evaluation");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}