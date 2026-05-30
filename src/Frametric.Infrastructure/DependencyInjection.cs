using Frametric.Application.Interfaces;
using Frametric.Infrastructure.Importer;
using Frametric.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Frametric.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FrametricDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<FrametricDbContext>());
        
        services.AddScoped<ILetterboxdImporter, LetterboxdZipImporter>();

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

        services.AddHostedService<Frametric.Infrastructure.BackgroundJobs.TmdbEnrichmentBackgroundService>();

        return services;
    }
}
