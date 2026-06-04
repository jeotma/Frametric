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
        int quantity,
        int? maxRuntime = null);

    protected static string FormatReasons(List<string> reasons)
    {
        if (reasons == null || reasons.Count == 0) return string.Empty;
        if (reasons.Count == 1) return reasons[0];
        if (reasons.Count == 2) return $"{reasons[0]} and {reasons[1]}";
        
        return $"{string.Join(", ", reasons.Take(reasons.Count - 1))}, and {reasons.Last()}";
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
}
