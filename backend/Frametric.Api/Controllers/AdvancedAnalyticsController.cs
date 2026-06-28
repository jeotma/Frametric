// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Analytics.Bonus;
using Frametric.Application.Queries.Analytics.Watched;
using Frametric.Application.Queries.Analytics.Watchlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/analytics/advanced")]
[Authorize]
public class AdvancedAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AdvancedAnalyticsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    private Guid GetUserIdOrThrow()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    // --- WATCHED BASIC ---

    [HttpGet("watched")]
    public async Task<ActionResult<IEnumerable<WatchedMovieStatsDto>>> GetWatchedMovies([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchedMoviesQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/directors")]
    public async Task<ActionResult<IEnumerable<DirectorCountDto>>> GetWatchedDirectors([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchedDirectorsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/actors")]
    public async Task<ActionResult<IEnumerable<ActorCountDto>>> GetWatchedActors([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchedActorsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/genres")]
    public async Task<ActionResult<IEnumerable<GenreCountDto>>> GetWatchedMoviesByGenre([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchedMoviesByGenreQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/decades")]
    public async Task<ActionResult<IEnumerable<DecadeCountDto>>> GetWatchedMoviesByDecade([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchedMoviesByDecadeQuery(GetUserIdOrThrow(), filter)));

    // --- WATCHED ADVANCED ---

    [HttpGet("watched/most-repeated-actor")]
    public async Task<ActionResult<ActorCountDto>> GetMostRepeatedActor([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetMostRepeatedActorQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/most-watched-director")]
    public async Task<ActionResult<DirectorCountDto>> GetMostWatchedDirector([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetMostWatchedDirectorQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/predominant-era")]
    public async Task<ActionResult<EraBreakdownDto>> GetPredominantEra([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetPredominantEraQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/director-ranking")]
    public async Task<ActionResult<IEnumerable<DirectorLeaderboardDto>>> GetDirectorRankingByRating([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetDirectorRankingByRatingQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/total-time")]
    public async Task<ActionResult<TimeInvestedDto>> GetTotalTimeByDirectorOrGenre([FromQuery] string filterType, [FromQuery] string? filterName, [FromQuery] AnalyticsFilterDto filter)
    {
        if (string.IsNullOrWhiteSpace(filterName))
        {
            return Ok(new TimeInvestedDto(filterName ?? "", 0, 0));
        }
        return Ok(await _mediator.Send(new GetTotalTimeByDirectorOrGenreQuery(GetUserIdOrThrow(), filterType, filterName, filter)));
    }

    // --- WATCHED CORRELATIONS ---

    [HttpGet("watched/preferred-day")]
    public async Task<ActionResult<IEnumerable<PreferredDayDto>>> GetPreferredWatchDayOfWeek([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetPreferredWatchDayOfWeekQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/rating-evolution")]
    public async Task<ActionResult<IEnumerable<RatingEvolutionDto>>> GetRatingEvolution([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetRatingEvolutionQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/genre-streaks")]
    public async Task<ActionResult<IEnumerable<GenreStreakDto>>> GetGenreStreaks([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetGenreStreakQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/longest-movie")]
    public async Task<ActionResult<MovieSimpleDto>> GetLongestMovie([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetLongestWatchedMovieQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watched/casting-repetitions")]
    public async Task<ActionResult<IEnumerable<CastingPairDto>>> GetCastingRepetitions([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetCastingRepetitionsQuery(GetUserIdOrThrow(), filter)));

    // --- WATCHLIST BASIC ---

    [HttpGet("watchlist")]
    public async Task<ActionResult<IEnumerable<WatchlistMovieStatsDto>>> GetWatchlist([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchlistQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/directors")]
    public async Task<ActionResult<IEnumerable<DirectorCountDto>>> GetWatchlistDirectors([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchlistDirectorsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/actors")]
    public async Task<ActionResult<IEnumerable<ActorCountDto>>> GetWatchlistActors([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchlistActorsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/genres")]
    public async Task<ActionResult<IEnumerable<GenreCountDto>>> GetWatchlistByGenre([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchlistByGenreQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/decades")]
    public async Task<ActionResult<IEnumerable<DecadeCountDto>>> GetWatchlistByDecade([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchlistByDecadeQuery(GetUserIdOrThrow(), filter)));

    // --- WATCHLIST ADVANCED ---

    [HttpGet("watchlist/most-anticipated-director")]
    public async Task<ActionResult<DirectorCountDto>> GetMostAnticipatedDirector([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetMostAnticipatedDirectorQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/most-anticipated-actor")]
    public async Task<ActionResult<ActorCountDto>> GetMostAnticipatedActor([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetMostAnticipatedActorQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/total-watchtime")]
    public async Task<ActionResult<TimeInvestedDto>> GetTotalPendingWatchtime([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetTotalPendingWatchtimeQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/oldest-pending")]
    public async Task<ActionResult<MovieSimpleDto>> GetOldestPendingMovie([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetOldestPendingMovieQuery(GetUserIdOrThrow(), filter)));

    // --- WATCHLIST CORRELATIONS ---

    [HttpGet("watchlist/by-era")]
    public async Task<ActionResult<IEnumerable<EraBreakdownDto>>> GetWatchlistByEra([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchlistByEraQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/ghost-actor")]
    public async Task<ActionResult<GhostActorDto>> GetGhostActor([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetGhostActorQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/golden-director")]
    public async Task<ActionResult<GoldenDirectorDto>> GetGoldenDirector([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetGoldenPendingDirectorQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/duration-balance")]
    public async Task<ActionResult<IEnumerable<DurationBalanceDto>>> GetDurationBalance([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetPendingDurationBalanceQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("watchlist/genre-proportion")]
    public async Task<ActionResult<IEnumerable<GenreProportionDto>>> GetGenreProportion([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetGenreProportionWatchlistVsWatchedQuery(GetUserIdOrThrow(), filter)));

    // --- BONUS ---

    [HttpGet("bonus/weekend-warrior")]
    public async Task<ActionResult<WeekendWarriorDto>> GetWeekendWarrior([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWeekendWarriorStatsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("bonus/hidden-gems")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetHiddenGems([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetHiddenGemsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("bonus/watchlist-graveyard")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetWatchlistGraveyard([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetWatchlistGraveyardQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("bonus/cinematic-fatigue")]
    public async Task<ActionResult<CinematicFatigueExpandedDto>> GetCinematicFatigue([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetCinematicFatigueExpandedQuery(GetUserIdOrThrow(), filter)));

    // --- FINAL CUT ---

    [HttpGet("final-cut/bookends")]
    public async Task<ActionResult<BookendsDto>> GetBookends([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetBookendsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("final-cut/monthly-extremes")]
    public async Task<ActionResult<IEnumerable<MonthlyExtremeDto>>> GetMonthlyExtremes([FromQuery] AnalyticsFilterDto filter, [FromQuery] bool includeRewatches = false)
        => Ok(await _mediator.Send(new GetMonthlyExtremesQuery(GetUserIdOrThrow(), filter, includeRewatches)));

    [HttpGet("final-cut/top-bottom-rated")]
    public async Task<ActionResult<TopBottomMoviesDto>> GetTopAndBottomRated([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetTopAndBottomRatedMoviesQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("final-cut/most-rewatched")]
    public async Task<ActionResult<MostRewatchedDto>> GetMostRewatched([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetMostRewatchedMovieQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("final-cut/best-rookies")]
    public async Task<ActionResult<BestRookiesDto>> GetBestRookies([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetBestRookiesQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("final-cut/prime-time")]
    public async Task<ActionResult<PrimeTimeStatsDto>> GetPrimeTime([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetPrimeTimeStatsQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("final-cut/genre-landscape")]
    public async Task<ActionResult<IEnumerable<GenreWithRatingDto>>> GetGenreLandscape([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetGenresWithRatingQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("final-cut/shortest-movie")]
    public async Task<ActionResult<WrappedMovieDto>> GetShortestMovie([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetShortestWatchedMovieQuery(GetUserIdOrThrow(), filter)));

    [HttpGet("final-cut/director-actor-pairs")]
    public async Task<ActionResult<IEnumerable<DirectorActorPairDto>>> GetDirectorActorPairs([FromQuery] AnalyticsFilterDto filter)
        => Ok(await _mediator.Send(new GetDirectorActorPairingsQuery(GetUserIdOrThrow(), filter)));
}



