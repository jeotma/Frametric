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
    private readonly Dictionary<RecommendationStrategy, IRecommendationStrategy> _strategies;

    public GetCinematicRecommendationsQueryHandler(
        IRecommendationQueries recommendationQueries,
        IDistributedCache cache,
        ILogger<GetCinematicRecommendationsQueryHandler> logger,
        IEnumerable<IRecommendationStrategy> strategies)
    {
        _recommendationQueries = recommendationQueries;
        _cache = cache;
        _logger = logger;
        _strategies = strategies.ToDictionary(s => s.Strategy);
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
        if (!watched.Any())
        {
            _logger.LogInformation("User has no watch history. Recommending highly-rated movies in pool.");
            
            // Standard fallback using basic score/rating
            var random = new Random();
            var recommendations = filteredCandidates
                .Select(c =>
                {
                    double rating = c.CustomAverageRating ?? c.TmdbRating ?? 6.0;
                    double popFactor = (c.TmdbPopularity ?? 0.0) * 0.000001;
                    double hashFactor = Math.Abs(c.MovieId.GetHashCode() % 10000) / 10000000.0;
                    double match = 80.0 + (rating / 10.0) * 19.0 + popFactor + hashFactor;

                    return new RecommendedMovieDto(
                        c.MovieId,
                        c.Title,
                        c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                        c.ReleaseYear ?? 0,
                        Math.Round(match, 4),
                        "Highly acclaimed movie to start your cinematic journey.",
                        c.PosterUrl,
                        c.RuntimeMinutes,
                        c.CustomAverageRating
                    );
                })
                .OrderByDescending(r => r.MatchPercentage)
                .Take(request.Quantity)
                .ToList();
            return recommendations;
        }

        if (!_strategies.TryGetValue(request.Strategy, out var strategyEvaluator))
        {
            throw new ArgumentOutOfRangeException(nameof(request.Strategy), "Unsupported recommendation strategy.");
        }

        return strategyEvaluator.Recommend(filteredCandidates, watched, request.Quantity, request.MaxRuntimeMinutes);
    }
}
