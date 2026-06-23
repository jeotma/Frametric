// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSection = configuration.GetSection("Cors:AllowedOrigins");
        var allowedOrigins = corsSection.Get<string[]>();

        if (allowedOrigins is not { Length: > 0 })
        {
            var raw = corsSection.Value;
            allowedOrigins = !string.IsNullOrWhiteSpace(raw)
                ? raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : ["http://localhost:4200"];
        }

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
