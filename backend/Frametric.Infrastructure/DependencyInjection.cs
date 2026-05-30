using Frametric.Application.Interfaces;
using Frametric.Infrastructure.Importer;
using Frametric.Infrastructure.Persistence;
using Frametric.Infrastructure.Security;
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
        
        services.AddScoped<ILetterboxdImporter, LetterboxdZipImporter>();

        // Register Dapper DbConnectionFactory
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        // Register Decoupled Infrastructure Services
        services.AddScoped<IAnalyticsService, DapperAnalyticsService>();
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

        services.AddSingleton<Frametric.Infrastructure.BackgroundJobs.TmdbEnrichmentTrigger>();
        services.AddSingleton<ITmdbEnrichmentTrigger>(provider => provider.GetRequiredService<Frametric.Infrastructure.BackgroundJobs.TmdbEnrichmentTrigger>());

        services.AddHostedService<Frametric.Infrastructure.BackgroundJobs.TmdbEnrichmentBackgroundService>();

        return services;
    }
}
