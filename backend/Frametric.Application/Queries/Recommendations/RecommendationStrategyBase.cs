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

namespace Frametric.Application.Queries.Recommendations;

public abstract class RecommendationStrategyBase : IRecommendationStrategy
{
    public abstract RecommendationStrategy Strategy { get; }

    public abstract List<RecommendedMovieDto> Recommend(
        List<CandidateMovieDto> candidates,
        List<WatchedMovieDetailDto> watched,
        UserViewingProfile profile,
        int quantity,
        int? maxRuntime = null);

    protected static string FormatReasons(List<string> reasons)
    {
        if (reasons == null || reasons.Count == 0) return string.Empty;
        
        // Hard cap at 2 reason snippets to prevent run-on sentences
        var capped = reasons.Take(2).ToList();
        if (capped.Count == 1) return capped[0];
        
        return $"{capped[0]} and {capped[1]}";
    }

    protected static double ComputeJaccardSimilarity(IEnumerable<string>? setA, IEnumerable<string>? setB)
    {
        if (setA == null || setB == null) return 0.0;
        var listA = setA.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var listB = setB.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (!listA.Any() || !listB.Any()) return 0.0;
        
        int intersectCount = listA.Intersect(listB, StringComparer.OrdinalIgnoreCase).Count();
        int unionCount = listA.Union(listB, StringComparer.OrdinalIgnoreCase).Count();
        return unionCount == 0 ? 0.0 : (double)intersectCount / unionCount;
    }

    protected static double ComputeCosineSimilarity(Dictionary<string, double> vectorA, Dictionary<string, double> vectorB)
    {
        if (vectorA == null || vectorB == null || !vectorA.Any() || !vectorB.Any()) return 0.0;
        
        double dotProduct = 0.0;
        foreach (var key in vectorA.Keys)
        {
            if (vectorB.TryGetValue(key, out double valB))
            {
                dotProduct += vectorA[key] * valB;
            }
        }
        
        double magnitudeA = Math.Sqrt(vectorA.Values.Sum(v => v * v));
        double magnitudeB = Math.Sqrt(vectorB.Values.Sum(v => v * v));
        
        if (magnitudeA == 0.0 || magnitudeB == 0.0) return 0.0;
        return dotProduct / (magnitudeA * magnitudeB);
    }

    protected static double GetTemporalDecayWeight(DateTime watchDate, DateTime latestWatchDate, double halfLifeDays = 45.0)
    {
        double daysDiff = Math.Max(0, (latestWatchDate - watchDate).TotalDays);
        double lambda = Math.Log(2.0) / halfLifeDays;
        return Math.Exp(-lambda * daysDiff);
    }

    protected static double GetAggregatedRating(CandidateMovieDto c)
    {
        var ratings = new List<double>();
        if (c.CustomAverageRating.HasValue) ratings.Add(c.CustomAverageRating.Value);
        if (c.TmdbRating.HasValue) ratings.Add(c.TmdbRating.Value);
        if (c.ImdbRating.HasValue) ratings.Add(c.ImdbRating.Value);
        if (c.RottenTomatoesRating.HasValue) ratings.Add(c.RottenTomatoesRating.Value);
        if (c.MetacriticRating.HasValue) ratings.Add(c.MetacriticRating.Value);

        if (!ratings.Any()) return 6.0;

        double sum = 0.0;
        double weightSum = 0.0;

        for (int i = 0; i < ratings.Count; i++)
        {
            double weight = 1.0;
            if (i == 0 && c.CustomAverageRating.HasValue) weight = 2.5; 
            sum += ratings[i] * weight;
            weightSum += weight;
        }

        double rawAvg = sum / weightSum;
        double v = ratings.Count;
        double m = 2.0;
        double priorMean = 6.5;
        return (v * rawAvg + m * priorMean) / (v + m);
    }

    protected static (int OscarWins, int OscarNoms, int OtherWins, int OtherNoms) ParseAwards(string? awards)
    {
        if (string.IsNullOrEmpty(awards)) return (0, 0, 0, 0);
        
        int oscarWins = 0, oscarNoms = 0, otherWins = 0, otherNoms = 0;
        string lower = awards.ToLowerInvariant();

        var words = lower.Split(new[] { ' ', '.', ',', '&', ';', '-' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i] == "oscar" || words[i] == "oscars")
            {
                if (i > 0 && int.TryParse(words[i - 1], out int num))
                {
                    if (lower.Contains("won") && lower.IndexOf("won", StringComparison.Ordinal) < lower.IndexOf("oscar", StringComparison.Ordinal))
                    {
                        oscarWins = num;
                    }
                    else
                    {
                        oscarNoms = num;
                    }
                }
            }
            else if (words[i] == "win" || words[i] == "wins" || words[i] == "won")
            {
                if (i > 0 && int.TryParse(words[i - 1], out int num))
                {
                    otherWins = num;
                }
            }
            else if (words[i] == "nomination" || words[i] == "nominations" || words[i] == "nominated")
            {
                if (i > 0 && int.TryParse(words[i - 1], out int num))
                {
                    otherNoms = num;
                }
            }
        }

        if (oscarWins == 0 && oscarNoms == 0)
        {
            if (lower.Contains("won") && lower.Contains("oscar")) oscarWins = 1;
            else if (lower.Contains("nominate") && lower.Contains("oscar")) oscarNoms = 1;
        }

        return (oscarWins, oscarNoms, otherWins, otherNoms);
    }

    protected static double CalculateTieBreaker(CandidateMovieDto c)
    {
        double popFactor = (c.TmdbPopularity ?? 0.0) * 0.000001;
        double ratingFactor = (c.CustomAverageRating ?? c.TmdbRating ?? 0.0) * 0.00001;
        double hashFactor = Math.Abs(c.MovieId.GetHashCode() % 10000) / 100000.0;
        
        return popFactor + ratingFactor + hashFactor;
    }

    protected UserViewingProfile BuildViewingProfile(List<WatchedMovieDetailDto> watched)
    {
        var profile = new UserViewingProfile();
        if (watched == null || !watched.Any()) return profile;

        profile.TotalWatches = watched.Count;
        
        var ratedWatches = watched.Where(w => w.UserRating.HasValue).ToList();
        profile.AverageUserRating = ratedWatches.Any() 
            ? ratedWatches.Average(w => w.UserRating!.Value) 
            : 7.0;

        var runtimeWatches = watched.Where(w => w.RuntimeMinutes.HasValue && w.RuntimeMinutes.Value > 0).ToList();
        profile.PreferredRuntime = runtimeWatches.Any() 
            ? runtimeWatches.Average(w => (double)w.RuntimeMinutes!.Value) 
            : 100.0;

        double totalOscarWins = 0;
        double totalOscarNoms = 0;
        double totalOtherWins = 0;
        double totalBoxOffice = 0;
        int prestigeCount = 0;
        int boxOfficeCount = 0;

        foreach (var w in watched)
        {
            double ratingMultiplier = w.UserRating.HasValue ? Math.Max(0.2, w.UserRating.Value / 5.0) : 0.8;
            double likedMultiplier = w.Liked ? 2.0 : 1.0;
            double watchlistMultiplier = w.IsWatchlisted ? 1.2 : 1.0;
            double customListMultiplier = w.IsInCustomList ? 1.5 : 1.0;
            double rewatchMultiplier = 1.0 + Math.Max(0, w.DiaryCount - 1);

            // Quality multiplier: progressive bonus for highly-rated watched films (no penalty below threshold).
            // Scale: 6.5 → 1.0x, 7.0 → ~1.1x, 8.0 → ~1.35x, 9.0 → ~1.7x, 10.0 → 2.0x (capped).
            double qualityMultiplier = 1.0;
            if (w.UserRating.HasValue && w.UserRating.Value > 6.5)
            {
                double excessAboveBase = w.UserRating.Value - 6.5; // range 0..3.5 (6.5..10)
                qualityMultiplier = 1.0 + (excessAboveBase / 3.5) * 1.0; // linearly up to 2.0x at 10
                qualityMultiplier = Math.Clamp(qualityMultiplier, 1.0, 2.0);
            }

            double weight = ratingMultiplier * likedMultiplier * watchlistMultiplier * customListMultiplier * rewatchMultiplier * qualityMultiplier;

            // Genres
            var genresList = w.Genres?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var g in genresList)
            {
                var trimmed = g.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    profile.Genres[trimmed] = profile.Genres.GetValueOrDefault(trimmed) + weight;
            }

            // Directors
            var dirList = w.Directors?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var d in dirList)
            {
                var trimmed = d.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    profile.Directors[trimmed] = profile.Directors.GetValueOrDefault(trimmed) + weight;
            }

            // Actors
            var actList = w.Actors?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var a in actList)
            {
                var trimmed = a.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    profile.Actors[trimmed] = profile.Actors.GetValueOrDefault(trimmed) + weight;
            }

            // Writers
            var writersList = w.Writers?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var wr in writersList)
            {
                var trimmed = wr.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    profile.Writers[trimmed] = profile.Writers.GetValueOrDefault(trimmed) + weight;
            }

            // Keywords
            var kwList = w.Keywords?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var kw in kwList)
            {
                var trimmed = kw.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    profile.Keywords[trimmed] = profile.Keywords.GetValueOrDefault(trimmed) + weight;
            }

            // Decades
            if (w.ReleaseYear.HasValue)
            {
                int decade = (w.ReleaseYear.Value / 10) * 10;
                profile.Decades[decade] = profile.Decades.GetValueOrDefault(decade) + weight;
            }

            // Languages & Countries
            var langList = w.Language?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var lang in langList)
            {
                var trimmed = lang.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    profile.Languages[trimmed] = profile.Languages.GetValueOrDefault(trimmed) + weight;
            }

            var countryList = w.Country?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var cnt in countryList)
            {
                var trimmed = cnt.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    profile.Countries[trimmed] = profile.Countries.GetValueOrDefault(trimmed) + weight;
            }

            // Prestige
            if (!string.IsNullOrEmpty(w.Awards))
            {
                var (wins, noms, otherWins, _) = ParseAwards(w.Awards);
                totalOscarWins += wins * weight;
                totalOscarNoms += noms * weight;
                totalOtherWins += otherWins * weight;
                prestigeCount++;
            }

            // Box office
            if (!string.IsNullOrEmpty(w.BoxOffice))
            {
                double boVal = ParseBoxOfficeValue(w.BoxOffice);
                if (boVal > 0)
                {
                    totalBoxOffice += boVal * weight;
                    boxOfficeCount++;
                }
            }
        }

        if (prestigeCount > 0)
        {
            profile.AverageOscarWins = totalOscarWins / watched.Count;
            profile.AverageOscarNoms = totalOscarNoms / watched.Count;
            profile.AverageOtherWins = totalOtherWins / watched.Count;
        }

        if (boxOfficeCount > 0)
        {
            profile.AverageBoxOffice = totalBoxOffice / watched.Count;
        }

        return profile;
    }

    protected double CalculateProfileMatchScore(CandidateMovieDto c, UserViewingProfile profile)
    {
        if (profile.TotalWatches == 0) return 50.0;

        double score = 0.0;

        // 1. Genres Match (Max 35 points)
        var cGenres = (c.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => g.Trim()).ToList();
        double genreOverlap = cGenres.Sum(g => profile.Genres.GetValueOrDefault(g));
        score += Math.Min(35.0, genreOverlap * 2.0);

        // 2. Keywords Match (Max 25 points)
        var cKws = (c.Keywords?.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()).Select(k => k.Trim()).ToList();
        double kwOverlap = cKws.Sum(k => profile.Keywords.GetValueOrDefault(k));
        score += Math.Min(25.0, kwOverlap * 1.0);

        // 3. Directors Match (Max 20 points)
        var cDirs = (c.Directors?.Split(',') ?? Array.Empty<string>()).Select(d => d.Trim()).ToList();
        double dirOverlap = cDirs.Sum(d => profile.Directors.GetValueOrDefault(d));
        score += Math.Min(20.0, dirOverlap * 4.0);

        // 4. Actors & Writers Match (Max 10 points each)
        var cActs = (c.Actors?.Split(',') ?? Array.Empty<string>()).Select(a => a.Trim()).ToList();
        double actOverlap = cActs.Sum(a => profile.Actors.GetValueOrDefault(a));
        score += Math.Min(10.0, actOverlap * 2.0);

        var cWriters = (c.Writers?.Split(',') ?? Array.Empty<string>()).Select(w => w.Trim()).ToList();
        double writerOverlap = cWriters.Sum(w => profile.Writers.GetValueOrDefault(w));
        score += Math.Min(10.0, writerOverlap * 2.0);

        // 5. Decade Match (Max 10 points)
        if (c.ReleaseYear.HasValue)
        {
            int cDecade = (c.ReleaseYear.Value / 10) * 10;
            double decadeWeight = profile.Decades.GetValueOrDefault(cDecade);
            score += Math.Min(10.0, decadeWeight * 1.5);
        }

        // 6. Runtime Match (Max 8 points)
        if (c.RuntimeMinutes.HasValue)
        {
            double diff = Math.Abs(c.RuntimeMinutes.Value - profile.PreferredRuntime);
            if (diff <= 15) score += 8.0;
            else if (diff <= 30) score += 4.0;
        }

        // 7. Awards & Prestige Match (Max 5 points)
        if (!string.IsNullOrEmpty(c.Awards))
        {
            var (wins, noms, otherWins, _) = ParseAwards(c.Awards);
            if (wins > 0 && profile.AverageOscarWins > 0) score += 3.0;
            else if (noms > 0 && profile.AverageOscarNoms > 0) score += 2.0;
            else if (otherWins > 0 && profile.AverageOtherWins > 0) score += 1.0;
        }

        // 8. Language & Country Match (Max 3 points)
        var cLangs = (c.Language?.Split(',') ?? Array.Empty<string>()).Select(l => l.Trim()).ToList();
        double langOverlap = cLangs.Sum(l => profile.Languages.GetValueOrDefault(l));
        score += Math.Min(1.5, langOverlap * 0.5);

        var cCountries = (c.Country?.Split(',') ?? Array.Empty<string>()).Select(cnt => cnt.Trim()).ToList();
        double countryOverlap = cCountries.Sum(cnt => profile.Countries.GetValueOrDefault(cnt));
        score += Math.Min(1.5, countryOverlap * 0.5);

        // 9. Box Office Scale Match (Max 2 points)
        if (!string.IsNullOrEmpty(c.BoxOffice))
        {
            double boVal = ParseBoxOfficeValue(c.BoxOffice);
            if (boVal > 0 && profile.AverageBoxOffice > 0)
            {
                double ratio = Math.Min(boVal, profile.AverageBoxOffice) / Math.Max(boVal, profile.AverageBoxOffice);
                score += ratio * 2.0;
            }
        }

        return Math.Clamp(score, 0.0, 100.0);
    }

    protected List<WatchedMovieDetailDto> ExtractRecentMovies(List<WatchedMovieDetailDto> watched)
    {
        if (watched == null || !watched.Any()) return new List<WatchedMovieDetailDto>();

        var sorted = watched.OrderByDescending(w => w.WatchDate).ToList();
        var fourteenDaysAgo = DateTime.UtcNow.AddDays(-14);
        var recent = sorted.Where(w => w.WatchDate >= fourteenDaysAgo).ToList();

        if (recent.Count < 4)
        {
            var padded = recent.ToList();
            foreach (var w in sorted)
            {
                if (padded.Count >= 4) break;
                if (!padded.Any(p => p.MovieId == w.MovieId))
                {
                    padded.Add(w);
                }
            }
            return padded;
        }

        return recent;
    }

    private static double ParseBoxOfficeValue(string? boxOffice)
    {
        if (string.IsNullOrEmpty(boxOffice)) return 0.0;
        var digits = new string(boxOffice.Where(char.IsDigit).ToArray());
        if (double.TryParse(digits, out double val)) return val;
        return 0.0;
    }
}

public class UserViewingProfile
{
    public Dictionary<string, double> Genres { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Directors { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Actors { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Writers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Keywords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<int, double> Decades { get; set; } = new();
    
    public double PreferredRuntime { get; set; } = 100.0;
    public double AverageUserRating { get; set; } = 7.0;
    public int TotalWatches { get; set; }
    
    public Dictionary<string, double> Languages { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Countries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    public double AverageOscarWins { get; set; }
    public double AverageOscarNoms { get; set; }
    public double AverageOtherWins { get; set; }
    
    public double AverageBoxOffice { get; set; }
}
