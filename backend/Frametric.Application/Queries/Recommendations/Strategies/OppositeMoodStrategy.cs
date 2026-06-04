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

public class OppositeMoodStrategy : RecommendationStrategyBase
{
    public override RecommendationStrategy Strategy => RecommendationStrategy.OppositeMood;

    public override List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        int quantity,
        int? maxRuntime = null)
    {
        var recent = watched.OrderByDescending(w => w.WatchDate).Take(15).ToList();
        var latestWatchDate = recent.FirstOrDefault()?.WatchDate ?? DateTime.UtcNow;

        var recentGenres = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var recentKeywords = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        double avgRuntime = 100.0;

        foreach (var r in recent)
        {
            double ratingWeight = r.UserRating.HasValue ? Math.Max(0.1, r.UserRating.Value / 10.0) : 0.7;
            double decay = GetTemporalDecayWeight(r.WatchDate, latestWatchDate, 45.0);
            double weight = ratingWeight * decay;

            if (r.RuntimeMinutes.HasValue && r.RuntimeMinutes.Value > 0)
            {
                avgRuntime += r.RuntimeMinutes.Value;
            }

            var genresList = r.Genres?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var g in genresList)
            {
                var trimmed = g.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    recentGenres[trimmed] = recentGenres.GetValueOrDefault(trimmed) + weight;
                }
            }

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

        if (recent.Any()) avgRuntime /= recent.Count;

        // Determine main recent genre mood category
        double intensityScore = recentGenres.GetValueOrDefault("Action") + recentGenres.GetValueOrDefault("Thriller") + recentGenres.GetValueOrDefault("Horror") + recentGenres.GetValueOrDefault("Sci-Fi") + recentGenres.GetValueOrDefault("Adventure");
        double lightheartedScore = recentGenres.GetValueOrDefault("Comedy") + recentGenres.GetValueOrDefault("Family") + recentGenres.GetValueOrDefault("Animation") + recentGenres.GetValueOrDefault("Fantasy");
        double reflectiveScore = recentGenres.GetValueOrDefault("Drama") + recentGenres.GetValueOrDefault("Romance") + recentGenres.GetValueOrDefault("Documentary") + recentGenres.GetValueOrDefault("History") + recentGenres.GetValueOrDefault("Mystery");

        return candidates.Select(c =>
        {
            double score = 0;

            // 1. Genre Inversion
            var cGenres = (c.Genres?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                .Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g)).ToList();
            var cGenreVector = cGenres.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(g => g, g => 1.0, StringComparer.OrdinalIgnoreCase);
            double genreSim = ComputeCosineSimilarity(recentGenres, cGenreVector);
            
            score += (1.0 - genreSim) * 35.0;

            // Polar mood shift
            double cIntensity = cGenres.Count(g => g == "Action" || g == "Thriller" || g == "Horror" || g == "Sci-Fi" || g == "Adventure");
            double cLighthearted = cGenres.Count(g => g == "Comedy" || g == "Family" || g == "Animation" || g == "Fantasy");
            double cReflective = cGenres.Count(g => g == "Drama" || g == "Romance" || g == "Documentary" || g == "History" || g == "Mystery");

            int moodShiftType = 0; // 0 = none, 1 = high intensity to reflective/light, 2 = comedy to thrill/drama, 3 = drama to action/comedy
            if (intensityScore > lightheartedScore && intensityScore > reflectiveScore)
            {
                if (cReflective > 0 || cLighthearted > 0)
                {
                    score += 15.0;
                    moodShiftType = 1;
                }
            }
            else if (lightheartedScore > intensityScore && lightheartedScore > reflectiveScore)
            {
                if (cIntensity > 0 || cReflective > 0)
                {
                    score += 15.0;
                    moodShiftType = 2;
                }
            }
            else if (reflectiveScore > intensityScore && reflectiveScore > lightheartedScore)
            {
                if (cIntensity > 0 || cLighthearted > 0)
                {
                    score += 15.0;
                    moodShiftType = 3;
                }
            }

            // 2. Keyword/Theme Inversion
            var cKws = (c.Keywords?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                .Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();
            var cKwVector = cKws.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(k => k, k => 1.0, StringComparer.OrdinalIgnoreCase);
            double kwSim = ComputeCosineSimilarity(recentKeywords, cKwVector);
            score += (1.0 - kwSim) * 20.0;

            // 3. Pacing Inversion
            int pacingShift = 0; // 0 = none, 1 = fast, 2 = slow
            if (c.RuntimeMinutes.HasValue)
            {
                if (avgRuntime > 115 && c.RuntimeMinutes < 95)
                {
                    score += 15.0;
                    pacingShift = 1;
                }
                else if (avgRuntime < 95 && c.RuntimeMinutes > 120)
                {
                    score += 15.0;
                    pacingShift = 2;
                }
            }

            // Global rating & awards support
            double aggRating = GetAggregatedRating(c);
            score += (aggRating / 10.0) * 10.0;

            var (wins, noms, _, _) = ParseAwards(c.Awards);
            score += Math.Min(3.0, wins * 0.4 + noms * 0.1);

            double tieBreaker = CalculateTieBreaker(c);
            double finalScore = Math.Min(99.9, Math.Max(10.0, score)) + tieBreaker;
            double match = Math.Round(finalScore, 0);

            string reason = GenerateReason(genreSim, kwSim, moodShiftType, pacingShift);

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

    private string GenerateReason(double genreSim, double kwSim, int moodShiftType, int pacingShift)
    {
        var reasons = new List<string>();

        // Genre Sim
        if (genreSim < 0.15)
        {
            if (genreSim < 0.05)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "offers an absolute genre shift" 
                    : "completely breaks from your recent genre patterns");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "provides a refreshing divergence in genre" 
                    : "leads you into less familiar genre territories");
            }
        }

        // Mood Shift
        if (moodShiftType == 1)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "switches from high-intensity action to a lighter or more reflective tone" 
                : "cools down the adrenaline with a calm or lighthearted atmosphere");
        }
        else if (moodShiftType == 2)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "moves away from comedy to something more thrilling or dramatic" 
                : "pivots from easy laughs to suspenseful or dramatic storytelling");
        }
        else if (moodShiftType == 3)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "balances deep drama with a high-tempo or lighthearted alternative" 
                : "breaks up heavy drama with a faster pace or comedic energy");
        }

        // Keyword Similarity
        if (kwSim < 0.05)
        {
            if (kwSim < 0.01)
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "explores completely fresh, unfamiliar plot themes" 
                    : "ventures into entirely unique thematic grounds");
            }
            else
            {
                reasons.Add(Random.Shared.Next(2) == 0 
                    ? "introduces different narrative concepts" 
                    : "dives into novel story threads");
            }
        }

        // Pacing Shift
        if (pacingShift == 1)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "provides a fast-paced, shorter story" 
                : "keeps things brief and energetic");
        }
        else if (pacingShift == 2)
        {
            reasons.Add(Random.Shared.Next(2) == 0 
                ? "invites you to settle in for an epic, slow-burn narrative" 
                : "gives you space to breathe with a patient, cinematic experience");
        }

        if (reasons.Any())
        {
            var prefixes = new[]
            {
                "A great palette cleanser because it",
                "A refreshing change of pace as it",
                "Breaks your current pattern since it",
                "Cleanses your cinematic palate because it",
                "Acts as a solid reset button as it"
            };
            var chosenPrefix = prefixes[Random.Shared.Next(prefixes.Length)];
            return $"{chosenPrefix} {FormatReasons(reasons)}.";
        }
        
        var defaultMessages = new[]
        {
            "A refreshing change of pace from your recent watches.",
            "A delightful departure from your regular viewing routine.",
            "A perfect detour to shake up your dashboard pattern."
        };
        return defaultMessages[Random.Shared.Next(defaultMessages.Length)];
    }
}
