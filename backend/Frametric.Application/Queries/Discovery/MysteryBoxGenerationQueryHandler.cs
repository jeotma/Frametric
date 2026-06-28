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
            throw new InvalidOperationException("No mystery box candidates are available for the selected discovery pool.");
        }

        string? filterType = null;
        string? filterValue = null;
        var (candidates, ft, fv) = request.Variant switch
        {
            MysteryBoxVariant.Thematic => BuildThematicPool(pool),
            MysteryBoxVariant.ActorFocus => BuildActorFocusPool(pool),
            MysteryBoxVariant.DirectorFocus => BuildDirectorFocusPool(pool),
            MysteryBoxVariant.Strategy => (BuildStrategyPool(pool), null, null),
            _ => (pool, null, null)
        };
        filterType = ft;
        filterValue = fv;

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

        return new MysteryBoxDto(boxIds, request.Variant, DateTime.UtcNow, hints, filterType, filterValue);
    }

    private static T SelectWeightedRandom<T>(IEnumerable<(T Item, double Weight)> items)
    {
        var list = items.ToList();
        var totalWeight = list.Sum(x => x.Weight);
        var r = Random.Shared.NextDouble() * totalWeight;
        double currentSum = 0;
        foreach (var elem in list)
        {
            currentSum += elem.Weight;
            if (r <= currentSum)
            {
                return elem.Item;
            }
        }
        return list.Last().Item;
    }

    private static (IEnumerable<DiscoveryMoviePoolItemDto> Pool, string? FilterType, string? FilterValue) BuildThematicPool(IEnumerable<DiscoveryMoviePoolItemDto> pool)
    {
        var genreGroups = pool
            .Where(item => !string.IsNullOrWhiteSpace(item.Genres))
            .SelectMany(item => item.Genres!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(genre => !string.IsNullOrWhiteSpace(genre))
            .GroupBy(genre => genre, StringComparer.OrdinalIgnoreCase)
            .Select(group => new { Genre = group.Key, Count = group.Count() })
            .ToList();

        if (!genreGroups.Any())
        {
            return (pool, null, null);
        }

        var weightedList = genreGroups.Select(g => (g.Genre, Weight: (double)g.Count));
        var selectedGenre = SelectWeightedRandom(weightedList);

        var themed = pool.Where(item =>
            !string.IsNullOrWhiteSpace(item.Genres) &&
            item.Genres!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(g => g.Equals(selectedGenre, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return (themed, "Genre", selectedGenre);
    }

    private static (IEnumerable<DiscoveryMoviePoolItemDto> Pool, string? FilterType, string? FilterValue) BuildActorFocusPool(IEnumerable<DiscoveryMoviePoolItemDto> pool)
    {
        var actorGroups = pool
            .Where(item => !string.IsNullOrWhiteSpace(item.ActorNames))
            .SelectMany(item => item.ActorNames!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(actor => !string.IsNullOrWhiteSpace(actor))
            .GroupBy(actor => actor, StringComparer.OrdinalIgnoreCase)
            .Select(group => new { Actor = group.Key, Count = group.Count() })
            .ToList();

        if (!actorGroups.Any())
        {
            return (pool, null, null);
        }

        var eligibleA = actorGroups.Where(g => g.Count >= 3).ToList();
        var eligibleB = actorGroups.Where(g => g.Count < 3).ToList();

        string selectedActor;
        if (eligibleA.Any() && Random.Shared.NextDouble() < 0.9)
        {
            var weightedList = eligibleA.Select(g => (g.Actor, Weight: (double)g.Count));
            selectedActor = SelectWeightedRandom(weightedList);
        }
        else
        {
            var remaining = eligibleB.Any() ? eligibleB : actorGroups;
            var weightedList = remaining.Select(g => (g.Actor, Weight: (double)g.Count));
            selectedActor = SelectWeightedRandom(weightedList);
        }

        var filtered = pool.Where(item =>
            !string.IsNullOrWhiteSpace(item.ActorNames) &&
            item.ActorNames!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(a => a.Equals(selectedActor, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return (filtered, "Actor", selectedActor);
    }

    private static (IEnumerable<DiscoveryMoviePoolItemDto> Pool, string? FilterType, string? FilterValue) BuildDirectorFocusPool(IEnumerable<DiscoveryMoviePoolItemDto> pool)
    {
        var directorGroups = pool
            .Where(item => !string.IsNullOrWhiteSpace(item.DirectorName))
            .SelectMany(item => item.DirectorName!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(director => !string.IsNullOrWhiteSpace(director))
            .GroupBy(director => director, StringComparer.OrdinalIgnoreCase)
            .Select(group => new { Director = group.Key, Count = group.Count() })
            .ToList();

        if (!directorGroups.Any())
        {
            return (pool, null, null);
        }

        var eligibleA = directorGroups.Where(g => g.Count >= 3).ToList();
        var eligibleB = directorGroups.Where(g => g.Count < 3).ToList();

        string selectedDirector;
        if (eligibleA.Any() && Random.Shared.NextDouble() < 0.9)
        {
            var weightedList = eligibleA.Select(g => (g.Director, Weight: (double)g.Count));
            selectedDirector = SelectWeightedRandom(weightedList);
        }
        else
        {
            var remaining = eligibleB.Any() ? eligibleB : directorGroups;
            var weightedList = remaining.Select(g => (g.Director, Weight: (double)g.Count));
            selectedDirector = SelectWeightedRandom(weightedList);
        }

        var filtered = pool.Where(item =>
            !string.IsNullOrWhiteSpace(item.DirectorName) &&
            item.DirectorName!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(d => d.Equals(selectedDirector, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return (filtered, "Director", selectedDirector);
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
        var selectedItems = new List<DiscoveryMoviePoolItemDto>();
        if (!pool.Any()) return selectedItems;

        var selectedIds = new HashSet<Guid>();
        foreach (var candidate in pool.OrderBy(_ => Random.Shared.Next()))
        {
            if (selectedIds.Add(candidate.MovieId))
            {
                selectedItems.Add(candidate);
            }
            if (selectedItems.Count >= boxCount) break;
        }

        while (selectedItems.Count < boxCount)
        {
            var candidate = pool[Random.Shared.Next(pool.Count)];
            selectedItems.Add(candidate);
        }

        return selectedItems;
    }
}
