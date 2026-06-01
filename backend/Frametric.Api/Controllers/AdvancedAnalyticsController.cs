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

    [HttpGet("watched/by-release-year")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetWatchedMoviesByReleaseYear([FromQuery] int releaseYear)
        => Ok(await _mediator.Send(new GetWatchedMoviesByReleaseYearQuery(GetUserIdOrThrow(), releaseYear)));

    [HttpGet("watched/directors")]
    public async Task<ActionResult<IEnumerable<DirectorCountDto>>> GetWatchedDirectors([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetWatchedDirectorsQuery(GetUserIdOrThrow(), year)));

    [HttpGet("watched/actors")]
    public async Task<ActionResult<IEnumerable<ActorCountDto>>> GetWatchedActors([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetWatchedActorsQuery(GetUserIdOrThrow(), year)));

    [HttpGet("watched/genres")]
    public async Task<ActionResult<IEnumerable<GenreCountDto>>> GetWatchedMoviesByGenre()
        => Ok(await _mediator.Send(new GetWatchedMoviesByGenreQuery(GetUserIdOrThrow())));

    [HttpGet("watched/decades")]
    public async Task<ActionResult<IEnumerable<DecadeCountDto>>> GetWatchedMoviesByDecade([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetWatchedMoviesByDecadeQuery(GetUserIdOrThrow(), year)));

    // --- WATCHED ADVANCED ---

    [HttpGet("watched/most-repeated-actor")]
    public async Task<ActionResult<ActorCountDto>> GetMostRepeatedActor()
        => Ok(await _mediator.Send(new GetMostRepeatedActorQuery(GetUserIdOrThrow())));

    [HttpGet("watched/most-watched-director")]
    public async Task<ActionResult<DirectorCountDto>> GetMostWatchedDirector()
        => Ok(await _mediator.Send(new GetMostWatchedDirectorQuery(GetUserIdOrThrow())));

    [HttpGet("watched/predominant-era")]
    public async Task<ActionResult<EraBreakdownDto>> GetPredominantEra([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetPredominantEraQuery(GetUserIdOrThrow(), year)));

    [HttpGet("watched/director-ranking")]
    public async Task<ActionResult<IEnumerable<DirectorLeaderboardDto>>> GetDirectorRankingByRating()
        => Ok(await _mediator.Send(new GetDirectorRankingByRatingQuery(GetUserIdOrThrow())));

    [HttpGet("watched/total-time")]
    public async Task<ActionResult<TimeInvestedDto>> GetTotalTimeByDirectorOrGenre([FromQuery] string filterType, [FromQuery] string filterName)
        => Ok(await _mediator.Send(new GetTotalTimeByDirectorOrGenreQuery(GetUserIdOrThrow(), filterType, filterName)));

    // --- WATCHED CORRELATIONS ---

    [HttpGet("watched/preferred-day")]
    public async Task<ActionResult<IEnumerable<PreferredDayDto>>> GetPreferredWatchDayOfWeek()
        => Ok(await _mediator.Send(new GetPreferredWatchDayOfWeekQuery(GetUserIdOrThrow())));

    [HttpGet("watched/rating-evolution")]
    public async Task<ActionResult<IEnumerable<RatingEvolutionDto>>> GetRatingEvolution([FromQuery] int year)
        => Ok(await _mediator.Send(new GetRatingEvolutionQuery(GetUserIdOrThrow(), year)));

    [HttpGet("watched/genre-streaks")]
    public async Task<ActionResult<IEnumerable<GenreStreakDto>>> GetGenreStreaks([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetGenreStreakQuery(GetUserIdOrThrow(), year)));

    [HttpGet("watched/longest-movie")]
    public async Task<ActionResult<MovieSimpleDto>> GetLongestMovie([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetLongestWatchedMovieQuery(GetUserIdOrThrow(), year)));

    [HttpGet("watched/casting-repetitions")]
    public async Task<ActionResult<IEnumerable<CastingPairDto>>> GetCastingRepetitions([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetCastingRepetitionsQuery(GetUserIdOrThrow(), year)));

    // --- WATCHLIST BASIC ---

    [HttpGet("watchlist/by-release-year")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetWatchlistByYear([FromQuery] int releaseYear)
        => Ok(await _mediator.Send(new GetWatchlistByYearQuery(GetUserIdOrThrow(), releaseYear)));

    [HttpGet("watchlist/directors")]
    public async Task<ActionResult<IEnumerable<DirectorCountDto>>> GetWatchlistDirectors()
        => Ok(await _mediator.Send(new GetWatchlistDirectorsQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/actors")]
    public async Task<ActionResult<IEnumerable<ActorCountDto>>> GetWatchlistActors()
        => Ok(await _mediator.Send(new GetWatchlistActorsQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/genres")]
    public async Task<ActionResult<IEnumerable<GenreCountDto>>> GetWatchlistByGenre()
        => Ok(await _mediator.Send(new GetWatchlistByGenreQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/decades")]
    public async Task<ActionResult<IEnumerable<DecadeCountDto>>> GetWatchlistByDecade()
        => Ok(await _mediator.Send(new GetWatchlistByDecadeQuery(GetUserIdOrThrow())));

    // --- WATCHLIST ADVANCED ---

    [HttpGet("watchlist/most-anticipated-director")]
    public async Task<ActionResult<DirectorCountDto>> GetMostAnticipatedDirector()
        => Ok(await _mediator.Send(new GetMostAnticipatedDirectorQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/most-anticipated-actor")]
    public async Task<ActionResult<ActorCountDto>> GetMostAnticipatedActor()
        => Ok(await _mediator.Send(new GetMostAnticipatedActorQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/total-watchtime")]
    public async Task<ActionResult<TimeInvestedDto>> GetTotalPendingWatchtime()
        => Ok(await _mediator.Send(new GetTotalPendingWatchtimeQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/oldest-pending")]
    public async Task<ActionResult<MovieSimpleDto>> GetOldestPendingMovie()
        => Ok(await _mediator.Send(new GetOldestPendingMovieQuery(GetUserIdOrThrow())));

    // --- WATCHLIST CORRELATIONS ---

    [HttpGet("watchlist/by-era")]
    public async Task<ActionResult<IEnumerable<EraBreakdownDto>>> GetWatchlistByEra()
        => Ok(await _mediator.Send(new GetWatchlistByEraQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/ghost-actor")]
    public async Task<ActionResult<GhostActorDto>> GetGhostActor()
        => Ok(await _mediator.Send(new GetGhostActorQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/golden-director")]
    public async Task<ActionResult<GoldenDirectorDto>> GetGoldenDirector()
        => Ok(await _mediator.Send(new GetGoldenPendingDirectorQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/duration-balance")]
    public async Task<ActionResult<IEnumerable<DurationBalanceDto>>> GetDurationBalance()
        => Ok(await _mediator.Send(new GetPendingDurationBalanceQuery(GetUserIdOrThrow())));

    [HttpGet("watchlist/genre-proportion")]
    public async Task<ActionResult<IEnumerable<GenreProportionDto>>> GetGenreProportion()
        => Ok(await _mediator.Send(new GetGenreProportionWatchlistVsWatchedQuery(GetUserIdOrThrow())));

    // --- BONUS ---

    [HttpGet("bonus/weekend-warrior")]
    public async Task<ActionResult<WeekendWarriorDto>> GetWeekendWarrior([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetWeekendWarriorStatsQuery(GetUserIdOrThrow(), year)));

    [HttpGet("bonus/hidden-gems")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetHiddenGems([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetHiddenGemsQuery(GetUserIdOrThrow(), year)));

    [HttpGet("bonus/watchlist-graveyard")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetWatchlistGraveyard()
        => Ok(await _mediator.Send(new GetWatchlistGraveyardQuery(GetUserIdOrThrow())));

    [HttpGet("bonus/cinematic-fatigue")]
    public async Task<ActionResult<CinematicFatigueExpandedDto>> GetCinematicFatigue([FromQuery] int year)
        => Ok(await _mediator.Send(new GetCinematicFatigueExpandedQuery(GetUserIdOrThrow(), year)));

    // --- FINAL CUT ---

    [HttpGet("final-cut/bookends")]
    public async Task<ActionResult<BookendsDto>> GetBookends([FromQuery] int year)
        => Ok(await _mediator.Send(new GetBookendsQuery(GetUserIdOrThrow(), year)));

    [HttpGet("final-cut/monthly-extremes")]
    public async Task<ActionResult<IEnumerable<MonthlyExtremeDto>>> GetMonthlyExtremes([FromQuery] int year, [FromQuery] bool includeRewatches = false)
        => Ok(await _mediator.Send(new GetMonthlyExtremesQuery(GetUserIdOrThrow(), year, includeRewatches)));

    [HttpGet("final-cut/top-bottom-rated")]
    public async Task<ActionResult<TopBottomMoviesDto>> GetTopAndBottomRated([FromQuery] int year)
        => Ok(await _mediator.Send(new GetTopAndBottomRatedMoviesQuery(GetUserIdOrThrow(), year)));

    [HttpGet("final-cut/most-rewatched")]
    public async Task<ActionResult<MostRewatchedDto>> GetMostRewatched([FromQuery] int year)
        => Ok(await _mediator.Send(new GetMostRewatchedMovieQuery(GetUserIdOrThrow(), year)));

    [HttpGet("final-cut/best-rookies")]
    public async Task<ActionResult<BestRookiesDto>> GetBestRookies([FromQuery] int year)
        => Ok(await _mediator.Send(new GetBestRookiesQuery(GetUserIdOrThrow(), year)));

    [HttpGet("final-cut/prime-time")]
    public async Task<ActionResult<PrimeTimeStatsDto>> GetPrimeTime([FromQuery] int year)
        => Ok(await _mediator.Send(new GetPrimeTimeStatsQuery(GetUserIdOrThrow(), year)));

    [HttpGet("final-cut/genre-landscape")]
    public async Task<ActionResult<IEnumerable<GenreWithRatingDto>>> GetGenreLandscape([FromQuery] int year)
        => Ok(await _mediator.Send(new GetGenresWithRatingQuery(GetUserIdOrThrow(), year)));

    [HttpGet("final-cut/shortest-movie")]
    public async Task<ActionResult<WrappedMovieDto>> GetShortestMovie([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetShortestWatchedMovieQuery(GetUserIdOrThrow(), year)));

    [HttpGet("final-cut/director-actor-pairs")]
    public async Task<ActionResult<IEnumerable<DirectorActorPairDto>>> GetDirectorActorPairs([FromQuery] int? year)
        => Ok(await _mediator.Send(new GetDirectorActorPairingsQuery(GetUserIdOrThrow(), year)));
}

