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
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Queries.Recommendations;

public class GetCinematicRecommendationsQueryHandler : IRequestHandler<GetCinematicRecommendationsQuery, IEnumerable<RecommendedMovieDto>>
{
    private readonly IRecommendationQueries _recommendationQueries;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetCinematicRecommendationsQueryHandler> _logger;

    public GetCinematicRecommendationsQueryHandler(
        IRecommendationQueries recommendationQueries,
        IDistributedCache cache,
        ILogger<GetCinematicRecommendationsQueryHandler> logger)
    {
        _recommendationQueries = recommendationQueries;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<RecommendedMovieDto>> Handle(GetCinematicRecommendationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating cinematic recommendations for user {UserId} using strategy {Strategy} and scope {Scope}",
            request.UserId, request.Strategy, request.Scope);

        // 1. Fetch data
        var watched = (await _recommendationQueries.GetWatchedMovieDetailsAsync(request.UserId, cancellationToken)).ToList();
        var candidates = (await _recommendationQueries.GetCandidateMoviesAsync(request.UserId, request.Scope, request.MaxRuntimeMinutes, cancellationToken)).ToList();

        if (!candidates.Any())
        {
            _logger.LogWarning("No candidate movies found for recommendation in scope {Scope}", request.Scope);
            return Enumerable.Empty<RecommendedMovieDto>();
        }

        // 2. Filter out skipped movies from cache
        var filteredCandidates = new List<CandidateMovieDto>();
        foreach (var candidate in candidates)
        {
            var cacheKey = $"skip_recommendation:{request.UserId}:{candidate.MovieId}";
            var isSkipped = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (isSkipped == null)
            {
                filteredCandidates.Add(candidate);
            }
        }

        if (!filteredCandidates.Any())
        {
            _logger.LogWarning("All candidates were filtered out by the skip cache. Falling back to unfiltered candidates.");
            filteredCandidates = candidates; // Fallback
        }

        // 3. Compute recommendations based on strategy
        List<RecommendedMovieDto> recommendations;

        if (!watched.Any())
        {
            _logger.LogInformation("User has no watch history. Recommending highly-rated movies in pool.");
            recommendations = filteredCandidates
                .Select(c => new RecommendedMovieDto(
                    c.MovieId,
                    c.Title,
                    c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                    c.ReleaseYear ?? 0,
                    90.0,
                    "Highly acclaimed movie to start your cinematic journey.",
                    c.PosterUrl,
                    c.RuntimeMinutes
                ))
                .OrderByDescending(r => r.MatchPercentage)
                .Take(request.Quantity)
                .ToList();
            return recommendations;
        }

        switch (request.Strategy)
        {
            case RecommendationStrategy.RecentMood:
                recommendations = ApplyRecentMood(filteredCandidates, watched, request.Quantity);
                break;
            case RecommendationStrategy.OppositeMood:
                recommendations = ApplyOppositeMood(filteredCandidates, watched, request.Quantity);
                break;
            case RecommendationStrategy.ComfortZoneDisruptor:
                recommendations = ApplyComfortZoneDisruptor(filteredCandidates, watched, request.Quantity);
                break;
            case RecommendationStrategy.GuiltyPleasure:
                recommendations = ApplyGuiltyPleasure(filteredCandidates, watched, request.Quantity);
                break;
            case RecommendationStrategy.CinephileElite:
                recommendations = ApplyCinephileElite(filteredCandidates, watched, request.Quantity);
                break;
            case RecommendationStrategy.DirectorsTrajectory:
                recommendations = ApplyDirectorsTrajectory(filteredCandidates, watched, request.Quantity);
                break;
            case RecommendationStrategy.RuntimeContext:
                recommendations = ApplyRuntimeContext(filteredCandidates, watched, request.Quantity, request.MaxRuntimeMinutes);
                break;
            case RecommendationStrategy.PureRandom:
                recommendations = ApplyPureRandom(filteredCandidates, request.Quantity);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Strategy), "Unsupported recommendation strategy.");
        }

        return recommendations;
    }

    // ── Algorithmic Strategies ──────────────────────────────────────────────────

    private List<RecommendedMovieDto> ApplyRecentMood(List<CandidateMovieDto> candidates, List<WatchedMovieDetailDto> watched, int quantity)
    {
        var recent = watched.OrderByDescending(w => w.WatchDate).Take(10).ToList();
        var recentGenres = recent.SelectMany(r => r.Genres?.Split(',') ?? Array.Empty<string>())
            .GroupBy(g => g).ToDictionary(g => g.Key, g => g.Count());
        var recentDecades = recent.Where(r => r.ReleaseYear.HasValue)
            .Select(r => (r.ReleaseYear!.Value / 10) * 10)
            .GroupBy(d => d).ToDictionary(d => d.Key, d => d.Count());
        var recentDirectors = recent.SelectMany(r => r.Directors?.Split(',') ?? Array.Empty<string>()).Distinct().ToList();
        var recentActors = recent.SelectMany(r => r.Actors?.Split(',') ?? Array.Empty<string>()).Distinct().ToList();
        double avgRuntime = recent.Any(r => r.RuntimeMinutes > 0) ? recent.Where(r => r.RuntimeMinutes > 0).Average(r => r.RuntimeMinutes!.Value) : 100.0;

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Genre alignment
            var cGenres = c.Genres?.Split(',') ?? Array.Empty<string>();
            double genreOverlap = 0;
            foreach (var cg in cGenres)
            {
                if (recentGenres.TryGetValue(cg, out int count))
                {
                    genreOverlap += count;
                }
            }
            if (genreOverlap > 0)
            {
                double weight = Math.Min(40.0, genreOverlap * 8.0);
                score += weight;
                reasons.Add("shares recent favorite genres");
            }

            // Runtime alignment
            if (c.RuntimeMinutes.HasValue && Math.Abs(c.RuntimeMinutes.Value - avgRuntime) <= 20)
            {
                score += 20;
                reasons.Add("perfect pacing match");
            }

            // Decade alignment
            if (c.ReleaseYear.HasValue)
            {
                int cDecade = (c.ReleaseYear.Value / 10) * 10;
                if (recentDecades.TryGetValue(cDecade, out int count))
                {
                    score += Math.Min(20.0, count * 5.0);
                    reasons.Add($"matches your recent era ({cDecade}s)");
                }
            }

            // Director alignment
            var cDirs = c.Directors?.Split(',') ?? Array.Empty<string>();
            if (cDirs.Any(d => recentDirectors.Contains(d)))
            {
                score += 10;
                reasons.Add("directed by a filmmaker you watched recently");
            }

            // Actor alignment
            var cActors = c.Actors?.Split(',') ?? Array.Empty<string>();
            if (cActors.Any(a => recentActors.Contains(a)))
            {
                score += 10;
                reasons.Add("stars familiar faces");
            }

            // Normalize rating contribution
            double ratingValue = c.CustomAverageRating ?? c.TmdbRating ?? 6.0;
            score += (ratingValue / 10.0) * 10.0;

            double match = Math.Min(100.0, Math.Round(score, 1));
            string reason = reasons.Any() ? $"Aligns with your recent mood because it {FormatReasons(reasons)}." : "Fits well with your recent viewing history.";

            return new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                match,
                reason,
                c.PosterUrl,
                c.RuntimeMinutes
            );
        }).OrderByDescending(r => r.MatchPercentage).Take(quantity).ToList();
    }

    private List<RecommendedMovieDto> ApplyOppositeMood(List<CandidateMovieDto> candidates, List<WatchedMovieDetailDto> watched, int quantity)
    {
        var recent = watched.OrderByDescending(w => w.WatchDate).Take(10).ToList();
        var recentGenres = recent.SelectMany(r => r.Genres?.Split(',') ?? Array.Empty<string>()).Distinct().ToList();
        double avgRuntime = recent.Any(r => r.RuntimeMinutes > 0) ? recent.Where(r => r.RuntimeMinutes > 0).Average(r => r.RuntimeMinutes!.Value) : 100.0;

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Genre inversion
            var cGenres = c.Genres?.Split(',') ?? Array.Empty<string>();
            if (!cGenres.Any(cg => recentGenres.Contains(cg)))
            {
                score += 40;
                reasons.Add("breaks away from recent genres");
            }

            // Pacing inversion
            if (c.RuntimeMinutes.HasValue)
            {
                if (avgRuntime > 115 && c.RuntimeMinutes < 95)
                {
                    score += 30;
                    reasons.Add("offers a quicker, faster-paced watch");
                }
                else if (avgRuntime < 95 && c.RuntimeMinutes > 115)
                {
                    score += 30;
                    reasons.Add("dives into a deeper, slower-paced cinematic experience");
                }
            }

            // Quality score contribution
            double ratingValue = c.CustomAverageRating ?? c.TmdbRating ?? 6.0;
            score += (ratingValue / 10.0) * 30.0;

            double match = Math.Min(100.0, Math.Round(score, 1));
            string reason = reasons.Any() ? $"A perfect palette cleanser: it {FormatReasons(reasons)}." : "Great selection to diversify your movie cycle.";

            return new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                match,
                reason,
                c.PosterUrl,
                c.RuntimeMinutes
            );
        }).OrderByDescending(r => r.MatchPercentage).Take(quantity).ToList();
    }

    private List<RecommendedMovieDto> ApplyComfortZoneDisruptor(List<CandidateMovieDto> candidates, List<WatchedMovieDetailDto> watched, int quantity)
    {
        int total = watched.Count;
        var comfortGenres = watched.SelectMany(r => r.Genres?.Split(',') ?? Array.Empty<string>())
            .GroupBy(g => g)
            .Where(g => (double)g.Count() / total > 0.35)
            .Select(g => g.Key).ToList();

        var comfortEras = watched.Where(w => w.ReleaseYear.HasValue)
            .Select(w => (w.ReleaseYear!.Value / 10) * 10)
            .GroupBy(d => d)
            .Where(d => (double)d.Count() / total > 0.35)
            .Select(d => d.Key).ToList();

        // Highly-rated directors or actors (average rating >= 4.0 out of 5 stars, which maps to >= 8.0 out of 10)
        var highRatedDirectors = watched.Where(w => w.UserRating.HasValue && w.UserRating.Value >= 8.0)
            .SelectMany(w => w.Directors?.Split(',') ?? Array.Empty<string>()).Distinct().ToList();

        var highRatedActors = watched.Where(w => w.UserRating.HasValue && w.UserRating.Value >= 8.0)
            .SelectMany(w => w.Actors?.Split(',') ?? Array.Empty<string>()).Distinct().ToList();

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Genre & Era disruption
            var cGenres = c.Genres?.Split(',') ?? Array.Empty<string>();
            int cDecade = c.ReleaseYear.HasValue ? (c.ReleaseYear.Value / 10) * 10 : 0;

            if (!cGenres.Any(cg => comfortGenres.Contains(cg)) && !comfortEras.Contains(cDecade))
            {
                score += 50;
                reasons.Add("takes you completely out of your usual comfort zone");
            }

            // High-rated actor or director anchor
            var cDirs = c.Directors?.Split(',') ?? Array.Empty<string>();
            var cActs = c.Actors?.Split(',') ?? Array.Empty<string>();

            bool hasFamiliarAnchor = false;
            if (cDirs.Any(d => highRatedDirectors.Contains(d)))
            {
                score += 40;
                hasFamiliarAnchor = true;
                reasons.Add("anchored by a director you highly rated in the past");
            }
            else if (cActs.Any(a => highRatedActors.Contains(a)))
            {
                score += 40;
                hasFamiliarAnchor = true;
                reasons.Add("anchored by an actor you love");
            }

            // Quality score contribution
            double ratingValue = c.CustomAverageRating ?? c.TmdbRating ?? 6.0;
            score += (ratingValue / 10.0) * 10.0;

            double match = Math.Min(100.0, Math.Round(score, 1));
            string reason = reasons.Any() ? $"Disrupts your comfort zone: it {FormatReasons(reasons)}." : "Pushes your boundaries with a solid critical consensus.";

            // If we don't have a familiar anchor, match score should be lower so we prioritize anchored disruptors
            if (!hasFamiliarAnchor) match = Math.Max(10.0, match - 30.0);

            return new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                match,
                reason,
                c.PosterUrl,
                c.RuntimeMinutes
            );
        }).OrderByDescending(r => r.MatchPercentage).Take(quantity).ToList();
    }

    private List<RecommendedMovieDto> ApplyGuiltyPleasure(List<CandidateMovieDto> candidates, List<WatchedMovieDetailDto> watched, int quantity)
    {
        double userAvgRating = watched.Where(w => w.UserRating.HasValue).Average(w => w.UserRating!.Value);

        // Find sub-genres where user historically rates higher than their average rating
        var genreStats = watched.SelectMany(w => (w.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => new { Genre = g, Rating = w.UserRating }))
            .Where(x => x.Rating.HasValue)
            .GroupBy(x => x.Genre)
            .Select(g => new { Genre = g.Key, Avg = g.Average(x => x.Rating!.Value) })
            .Where(x => x.Avg > userAvgRating)
            .Select(x => x.Genre).ToList();

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Low popularity / global score
            double pop = c.TmdbPopularity ?? 50.0;
            double rating = c.CustomAverageRating ?? c.TmdbRating ?? 6.5;

            if (pop < 25.0 && rating < 7.0)
            {
                score += 40;
                reasons.Add("is a hidden, less-mainstream title");
            }

            // Genre matches user preference
            var cGenres = c.Genres?.Split(',') ?? Array.Empty<string>();
            if (cGenres.Any(g => genreStats.Contains(g)))
            {
                score += 60;
                reasons.Add("precisely fits a sub-genre you rate higher than average");
            }

            double match = Math.Min(100.0, Math.Round(score, 1));
            string reason = reasons.Any() ? $"A guilty pleasure pick: it {FormatReasons(reasons)}." : "Underrated movie matching your niche genre ratings.";

            return new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                match,
                reason,
                c.PosterUrl,
                c.RuntimeMinutes
            );
        }).OrderByDescending(r => r.MatchPercentage).Take(quantity).ToList();
    }

    private List<RecommendedMovieDto> ApplyCinephileElite(List<CandidateMovieDto> candidates, List<WatchedMovieDetailDto> watched, int quantity)
    {
        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            double rating = c.CustomAverageRating ?? c.TmdbRating ?? 0.0;
            double popularity = c.TmdbPopularity ?? 100.0;

            if (rating >= 8.2)
            {
                score += 70;
                reasons.Add("is widely acclaimed as a critical masterpiece");
            }
            else if (rating >= 7.6)
            {
                score += 40;
                reasons.Add("has exceptionally high reviews");
            }

            if (popularity < 35.0)
            {
                score += 30;
                reasons.Add("retains high prestige away from mainstream popularity");
            }

            double match = Math.Min(100.0, Math.Round(score, 1));
            string reason = reasons.Any() ? $"Cinephile Elite choice: it {FormatReasons(reasons)}." : "Acclaimed cinematic masterpiece.";

            return new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                match,
                reason,
                c.PosterUrl,
                c.RuntimeMinutes
            );
        }).OrderByDescending(r => r.MatchPercentage).Take(quantity).ToList();
    }

    private List<RecommendedMovieDto> ApplyDirectorsTrajectory(List<CandidateMovieDto> candidates, List<WatchedMovieDetailDto> watched, int quantity)
    {
        // Find directors the user has watched >= 2 times
        var topDirectors = watched.SelectMany(w => w.Directors?.Split(',') ?? Array.Empty<string>())
            .GroupBy(d => d)
            .Where(g => g.Count() >= 2)
            .Select(g => g.Key).ToList();

        return candidates
            .Where(c => !string.IsNullOrEmpty(c.Directors) && c.Directors.Split(',').Any(d => topDirectors.Contains(d)))
            .Select(c =>
            {
                var dir = c.Directors?.Split(',').FirstOrDefault(d => topDirectors.Contains(d)) ?? "Unknown Director";
                return new RecommendedMovieDto(
                    c.MovieId,
                    c.Title,
                    c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                    c.ReleaseYear ?? 0,
                    100.0,
                    $"Continues your exploration of {dir}'s filmography chronologically.",
                    c.PosterUrl,
                    c.RuntimeMinutes
                );
            })
            .OrderBy(r => r.ReleaseYear) // Chronological order
            .Take(quantity)
            .ToList();
    }

    private List<RecommendedMovieDto> ApplyRuntimeContext(List<CandidateMovieDto> candidates, List<WatchedMovieDetailDto> watched, int quantity, int? maxRuntime)
    {
        double targetRuntime = maxRuntime ?? 90.0;

        return candidates.Select(c =>
        {
            double score = 0;
            var reasons = new List<string>();

            if (c.RuntimeMinutes.HasValue)
            {
                double diff = Math.Abs(c.RuntimeMinutes.Value - targetRuntime);
                if (diff <= 10)
                {
                    score += 50;
                    reasons.Add("perfectly fits your time availability");
                }
                else if (diff <= 20)
                {
                    score += 30;
                    reasons.Add("fits comfortably in your schedule");
                }
            }

            // High tempo genres for short limits
            var cGenres = c.Genres?.Split(',') ?? Array.Empty<string>();
            bool isShort = targetRuntime <= 95.0;
            if (isShort && cGenres.Any(g => g == "Comedy" || g == "Action" || g == "Thriller" || g == "Horror"))
            {
                score += 30;
                reasons.Add("delivers high-tempo pacing ideal for shorter sessions");
            }

            // Quality score contribution
            double ratingValue = c.CustomAverageRating ?? c.TmdbRating ?? 6.0;
            score += (ratingValue / 10.0) * 20.0;

            double match = Math.Min(100.0, Math.Round(score, 1));
            string reason = reasons.Any() ? $"Perfect context fit: it {FormatReasons(reasons)}." : "Great runtime match for your availability.";

            return new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                match,
                reason,
                c.PosterUrl,
                c.RuntimeMinutes
            );
        }).OrderByDescending(r => r.MatchPercentage).Take(quantity).ToList();
    }

    private List<RecommendedMovieDto> ApplyPureRandom(List<CandidateMovieDto> candidates, int quantity)
    {
        var random = new Random();
        return candidates.OrderBy(c => random.Next())
            .Take(quantity)
            .Select(c => new RecommendedMovieDto(
                c.MovieId,
                c.Title,
                c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                c.ReleaseYear ?? 0,
                50.0,
                "A completely random pick to let chance guide your night.",
                c.PosterUrl,
                c.RuntimeMinutes
            ))
            .ToList();
    }

    private static string FormatReasons(List<string> reasons)
    {
        if (reasons == null || reasons.Count == 0) return string.Empty;
        if (reasons.Count == 1) return reasons[0];
        if (reasons.Count == 2) return $"{reasons[0]} and {reasons[1]}";
        
        return $"{string.Join(", ", reasons.Take(reasons.Count - 1))}, and {reasons.Last()}";
    }
}
