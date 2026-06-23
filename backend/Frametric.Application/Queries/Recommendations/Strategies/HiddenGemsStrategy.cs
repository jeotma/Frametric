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

public class HiddenGemsStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.HiddenGems;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        UserViewingProfile profile,
        int quantity,
        int? maxRuntime = null)
    {
        return candidates
            .Where(c => 
            {
                var ratings = new List<double>();
                if (c.CustomAverageRating.HasValue) ratings.Add(c.CustomAverageRating.Value);
                if (c.TmdbRating.HasValue) ratings.Add(c.TmdbRating.Value);
                if (c.ImdbRating.HasValue) ratings.Add(c.ImdbRating.Value);
                if (c.RottenTomatoesRating.HasValue) ratings.Add(c.RottenTomatoesRating.Value);
                if (c.MetacriticRating.HasValue) ratings.Add(c.MetacriticRating.Value);
                double rawAvg = ratings.Any() ? ratings.Average() : 6.0;
                return rawAvg >= 8.0;
            })
            .Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            double profileMatch = CalculateProfileMatchScore(c, profile);

            // Prestige rating components
            double metacritic = (c.MetacriticRating ?? 5.0) * 10.0;
            double rt = (c.RottenTomatoesRating ?? 5.0) * 10.0;
            double imdb = (c.ImdbRating ?? 5.0) * 10.0;
            double tmdb = (c.TmdbRating ?? 5.0) * 10.0;
            double prestigeRating = (metacritic * 0.35) + (rt * 0.35) + (imdb * 0.20) + (tmdb * 0.10);
            score += (prestigeRating / 100.0) * 22.0;

            double obscureBonus = 0;
            if (c.TmdbPopularity.HasValue)
            {
                double pop = c.TmdbPopularity.Value;
                double popBaseDiff = 35.0 - pop;
                if (popBaseDiff > 0)
                {
                    obscureBonus = Math.Min(12.0, popBaseDiff * 0.5);
                    score += obscureBonus;
                }
                else
                {
                    double mainstreamPenalty = Math.Min(35.0, Math.Abs(popBaseDiff) * 0.5);
                    score -= mainstreamPenalty;
                }
            }

            // Ratio of custom rating vs popularity
            double customRating = c.CustomAverageRating ?? c.TmdbRating ?? 6.0;
            double popularity = Math.Max(1.0, c.TmdbPopularity ?? 30.0);
            double ratingToPopularityRatio = customRating / popularity;
            
            // Higher scaling and cap to reward contrast between rating and popularity
            double ratioBonus = Math.Min(30.0, ratingToPopularityRatio * 25.0);
            score += ratioBonus;

            // Extra contrast boost for high rating (> 8.0) and low popularity (< 10.0)
            if (customRating > 8.0 && popularity < 10.0)
            {
                double highContrastBonus = (customRating - 7.0) * (10.0 - popularity) * 1.5;
                score += Math.Min(20.0, highContrastBonus);
            }

            // Awards parse
            var (wins, noms, otherWins, otherNoms) = ParseAwards(c.Awards);
            double awardsScore = (wins * 4.0) + (noms * 1.5) + (otherWins * 0.3) + (otherNoms * 0.1);
            if (awardsScore > 0)
            {
                score += Math.Min(8.0, awardsScore * 0.3);
            }

            // Country / Foreign status
            if (!string.IsNullOrEmpty(c.Country))
            {
                if (!c.Country.Contains("USA", StringComparison.OrdinalIgnoreCase) && !c.Country.Contains("United States", StringComparison.OrdinalIgnoreCase))
                {
                    double nonUsaBonus = 8.0;
                    if (!c.Country.Contains("UK", StringComparison.OrdinalIgnoreCase) && !c.Country.Contains("United Kingdom", StringComparison.OrdinalIgnoreCase))
                    {
                        nonUsaBonus = 12.0;
                    }
                    score += nonUsaBonus;
                }
            }

            // Box office returns scoring
            double? boxOffice = ParseBoxOffice(c.BoxOffice);
            if (boxOffice.HasValue)
            {
                if (boxOffice.Value < 5000000.0)
                {
                    double lowBoxOfficeBonus = Math.Min(5.0, (5000000.0 - boxOffice.Value) / 1000000.0);
                    score += lowBoxOfficeBonus;
                }
                else if (boxOffice.Value > 25000000.0)
                {
                    double highBoxOfficePenalty = Math.Min(12.0, (boxOffice.Value - 25000000.0) / 5000000.0);
                    score -= highBoxOfficePenalty;
                }
            }

            // Duration
            if (c.RuntimeMinutes.HasValue && c.RuntimeMinutes.Value >= 130)
            {
                score += 3.0;
            }

            // Overview keyword match
            if (!string.IsNullOrEmpty(c.Overview))
            {
                string overviewLower = c.Overview.ToLowerInvariant();
                if (overviewLower.Contains("existential") || overviewLower.Contains("philosoph") || 
                    overviewLower.Contains("surreal") || overviewLower.Contains("satire") || 
                    overviewLower.Contains("melancholy") || overviewLower.Contains("poetic"))
                {
                    score += 3.0;
                }
            }

            double tieBreaker = CalculateTieBreaker(c);
            // Blend existing strategy score with global profile match
            double blendedScore = (score * 0.5) + (profileMatch * 0.5);
            double finalScore = Math.Min(99.9, Math.Max(10.0, blendedScore)) + tieBreaker;
            double match = Math.Round(finalScore, 0);

            string reason = GenerateReason(
                obscureBonus,
                ratioBonus,
                prestigeRating,
                wins,
                noms,
                c.Country,
                boxOffice,
                c.RuntimeMinutes);

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

    private string GenerateReason(
        double obscureBonus,
        double ratioBonus,
        double prestigeRating,
        int wins,
        int noms,
        string? country,
        double? boxOffice,
        int? runtimeMinutes)
    {
        var reasons = new List<string>();

        // Country
        if (!string.IsNullOrEmpty(country) && 
            !country.Contains("USA", StringComparison.OrdinalIgnoreCase) && 
            !country.Contains("United States", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "belongs to the rich tradition of international art-house cinema" 
                : "expands horizons with its foreign cinematic language");
        }

        // Obscurity
        if (obscureBonus > 0)
        {
            if (obscureBonus <= 4.0)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "is a relatively fresh alternative to mainstream cinema" 
                    : "is not completely mainstream but remains a bit hidden");
            }
            else if (obscureBonus <= 8.0)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "is a highly underrated choice" 
                    : "remains an overlooked gem");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "remains a hidden masterpiece waiting to be uncovered" 
                    : "is a deep-cut masterpiece for true cinephiles");
            }
        }

        // Ratio
        if (ratioBonus > 0)
        {
            if (ratioBonus <= 5.0)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "shows positive reception relative to its audience size" 
                    : "is appreciated by the few who have seen it");
            }
            else if (ratioBonus <= 10.0)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "has very high ratings compared to its small audience" 
                    : "boasts a surprisingly strong reception for its size");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "has exceptionally high ratings relative to its small audience" 
                    : "is an absolute critical darling with a tiny footprint");
            }
        }

        // Prestige
        if (prestigeRating >= 80.0)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "is widely considered a cinematic masterpiece by critics" 
                : "boasts near-universal acclaim from cinematic experts");
        }

        // Awards
        if (wins > 0)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? $"secured {wins} prestigious award wins" 
                : $"was celebrated with {wins} major awards");
        }
        else if (noms > 0)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? $"received {noms} major award nominations" 
                : $"was honored with {noms} nominations by leading bodies");
        }

        // Box Office
        if (boxOffice.HasValue && boxOffice.Value < 5000000.0)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "was produced outside the Hollywood blockbusters circle" 
                : "proves that cinematic art doesn't require blockbuster budgets");
        }

        // Runtime
        if (runtimeMinutes.HasValue && runtimeMinutes.Value >= 130)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "features an immersive runtime typical of grand cinematic visions" 
                : "offers a sweeping, patient narrative length");
        }

        if (reasons.Any())
        {
            var prefixes = new[]
            {
                "This curated selection",
                "This recommended pick",
                "This film",
                "This masterpiece",
                "This choice"
            };
            var chosenPrefix = prefixes[Random.Shared.Next(prefixes.Length)];
            return $"{chosenPrefix} {FormatReasons(reasons)}.";
        }
        
        var defaultMessages = new[]
        {
            "Highly-acclaimed, award-winning cinematic classic.",
            "A curated masterpiece for the refined viewer.",
            "Prestige cinema selection with near-universal critical praise."
        };
        return defaultMessages[Random.Shared.Next(defaultMessages.Length)];
    }

    private static double? ParseBoxOffice(string? boxOfficeStr)
    {
        if (string.IsNullOrEmpty(boxOfficeStr)) return null;
        var clean = new string(boxOfficeStr.Where(char.IsDigit).ToArray());
        if (double.TryParse(clean, out double value))
        {
            return value;
        }
        return null;
    }
}
