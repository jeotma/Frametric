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

public class MysteryBoxGenerationQueryHandler : IRequestHandler<MysteryBoxGenerationQuery, MysteryBoxDto>
{
    private readonly IDiscoveryQueries _discoveryQueries;
    private readonly ILogger<MysteryBoxGenerationQueryHandler> _logger;

    public MysteryBoxGenerationQueryHandler(
        IDiscoveryQueries discoveryQueries,
        ILogger<MysteryBoxGenerationQueryHandler> logger)
    {
        _discoveryQueries = discoveryQueries;
        _logger = logger;
    }

    public async Task<MysteryBoxDto> Handle(MysteryBoxGenerationQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating mystery box for user {UserId} with variant {Variant} and box count {BoxCount}", request.UserId, request.Variant, request.BoxCount);

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
            throw new InvalidOperationException("No mystery box candidates are available for the selected discovery pool.");
        }

        var candidates = request.Variant switch
        {
            MysteryBoxVariant.Thematic => BuildThematicPool(pool),
            MysteryBoxVariant.Premium => BuildPremiumPool(pool),
            MysteryBoxVariant.FullReveal => BuildFullRevealPool(pool),
            MysteryBoxVariant.Strategy => BuildStrategyPool(pool),
            _ => pool
        };

        var selected = SelectUniqueMovieIds(candidates.ToList(), request.BoxCount).ToList();
        if (!selected.Any())
        {
            throw new InvalidOperationException("Unable to build mystery box due to insufficient candidates.");
        }

        var boxIds = selected.Select(s => s.MovieId).ToList();

        IReadOnlyList<MysteryBoxHintDto>? hints = null;
        if (request.Variant == MysteryBoxVariant.Strategy)
        {
            hints = selected.Select(s =>
            {
                var genreHint = (s.Genres ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "mystery";
                return new MysteryBoxHintDto(s.MovieId, $"Genre hint: {genreHint}");
            }).ToList();
        }

        return new MysteryBoxDto(boxIds, request.Variant, DateTime.UtcNow, hints);
    }

    private static IEnumerable<DiscoveryMoviePoolItemDto> BuildThematicPool(IEnumerable<DiscoveryMoviePoolItemDto> pool)
    {
        var genres = pool
            .Where(item => !string.IsNullOrWhiteSpace(item.Genres))
            .SelectMany(item => item.Genres!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(genre => !string.IsNullOrWhiteSpace(genre))
            .GroupBy(genre => genre, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .ToList();

        if (!genres.Any())
        {
            return pool;
        }

        var selectedGenre = genres[Random.Shared.Next(genres.Count)];
        var themed = pool.Where(item =>
            !string.IsNullOrWhiteSpace(item.Genres) &&
            item.Genres!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(g => g.Equals(selectedGenre, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return themed.Count >= 5 ? themed : pool;
    }

    private static IEnumerable<DiscoveryMoviePoolItemDto> BuildPremiumPool(IEnumerable<DiscoveryMoviePoolItemDto> pool)
    {
        var scored = pool
            .Select(item => new
            {
                Item = item,
                Score = (item.CustomAverageRating ?? item.TmdbRating ?? 6.0) * 0.8 + (item.TmdbPopularity ?? 30.0) * 0.2
            })
            .OrderByDescending(pair => pair.Score)
            .ToList();

        var topCount = Math.Max(5, scored.Count * 40 / 100);
        return scored.Take(topCount).Select(pair => pair.Item);
    }

    private static IEnumerable<DiscoveryMoviePoolItemDto> BuildFullRevealPool(IEnumerable<DiscoveryMoviePoolItemDto> pool)
    {
        return pool
            .OrderByDescending(m => m.CustomAverageRating ?? m.TmdbRating ?? 0)
            .Take(Math.Max(10, pool.Count() / 2))
            .ToList();
    }

    private static IEnumerable<DiscoveryMoviePoolItemDto> BuildStrategyPool(IEnumerable<DiscoveryMoviePoolItemDto> pool)
    {
        var genreGroups = pool
            .Where(m => !string.IsNullOrWhiteSpace(m.Genres))
            .SelectMany(m =>
            {
                var genres = m.Genres!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return genres.Select(g => (Genre: g, Movie: m));
            })
            .GroupBy(x => x.Genre, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Movie).Distinct().ToList());

        var selected = new HashSet<Guid>();
        var result = new List<DiscoveryMoviePoolItemDto>();

        foreach (var group in genreGroups.Values.OrderByDescending(g => g.Count))
        {
            foreach (var movie in group)
            {
                if (selected.Add(movie.MovieId))
                {
                    result.Add(movie);
                }
            }
        }

        return result.Concat(pool.Where(m => selected.Add(m.MovieId)));
    }

    private static List<DiscoveryMoviePoolItemDto> SelectUniqueMovieIds(IReadOnlyList<DiscoveryMoviePoolItemDto> pool, int boxCount)
    {
        var selectedIds = new HashSet<Guid>();
        var selectedItems = new List<DiscoveryMoviePoolItemDto>();
        var attempts = 0;
        while (selectedItems.Count < boxCount && attempts < pool.Count * 3)
        {
            var candidate = pool[Random.Shared.Next(pool.Count)];
            if (selectedIds.Add(candidate.MovieId))
            {
                selectedItems.Add(candidate);
            }
            attempts++;
        }

        return selectedItems.Take(boxCount).ToList();
    }
}
