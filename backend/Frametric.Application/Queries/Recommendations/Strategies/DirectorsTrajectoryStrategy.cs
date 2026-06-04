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

public class DirectorsTrajectoryStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.DirectorsTrajectory;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        var directorRatings = watched
            .Where(w => !string.IsNullOrEmpty(w.Directors))
            .SelectMany(w => (w.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => new { Director = d.Trim(), Rating = w.UserRating }))
            .GroupBy(x => x.Director)
            .Select(g => new { Director = g.Key, Avg = g.Average(x => x.Rating ?? 7.0), Count = g.Count() })
            .Where(x => x.Avg >= 7.2)
            .OrderByDescending(x => x.Count)
            .ThenByDescending(x => x.Avg)
            .ToList();

        var targetDirectorIds = directorRatings.Select(dr => dr.Director).ToList();

        return candidates
            .Where(c => !string.IsNullOrEmpty(c.Directors) && c.Directors.Split(',').Select(d => d.Trim()).Any(d => targetDirectorIds.Contains(d)))
            .Select(c =>
            {
                var dir = c.Directors?.Split(',').Select(d => d.Trim()).FirstOrDefault(d => targetDirectorIds.Contains(d)) ?? "Unknown";
                var stats = directorRatings.First(dr => dr.Director.Equals(dir, StringComparison.OrdinalIgnoreCase));

                double score = 0;
                score += stats.Avg * 8.0;

                var directorWatches = watched.Where(w => w.Directors != null && w.Directors.Contains(dir, StringComparison.OrdinalIgnoreCase) && w.ReleaseYear.HasValue).ToList();
                int? lastWatchedYear = directorWatches.OrderByDescending(w => w.WatchDate).FirstOrDefault()?.ReleaseYear;

                if (c.ReleaseYear.HasValue && lastWatchedYear.HasValue)
                {
                    if (c.ReleaseYear.Value > lastWatchedYear.Value)
                    {
                        score += 15.0;
                    }
                    else
                    {
                        score += 5.0;
                    }
                }

                double globalRating = GetAggregatedRating(c);
                score += globalRating;

                if (!string.IsNullOrEmpty(c.Writers) && c.Writers.Contains(dir, StringComparison.OrdinalIgnoreCase))
                {
                    score += 5.0;
                }

                double tieBreaker = CalculateTieBreaker(c);
                double finalScore = Math.Min(99.9, Math.Max(10.0, score)) + tieBreaker;
                double match = Math.Round(finalScore, 0);

                string reason = GenerateReason(dir, lastWatchedYear, c.ReleaseYear);

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
            })
            .OrderByDescending(r => r.MatchPercentage)
            .Take(quantity)
            .ToList();
    }

    private string GenerateReason(string director, int? lastWatchedYear, int? candidateYear)
    {
        if (lastWatchedYear.HasValue && candidateYear.HasValue && candidateYear.Value > lastWatchedYear.Value)
        {
            return Random.Shared.Next(2) == 0
                ? $"Continues your exploration of {director}'s filmography chronologically (moving forward to {candidateYear})."
                : $"Advances your journey through {director}'s work by moving forward to their {candidateYear} release.";
        }
        else
        {
            return Random.Shared.Next(2) == 0
                ? $"Fills a gap in your journey through the filmography of {director}."
                : $"Uncovers an essential missing chapter in your experience of {director}'s cinematic history.";
        }
    }
}
