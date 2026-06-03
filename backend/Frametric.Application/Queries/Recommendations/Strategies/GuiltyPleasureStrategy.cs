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

public class GuiltyPleasureStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.GuiltyPleasure;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        double userAvgRating = watched.Any(w => w.UserRating.HasValue) ? watched.Where(w => w.UserRating.HasValue).Average(w => w.UserRating!.Value) : 6.0;

        // Niche genres where user rates higher than overall average
        var genreStats = watched.SelectMany(w => (w.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => new { Genre = g.Trim(), Rating = w.UserRating }))
            .Where(x => x.Rating.HasValue)
            .GroupBy(x => x.Genre)
            .Select(g => new { Genre = g.Key, Avg = g.Average(x => x.Rating!.Value) })
            .Where(x => x.Avg > userAvgRating)
            .Select(x => x.Genre).ToList();

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Audience-Critic discrepancy
            double criticRatingSum = 0;
            int criticRatingCount = 0;
            if (c.MetacriticRating.HasValue)
            {
                criticRatingSum += c.MetacriticRating.Value;
                criticRatingCount++;
            }
            if (c.RottenTomatoesRating.HasValue)
            {
                criticRatingSum += c.RottenTomatoesRating.Value;
                criticRatingCount++;
            }

            double audienceRating = c.TmdbRating ?? c.CustomAverageRating ?? 6.0;

            if (criticRatingCount > 0)
            {
                double avgCriticRating = criticRatingSum / criticRatingCount;
                double discrepancy = (audienceRating * 10.0) - avgCriticRating;
                if (discrepancy > 8.0)
                {
                    score += Math.Min(35.0, discrepancy * 2.0);
                    reasons.Add("loved by audiences despite being heavily criticized by reviews");
                }
            }
            else
            {
                score += 15.0;
            }

            // Genre matches user highly rated genres
            var cGenres = (c.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => g.Trim()).ToList();
            if (cGenres.Any(g => genreStats.Contains(g, StringComparer.OrdinalIgnoreCase)))
            {
                score += 30.0;
                reasons.Add("aligns with niche genres you historically rate highly");
            }

            // Popularity penalty/bonus
            double pop = c.TmdbPopularity ?? 30.0;
            if (pop > 10.0 && pop < 75.0)
            {
                score += 15.0;
                reasons.Add("sits in that perfect sweet-spot of underrated cult popularity");
            }

            // Certifications
            if (!string.IsNullOrEmpty(c.Certification) && 
                (c.Certification.Contains("R") || c.Certification.Contains("PG-13") || c.Certification.Contains("16") || c.Certification.Contains("18")))
            {
                score += 10.0;
            }

            // Awards penalty
            var (wins, _, _, _) = ParseAwards(c.Awards);
            if (wins == 0)
            {
                score += 5.0;
            }

            double tieBreaker = CalculateTieBreaker(c);
            double finalScore = Math.Min(99.9, Math.Max(10.0, score)) + tieBreaker;
            double match = Math.Round(finalScore, 4);

            string reason = reasons.Any() ? $"A guilty pleasure pick: it {FormatReasons(reasons)}." : "Fun, crowd-pleasing option matching your historical preferences.";

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
