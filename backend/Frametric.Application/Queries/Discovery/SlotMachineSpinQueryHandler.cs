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

    private static readonly IReadOnlyList<string> DurationLabels = new[] { "< 60 min", "60-90 min", "90-120 min", "120-150 min", "> 150 min" };

    public SlotMachineSpinQueryHandler(IDiscoveryQueries discoveryQueries, ILogger<SlotMachineSpinQueryHandler> logger)
    {
        _discoveryQueries = discoveryQueries;
        _logger = logger;
    }

    public async Task<SlotMachineResultDto> Handle(SlotMachineSpinQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing slot machine spin for user {UserId} with scope {Scope}", request.UserId, request.Scope);

        var customSourceIds = await ResolveCustomSourceIds(request, cancellationToken);
        var pool = (await _discoveryQueries.GetDiscoveryPoolAsync(request.UserId, request.Scope, customSourceIds, cancellationToken)).ToList();

        if (!pool.Any())
        {
            throw new InvalidOperationException("No movies are available for slot machine in the chosen discovery pool.");
        }

        var genre = ResolveGenre(request.Genre, pool);
        var decade = ResolveDecade(request.Decade, pool);
        var director = ResolveDirector(request.Director, pool);
        var (durationLabel, durationMin, durationMax) = ResolveDuration(request.Duration, pool);
        var country = ResolveCountry(request.Country, pool);

        var filteredPool = FilterPool(pool, genre, decade, director, durationMin, durationMax, country);
        var selected = filteredPool.Any() ? filteredPool[Random.Shared.Next(filteredPool.Count)] : pool[Random.Shared.Next(pool.Count)];

        var isJackpot = CheckJackpot(genre, decade, director, durationLabel, country, selected);

        var reelResults = new List<SlotReelResultDto>
        {
            new("Genre", genre ?? "Any"),
            new("Decade", decade.HasValue ? $"{decade.Value}s" : "Any"),
            new("Director", director ?? "Any"),
            new("Duration", durationLabel ?? "Any"),
            new("Country", country ?? "Any"),
        };

        return new SlotMachineResultDto(
            selected.MovieId,
            selected.Title,
            selected.DirectorName ?? "Unknown",
            selected.ReleaseYear ?? 0,
            selected.PosterUrl,
            selected.RuntimeMinutes,
            filteredPool.Any()
                ? $"Slot machine matched {filteredPool.Count} film(s) with the spun combination."
                : "Fell back to full pool — no exact combination match found.",
            reelResults,
            isJackpot);
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

    private static string? ResolveGenre(string? genre, IReadOnlyList<DiscoveryMoviePoolItemDto> pool)
    {
        if (!string.IsNullOrWhiteSpace(genre)) return genre;

        var allGenres = pool
            .SelectMany(m => (m.Genres ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return allGenres.Any() ? allGenres[Random.Shared.Next(allGenres.Count)] : null;
    }

    private static int? ResolveDecade(int? decade, IReadOnlyList<DiscoveryMoviePoolItemDto> pool)
    {
        if (decade.HasValue) return decade;

        var allDecades = pool
            .Select(m => m.ReleaseYear)
            .Where(y => y.HasValue)
            .Select(y => (y.Value / 10) * 10)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        return allDecades.Any() ? allDecades[Random.Shared.Next(allDecades.Count)] : null;
    }

    private static string? ResolveDirector(string? director, IReadOnlyList<DiscoveryMoviePoolItemDto> pool)
    {
        if (!string.IsNullOrWhiteSpace(director)) return director;

        var allDirectors = pool
            .Select(m => m.DirectorName)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .SelectMany(d => d!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return allDirectors.Any() ? allDirectors[Random.Shared.Next(allDirectors.Count)] : null;
    }

    private static (string? Label, int? Min, int? Max) ResolveDuration(string? duration, IReadOnlyList<DiscoveryMoviePoolItemDto> pool)
    {
        if (!string.IsNullOrWhiteSpace(duration))
        {
            var (min, max) = duration switch
            {
                "< 60 min" => ((int?)null, (int?)60),
                "60-90 min" => ((int?)60, (int?)90),
                "90-120 min" => ((int?)90, (int?)120),
                "120-150 min" => ((int?)120, (int?)150),
                "> 150 min" => ((int?)150, (int?)null),
                _ => ((int?)null, (int?)null)
            };
            return (duration, min, max);
        }

        var allRuntimes = pool
            .Select(m => m.RuntimeMinutes)
            .Where(r => r.HasValue)
            .Select(r => r.Value)
            .ToList();

        if (!allRuntimes.Any()) return (null, null, null);

        var labelIndex = Random.Shared.Next(DurationLabels.Count);
        var chosenLabel = DurationLabels[labelIndex];

        var (cmin, cmax) = labelIndex switch
        {
            0 => ((int?)null, (int?)60),
            1 => ((int?)60, (int?)90),
            2 => ((int?)90, (int?)120),
            3 => ((int?)120, (int?)150),
            4 => ((int?)150, (int?)null),
            _ => ((int?)null, (int?)null)
        };

        return (chosenLabel, cmin, cmax);
    }

    private static string? ResolveCountry(string? country, IReadOnlyList<DiscoveryMoviePoolItemDto> pool)
    {
        if (!string.IsNullOrWhiteSpace(country)) return country;

        var allCountries = pool
            .Select(m => m.Country)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return allCountries.Any() ? allCountries[Random.Shared.Next(allCountries.Count)] : null;
    }

    private static List<DiscoveryMoviePoolItemDto> FilterPool(
        List<DiscoveryMoviePoolItemDto> pool,
        string? genre, int? decade,
        string? director,
        int? durationMin, int? durationMax,
        string? country)
    {
        return pool.Where(m =>
        {
            if (!string.IsNullOrWhiteSpace(genre))
            {
                var movieGenres = (m.Genres ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!movieGenres.Contains(genre, StringComparer.OrdinalIgnoreCase))
                    return false;
            }

            if (decade.HasValue && m.ReleaseYear.HasValue)
            {
                var movieDecade = (m.ReleaseYear.Value / 10) * 10;
                if (movieDecade != decade.Value)
                    return false;
            }
            else if (decade.HasValue)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(director))
            {
                var movieDirectors = (m.DirectorName ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!movieDirectors.Contains(director, StringComparer.OrdinalIgnoreCase))
                    return false;
            }

            if (durationMin.HasValue || durationMax.HasValue)
            {
                var runtime = m.RuntimeMinutes;
                if (!runtime.HasValue) return false;
                if (durationMin.HasValue && runtime.Value < durationMin.Value) return false;
                if (durationMax.HasValue && runtime.Value >= durationMax.Value) return false;
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                if (!string.Equals(m.Country, country, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }).ToList();
    }

    private static bool CheckJackpot(string? genre, int? decade, string? director, string? duration, string? country, DiscoveryMoviePoolItemDto selected)
    {
        var allSpecified = !string.IsNullOrWhiteSpace(genre)
            && decade.HasValue
            && !string.IsNullOrWhiteSpace(director)
            && !string.IsNullOrWhiteSpace(duration)
            && !string.IsNullOrWhiteSpace(country);

        if (!allSpecified) return false;

        return (selected.TmdbRating ?? 0) >= 8.5 && (selected.TmdbPopularity ?? 0) > 50;
    }
}
