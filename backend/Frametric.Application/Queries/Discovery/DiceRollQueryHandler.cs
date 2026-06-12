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
        [DiceType.Quality] = 8,
        [DiceType.Rarity] = 6,
        [DiceType.Risk] = 6,
        [DiceType.Complexity] = 8,
        [DiceType.Exploration] = 6,
    };

    public DiceRollQueryHandler(IDiscoveryQueries discoveryQueries, ILogger<DiceRollQueryHandler> logger)
    {
        _discoveryQueries = discoveryQueries;
        _logger = logger;
    }

    public async Task<DiceRollResultDto> Handle(DiceRollQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing dice roll for user {UserId} with scope {Scope}", request.UserId, request.Scope);

        var diceToRoll = request.DiceTypes ?? new List<DiceType> { DiceType.Quality, DiceType.Rarity, DiceType.Risk, DiceType.Complexity, DiceType.Exploration };

        var rollResults = diceToRoll.Select(RollDie).ToList();
        var constraints = BuildConstraints(rollResults);

        var customSourceIds = await ResolveCustomSourceIds(request, cancellationToken);
        var pool = (await _discoveryQueries.GetDiscoveryPoolAsync(request.UserId, request.Scope, customSourceIds, cancellationToken)).ToList();

        if (!pool.Any())
        {
            throw new InvalidOperationException("No movies are available for dice roll in the chosen discovery pool.");
        }

        var filteredPool = FilterPool(pool, constraints);
        var selected = filteredPool.Any() ? filteredPool[Random.Shared.Next(filteredPool.Count)] : pool[Random.Shared.Next(pool.Count)];
        var specialEvent = DetectSpecialEvent(rollResults);

        return new DiceRollResultDto(
            selected.MovieId,
            selected.Title,
            selected.DirectorName ?? "Unknown",
            selected.ReleaseYear ?? 0,
            selected.PosterUrl,
            selected.RuntimeMinutes,
            filteredPool.Any() ? $"Dice-selected from {filteredPool.Count} matching movies." : "Fell back to full pool — no exact dice match found.",
            rollResults,
            specialEvent);
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

    private static SingleDieResultDto RollDie(DiceType diceType)
    {
        var max = MaxValues[diceType];
        var roll = Random.Shared.Next(1, max + 1);

        var (label, description) = diceType switch
        {
            DiceType.Quality => roll switch
            {
                <= 2 => ("Low", "An entertaining but not particularly notable film."),
                <= 4 => ("Medium", "A solid, well-crafted film."),
                <= 6 => ("High", "A very good film with strong craftsmanship."),
                _ => ("Critical", "A potential masterpiece of cinema.")
            },
            DiceType.Rarity => roll switch
            {
                1 => ("Popular", "A widely-known mainstream film."),
                2 => ("Known", "A film most cinephiles recognize."),
                3 => ("Lesser-known", "A film that flew under most radars."),
                4 => ("Hidden gem", "A true hidden gem awaiting discovery."),
                _ => ("Extreme discovery", "An obscurity even dedicated cinephiles may have missed.")
            },
            DiceType.Risk => roll switch
            {
                1 => ("Safe", "Comfortably within your established taste."),
                2 => ("Slight shift", "A gentle nudge outside your usual zone."),
                3 => ("Moderate", "A noticeable departure from your norms."),
                4 => ("Risky", "Far outside your usual cinematic territory."),
                _ => ("Total chaos", "Complete unpredictability — anything could happen.")
            },
            DiceType.Complexity => roll switch
            {
                <= 2 => ("Easy", "A light, accessible watch — no preparation needed."),
                <= 4 => ("Conventional", "Standard narrative structure, easy to follow."),
                <= 6 => ("Deep", "Requires attention and some cinematic literacy."),
                7 => ("Complex", "Dense narrative, challenging themes, rewarding."),
                _ => ("Challenging", "An endurance test for the dedicated viewer.")
            },
            DiceType.Exploration => roll switch
            {
                1 => ("Habitual", "Familiar cinematic territory."),
                2 => ("International", "A window into another culture."),
                3 => ("Uncommon", "From a lesser-seen film industry."),
                4 => ("Global cinema", "Geographically and culturally distant."),
                _ => ("Extreme discovery", "From a rarely-explored cinematic tradition.")
            },
            _ => ("Unknown", "An indeterminate roll.")
        };

        return new SingleDieResultDto(diceType, roll, label, description);
    }

    private static DiceConstraints BuildConstraints(IReadOnlyList<SingleDieResultDto> rolls)
    {
        double? minRating = null, maxRating = null;
        int? minRuntime = null, maxRuntime = null;
        double? minPopularity = null, maxPopularity = null;

        foreach (var roll in rolls)
        {
            switch (roll.DiceType)
            {
                case DiceType.Quality:
                    if (roll.RollValue <= 2) { minRating = null; maxRating = 6.0; }
                    else if (roll.RollValue <= 4) { minRating = 6.0; maxRating = 7.5; }
                    else if (roll.RollValue <= 6) { minRating = 7.5; maxRating = 8.5; }
                    else { minRating = 8.5; maxRating = null; }
                    break;

                case DiceType.Rarity:
                    switch (roll.RollValue)
                    {
                        case 1: minPopularity = 80.0; maxPopularity = null; break;
                        case 2: minPopularity = 60.0; maxPopularity = 80.0; break;
                        case 3: minPopularity = 40.0; maxPopularity = 60.0; break;
                        case 4: minPopularity = 20.0; maxPopularity = 40.0; break;
                        default: minPopularity = null; maxPopularity = 20.0; break;
                    }
                    break;

                case DiceType.Complexity:
                    if (roll.RollValue <= 2) { minRuntime = null; maxRuntime = 90; }
                    else if (roll.RollValue <= 4) { minRuntime = 90; maxRuntime = 120; }
                    else if (roll.RollValue <= 6) { minRuntime = 120; maxRuntime = 150; }
                    else if (roll.RollValue == 7) { minRuntime = 150; maxRuntime = 180; }
                    else { minRuntime = 180; maxRuntime = null; }
                    break;
            }
        }

        return new DiceConstraints(minRating, maxRating, minRuntime, maxRuntime, minPopularity, maxPopularity);
    }

    private static List<DiscoveryMoviePoolItemDto> FilterPool(List<DiscoveryMoviePoolItemDto> pool, DiceConstraints constraints)
    {
        return pool.Where(m => MatchesConstraints(m, constraints)).ToList();
    }

    private static bool MatchesConstraints(DiscoveryMoviePoolItemDto movie, DiceConstraints constraints)
    {
        if (constraints.MinRating.HasValue && (movie.TmdbRating ?? 0) < constraints.MinRating.Value) return false;
        if (constraints.MaxRating.HasValue && (movie.TmdbRating ?? 10) > constraints.MaxRating.Value) return false;
        if (constraints.MinPopularity.HasValue && (movie.TmdbPopularity ?? 0) < constraints.MinPopularity.Value) return false;
        if (constraints.MaxPopularity.HasValue && (movie.TmdbPopularity ?? 100) > constraints.MaxPopularity.Value) return false;
        if (constraints.MinRuntime.HasValue && (movie.RuntimeMinutes ?? 0) < constraints.MinRuntime.Value) return false;
        if (constraints.MaxRuntime.HasValue && (movie.RuntimeMinutes ?? 1000) > constraints.MaxRuntime.Value) return false;
        return true;
    }

    private static string? DetectSpecialEvent(IReadOnlyList<SingleDieResultDto> rolls)
    {
        var qualityRoll = rolls.FirstOrDefault(r => r.DiceType == DiceType.Quality);
        var riskRoll = rolls.FirstOrDefault(r => r.DiceType == DiceType.Risk);

        if (qualityRoll?.RollValue >= 7)
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
        double? MinPopularity, double? MaxPopularity);
}
