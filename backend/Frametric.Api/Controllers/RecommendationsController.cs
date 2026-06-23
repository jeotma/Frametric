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
using Frametric.Api.DTOs;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Recommendations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/v1/recommendations")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDistributedCache _cache;

    public RecommendationsController(IMediator mediator, ICurrentUserService currentUserService, IDistributedCache cache)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _cache = cache;
    }

    private Guid GetUserIdOrThrow()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    [HttpPost("generate")]
    public async Task<ActionResult<IEnumerable<RecommendedMovieDto>>> Generate(
        [FromBody] RecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetCinematicRecommendationsQuery(
            GetUserIdOrThrow(),
            request.Strategy,
            request.Scope,
            request.Quantity,
            request.MaxRuntimeMinutes,
            request.MinRuntimeMinutes
        );

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("skip/{movieId:guid}")]
    public async Task<IActionResult> Skip(Guid movieId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var cacheKey = $"skip_recommendation:{userId}:{movieId}";

        // Temporarily cache the skipped movie ID for 24 hours to prevent it from reappearing immediately
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        await _cache.SetStringAsync(cacheKey, "skipped", options, cancellationToken);

        return NoContent();
    }

    [HttpPost("skip-haunting")]
    public async Task<IActionResult> SkipHaunting(CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var cacheKey = $"skip_watchlist_haunting:{userId}";

        // Permanently cache the preference (using a very long expiration, e.g. 10 years)
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365 * 10)
        };

        await _cache.SetStringAsync(cacheKey, "disabled", options, cancellationToken);
        return NoContent();
    }

    [HttpPost("dismiss-wellness")]
    public async Task<IActionResult> DismissWellness(CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var cacheKey = $"skip_wellness_check:{userId}";

        // Temporarily cache the skip choice for 7 days
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        };

        await _cache.SetStringAsync(cacheKey, "dismissed", options, cancellationToken);
        return NoContent();
    }
}
