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
public record GetShortestWatchedMovieQuery(Guid UserId, int? Year = null) : IRequest<WrappedMovieDto?>;
public class GetShortestWatchedMovieQueryHandler : IRequestHandler<GetShortestWatchedMovieQuery, WrappedMovieDto?>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetShortestWatchedMovieQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<WrappedMovieDto?> Handle(GetShortestWatchedMovieQuery request, CancellationToken ct)
        => await _queries.GetShortestWatchedMovieAsync(request.UserId, request.Year, ct);
}

// --- Director-Actor Pairs (Dynamic Duos & Perfect Pairs) ---
public record GetDirectorActorPairingsQuery(Guid UserId, int? Year = null) : IRequest<IEnumerable<DirectorActorPairDto>>;
public class GetDirectorActorPairingsQueryHandler : IRequestHandler<GetDirectorActorPairingsQuery, IEnumerable<DirectorActorPairDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetDirectorActorPairingsQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<IEnumerable<DirectorActorPairDto>> Handle(GetDirectorActorPairingsQuery request, CancellationToken ct)
        => await _queries.GetDirectorActorPairingsAsync(request.UserId, request.Year, ct);
}

// --- Prime Time Stats (Peak & Slump habits) ---
public record GetPrimeTimeStatsQuery(Guid UserId, int Year) : IRequest<PrimeTimeStatsDto?>;
public class GetPrimeTimeStatsQueryHandler : IRequestHandler<GetPrimeTimeStatsQuery, PrimeTimeStatsDto?>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetPrimeTimeStatsQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<PrimeTimeStatsDto?> Handle(GetPrimeTimeStatsQuery request, CancellationToken ct)
        => await _queries.GetPrimeTimeStatsAsync(request.UserId, request.Year, ct);
}

// --- Genre Landscape Expanded (with ratings) ---
public record GetGenresWithRatingQuery(Guid UserId, int Year) : IRequest<IEnumerable<GenreWithRatingDto>>;
public class GetGenresWithRatingQueryHandler : IRequestHandler<GetGenresWithRatingQuery, IEnumerable<GenreWithRatingDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetGenresWithRatingQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;
    public async Task<IEnumerable<GenreWithRatingDto>> Handle(GetGenresWithRatingQuery request, CancellationToken ct)
        => await _queries.GetGenresWithRatingAsync(request.UserId, request.Year, ct);
}
