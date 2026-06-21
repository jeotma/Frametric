// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watched;

// --- Shortest Watched Movie (David and Goliath) ---
public record GetShortestWatchedMovieQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<WrappedMovieDto?>;
public class GetShortestWatchedMovieQueryHandler : IRequestHandler<GetShortestWatchedMovieQuery, WrappedMovieDto?>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetShortestWatchedMovieQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<WrappedMovieDto?> Handle(GetShortestWatchedMovieQuery request, CancellationToken ct)
        => await _queries.GetShortestWatchedMovieAsync(request.UserId, request.Filter, ct);
}

// --- Director-Actor Pairs (Dynamic Duos & Perfect Pairs) ---
public record GetDirectorActorPairingsQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<DirectorActorPairDto>>;
public class GetDirectorActorPairingsQueryHandler : IRequestHandler<GetDirectorActorPairingsQuery, IEnumerable<DirectorActorPairDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetDirectorActorPairingsQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<IEnumerable<DirectorActorPairDto>> Handle(GetDirectorActorPairingsQuery request, CancellationToken ct)
        => await _queries.GetDirectorActorPairingsAsync(request.UserId, request.Filter, ct);
}

// --- Prime Time Stats (Peak & Slump habits) ---
public record GetPrimeTimeStatsQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<PrimeTimeStatsDto?>;
public class GetPrimeTimeStatsQueryHandler : IRequestHandler<GetPrimeTimeStatsQuery, PrimeTimeStatsDto?>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetPrimeTimeStatsQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<PrimeTimeStatsDto?> Handle(GetPrimeTimeStatsQuery request, CancellationToken ct)
        => await _queries.GetPrimeTimeStatsAsync(request.UserId, request.Filter, ct);
}

// --- Genre Landscape Expanded (with ratings) ---
public record GetGenresWithRatingQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<GenreWithRatingDto>>;
public class GetGenresWithRatingQueryHandler : IRequestHandler<GetGenresWithRatingQuery, IEnumerable<GenreWithRatingDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetGenresWithRatingQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<IEnumerable<GenreWithRatingDto>> Handle(GetGenresWithRatingQuery request, CancellationToken ct)
        => await _queries.GetGenresWithRatingAsync(request.UserId, request.Filter, ct);
}



