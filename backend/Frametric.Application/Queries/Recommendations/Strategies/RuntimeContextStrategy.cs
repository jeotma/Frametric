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

public class RuntimeContextStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.RuntimeContext;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        double targetRuntime = maxRuntime ?? 95.0;

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            if (c.RuntimeMinutes.HasValue)
            {
                double diff = Math.Abs(c.RuntimeMinutes.Value - targetRuntime);
                double runtimeMatchScore = Math.Max(0.0, 1.0 - (diff / targetRuntime));
                score += runtimeMatchScore * 40.0;

                if (diff <= 10) reasons.Add("matches your timing availability perfectly");
                else if (diff <= 20) reasons.Add("fits comfortably in your slot");
            }

            var cGenres = (c.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => g.Trim()).ToList();
            bool isShort = targetRuntime <= 95.0;

            if (isShort)
            {
                if (cGenres.Any(g => g == "Comedy" || g == "Action" || g == "Thriller" || g == "Horror"))
                {
                    score += 25.0;
                    reasons.Add("offers a high-intensity, quick-moving narrative");
                }
            }
            else
            {
                if (cGenres.Any(g => g == "Drama" || g == "Sci-Fi" || g == "History" || g == "Biography"))
                {
                    score += 25.0;
                    reasons.Add("leverages a larger duration for a deep story experience");
                }
            }

            double ratingValue = GetAggregatedRating(c);
            score += (ratingValue / 10.0) * 25.0;

            if (c.TmdbPopularity.HasValue)
            {
                score += Math.Min(10.0, c.TmdbPopularity.Value * 0.02);
            }

            double tieBreaker = CalculateTieBreaker(c);
            double finalScore = Math.Min(99.9, Math.Max(10.0, score)) + tieBreaker;
            double match = Math.Round(finalScore, 4);

            string reason = reasons.Any() ? $"Optimized runtime selection: it {FormatReasons(reasons)}." : "Great runtime match for your availability.";

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
