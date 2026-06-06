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

public class PureRandomStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.PureRandom;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        var random = new Random();
        return candidates.OrderBy(c => random.Next())
            .Take(quantity)
            .Select(c =>
            {
                double tieBreaker = CalculateTieBreaker(c);
                double finalScore = 50.0 + tieBreaker;
                double match = Math.Round(finalScore, 0);
                string reason = GenerateReason();
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
            .ToList();
    }

    private string GenerateReason()
    {
        var choices = new[]
        {
            "A completely random pick to let chance guide your night.",
            "A wild card selection to shake up your viewing routine.",
            "Rolled the dice and landed on this surprise choice.",
            "A spontaneous recommendation powered by pure chaos."
        };
        return choices[Random.Shared.Next(choices.Length)];
    }
}
