using System.Reflection;
using Frametric.Application.Interfaces;
using Frametric.Application.Services;
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
        
        return services;
    }
}
