// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Admin;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.Admin;

public class GetProviderDiagnosticsQueryHandler : IRequestHandler<GetProviderDiagnosticsQuery, ProviderDiagnosticsDto>
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IApplicationDbContext _context;

    public GetProviderDiagnosticsQueryHandler(IHttpClientFactory clientFactory, IApplicationDbContext context)
    {
        _clientFactory = clientFactory;
        _context = context;
    }

    public async Task<ProviderDiagnosticsDto> Handle(GetProviderDiagnosticsQuery request, CancellationToken cancellationToken)
    {
        string tmdbStatus = "Unknown";
        long tmdbLatency = 0;
        string omdbStatus = "Unknown";
        long omdbLatency = 0;
        string backendStatus = "Unknown";
        long backendLatency = 0;

        // Test Database / Backend Connection
        try
        {
            var dbStopwatch = Stopwatch.StartNew();
            bool canConnect = false;
            if (_context is DbContext dbContext)
            {
                canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            }
            dbStopwatch.Stop();
            backendLatency = dbStopwatch.ElapsedMilliseconds;
            backendStatus = canConnect ? "Healthy" : "Database Offline";
        }
        catch (Exception ex)
        {
            backendStatus = $"Error: {ex.Message}";
        }

        // Test TMDB
        try
        {
            var client = _clientFactory.CreateClient("ITmdbService");
            var stopwatch = Stopwatch.StartNew();
            // Call a lightweight configuration endpoint to test authorization and reachability
            var response = await client.GetAsync("configuration", cancellationToken);
            stopwatch.Stop();
            tmdbLatency = stopwatch.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                tmdbStatus = "Healthy";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                tmdbStatus = "Unauthorized";
            }
            else
            {
                tmdbStatus = $"Unhealthy ({(int)response.StatusCode})";
            }
        }
        catch (Exception ex)
        {
            tmdbStatus = $"Error: {ex.Message}";
        }

        // Test OMDb
        try
        {
            var client = _clientFactory.CreateClient("IOmdbService");
            var stopwatch = Stopwatch.StartNew();
            // Call with a dummy query; OMDb requires api key in query string, which the client base path or default queries should handle.
            // Let's see: in OmdbService, does the handler append api key? Let's check.
            // If the query is just a ping, we can call "?s=Inception" to verify.
            var response = await client.GetAsync("?s=Inception", cancellationToken);
            stopwatch.Stop();
            omdbLatency = stopwatch.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                omdbStatus = "Healthy";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                omdbStatus = "Unauthorized";
            }
            else
            {
                omdbStatus = $"Unhealthy ({(int)response.StatusCode})";
            }
        }
        catch (Exception ex)
        {
            omdbStatus = $"Error: {ex.Message}";
        }

        return new ProviderDiagnosticsDto(tmdbStatus, tmdbLatency, omdbStatus, omdbLatency, backendStatus, backendLatency);
    }
}
