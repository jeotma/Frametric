// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Domain.Enums;

namespace Frametric.Application.Interfaces.Analytics;

public record WatchedMovieDetailDto(
    Guid MovieId,
    int? ReleaseYear,
    int? RuntimeMinutes,
    string? Genres,
    string? Directors,
    string? Actors,
    double? UserRating,
    DateTime WatchDate,
    string? Keywords = null
);

public record CandidateMovieDto(
    Guid MovieId,
    string Title,
    int? ReleaseYear,
    int? RuntimeMinutes,
    string? PosterUrl,
    double? TmdbRating,
    double? TmdbPopularity,
    double? CustomAverageRating,
    string? Genres,
    string? Directors,
    string? Actors,
    string? Keywords = null,
    string? Awards = null,
    string? Writers = null,
    string? Language = null,
    string? Country = null,
    string? BoxOffice = null,
    string? Certification = null,
    string? StreamingProviders = null,
    string? Overview = null,
    double? ImdbRating = null,
    double? RottenTomatoesRating = null,
    double? MetacriticRating = null
);

public interface IRecommendationQueries
{
    Task<IEnumerable<WatchedMovieDetailDto>> GetWatchedMovieDetailsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<CandidateMovieDto>> GetCandidateMoviesAsync(Guid userId, RecommendationScope scope, int? maxRuntimeMinutes, CancellationToken ct = default);
}
