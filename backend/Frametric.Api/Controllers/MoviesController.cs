// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Commands.EntityDetails;
using Frametric.Application.Commands.Movies;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.DTOs.EntityDetails;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.EntityDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public MoviesController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailsDto>> GetMovieDetails(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized("User is not authenticated.");

        var result = await _mediator.Send(new GetMovieDetailsQuery(userId.Value, id), cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost("enrich-from-tmdb")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieSimpleDto>> EnrichFromTmdb([FromBody] EnrichMovieRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EnrichSpecificMovieCommand(request.TmdbId), cancellationToken);
        if (result == null) return NotFound("Movie not found or is a TV show.");

        return Ok(result);
    }

    [HttpPost("{id}/log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LogMovieWatch(Guid id, [FromBody] LogMovieWatchRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized("User is not authenticated.");

        var success = await _mediator.Send(new LogMovieWatchCommand(userId.Value, id, request.DateWatched, request.Rating, request.IsRewatch), cancellationToken);
        
        if (!success) return NotFound();
        return Ok();
    }

    [HttpDelete("{id}/log/{entryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlogMovieWatch(Guid id, Guid entryId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized("User is not authenticated.");

        var success = await _mediator.Send(new UnlogMovieWatchCommand(userId.Value, id, entryId), cancellationToken);

        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/watchlist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToWatchlist(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized("User is not authenticated.");

        var success = await _mediator.Send(new AddMovieToWatchlistCommand(userId.Value, id), cancellationToken);
        if (!success) return NotFound();
        return Ok();
    }

    [HttpDelete("{id}/watchlist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWatchlist(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized("User is not authenticated.");

        var success = await _mediator.Send(new RemoveMovieFromWatchlistCommand(userId.Value, id), cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }
}

public class LogMovieWatchRequest
{
    public DateOnly DateWatched { get; set; }
    public double? Rating { get; set; }
    public bool IsRewatch { get; set; }
}

public class EnrichMovieRequest
{
    public int TmdbId { get; set; }
}

