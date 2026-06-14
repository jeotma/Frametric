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
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Queries.Discovery;

public class RouletteSelectionQueryHandler : IRequestHandler<RouletteSelectionQuery, RouletteRaceResultDto>
{
    private readonly IDiscoveryQueries _discoveryQueries;
    private readonly ILogger<RouletteSelectionQueryHandler> _logger;

    public RouletteSelectionQueryHandler(
        IDiscoveryQueries discoveryQueries,
        ILogger<RouletteSelectionQueryHandler> logger)
    {
        _discoveryQueries = discoveryQueries;
        _logger = logger;
    }

    public async Task<RouletteRaceResultDto> Handle(RouletteSelectionQuery request, CancellationToken cancellationToken)
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

        var pool = (await _discoveryQueries.GetDiscoveryPoolAsync(request.UserId, request.Scope, customSourceIds, request.ExcludeWatched, cancellationToken)).ToList();
        if (!pool.Any())
        {
            throw new InvalidOperationException("No movies are available for roulette selection in the chosen discovery pool.");
        }

        var sequence = new List<SelectionResultDto>();
        SelectionResultDto? winner = null;

        if (request.WinningThreshold <= 1)
        {
            // Populate sequence with random candidates so the visual wheel has slices to render
            int displaySpins = Math.Min(pool.Count, 50);
            if (displaySpins < 20 && pool.Count > 0) displaySpins = 20; // Ensure a decent amount of slices even for small pools by repeating

            for (int i = 0; i < displaySpins - 1; i++)
            {
                var filler = pool[Random.Shared.Next(pool.Count)];
                sequence.Add(MapToDto(filler, "Visual filler for roulette wheel"));
            }

            var selected = pool[Random.Shared.Next(pool.Count)];
            winner = MapToDto(selected, "Roulette selected a random movie from the discovery pool.");
            sequence.Add(winner);
            return new RouletteRaceResultDto(winner, sequence);
        }

        var counts = new Dictionary<Guid, int>();
        // Simulate race
        while (true)
        {
            var candidate = pool[Random.Shared.Next(pool.Count)];
            var dto = MapToDto(candidate, $"Roulette race in progress. Winning threshold: {request.WinningThreshold}");
            sequence.Add(dto);

            counts[candidate.MovieId] = counts.TryGetValue(candidate.MovieId, out var c) ? c + 1 : 1;
            if (counts[candidate.MovieId] >= request.WinningThreshold)
            {
                winner = MapToDto(candidate, $"Roulette consensus threshold {request.WinningThreshold} reached.");
                sequence[^1] = winner;
                break;
            }
            
            // Safety break in case pool is huge and threshold is high to prevent infinite memory growth
            if (sequence.Count > 10000)
            {
                winner = MapToDto(candidate, "Roulette race hit simulation limit. Selected by sudden death.");
                sequence[^1] = winner;
                break;
            }
        }

        return new RouletteRaceResultDto(winner, sequence);
    }

    private SelectionResultDto MapToDto(DiscoveryMoviePoolItemDto selected, string metadata) =>
        new SelectionResultDto(
            selected.MovieId,
            selected.Title,
            selected.DirectorName ?? "Unknown",
            selected.ReleaseYear ?? 0,
            metadata,
            selected.PosterUrl,
            selected.RuntimeMinutes,
            selected.Overview);
}
