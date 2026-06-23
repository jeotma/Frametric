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

public class BlastFromThePastStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.BlastFromThePast;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        UserViewingProfile profile,
        int quantity,
        int? maxRuntime = null)
    {
        return candidates
            .Where(c => c.ReleaseYear.HasValue && c.ReleaseYear.Value < 1990)
            .Select(c =>
            {
                double score = 0;
                var reasons = new List<string>();

                double profileMatch = CalculateProfileMatchScore(c, profile);

                // Year age bonus (older film = more classic score bonus)
                int ageBonus = 1990 - c.ReleaseYear!.Value;
                score += Math.Min(15.0, ageBonus * 0.4);

                // Good rating weighting
                double ratingValue = GetAggregatedRating(c);
                score += (ratingValue / 10.0) * 35.0;

                // Popularity penalty/bonus depending on classic status
                if (c.TmdbPopularity.HasValue)
                {
                    // Prefer cult or established classics over hyper-active modern popular reruns
                    if (c.TmdbPopularity.Value > 25.0)
                    {
                        score += 5.0; // Standard classic
                    }
                    else
                    {
                        score += 10.0; // Cult status
                    }
                }

                // Award check bonus
                var (wins, noms, _, _) = ParseAwards(c.Awards);
                if (wins > 0 || noms > 0)
                {
                    score += 15.0;
                }

                double tieBreaker = CalculateTieBreaker(c);
                double blendedScore = (score * 0.5) + (profileMatch * 0.5);
                double finalScore = Math.Min(99.9, Math.Max(10.0, blendedScore)) + tieBreaker;
                double match = Math.Round(finalScore, 0);

                string reason = GenerateReason(c.ReleaseYear.Value, wins > 0, c.TmdbPopularity);

                return new RecommendedMovieDto(
                    c.MovieId,
                    c.Title,
                    c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                    c.ReleaseYear.Value,
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

    private string GenerateReason(int releaseYear, bool hasAwards, double? popularity)
    {
        var reasons = new List<string>();

        reasons.Add($"takes you back to the golden era of cinema in {releaseYear}");

        if (hasAwards)
        {
            reasons.Add("stands as an award-recognized cinematic achievement of its era");
        }

        if (popularity.HasValue && popularity.Value < 25.0)
        {
            reasons.Add("retains a dedicated cult reputation among classic film lovers");
        }

        if (reasons.Any())
        {
            var prefixes = new[]
            {
                "This vintage choice",
                "This classic pick",
                "This throwback film",
                "This historical selection"
            };
            var chosenPrefix = prefixes[Random.Shared.Next(prefixes.Length)];
            return $"{chosenPrefix} {FormatReasons(reasons)}.";
        }

        return $"A highly-rated classic release from {releaseYear}.";
    }
}
