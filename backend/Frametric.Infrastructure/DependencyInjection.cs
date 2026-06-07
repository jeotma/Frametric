using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Application.Interfaces.Discovery;
using Frametric.Application.Interfaces.EntityDetails;
using Frametric.Infrastructure.BackgroundJobs;
using Frametric.Infrastructure.Security;
using Frametric.Infrastructure.Importer;
using Frametric.Infrastructure.Persistence;
using Frametric.Infrastructure.Queries;
using Frametric.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Frametric.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FrametricDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<FrametricDbContext>());

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }
        
        services.AddScoped<ILetterboxdImporter, LetterboxdZipImporter>();

        // Register Dapper DbConnectionFactory
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        
        // Classic Analytics
        services.AddScoped<IAnalyticsService, DapperAnalyticsService>();

        // Advanced Analytics CQRS Queries
        services.AddScoped<IWatchedBasicQueries, WatchedQueriesImpl>();
        services.AddScoped<IWatchedAdvancedStatsQueries, WatchedQueriesImpl>();
        services.AddScoped<IWatchedComplexCorrelationsQueries, WatchedQueriesImpl>();
        services.AddScoped<IWatchlistBasicQueries, WatchlistQueriesImpl>();
        services.AddScoped<IWatchlistAdvancedStatsQueries, WatchlistQueriesImpl>();
        services.AddScoped<IWatchlistComplexCorrelationsQueries, WatchlistQueriesImpl>();
        services.AddScoped<IBonusQueries, BonusQueriesImpl>();
        services.AddScoped<IRecommendationQueries, RecommendationQueriesImpl>();
        services.AddScoped<IDiscoveryQueries, DiscoveryQueriesImpl>();
        services.AddScoped<IEntityDetailsQueries, EntityDetailsQueriesImpl>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Register JWT Security and Current User services
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddHttpClient<ITmdbService, Frametric.Infrastructure.Providers.Tmdb.TmdbService>(client =>
        {
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            
            var token = configuration["Tmdb:AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        });

        services.AddHttpClient<IOmdbService, Frametric.Infrastructure.Providers.Omdb.OmdbService>(client =>
        {
            client.BaseAddress = new Uri("http://www.omdbapi.com/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddSingleton<Frametric.Infrastructure.BackgroundJobs.TmdbEnrichmentTrigger>();
        services.AddSingleton<ITmdbEnrichmentTrigger>(provider => provider.GetRequiredService<Frametric.Infrastructure.BackgroundJobs.TmdbEnrichmentTrigger>());

        services.AddHostedService<Frametric.Infrastructure.BackgroundJobs.TmdbEnrichmentBackgroundService>();

        return services;
    }
}
