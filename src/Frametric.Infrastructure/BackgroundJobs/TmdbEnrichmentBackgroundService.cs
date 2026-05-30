using Frametric.Application.Commands.EnrichMovies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Frametric.Infrastructure.BackgroundJobs;

public class TmdbEnrichmentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TmdbEnrichmentBackgroundService> _logger;
    private readonly TmdbEnrichmentTrigger _trigger;

    public TmdbEnrichmentBackgroundService(IServiceProvider serviceProvider, ILogger<TmdbEnrichmentBackgroundService> logger, TmdbEnrichmentTrigger trigger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _trigger = trigger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TmdbEnrichmentBackgroundService started. Waiting for triggers...");

        await foreach (var _ in _trigger.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Enrichment triggered. Processing pending movies...");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var batchSize = 20;
                    var enrichedCount = await mediator.Send(new EnrichPendingMoviesCommand(batchSize), stoppingToken);

                    if (enrichedCount > 0)
                    {
                        _logger.LogInformation("Enriched {Count} movies from TMDB.", enrichedCount);
                        // Sleep for 10 seconds before processing the next batch to respect rate limits
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    else
                    {
                        // No more pending movies, break inner loop and wait for next trigger
                        _logger.LogInformation("No more pending movies. Sleeping...");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred executing TmdbEnrichmentBackgroundService.");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait before retrying on error
                }
            }
        }

        _logger.LogInformation("TmdbEnrichmentBackgroundService stopping.");
    }
}
