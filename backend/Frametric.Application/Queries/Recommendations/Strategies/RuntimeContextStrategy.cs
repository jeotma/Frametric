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
            double diff = 0;
            bool hasRuntime = false;

            if (c.RuntimeMinutes.HasValue)
            {
                hasRuntime = true;
                diff = Math.Abs(c.RuntimeMinutes.Value - targetRuntime);
                double runtimeMatchScore = Math.Max(0.0, 1.0 - (diff / targetRuntime));
                score += runtimeMatchScore * 40.0;
            }

            var cGenres = (c.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => g.Trim()).ToList();
            bool isShort = targetRuntime <= 95.0;
            bool hasPacingMatch = false;

            if (isShort)
            {
                if (cGenres.Any(g => g == "Comedy" || g == "Action" || g == "Thriller" || g == "Horror"))
                {
                    score += 25.0;
                    hasPacingMatch = true;
                }
            }
            else
            {
                if (cGenres.Any(g => g == "Drama" || g == "Sci-Fi" || g == "History" || g == "Biography"))
                {
                    score += 25.0;
                    hasPacingMatch = true;
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
            double match = Math.Round(finalScore, 0);

            string reason = GenerateReason(hasRuntime, diff, isShort, hasPacingMatch);

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

    private string GenerateReason(bool hasRuntime, double diff, bool isShort, bool hasPacingMatch)
    {
        var reasons = new List<string>();

        if (hasRuntime)
        {
            if (diff <= 10)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "matches your timing availability perfectly" 
                    : "fits your schedule without a minute to spare");
            }
            else if (diff <= 20)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "fits comfortably in your slot" 
                    : "aligns well with your available screen time");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "is slightly off but still manageable for your slot" 
                    : "approximates your target watch window decently");
            }
        }

        if (hasPacingMatch)
        {
            if (isShort)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "offers a high-intensity, quick-moving narrative" 
                    : "delivers quick entertainment suited for shorter viewings");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "leverages a larger duration for a deep story experience" 
                    : "utilizes its epic length to craft a rich, fleshed-out plot");
            }
        }

        if (reasons.Any())
        {
            var prefixes = new[]
            {
                "A duration choice that",
                "A runtime option that",
                "This selection",
                "A film that",
                "A pick that"
            };
            var chosenPrefix = prefixes[Random.Shared.Next(prefixes.Length)];
            return $"{chosenPrefix} {FormatReasons(reasons)}.";
        }
        
        var defaultMessages = new[]
        {
            "Great runtime match for your availability.",
            "Comes in at a suitable length for your session.",
            "Fits snugly into your target watch window."
        };
        return defaultMessages[Random.Shared.Next(defaultMessages.Length)];
    }
}
