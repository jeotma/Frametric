// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces.Discovery;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Queries.Discovery;

public class RouletteSelectionQueryHandler : IRequestHandler<RouletteSelectionQuery, SelectionResultDto>
{
    private readonly IDiscoveryQueries _discoveryQueries;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RouletteSelectionQueryHandler> _logger;

    public RouletteSelectionQueryHandler(
        IDiscoveryQueries discoveryQueries,
        IDistributedCache cache,
        ILogger<RouletteSelectionQueryHandler> logger)
    {
        _discoveryQueries = discoveryQueries;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SelectionResultDto> Handle(RouletteSelectionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing roulette selection for user {UserId} with scope {Scope}", request.UserId, request.Scope);

        IEnumerable<Guid>? customSourceIds = request.CustomSourceIds?.ToArray();
        if (request.Scope == DiscoveryDataSourceScope.CustomCollection)
        {
            var titles = request.CustomSourceTitles?.Where(title => !string.IsNullOrWhiteSpace(title)).Select(title => title!.Trim()).ToArray();
            if ((customSourceIds == null || !customSourceIds.Any()) && (titles == null || !titles.Any()))
            {
                throw new InvalidOperationException("Custom collection requests require a list of source IDs or titles.");
            }

            if (customSourceIds == null || !customSourceIds.Any())
            {
                customSourceIds = (await _discoveryQueries.ResolveMovieIdsByTitlesAsync(titles!, cancellationToken)).ToArray();
            }
        }

        var pool = (await _discoveryQueries.GetDiscoveryPoolAsync(request.UserId, request.Scope, customSourceIds, cancellationToken)).ToList();
        if (!pool.Any())
        {
            throw new InvalidOperationException("No movies are available for roulette selection in the chosen discovery pool.");
        }

        var selected = request.PersistenceThreshold.HasValue && request.PersistenceThreshold.Value > 1
            ? await SelectWithPersistenceAsync(pool, request.PersistenceThreshold.Value, request.UserId, cancellationToken)
            : pool[Random.Shared.Next(pool.Count)];

        return new SelectionResultDto(
            selected.MovieId,
            selected.Title,
            selected.DirectorName ?? "Unknown",
            selected.ReleaseYear ?? 0,
            request.PersistenceThreshold.HasValue && request.PersistenceThreshold.Value > 1
                ? $"Roulette consensus threshold {request.PersistenceThreshold.Value} reached."
                : "Roulette selected a random movie from the discovery pool.",
            selected.PosterUrl,
            selected.RuntimeMinutes);
    }

    private async Task<DiscoveryMoviePoolItemDto> SelectWithPersistenceAsync(
        IReadOnlyList<DiscoveryMoviePoolItemDto> pool,
        int persistenceThreshold,
        Guid userId,
        CancellationToken cancellationToken)
    {
        DiscoveryMoviePoolItemDto? bestCandidate = null;
        int bestCount = 0;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var candidate = pool[Random.Shared.Next(pool.Count)];
            var count = await IncrementAppearanceCountAsync(userId, candidate.MovieId, cancellationToken);
            if (count >= persistenceThreshold)
            {
                return candidate;
            }

            if (count > bestCount)
            {
                bestCount = count;
                bestCandidate = candidate;
            }
        }

        return bestCandidate ?? pool[0];
    }

    private async Task<int> IncrementAppearanceCountAsync(Guid userId, Guid movieId, CancellationToken cancellationToken)
    {
        var cacheKey = $"discovery:roulette:counter:{userId}:{movieId}";
        var counterValue = await _cache.GetStringAsync(cacheKey, cancellationToken);
        var currentCount = 0;

        if (!string.IsNullOrWhiteSpace(counterValue) && int.TryParse(counterValue, out var parsed))
        {
            currentCount = parsed;
        }

        var nextCount = currentCount + 1;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        };

        await _cache.SetStringAsync(cacheKey, nextCount.ToString(), options, cancellationToken);
        return nextCount;
    }
}
