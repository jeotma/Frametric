using System.Reflection;
using Frametric.Application.Interfaces;
using Frametric.Application.Services;
using Frametric.Application.Queries.Recommendations;
using Frametric.Application.Queries.Recommendations.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Frametric.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register Application Facades (Controllers -> Application Facade -> UseCase/MediatR)
        services.AddScoped<IUserApplication, UserApplication>();
        services.AddScoped<IImportApplication, ImportApplication>();
        services.AddScoped<IAnalyticsApplication, AnalyticsApplication>();

        // Register Recommendation Strategies
        services.AddScoped<IRecommendationStrategy, RecentMoodStrategy>();
        services.AddScoped<IRecommendationStrategy, OppositeMoodStrategy>();
        services.AddScoped<IRecommendationStrategy, ComfortZoneDisruptorStrategy>();
        services.AddScoped<IRecommendationStrategy, GuiltyPleasureStrategy>();
        services.AddScoped<IRecommendationStrategy, HiddenGemsStrategy>();
        services.AddScoped<IRecommendationStrategy, DirectorsTrajectoryStrategy>();
        services.AddScoped<IRecommendationStrategy, BlastFromThePastStrategy>();
        services.AddScoped<IRecommendationStrategy, PureRandomStrategy>();
        services.AddScoped<IRecommendationStrategy, OutOfCharacterStrategy>();


        return services;
    }
}
