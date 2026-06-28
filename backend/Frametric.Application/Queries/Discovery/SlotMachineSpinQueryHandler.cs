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

public class SlotMachineSpinQueryHandler : IRequestHandler<SlotMachineSpinQuery, SlotMachineResultDto>
{
    private readonly IDiscoveryQueries _discoveryQueries;
    private readonly ILogger<SlotMachineSpinQueryHandler> _logger;

    public SlotMachineSpinQueryHandler(IDiscoveryQueries discoveryQueries, ILogger<SlotMachineSpinQueryHandler> logger)
    {
        _discoveryQueries = discoveryQueries;
        _logger = logger;
    }

    public async Task<SlotMachineResultDto> Handle(SlotMachineSpinQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing slot machine spin for user {UserId} with scope {Scope}", request.UserId, request.Scope);

        var customSourceIds = await ResolveCustomSourceIds(request, cancellationToken);
        var pool = (await _discoveryQueries.GetDiscoveryPoolAsync(request.UserId, request.Scope, customSourceIds, request.ExcludeWatched, null, cancellationToken)).ToList();

        if (!pool.Any())
        {
            throw new InvalidOperationException("No movies are available for slot machine in the chosen discovery pool.");
        }

        var genre = ResolveGenre(request.Genre);
        var decade = ResolveDecade(request.Decade);
        var popularity = ResolvePopularity(request.Popularity);
        var rating = ResolveRating(request.Rating);
        var country = ResolveCountry(request.Country);

        // Calculate match metrics for each movie in the pool
        var results = pool.Select(movie =>
        {
            var movieGenres = (movie.Genres ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var meetsGenre = movieGenres.Contains(genre, StringComparer.OrdinalIgnoreCase);

            var movieDecade = movie.ReleaseYear.HasValue ? (movie.ReleaseYear.Value / 10) * 10 : 0;
            var meetsDecade = movieDecade == decade;

            var pop = movie.TmdbPopularity ?? 0;
            var popClass = pop >= 50 ? "BLOCKBUSTER" : pop >= 20 ? "MAINSTREAM" : pop >= 8 ? "NICHE / CULT" : "HIDDEN GEM";
            var meetsPopularity = string.Equals(popClass, popularity, StringComparison.OrdinalIgnoreCase);

            var rat = movie.CustomAverageRating ?? movie.TmdbRating ?? 0;
            var ratClass = rat >= 8.0 ? "MASTERPIECE" : rat >= 7.0 ? "GREAT" : rat >= 6.0 ? "DECENT" : "UNDERDOG";
            var meetsRating = string.Equals(ratClass, rating, StringComparison.OrdinalIgnoreCase);

            // Handle comma-separated movie countries by splitting and checking matching strings
            var movieCountries = (movie.Country ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var meetsCountry = movieCountries.Contains(country, StringComparer.OrdinalIgnoreCase);

            var matchedList = new[] { meetsGenre, meetsDecade, meetsPopularity, meetsRating, meetsCountry };
            var matchCount = matchedList.Count(m => m);

            return new { Movie = movie, MatchedReels = matchedList, MatchCount = matchCount };
        }).ToList();

        // Complex fallback logic: find maximum possible matches (5 down to 0)
        var maxMatches = results.Max(r => r.MatchCount);
        var bestMatches = results.Where(r => r.MatchCount == maxMatches).ToList();
        var selectedItem = bestMatches[Random.Shared.Next(bestMatches.Count)];
        
        var selected = selectedItem.Movie;
        var isJackpot = maxMatches == 5 && (selected.TmdbRating ?? 0) >= 8.2;

        var reelResults = new List<SlotReelResultDto>
        {
            new("Genre", !string.IsNullOrWhiteSpace(request.Genre) ? request.Genre : (genre ?? "Any")),
            new("Decade", request.Decade.HasValue ? $"{request.Decade.Value}s" : (decade.HasValue ? $"{decade.Value}s" : "Any")),
            new("Popularity", !string.IsNullOrWhiteSpace(request.Popularity) ? request.Popularity : (popularity ?? "Any")),
            new("Rating", !string.IsNullOrWhiteSpace(request.Rating) ? request.Rating : (rating ?? "Any")),
            new("Country", !string.IsNullOrWhiteSpace(request.Country) ? request.Country : (country ?? "Any")),
        };

        var matchedReels = selectedItem.MatchedReels.ToList();

        return new SlotMachineResultDto(
            selected.MovieId,
            selected.Title,
            selected.DirectorName ?? "Unknown",
            selected.ReleaseYear ?? 0,
            selected.PosterUrl,
            selected.RuntimeMinutes,
            $"Slot machine matched {maxMatches}/5 reels for the selected film.",
            reelResults,
            isJackpot,
            maxMatches,
            matchedReels,
            selected.Overview);
    }

    private async Task<IEnumerable<Guid>?> ResolveCustomSourceIds(SlotMachineSpinQuery request, CancellationToken cancellationToken)
    {
        if (request.Scope != DiscoveryDataSourceScope.CustomCollection)
        {
            return null;
        }

        var ids = request.CustomSourceIds?.ToArray();
        var titles = request.CustomSourceTitles?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToArray();

        if ((ids == null || !ids.Any()) && (titles == null || !titles.Any()))
        {
            throw new InvalidOperationException("Custom collection requests require a list of source IDs or titles.");
        }

        return (ids != null && ids.Any()) ? ids : (await _discoveryQueries.ResolveMovieIdsByTitlesAsync(titles!, cancellationToken)).ToArray();
    }

    private static string? ResolveGenre(string? genre)
    {
        if (!string.IsNullOrWhiteSpace(genre)) return genre;
        var options = new[] { "ACTION", "COMEDY", "DRAMA", "HORROR", "THRILLER" };
        return options[Random.Shared.Next(options.Length)];
    }

    private static int? ResolveDecade(int? decade)
    {
        if (decade.HasValue) return decade;
        var options = new[] { 1980, 1990, 2000, 2010, 2020 };
        return options[Random.Shared.Next(options.Length)];
    }

    private static string? ResolvePopularity(string? popularity)
    {
        if (!string.IsNullOrWhiteSpace(popularity)) return popularity;
        var options = new[] { "BLOCKBUSTER", "MAINSTREAM", "NICHE / CULT", "HIDDEN GEM" };
        return options[Random.Shared.Next(options.Length)];
    }

    private static string? ResolveRating(string? rating)
    {
        if (!string.IsNullOrWhiteSpace(rating)) return rating;
        var options = new[] { "MASTERPIECE", "GREAT", "DECENT", "UNDERDOG" };
        return options[Random.Shared.Next(options.Length)];
    }

    private static string? ResolveCountry(string? country)
    {
        if (!string.IsNullOrWhiteSpace(country)) return country;
        var options = new[] { "USA", "UK", "FRANCE", "JAPAN", "SOUTH KOREA" };
        return options[Random.Shared.Next(options.Length)];
    }
}
