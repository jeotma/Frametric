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

public class OutOfCharacterStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.OutOfCharacter;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        UserViewingProfile profile,
        int quantity,
        int? maxRuntime = null)
    {
        return candidates
            .Select(c =>
            {
                double profileMatch = CalculateProfileMatchScore(c, profile);
                double inverseMatch = 100.0 - profileMatch;

                // Calculate Prestige / Rating Score (0-100)
                double metacritic = (c.MetacriticRating ?? 5.0) * 10.0;
                double rt = (c.RottenTomatoesRating ?? 5.0) * 10.0;
                double imdb = (c.ImdbRating ?? 5.0) * 10.0;
                double tmdb = (c.TmdbRating ?? 5.0) * 10.0;
                double customRating = (c.CustomAverageRating ?? c.TmdbRating ?? 5.0) * 10.0;

                double prestigeScore = (metacritic * 0.25) + (rt * 0.25) + (imdb * 0.15) + (tmdb * 0.10) + (customRating * 0.25);
                
                // OutOfCharacterScore = (100.0 - ProfileMatchScore) * 0.7 + (Prestige/RatingScore) * 0.3
                double rawScore = (inverseMatch * 0.7) + (prestigeScore * 0.3);

                double tieBreaker = CalculateTieBreaker(c);
                double finalScore = Math.Min(99.9, Math.Max(10.0, rawScore)) + tieBreaker;
                double matchPercentage = Math.Round(finalScore, 0);

                var reasons = new List<string>();
                if (inverseMatch > 85.0)
                {
                    reasons.Add("shares almost nothing with your typical favorites");
                }
                else if (inverseMatch > 70.0)
                {
                    reasons.Add("defies your usual genre and style preferences");
                }
                else
                {
                    reasons.Add("offers a notable departure from your usual tastes");
                }

                if (prestigeScore > 80.0)
                {
                    reasons.Add("stands as a highly acclaimed piece of cinema");
                }
                else if (prestigeScore > 70.0)
                {
                    reasons.Add("maintains solid critical respect despite being unconventional for you");
                }
                
                string reason = reasons.Any() 
                    ? $"This film {FormatReasons(reasons)}." 
                    : "A high-quality departure from your usual cinematic habits.";

                return new RecommendedMovieDto(
                    c.MovieId,
                    c.Title,
                    c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                    c.ReleaseYear ?? 0,
                    matchPercentage,
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
}
