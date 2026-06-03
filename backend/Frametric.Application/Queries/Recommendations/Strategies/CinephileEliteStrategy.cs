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

public class CinephileEliteStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.CinephileElite;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Prestige rating components
            double metacritic = c.MetacriticRating ?? 50.0;
            double rt = c.RottenTomatoesRating ?? 50.0;
            double imdb = (c.ImdbRating ?? 5.0) * 10.0;
            double tmdb = (c.TmdbRating ?? 5.0) * 10.0;

            double prestigeRating = (metacritic * 0.35) + (rt * 0.35) + (imdb * 0.20) + (tmdb * 0.10);
            score += (prestigeRating / 100.0) * 45.0;

            if (prestigeRating >= 80.0)
            {
                reasons.Add("is widely considered a cinematic masterpiece by critics");
            }

            // Awards parse
            var (wins, noms, otherWins, otherNoms) = ParseAwards(c.Awards);
            double awardsScore = (wins * 4.0) + (noms * 1.5) + (otherWins * 0.3) + (otherNoms * 0.1);
            if (awardsScore > 0)
            {
                score += Math.Min(30.0, awardsScore);
                string awardReason = wins > 0 ? $"secured {wins} prestigious award wins" : $"received {noms} major award nominations";
                reasons.Add(awardReason);
            }

            // Country / Foreign status
            if (!string.IsNullOrEmpty(c.Country) && !c.Country.Contains("USA") && !c.Country.Contains("United States"))
            {
                score += 15.0;
                reasons.Add("belongs to the rich tradition of international art-house cinema");
            }

            // Duration
            if (c.RuntimeMinutes.HasValue && c.RuntimeMinutes.Value >= 130)
            {
                score += 5.0;
                reasons.Add("features an immersive runtime typical of grand cinematic visions");
            }

            // Overview keyword match
            if (!string.IsNullOrEmpty(c.Overview))
            {
                string overviewLower = c.Overview.ToLowerInvariant();
                if (overviewLower.Contains("existential") || overviewLower.Contains("philosoph") || 
                    overviewLower.Contains("surreal") || overviewLower.Contains("satire") || 
                    overviewLower.Contains("melancholy") || overviewLower.Contains("poetic"))
                {
                    score += 5.0;
                }
            }

            double tieBreaker = CalculateTieBreaker(c);
            double finalScore = Math.Min(99.9, Math.Max(10.0, score)) + tieBreaker;
            double match = Math.Round(finalScore, 4);

            string reason = reasons.Any() ? $"Cinephile Elite choice: it {FormatReasons(reasons)}." : "Highly-acclaimed, award-winning cinematic classic.";

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
