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
using Frametric.Domain.Discovery.Enums;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Queries.Discovery;

public class DiceRollQueryHandler : IRequestHandler<DiceRollQuery, DiceRollResultDto>
{
    private readonly IDiscoveryQueries _discoveryQueries;
    private readonly ILogger<DiceRollQueryHandler> _logger;

    private static readonly IReadOnlyDictionary<DiceType, int> MaxValues = new Dictionary<DiceType, int>
    {
        [DiceType.Duration] = 3,     // D3
        [DiceType.Popularity] = 4,   // D4
        [DiceType.Risk] = 6,         // D6
        [DiceType.Quality] = 12,     // D12
        [DiceType.Genre] = 20,       // D20
    };

    private static readonly string[] GenresD20 = new[]
    {
        "Action",          // 1
        "Adventure",       // 2
        "Animation",       // 3
        "Comedy",          // 4
        "Crime",           // 5
        "Documentary",     // 6
        "Drama",           // 7
        "Family",          // 8
        "Fantasy",         // 9
        "History",         // 10
        "Horror",          // 11
        "Music",           // 12
        "Mystery",         // 13
        "Romance",         // 14
        "Science Fiction", // 15
        "Thriller",        // 16
        "War",             // 17
        "Western",         // 18
        "TV Movie"         // 19
    };

    public DiceRollQueryHandler(IDiscoveryQueries discoveryQueries, ILogger<DiceRollQueryHandler> logger)
    {
        _discoveryQueries = discoveryQueries;
        _logger = logger;
    }

    /// <summary>
    /// Builds a personalized genre table for the D20.
    /// Genres watched most by the user occupy the highest values (20 = most watched).
    /// Unwatched or unknown genres fill the remaining lower slots in default order.
    /// Value 20 is always Wildcard.
    /// </summary>
    private static string[] BuildPersonalizedGenreTable(IReadOnlyList<string> userTopGenres)
    {
        // GenresD20 has 19 slots (indices 0–18 → die values 1–19); value 20 = Wildcard.
        var personalized = new string[19];

        // Genres the user has watched, in descending order
        var watchedInOrder = userTopGenres
            .Where(g => GenresD20.Contains(g, StringComparer.OrdinalIgnoreCase))
            .ToList();

        // Genres NOT in user history, keeping default order
        var remaining = GenresD20
            .Where(g => !watchedInOrder.Contains(g, StringComparer.OrdinalIgnoreCase))
            .ToList();

        // Merge: most-watched genre → slot 18 (die value 19), second → slot 17, etc.
        // Remaining genres fill the lower slots (slot 0 = die value 1).
        var orderedFull = remaining.Concat(watchedInOrder).ToArray();

        for (int i = 0; i < 19; i++)
        {
            personalized[i] = orderedFull[i];
        }

        return personalized;
    }

    public async Task<DiceRollResultDto> Handle(DiceRollQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing dice roll for user {UserId} with scope {Scope}", request.UserId, request.Scope);

        var diceToRoll = request.DiceTypes ?? new List<DiceType> { DiceType.Duration, DiceType.Popularity, DiceType.Risk, DiceType.Quality, DiceType.Genre };

        // Build personalized genre table for D20 based on user's watch history
        var userTopGenres = await _discoveryQueries.GetUserTopGenresAsync(request.UserId, cancellationToken);
        var genreTable = userTopGenres.Any() ? BuildPersonalizedGenreTable(userTopGenres) : GenresD20;

        var rollResults = diceToRoll.Select(dt =>
        {
            int? preset = null;
            if (request.Presets != null && request.Presets.TryGetValue(dt, out var val))
            {
                preset = val;
            }
            return RollDie(dt, genreTable, preset);
        }).ToList();

        var customSourceIds = await ResolveCustomSourceIds(request, cancellationToken);
        var pool = (await _discoveryQueries.GetDiscoveryPoolAsync(request.UserId, request.Scope, customSourceIds, request.ExcludeWatched, cancellationToken)).ToList();

        if (!pool.Any())
        {
            throw new InvalidOperationException("No movies are available for dice roll in the chosen discovery pool.");
        }

        // Extract roll values
        int d3 = rollResults.FirstOrDefault(r => r.DiceType == DiceType.Duration)?.RollValue ?? 2;
        int d4 = rollResults.FirstOrDefault(r => r.DiceType == DiceType.Popularity)?.RollValue ?? 2;
        int d12 = rollResults.FirstOrDefault(r => r.DiceType == DiceType.Quality)?.RollValue ?? 6;
        int d20 = rollResults.FirstOrDefault(r => r.DiceType == DiceType.Genre)?.RollValue ?? 20;

        int adjD3 = d3;
        int adjD4 = d4;
        int adjD12 = d12;
        int adjD20 = d20;

        List<DiscoveryMoviePoolItemDto> filteredPool = new();
        int matchDistance = 0;
        bool found = false;

        // Try relaxation steps (limit to 100 steps to avoid infinite loops)
        for (int step = 0; step <= 100; step++)
        {
            var constraints = BuildConstraints(adjD3, adjD4, adjD12, adjD20, genreTable);
            filteredPool = pool.Where(m => MatchesConstraints(m, constraints)).ToList();
            if (filteredPool.Any())
            {
                found = true;
                break;
            }

            // Adjust constraints in round-robin sequence
            matchDistance++;
            int phase = step % 4;
            if (phase == 0)
            {
                adjD12 = RelaxQuality(adjD12, d12);
            }
            else if (phase == 1)
            {
                adjD4 = RelaxPopularity(adjD4, d4);
            }
            else if (phase == 2)
            {
                adjD3 = RelaxDuration(adjD3, d3);
            }
            else if (phase == 3)
            {
                if (adjD20 != 20)
                {
                    adjD20 = 20; // Relax D20 to Wildcard first
                }
            }
        }

        var selected = filteredPool.Any() ? filteredPool[Random.Shared.Next(filteredPool.Count)] : pool[Random.Shared.Next(pool.Count)];
        var specialEvent = DetectSpecialEvent(rollResults);

        string metadata = found 
            ? (matchDistance == 0 ? "Dice-selected perfect match." : $"Dice-selected with minor calibration (relaxed {matchDistance} steps).") 
            : "No match found — full pool fallback.";

        return new DiceRollResultDto(
            selected.MovieId,
            selected.Title,
            selected.DirectorName ?? "Unknown",
            selected.ReleaseYear ?? 0,
            selected.PosterUrl,
            selected.RuntimeMinutes,
            metadata,
            rollResults,
            specialEvent,
            selected.Overview,
            found ? matchDistance : 9999);
    }

    private async Task<IEnumerable<Guid>?> ResolveCustomSourceIds(DiceRollQuery request, CancellationToken cancellationToken)
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

    private static SingleDieResultDto RollDie(DiceType diceType, string[] genreTable, int? presetRoll = null)
    {
        var max = MaxValues[diceType];
        var roll = presetRoll ?? Random.Shared.Next(1, max + 1);

        var (label, description) = diceType switch
        {
            DiceType.Duration => roll switch
            {
                1 => ("<= 1h30m", "A quick watch under 90 minutes."),
                2 => ("1h30m - 2h30m", "A standard length feature film."),
                _ => (">= 2h30m", "An epic cinematic experience.")
            },
            DiceType.Popularity => roll switch
            {
                1 => ("Blockbuster", "A widely-known mainstream film."),
                2 => ("Mainstream", "A film most cinephiles recognize."),
                3 => ("Niche / Cult", "A film that flew under most radars."),
                _ => ("Hidden gem", "A true hidden gem awaiting discovery.")
            },
            DiceType.Risk => roll switch
            {
                1 => ("Safe", "Comfortably within your established taste."),
                2 => ("Slight shift", "A gentle nudge outside your usual zone."),
                3 => ("Moderate", "A noticeable departure from your norms."),
                4 => ("Risky", "Far outside your usual cinematic territory."),
                _ => ("Total chaos", "Complete unpredictability — anything could happen.")
            },
            DiceType.Quality => roll switch
            {
                1 => ("Terrible", "An entertaining but not particularly notable film."),
                2 => ("Poor", "Lacks polish or depth."),
                3 => ("Mediocre", "An average, run-of-the-mill production."),
                4 => ("Average", "A decent middle-ground watch."),
                5 => ("Solid", "A well-crafted, reliable movie."),
                6 => ("Good", "Highly enjoyable with solid execution."),
                7 => ("Really Good", "Stands out with strong direction and script."),
                8 => ("Great", "A highly acclaimed, near-flawless film."),
                9 => ("Excellent", "Superb artistic achievement."),
                10 => ("Masterpiece", "Among the very best of its genre."),
                11 => ("Legendary", "A historical landmark in cinema history."),
                _ => ("Absolute Cinema", "Cinematic peak — pure masterclass.")
            },
            DiceType.Genre => roll == 20
                ? ("Wildcard", "No genre constraint.")
                : (genreTable[roll - 1], $"Focus on {genreTable[roll - 1]} films."),
            _ => ("Unknown", "An indeterminate roll.")
        };

        return new SingleDieResultDto(diceType, roll, label, description);
    }

    private static DiceConstraints BuildConstraints(int d3, int d4, int d12, int d20, string[] genreTable)
    {
        double? minRating = null, maxRating = null;
        double? minPopularity = null, maxPopularity = null;
        int? minRuntime = null, maxRuntime = null;
        string? genre = null;

        // D3 (Duration)
        switch (d3)
        {
            case 1: minRuntime = null; maxRuntime = 90; break;
            case 2: minRuntime = 91; maxRuntime = 149; break;
            case 3: minRuntime = 150; maxRuntime = null; break;
        }

        // D4 (Popularity)
        switch (d4)
        {
            case 1: minPopularity = 50.0; maxPopularity = null; break;
            case 2: minPopularity = 20.0; maxPopularity = 50.0; break;
            case 3: minPopularity = 5.0; maxPopularity = 20.0; break;
            case 4: minPopularity = null; maxPopularity = 5.0; break;
        }

        // D12 (Quality)
        switch (d12)
        {
            case 1: minRating = null; maxRating = 3.5; break;
            case 2: minRating = 3.5; maxRating = 5.0; break;
            case 3: minRating = 5.0; maxRating = 6.0; break;
            case 4: minRating = 6.0; maxRating = 6.5; break;
            case 5: minRating = 6.5; maxRating = 7.0; break;
            case 6: minRating = 7.0; maxRating = 7.5; break;
            case 7: minRating = 7.5; maxRating = 8.0; break;
            case 8: minRating = 8.0; maxRating = 8.5; break;
            case 9: minRating = 8.5; maxRating = 9.0; break;
            case 10: minRating = 9.0; maxRating = 9.5; break;
            case 11: minRating = 9.5; maxRating = 10.0; break;
            case 12: minRating = 10.0; maxRating = null; break;
        }

        // D20 (Genre) — uses the genre name from the roll value via genreTable in BuildConstraints context.
        // The label is already resolved in RollDie; here we resolve what genre string to filter on.
        if (d20 >= 1 && d20 <= 19)
        {
            genre = genreTable[d20 - 1];
        }

        return new DiceConstraints(minRating, maxRating, minRuntime, maxRuntime, minPopularity, maxPopularity, genre);
    }

    private static bool MatchesConstraints(DiscoveryMoviePoolItemDto movie, DiceConstraints constraints)
    {
        if (constraints.MinRating.HasValue && (movie.TmdbRating ?? 0) < constraints.MinRating.Value) return false;
        if (constraints.MaxRating.HasValue && (movie.TmdbRating ?? 10) >= constraints.MaxRating.Value) return false;
        if (constraints.MinPopularity.HasValue && (movie.TmdbPopularity ?? 0) < constraints.MinPopularity.Value) return false;
        if (constraints.MaxPopularity.HasValue && (movie.TmdbPopularity ?? 1000) >= constraints.MaxPopularity.Value) return false;
        if (constraints.MinRuntime.HasValue && (movie.RuntimeMinutes ?? 0) < constraints.MinRuntime.Value) return false;
        if (constraints.MaxRuntime.HasValue && (movie.RuntimeMinutes ?? 1000) > constraints.MaxRuntime.Value) return false;
        
        if (!string.IsNullOrEmpty(constraints.Genre))
        {
            if (string.IsNullOrEmpty(movie.Genres) || !movie.Genres.Contains(constraints.Genre, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static int RelaxQuality(int current, int original)
    {
        if (original == 12) return Math.Max(1, current - 1);
        if (original == 1) return Math.Min(12, current + 1);
        
        int diff = current - original;
        if (diff == 0) return original - 1;
        if (diff < 0)
        {
            int k = -diff;
            int next = original + k;
            if (next <= 12) return next;
            return original - (k + 1);
        }
        else
        {
            int k = diff;
            int next = original - (k + 1);
            if (next >= 1) return next;
            return original + k + 1;
        }
    }

    private static int RelaxPopularity(int current, int original)
    {
        if (original == 4) return Math.Max(1, current - 1);
        if (original == 1) return Math.Min(4, current + 1);
        
        int diff = current - original;
        if (diff == 0) return original - 1;
        if (diff < 0)
        {
            int k = -diff;
            int next = original + k;
            if (next <= 4) return next;
            return original - (k + 1);
        }
        else
        {
            int k = diff;
            int next = original - (k + 1);
            if (next >= 1) return next;
            return original + k + 1;
        }
    }

    private static int RelaxDuration(int current, int original)
    {
        if (original == 3) return Math.Max(1, current - 1);
        if (original == 1) return Math.Min(3, current + 1);
        
        int diff = current - original;
        if (diff == 0) return original - 1;
        if (diff < 0)
        {
            int k = -diff;
            int next = original + k;
            if (next <= 3) return next;
            return original - (k + 1);
        }
        else
        {
            int k = diff;
            int next = original - (k + 1);
            if (next >= 1) return next;
            return original + k + 1;
        }
    }

    private static string? DetectSpecialEvent(IReadOnlyList<SingleDieResultDto> rolls)
    {
        var qualityRoll = rolls.FirstOrDefault(r => r.DiceType == DiceType.Quality);
        var riskRoll = rolls.FirstOrDefault(r => r.DiceType == DiceType.Risk);

        if (qualityRoll?.RollValue >= 12)
        {
            return "Critical Success — a potential masterpiece has been identified.";
        }

        if (qualityRoll?.RollValue <= 2 && riskRoll?.RollValue >= 5)
        {
            return "Chaos Mode — critical fumble! Brace for the unexpected.";
        }

        return null;
    }

    private readonly record struct DiceConstraints(
        double? MinRating, double? MaxRating,
        int? MinRuntime, int? MaxRuntime,
        double? MinPopularity, double? MaxPopularity,
        string? Genre);
}
