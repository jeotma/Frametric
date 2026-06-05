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

    private class DirectorStat
    {
        public string Director { get; set; } = string.Empty;
        public double Avg { get; set; }
        public int Count { get; set; }
    }

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        if (candidates == null || !candidates.Any())
        {
            return new List<RecommendedMovieDto>();
        }

        if (watched == null || !watched.Any())
        {
            return GetGlobalFallback(candidates, quantity);
        }

        // Tier 1: Highly rated directors
        var tier1Stats = watched
            .Where(w => !string.IsNullOrEmpty(w.Directors))
            .SelectMany(w => (w.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => new { Director = d.Trim(), Rating = w.UserRating }))
            .GroupBy(x => x.Director)
            .Select(g => new DirectorStat { Director = g.Key, Avg = g.Average(x => x.Rating ?? 7.5), Count = g.Count() })
            .Where(x => x.Avg >= 6.0)
            .OrderByDescending(x => x.Count)
            .ThenByDescending(x => x.Avg)
            .ToList();

        var recommendations = GetRecommendationsForDirectors(candidates, watched, tier1Stats, quantity);
        if (recommendations.Any())
        {
            return recommendations;
        }

        // Tier 2: Any director in history (relaxed filter)
        var tier2Stats = watched
            .Where(w => !string.IsNullOrEmpty(w.Directors))
            .SelectMany(w => (w.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => new { Director = d.Trim(), Rating = w.UserRating }))
            .GroupBy(x => x.Director)
            .Select(g => new DirectorStat { Director = g.Key, Avg = g.Average(x => x.Rating ?? 7.0), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        recommendations = GetRecommendationsForDirectors(candidates, watched, tier2Stats, quantity);
        if (recommendations.Any())
        {
            return recommendations;
        }

        // Tier 3: Global fallback
        return GetGlobalFallback(candidates, quantity);
    }

    private List<RecommendedMovieDto> GetRecommendationsForDirectors(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        List<DirectorStat> stats,
        int quantity)
    {
        var targetDirectorNames = stats.Select(s => s.Director).ToList();

        return candidates
            .Where(c => !string.IsNullOrEmpty(c.Directors) && c.Directors.Split(',').Select(d => d.Trim()).Any(d => targetDirectorNames.Contains(d, StringComparer.OrdinalIgnoreCase)))
            .Select(c =>
            {
                var dir = c.Directors?.Split(',').Select(d => d.Trim()).FirstOrDefault(d => targetDirectorNames.Contains(d, StringComparer.OrdinalIgnoreCase)) ?? "Unknown";
                var directorStat = stats.First(s => s.Director.Equals(dir, StringComparison.OrdinalIgnoreCase));

                double score = 0;
                score += directorStat.Avg * 8.0;

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

    private List<RecommendedMovieDto> GetGlobalFallback(List<CandidateMovieDto> candidates, int quantity)
    {
        return candidates
            .OrderByDescending(c => c.CustomAverageRating ?? c.TmdbRating ?? 0.0)
            .Take(quantity)
            .Select(c => new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                70.0 + CalculateTieBreaker(c),
                "Highly rated film to expand your cinematic catalog.",
                c.PosterUrl,
                c.RuntimeMinutes,
                c.CustomAverageRating
            ))
            .ToList();
    }

    private string GenerateReason(string director, int? lastWatchedYear, int? candidateYear)
    {
        if (lastWatchedYear.HasValue && candidateYear.HasValue && candidateYear.Value > lastWatchedYear.Value)
        {
            var options = new[]
            {
                $"Continues your exploration of {director}'s filmography chronologically (moving forward to {candidateYear}).",
                $"Advances your journey through {director}'s work by moving forward to their {candidateYear} release.",
                $"Builds on your {director} watches by advancing to their {candidateYear} period.",
                $"Keeps the director run going, stepping forward to their {candidateYear} movie."
            };
            return options[Random.Shared.Next(options.Length)];
        }
        else
        {
            var options = new[]
            {
                $"Fills a gap in your journey through the filmography of {director}.",
                $"Uncovers an essential missing chapter in your experience of {director}'s cinematic history.",
                $"Fleshes out your understanding of {director} by filling in this catalog gap.",
                $"Helps you explore a less seen period of {director}'s career."
            };
            return options[Random.Shared.Next(options.Length)];
        }
    }
}
