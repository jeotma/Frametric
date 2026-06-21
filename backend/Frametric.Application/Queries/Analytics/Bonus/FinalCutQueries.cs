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

namespace Frametric.Application.Queries.Analytics.Bonus;

// --- The Bookends: First & Last Movie of the Year ---
public record GetBookendsQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<BookendsDto?>;
public class GetBookendsQueryHandler : IRequestHandler<GetBookendsQuery, BookendsDto?>
{
    private readonly IBonusQueries _queries;
    public GetBookendsQueryHandler(IBonusQueries queries) => _queries = queries;
    public async Task<BookendsDto?> Handle(GetBookendsQuery request, CancellationToken ct)
        => await _queries.GetBookendsAsync(request.UserId, request.Filter, ct);
}

// --- The Monthly Extremes: Best & Worst per Month ---
public record GetMonthlyExtremesQuery(Guid UserId, AnalyticsFilterDto Filter, bool IncludeRewatches = false) : IRequest<IEnumerable<MonthlyExtremeDto>>;
public class GetMonthlyExtremesQueryHandler : IRequestHandler<GetMonthlyExtremesQuery, IEnumerable<MonthlyExtremeDto>>
{
    private readonly IBonusQueries _queries;
    public GetMonthlyExtremesQueryHandler(IBonusQueries queries) => _queries = queries;
    public async Task<IEnumerable<MonthlyExtremeDto>> Handle(GetMonthlyExtremesQuery request, CancellationToken ct)
        => await _queries.GetMonthlyExtremesAsync(request.UserId, request.Filter, request.IncludeRewatches, ct);
}

// --- The Hall of Fame / Golden Raspberry: Top & Bottom 5 ---
public record GetTopAndBottomRatedMoviesQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<TopBottomMoviesDto>;
public class GetTopAndBottomRatedMoviesQueryHandler : IRequestHandler<GetTopAndBottomRatedMoviesQuery, TopBottomMoviesDto>
{
    private readonly IBonusQueries _queries;
    public GetTopAndBottomRatedMoviesQueryHandler(IBonusQueries queries) => _queries = queries;
    public async Task<TopBottomMoviesDto> Handle(GetTopAndBottomRatedMoviesQuery request, CancellationToken ct)
        => await _queries.GetTopAndBottomRatedMoviesAsync(request.UserId, request.Filter, ct);
}

// --- The Return of the King: Most Rewatched ---
public record GetMostRewatchedMovieQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<MostRewatchedDto?>;
public class GetMostRewatchedMovieQueryHandler : IRequestHandler<GetMostRewatchedMovieQuery, MostRewatchedDto?>
{
    private readonly IBonusQueries _queries;
    public GetMostRewatchedMovieQueryHandler(IBonusQueries queries) => _queries = queries;
    public async Task<MostRewatchedDto?> Handle(GetMostRewatchedMovieQuery request, CancellationToken ct)
        => await _queries.GetMostRewatchedMovieAsync(request.UserId, request.Filter, ct);
}

// --- The Best Rookies: First-time Directors & Actors ---
public record GetBestRookiesQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<BestRookiesDto>;
public class GetBestRookiesQueryHandler : IRequestHandler<GetBestRookiesQuery, BestRookiesDto>
{
    private readonly IBonusQueries _queries;
    public GetBestRookiesQueryHandler(IBonusQueries queries) => _queries = queries;
    public async Task<BestRookiesDto> Handle(GetBestRookiesQuery request, CancellationToken ct)
        => await _queries.GetBestRookiesAsync(request.UserId, request.Filter, ct);
}

// --- Cinematic Fatigue (Expanded) ---
public record GetCinematicFatigueExpandedQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<CinematicFatigueExpandedDto?>;
public class GetCinematicFatigueExpandedQueryHandler : IRequestHandler<GetCinematicFatigueExpandedQuery, CinematicFatigueExpandedDto?>
{
    private readonly IBonusQueries _queries;
    public GetCinematicFatigueExpandedQueryHandler(IBonusQueries queries) => _queries = queries;
    public async Task<CinematicFatigueExpandedDto?> Handle(GetCinematicFatigueExpandedQuery request, CancellationToken ct)
        => await _queries.GetCinematicFatigueExpandedAsync(request.UserId, request.Filter, ct);
}



