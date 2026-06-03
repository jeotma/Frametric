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

public class RecentMoodStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.RecentMood;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        var recent = watched.OrderByDescending(w => w.WatchDate).Take(15).ToList();
        var latestWatchDate = recent.FirstOrDefault()?.WatchDate ?? DateTime.UtcNow;
        
        var recentGenres = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var recentDecades = new Dictionary<int, double>();
        var recentDirectors = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var recentActors = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var recentKeywords = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in recent)
        {
            double ratingWeight = r.UserRating.HasValue ? Math.Max(0.1, r.UserRating.Value / 10.0) : 0.7;
            double decay = GetTemporalDecayWeight(r.WatchDate, latestWatchDate, 45.0);
            double weight = ratingWeight * decay;

            // Genres
            var genresList = r.Genres?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var g in genresList)
            {
                var trimmed = g.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    recentGenres[trimmed] = recentGenres.GetValueOrDefault(trimmed) + weight;
                }
            }

            // Decades
            if (r.ReleaseYear.HasValue)
            {
                int decade = (r.ReleaseYear.Value / 10) * 10;
                recentDecades[decade] = recentDecades.GetValueOrDefault(decade) + weight;
            }

            // Directors
            var dirList = r.Directors?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var d in dirList)
            {
                var trimmed = d.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    recentDirectors[trimmed] = recentDirectors.GetValueOrDefault(trimmed) + weight;
                }
            }

            // Actors
            var actList = r.Actors?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var a in actList)
            {
                var trimmed = a.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    recentActors[trimmed] = recentActors.GetValueOrDefault(trimmed) + weight;
                }
            }

            // Keywords (from highly rated or recent watches)
            var kwList = r.Keywords?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var kw in kwList)
            {
                var trimmed = kw.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    recentKeywords[trimmed] = recentKeywords.GetValueOrDefault(trimmed) + weight;
                }
            }
        }

        // Weighted average runtime
        double avgRuntime = 100.0;
        double runtimeWeightSum = 0;
        double runtimeSum = 0;
        foreach (var r in recent)
        {
            if (r.RuntimeMinutes.HasValue && r.RuntimeMinutes.Value > 0)
            {
                double ratingWeight = r.UserRating.HasValue ? Math.Max(0.1, r.UserRating.Value / 10.0) : 0.7;
                double decay = GetTemporalDecayWeight(r.WatchDate, latestWatchDate, 45.0);
                double weight = ratingWeight * decay;
                runtimeSum += r.RuntimeMinutes.Value * weight;
                runtimeWeightSum += weight;
            }
        }
        if (runtimeWeightSum > 0)
        {
            avgRuntime = runtimeSum / runtimeWeightSum;
        }

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Genre Cosine Similarity
            var cGenres = (c.Genres?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                .Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g)).ToList();
            var cGenreVector = cGenres.ToDictionary(g => g, g => 1.0, StringComparer.OrdinalIgnoreCase);
            double genreSim = ComputeCosineSimilarity(recentGenres, cGenreVector);
            if (genreSim > 0)
            {
                score += genreSim * 35.0;
                reasons.Add("perfectly matches your recent genre trends");
            }

            // Keyword Cosine Similarity
            var cKws = (c.Keywords?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                .Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();
            var cKwVector = cKws.ToDictionary(k => k, k => 1.0, StringComparer.OrdinalIgnoreCase);
            double kwSim = ComputeCosineSimilarity(recentKeywords, cKwVector);
            if (kwSim > 0)
            {
                score += kwSim * 25.0;
                reasons.Add("shares strong thematic elements with your recent favorites");
            }

            // Runtime alignment
            if (c.RuntimeMinutes.HasValue)
            {
                double runtimeDiff = Math.Abs(c.RuntimeMinutes.Value - avgRuntime);
                if (runtimeDiff <= 15)
                {
                    score += 15.0;
                    reasons.Add("fits your recent pacing preference");
                }
                else if (runtimeDiff <= 30)
                {
                    score += 7.5;
                }
            }

            // Decade alignment
            if (c.ReleaseYear.HasValue)
            {
                int cDecade = (c.ReleaseYear.Value / 10) * 10;
                if (recentDecades.TryGetValue(cDecade, out double weight))
                {
                    score += Math.Min(10.0, weight * 3.0);
                    reasons.Add($"aligns with your interest in {cDecade}s cinema");
                }
            }

            // Director & Actor overlap
            var cDirs = (c.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => d.Trim()).ToList();
            double dirOverlap = cDirs.Sum(d => recentDirectors.GetValueOrDefault(d));
            if (dirOverlap > 0)
            {
                score += Math.Min(10.0, dirOverlap * 4.0);
                reasons.Add("directed by a creator you've watched recently");
            }

            var cActs = (c.Actors?.Split(',') ?? Array.Empty<string>()).Select(a => a.Trim()).ToList();
            double actOverlap = cActs.Sum(a => recentActors.GetValueOrDefault(a));
            if (actOverlap > 0)
            {
                score += Math.Min(8.0, actOverlap * 2.0);
                reasons.Add("stars actors from your recent viewing history");
            }

            // Awards bonus
            var (wins, noms, otherWins, otherNoms) = ParseAwards(c.Awards);
            double awardsBonus = Math.Min(5.0, (wins * 0.5) + (noms * 0.2) + (otherWins * 0.05));
            score += awardsBonus;

            // Global rating alignment
            double aggRating = GetAggregatedRating(c);
            score += (aggRating / 10.0) * 10.0;

            // Popularity / box office contextual refinement
            if (c.TmdbPopularity.HasValue)
            {
                score += Math.Min(2.0, c.TmdbPopularity.Value * 0.005);
            }

            double tieBreaker = CalculateTieBreaker(c);
            double finalScore = Math.Min(99.9, Math.Max(10.0, score)) + tieBreaker;
            double match = Math.Round(finalScore, 4);
            
            string reason = reasons.Any() ? $"Aligns with your recent mood because it {FormatReasons(reasons)}." : "Complements your recent viewing pattern.";

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
