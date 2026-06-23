// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Frametric.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddFrametricHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "Database")
            .AddCheck("TMDB Configuration", () =>
            {
                var token = configuration["Tmdb:AccessToken"];
                return string.IsNullOrEmpty(token)
                    ? HealthCheckResult.Degraded("TMDB AccessToken is missing from configuration.")
                    : HealthCheckResult.Healthy("TMDB is configured.");
            });

        return services;
    }

    public static void MapFrametricHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/api/admin/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Status = report.Status.ToString(),
                    TotalDuration = report.TotalDuration,
                    Checks = report.Entries.Select(e => new
                    {
                        Component = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description,
                        Duration = e.Value.Duration
                    })
                };

                await JsonSerializer.SerializeAsync(context.Response.Body, response);
            }
        })
        .AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var authHeader = httpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Health Check\"";
                return TypedResults.Empty;
            }

            var encodedCredentials = authHeader["Basic ".Length..].Trim();
            string credentials;
            try
            {
                credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            }
            catch (FormatException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Health Check\"";
                return TypedResults.Empty;
            }

            var separatorIndex = credentials.IndexOf(':');
            if (separatorIndex < 0)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Health Check\"";
                return TypedResults.Empty;
            }

            var username = credentials[..separatorIndex];
            var password = credentials[(separatorIndex + 1)..];

            var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expectedUsername = config["HealthCheck:Username"];
            var expectedPassword = config["HealthCheck:Password"];

            if (username != expectedUsername || password != expectedPassword)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Health Check\"";
                return TypedResults.Empty;
            }

            return await next(context);
        });
    }
}
