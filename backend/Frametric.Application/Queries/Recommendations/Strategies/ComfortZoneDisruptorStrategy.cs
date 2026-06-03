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
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Domain.Enums;

namespace Frametric.Application.Queries.Recommendations.Strategies;

public class ComfortZoneDisruptorStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.ComfortZoneDisruptor;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        int total = watched.Count;
        if (total == 0)
        {
            var random = new Random();
            return candidates.OrderBy(c => random.Next())
                .Take(quantity)
                .Select(c => new RecommendedMovieDto(
                    c.MovieId,
                    c.Title,
                    c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                    c.ReleaseYear ?? 0,
                    50.0 + CalculateTieBreaker(c),
                    "A completely random pick to let chance guide your night.",
                    c.PosterUrl,
                    c.RuntimeMinutes,
                    c.CustomAverageRating
                ))
                .ToList();
        }

        // Define comfort zones
        var comfortGenres = watched.SelectMany(r => r.Genres?.Split(',') ?? Array.Empty<string>())
            .Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g))
            .GroupBy(g => g)
            .Where(g => (double)g.Count() / total > 0.25)
            .Select(g => g.Key).ToList();

        var comfortEras = watched.Where(w => w.ReleaseYear.HasValue)
            .Select(w => (w.ReleaseYear!.Value / 10) * 10)
            .GroupBy(d => d)
            .Where(d => (double)d.Count() / total > 0.30)
            .Select(d => d.Key).ToList();

        var highlyRatedDirectors = watched.Where(w => w.UserRating.HasValue && w.UserRating.Value >= 7.5)
            .SelectMany(w => w.Directors?.Split(',') ?? Array.Empty<string>())
            .Select(d => d.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var highlyRatedActors = watched.Where(w => w.UserRating.HasValue && w.UserRating.Value >= 7.5)
            .SelectMany(w => w.Actors?.Split(',') ?? Array.Empty<string>())
            .Select(a => a.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Comfort zone novelty (genres + eras)
            var cGenres = (c.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => g.Trim()).ToList();
            int cDecade = c.ReleaseYear.HasValue ? (c.ReleaseYear.Value / 10) * 10 : 0;

            bool isGenreComfort = cGenres.Any(cg => comfortGenres.Contains(cg, StringComparer.OrdinalIgnoreCase));
            bool isEraComfort = comfortEras.Contains(cDecade);

            if (!isGenreComfort)
            {
                score += 35.0;
                reasons.Add("ventures into genres you rarely watch");
            }
            if (!isEraComfort)
            {
                score += 15.0;
                reasons.Add($"introduces an era outside your typical focus ({cDecade}s)");
            }

            // Familiarity anchors (highly rated creator connection)
            var cDirs = (c.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => d.Trim()).ToList();
            var cActs = (c.Actors?.Split(',') ?? Array.Empty<string>()).Select(a => a.Trim()).ToList();
            var cWriters = (c.Writers?.Split(',') ?? Array.Empty<string>()).Select(w => w.Trim()).ToList();

            bool hasFamiliarDirector = cDirs.Any(d => highlyRatedDirectors.Contains(d));
            bool hasFamiliarActor = cActs.Any(a => highlyRatedActors.Contains(a));
            bool hasFamiliarWriter = cWriters.Any(w => highlyRatedDirectors.Contains(w) || highlyRatedActors.Contains(w));

            if (hasFamiliarDirector)
            {
                score += 30.0;
                reasons.Add("anchored by a director you have rated highly in the past");
            }
            else if (hasFamiliarActor)
            {
                score += 25.0;
                reasons.Add("features leading cast members you highly enjoy");
            }
            else if (hasFamiliarWriter)
            {
                score += 15.0;
                reasons.Add("written by a creator whose previous work you rated highly");
            }

            // Global prestige and review density
            double aggRating = GetAggregatedRating(c);
            score += (aggRating / 10.0) * 15.0;

            if (c.TmdbPopularity.HasValue)
            {
                score += Math.Min(5.0, c.TmdbPopularity.Value * 0.015);
            }

            // Language/Country diversity bonus
            if (!string.IsNullOrEmpty(c.Language) && !c.Language.Contains("English", StringComparison.OrdinalIgnoreCase))
            {
                score += 5.0;
                reasons.Add("offers a rich international perspective");
            }

            double tieBreaker = CalculateTieBreaker(c);
            double finalScore = Math.Min(99.9, Math.Max(10.0, score)) + tieBreaker;
            double match = Math.Round(finalScore, 4);

            string reason = reasons.Any() ? $"Disrupts your comfort zone: it {FormatReasons(reasons)}." : "Pushes your boundaries with a solid critical consensus.";

            return new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                match,
                reason,
                c.PosterUrl,
                c.RuntimeMinutes,
                c.CustomAverageRating
            );
        }).OrderByDescending(r => r.MatchPercentage).Take(quantity).ToList();
    }
}
