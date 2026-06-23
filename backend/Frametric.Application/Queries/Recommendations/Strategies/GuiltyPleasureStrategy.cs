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
        UserViewingProfile profile,
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

        // Favorite directors (avg user rating >= userAvgRating or count >= 2)
        var directorStats = watched.Where(w => !string.IsNullOrEmpty(w.Directors))
            .SelectMany(w => (w.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => new { Director = d.Trim(), Rating = w.UserRating }))
            .GroupBy(x => x.Director)
            .Select(g => new { Director = g.Key, Avg = g.Average(x => x.Rating ?? 6.0), Count = g.Count() })
            .Where(x => x.Avg >= userAvgRating || x.Count >= 2)
            .ToDictionary(x => x.Director, x => x.Avg, StringComparer.OrdinalIgnoreCase);

        // Favorite actors (avg user rating >= userAvgRating or count >= 2)
        var actorStats = watched.Where(w => !string.IsNullOrEmpty(w.Actors))
            .SelectMany(w => (w.Actors?.Split(',') ?? Array.Empty<string>()).Select(a => new { Actor = a.Trim(), Rating = w.UserRating }))
            .GroupBy(x => x.Actor)
            .Select(g => new { Actor = g.Key, Avg = g.Average(x => x.Rating ?? 6.0), Count = g.Count() })
            .Where(x => x.Avg >= userAvgRating || x.Count >= 2)
            .ToDictionary(x => x.Actor, x => x.Avg, StringComparer.OrdinalIgnoreCase);

        // Favorite keywords (keywords from movies user rated highly)
        var userKeywords = watched.Where(w => !string.IsNullOrEmpty(w.Keywords) && (!w.UserRating.HasValue || w.UserRating.Value >= userAvgRating))
            .SelectMany(w => (w.Keywords?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()).Select(k => k.Trim()))
            .GroupBy(k => k)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        return candidates.Select(c =>
        {
            double score = 15.0;
            var reasons = new List<string>();

            double profileMatch = CalculateProfileMatchScore(c, profile);

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

            double discrepancy = 0;
            if (criticRatingCount > 0)
            {
                double avgCriticRating = (criticRatingSum / criticRatingCount) * 10.0;
                discrepancy = (audienceRating * 10.0) - avgCriticRating;
                if (discrepancy > 8.0)
                {
                    score += Math.Min(8.0, discrepancy * 0.4);
                }
            }
            else
            {
                score += 4.0;
            }

            // Genre matches user highly rated genres
            var cGenres = (c.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => g.Trim()).ToList();
            bool isGenreMatch = cGenres.Any(g => genreStats.Contains(g, StringComparer.OrdinalIgnoreCase));
            if (isGenreMatch)
            {
                score += 8.0;
            }

            // Keyword match (major bonus)
            var cKws = (c.Keywords?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                .Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            
            double keywordBonus = 0.0;
            foreach (var kw in cKws)
            {
                if (userKeywords.TryGetValue(kw, out int count))
                {
                    keywordBonus += 10.0 * Math.Min(3, count);
                }
            }
            double keywordBonusVal = 0.0;
            if (keywordBonus > 0)
            {
                keywordBonusVal = Math.Min(12.0, keywordBonus);
                score += keywordBonusVal;
            }

            // Familiar director/actor bonus
            var cDirs = (c.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => d.Trim()).ToList();
            var cActs = (c.Actors?.Split(',') ?? Array.Empty<string>()).Select(a => a.Trim()).ToList();

            double creatorBonus = 0.0;
            foreach (var d in cDirs)
            {
                if (directorStats.ContainsKey(d))
                {
                    creatorBonus += 10.0;
                }
            }
            foreach (var a in cActs)
            {
                if (actorStats.ContainsKey(a))
                {
                    creatorBonus += 8.0;
                }
            }
            double creatorBonusVal = 0.0;
            if (creatorBonus > 0)
            {
                creatorBonusVal = Math.Min(8.0, creatorBonus);
                score += creatorBonusVal;
            }

            // Writer connection to user's favorite creators
            var cWriters = (c.Writers?.Split(',') ?? Array.Empty<string>()).Select(w => w.Trim()).ToList();
            double writerBonus = 0.0;
            foreach (var w in cWriters)
            {
                if (directorStats.ContainsKey(w) || actorStats.ContainsKey(w))
                {
                    writerBonus += 8.0;
                }
            }
            if (writerBonus > 0)
            {
                score += Math.Min(4.0, writerBonus);
            }

            // Popularity scoring (guilty pleasures should be niche/cult, not blockbusters)
            double pop = c.TmdbPopularity ?? 30.0;
            bool isInPopularitySweetSpot = false;
            if (pop > 10.0 && pop <= 50.0)
            {
                score += 4.0;
                isInPopularitySweetSpot = true;
            }
            else if (pop > 50.0)
            {
                double popPenalty = Math.Min(40.0, (pop - 50.0) * 0.3);
                score -= popPenalty;
            }

            // Certifications
            if (!string.IsNullOrEmpty(c.Certification) && 
                (c.Certification.Contains("R") || c.Certification.Contains("PG-13") || c.Certification.Contains("16") || c.Certification.Contains("18")))
            {
                score += 2.0;
            }

            // Gradual awards penalty & no-awards bonus
            var (wins, noms, otherWins, otherNoms) = ParseAwards(c.Awards);
            double totalAwardsWeight = (wins * 5.0) + (noms * 2.0) + (otherWins * 0.5) + (otherNoms * 0.2);
            bool isNoAwards = false;
            if (totalAwardsWeight == 0)
            {
                score += 5.0;
                isNoAwards = true;
            }
            else
            {
                double awardsPenalty = Math.Min(35.0, totalAwardsWeight * 2.0);
                score -= awardsPenalty;
            }

            // Gradual average score rating curves
            double avgRating = GetAggregatedRating(c);
            if (avgRating < 6.5)
            {
                double ratingBonus = Math.Min(10.0, (6.5 - avgRating) * 8.0);
                score += ratingBonus;
            }
            else if (avgRating > 6.5)
            {
                double ratingPenalty = Math.Min(50.0, (avgRating - 6.5) * 35.0);
                score -= ratingPenalty;
            }

            double tieBreaker = CalculateTieBreaker(c);
            double blendedScore = (score * 0.5) + (profileMatch * 0.5);
            double finalScore = Math.Min(99.9, Math.Max(10.0, blendedScore)) + tieBreaker;
            double match = Math.Round(finalScore, 0);

            string reason = GenerateReason(keywordBonusVal, creatorBonusVal, isInPopularitySweetSpot, isNoAwards, discrepancy, isGenreMatch);

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

    private string GenerateReason(double keywordBonusVal, double creatorBonusVal, bool isInPopularitySweetSpot, bool isNoAwards, double discrepancy, bool isGenreMatch)
    {
        var reasons = new List<string>();

        if (discrepancy > 8.0)
        {
            if (discrepancy > 15.0)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "loved by audiences despite being heavily criticized by reviews" 
                    : "stands as a massive audience favorite despite critical rejection");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "has a noticeable positive audience-critic split" 
                    : "resonates much better with viewers than mainstream critics");
            }
        }

        if (isGenreMatch)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "aligns with niche genres you historically rate highly" 
                : "belongs to specific genre categories you have favored in the past");
        }

        if (keywordBonusVal > 0)
        {
            if (keywordBonusVal <= 6.0)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "touches on a few familiar themes you enjoy" 
                    : "shares some subtle tropes with your favorite movies");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "delves deep into niche themes and tropes you enjoy" 
                    : "completely aligns with your favorite narrative keywords");
            }
        }

        if (creatorBonusVal > 0)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "features familiar creators from your niche favorites" 
                : "brings in directors or actors you have a soft spot for");
        }

        if (isInPopularitySweetSpot)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "sits in that perfect sweet-spot of underrated cult popularity" 
                : "enjoys a dedicated cult following without being mainstream");
        }

        if (isNoAwards)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "fled the radar of critical award bodies" 
                : "remains completely free from prestigious award bias");
        }

        if (reasons.Any())
        {
            var prefixes = new[]
            {
                "A guilty pleasure pick because it",
                "A fun, comfort choice since it",
                "A crowd-pleasing option as it",
                "A pure entertainment pick because it",
                "An easy-watch candidate since it"
            };
            var chosenPrefix = prefixes[Random.Shared.Next(prefixes.Length)];
            return $"{chosenPrefix} {FormatReasons(reasons)}.";
        }
        
        var defaultMessages = new[]
        {
            "Fun, crowd-pleasing option matching your historical preferences.",
            "An easy, entertaining watch to enjoy without pressure.",
            "A pure entertainment choice to relax with."
        };
        return defaultMessages[Random.Shared.Next(defaultMessages.Length)];
    }
}
