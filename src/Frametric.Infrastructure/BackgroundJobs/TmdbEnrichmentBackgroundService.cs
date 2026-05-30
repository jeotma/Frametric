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

    public TmdbEnrichmentBackgroundService(IServiceProvider serviceProvider, ILogger<TmdbEnrichmentBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TmdbEnrichmentBackgroundService started.");

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
                    // No pending movies found, sleep for 1 minute before checking again
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred executing TmdbEnrichmentBackgroundService.");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait before retrying on error
            }
        }

        _logger.LogInformation("TmdbEnrichmentBackgroundService stopping.");
    }
}
