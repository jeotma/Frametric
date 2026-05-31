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

    // --- WATCHED CORRELATIONS ---

    [HttpGet("watched/genre-streaks")]
    public async Task<ActionResult<IEnumerable<GenreStreakDto>>> GetGenreStreaks()
        => Ok(await _mediator.Send(new GetGenreStreakQuery(GetUserIdOrThrow())));

    [HttpGet("watched/longest-movie")]
    public async Task<ActionResult<MovieSimpleDto>> GetLongestMovie()
        => Ok(await _mediator.Send(new GetLongestWatchedMovieQuery(GetUserIdOrThrow())));

    [HttpGet("watched/casting-repetitions")]
    public async Task<ActionResult<IEnumerable<CastingPairDto>>> GetCastingRepetitions()
        => Ok(await _mediator.Send(new GetCastingRepetitionsQuery(GetUserIdOrThrow())));

    // --- WATCHLIST CORRELATIONS ---

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
    public async Task<ActionResult<WeekendWarriorDto>> GetWeekendWarrior()
        => Ok(await _mediator.Send(new GetWeekendWarriorStatsQuery(GetUserIdOrThrow())));

    [HttpGet("bonus/hidden-gems")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetHiddenGems()
        => Ok(await _mediator.Send(new GetHiddenGemsQuery(GetUserIdOrThrow())));

    [HttpGet("bonus/watchlist-graveyard")]
    public async Task<ActionResult<IEnumerable<MovieSimpleDto>>> GetWatchlistGraveyard()
        => Ok(await _mediator.Send(new GetWatchlistGraveyardQuery(GetUserIdOrThrow())));

    [HttpGet("bonus/cinematic-fatigue")]
    public async Task<ActionResult<CinematicFatigueDto>> GetCinematicFatigue()
        => Ok(await _mediator.Send(new GetCinematicFatigueQuery(GetUserIdOrThrow())));
}
