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

        return services;
    }
}
