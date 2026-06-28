using System.Collections.Concurrent;
using Frametric.Application.Commands.EnrichMovies;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Frametric.Infrastructure.BackgroundJobs;

public class TmdbEnrichmentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TmdbEnrichmentBackgroundService> _logger;
    private readonly TmdbEnrichmentTrigger _trigger;
    private readonly int _batchSize;
    private readonly int _startupRecoveryBatchSize;
    private readonly int _delayBetweenBatchesSeconds;
    private readonly int _retryDelaySeconds;
    private readonly ConcurrentDictionary<Guid, bool> _triggeredImports = new();

    public TmdbEnrichmentBackgroundService(IServiceProvider serviceProvider, ILogger<TmdbEnrichmentBackgroundService> logger, TmdbEnrichmentTrigger trigger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _trigger = trigger;
        _batchSize = configuration.GetValue<int>("TmdbEnrichment:BatchSize", 20);
        _startupRecoveryBatchSize = configuration.GetValue<int>("TmdbEnrichment:StartupRecoveryBatchSize", 50);
        _delayBetweenBatchesSeconds = configuration.GetValue<int>("TmdbEnrichment:DelayBetweenBatchesSeconds", 10);
        _retryDelaySeconds = configuration.GetValue<int>("TmdbEnrichment:RetryDelaySeconds", 30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TmdbEnrichmentBackgroundService started. Waiting for triggers...");

        // On application startup, delicately retry processing previously failed or not found movies
        try
        {
            _logger.LogInformation("Startup recovery started. Processing failed or not found movies...");
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            // Process up to 50 failed or not found movies on startup
            var recoveredCount = await mediator.Send(new EnrichFailedMoviesCommand(_startupRecoveryBatchSize), stoppingToken);
            _logger.LogInformation("Startup recovery finished. Successfully recovered {Count} movies.", recoveredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run startup recovery for failed/not found movies.");
        }

        // Auto-trigger on startup in case there are movies left pending from a previous session
        _trigger.TriggerEnrichment();

        await foreach (var _triggerSignal in _trigger.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Enrichment triggered. Processing pending movies...");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var enrichedCount = await mediator.Send(new EnrichPendingMoviesCommand(_batchSize), stoppingToken);

                    if (enrichedCount > 0)
                    {
                        _logger.LogInformation("Enriched {Count} movies from TMDB.", enrichedCount);

                        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                        var profileService = scope.ServiceProvider.GetRequiredService<IUserViewingProfileService>();

                        var enrichingImports = await dbContext.ImportHistories
                            .Where(ih => ih.Status == "Enriching")
                            .ToListAsync(stoppingToken);

                        var enrichingIds = enrichingImports.Select(i => i.Id).ToHashSet();
                        // Clean up tracked imports that are no longer enriching
                        foreach (var key in _triggeredImports.Keys)
                        {
                            if (!enrichingIds.Contains(key))
                            {
                                _triggeredImports.TryRemove(key, out _);
                            }
                        }

                        foreach (var import in enrichingImports)
                        {
                            if (!_triggeredImports.ContainsKey(import.Id))
                            {
                                var totalMovies = await dbContext.WatchedMovies.CountAsync(w => w.ImportHistoryId == import.Id, stoppingToken);
                                if (totalMovies > 0)
                                {
                                    var completedMovies = await dbContext.WatchedMovies
                                        .Include(w => w.Movie)
                                        .CountAsync(w => w.ImportHistoryId == import.Id && w.Movie.EnrichmentStatus == Frametric.Domain.Enums.EnrichmentStatus.Completed, stoppingToken);
                                    
                                    double progress = (double)completedMovies / totalMovies;
                                    if (progress >= 0.75)
                                    {
                                        _logger.LogInformation("Import {ImportId} crossed 75% completion. Triggering immediate profile rebuild for User {UserId}.", import.Id, import.UserId);
                                        await profileService.RebuildProfileAsync(import.UserId);
                                        _triggeredImports.TryAdd(import.Id, true);
                                    }
                                }
                            }
                        }

                        // Sleep before processing the next batch to respect rate limits
                        await Task.Delay(TimeSpan.FromSeconds(_delayBetweenBatchesSeconds), stoppingToken);
                    }
                    else
                    {
                        // No more pending movies, mark imports as completed
                        await mediator.Send(new Frametric.Application.Commands.Imports.MarkImportsCompletedCommand(), stoppingToken);

                        // No more pending movies, break inner loop and wait for next trigger
                        _logger.LogInformation("No more pending movies. Sleeping...");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown, exit loop
                    break;
                }
                catch (Exception ex)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogError(ex, "An error occurred executing TmdbEnrichmentBackgroundService.");
                        try 
                        {
                            await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds), stoppingToken); // Wait before retrying on error
                        }
                        catch (OperationCanceledException) { break; }
                    }
                }
            }
        }

        _logger.LogInformation("TmdbEnrichmentBackgroundService stopping.");
    }
}
