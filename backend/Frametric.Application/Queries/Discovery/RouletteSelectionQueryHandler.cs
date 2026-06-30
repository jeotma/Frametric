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

        Guid? partnerUserId = null;
        if (request.Scope == DiscoveryDataSourceScope.MergedWatchlists && !string.IsNullOrWhiteSpace(request.PartnerUsername))
        {
            partnerUserId = await _discoveryQueries.GetUserIdByUsernameAsync(request.PartnerUsername, cancellationToken);
            if (partnerUserId == null)
            {
                throw new InvalidOperationException($"User '{request.PartnerUsername}' not found.");
            }
        }

        var pool = (await _discoveryQueries.GetDiscoveryPoolAsync(request.UserId, request.Scope, customSourceIds, request.ExcludeWatched, partnerUserId, cancellationToken)).ToList();
        if (!pool.Any())
        {
            throw new InvalidOperationException("No movies are available for roulette selection in the chosen discovery pool.");
        }

        if (request.Scope != DiscoveryDataSourceScope.CustomCollection && pool.Count > 50)
        {
            pool = pool.OrderBy(_ => Random.Shared.Next()).Take(50).ToList();
        }

        var sequence = new List<SelectionResultDto>();
        SelectionResultDto? winner = null;

        if (request.WinningThreshold <= 1)
        {
            // Populate sequence first with ALL elements from the pool to guarantee their representation as slices
            foreach (var movie in pool)
            {
                sequence.Add(MapToDto(movie, "Pool option for roulette wheel", request.CustomAliases));
            }

            int targetSpins = Math.Max(20, Math.Min(pool.Count * 2, 50));
            while (sequence.Count < targetSpins - 1)
            {
                var filler = pool[Random.Shared.Next(pool.Count)];
                sequence.Add(MapToDto(filler, "Visual filler for roulette wheel", request.CustomAliases));
            }

            var selected = pool[Random.Shared.Next(pool.Count)];
            winner = MapToDto(selected, "Roulette selected a random movie from the discovery pool.", request.CustomAliases);
            
            // If the selected winner is not already the last element, add it or replace last element
            if (sequence.Count >= targetSpins)
            {
                sequence[^1] = winner;
            }
            else
            {
                sequence.Add(winner);
            }
            return new RouletteRaceResultDto(winner, sequence, new[] { winner });
        }

        var counts = new Dictionary<Guid, int>();
        var winners = new List<SelectionResultDto>();
        var winnerIds = new HashSet<Guid>();

        int targetWinnerCount = request.AllowMultipleWinners ? Math.Max(1, request.WinnerCount) : 1;

        // Simulate race
        while (true)
        {
            var candidate = pool[Random.Shared.Next(pool.Count)];
            var dto = MapToDto(candidate, $"Roulette race in progress. Winning threshold: {request.WinningThreshold}", request.CustomAliases);
            sequence.Add(dto);

            counts[candidate.MovieId] = counts.TryGetValue(candidate.MovieId, out var c) ? c + 1 : 1;
            if (counts[candidate.MovieId] >= request.WinningThreshold && !winnerIds.Contains(candidate.MovieId))
            {
                var stepWinner = MapToDto(candidate, $"Roulette consensus threshold {request.WinningThreshold} reached.", request.CustomAliases);
                sequence[^1] = stepWinner;
                
                winners.Add(stepWinner);
                winnerIds.Add(candidate.MovieId);

                if (winners.Count >= targetWinnerCount || winnerIds.Count >= pool.Count)
                {
                    winner = stepWinner;
                    break;
                }
            }
            
            // Safety break: cap at 100 * targetWinnerCount race entries to prevent infinite spinning on the frontend
            if (sequence.Count > 100 * targetWinnerCount + pool.Count)
            {
                var leaders = counts
                    .Where(kvp => !winnerIds.Contains(kvp.Key))
                    .OrderByDescending(kvp => kvp.Value)
                    .Select(kvp => kvp.Key)
                    .Take(targetWinnerCount - winners.Count)
                    .ToList();

                foreach (var leader in leaders)
                {
                    var leaderMovie = pool.First(m => m.MovieId == leader);
                    var stepWinner = MapToDto(leaderMovie, $"Roulette race capped. Winner by majority.", request.CustomAliases);
                    sequence.Add(stepWinner);
                    winners.Add(stepWinner);
                    winnerIds.Add(leader);
                }

                if (winners.Any())
                {
                    winner = winners.Last();
                }
                else
                {
                    var defaultWinner = pool.First();
                    winner = MapToDto(defaultWinner, "Default winner due to capping", request.CustomAliases);
                    winners.Add(winner);
                }
                break;
            }
        }

        return new RouletteRaceResultDto(winner, sequence, winners);
    }

    private SelectionResultDto MapToDto(DiscoveryMoviePoolItemDto selected, string metadata, Dictionary<Guid, string>? aliases = null)
    {
        var displayTitle = (aliases != null && aliases.TryGetValue(selected.MovieId, out var alias) && !string.IsNullOrWhiteSpace(alias))
            ? alias
            : selected.Title;

        return new SelectionResultDto(
            selected.MovieId,
            displayTitle,
            selected.DirectorName ?? "Unknown",
            selected.ReleaseYear ?? 0,
            metadata,
            selected.PosterUrl,
            selected.RuntimeMinutes,
            selected.Overview,
            selected.InCurrentUserWatchlist,
            selected.InPartnerUserWatchlist);
    }
}
