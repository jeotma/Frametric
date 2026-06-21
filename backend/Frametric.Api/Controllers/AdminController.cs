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
using Frametric.Application.Commands.Admin;
using Frametric.Application.Commands.EnrichMovies;
using Frametric.Application.DTOs.Admin;
using Frametric.Application.Queries.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("users/{userId}/promote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PromoteUser(Guid userId, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new PromoteUserCommand(userId), cancellationToken);
        if (!success) return NotFound("User not found.");
        return Ok(new { message = "User promoted to Admin successfully." });
    }

    [HttpGet("diagnostics/database")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DatabaseStatsDto>> GetDatabaseStats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDatabaseStatsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("diagnostics/providers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderDiagnosticsDto>> GetProviderDiagnostics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProviderDiagnosticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("diagnostics/logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LogEntryDto>>> GetRecentLogs(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRecentLogsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("maintenance/purge-orphans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PurgeOrphanResultDto>> PurgeOrphanEntities(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PurgeOrphanEntitiesCommand(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("maintenance/clear-cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ClearCache(
        [FromServices] IMemoryCache? memoryCache, 
        [FromServices] IDistributedCache distributedCache)
    {
        if (memoryCache is MemoryCache mc)
        {
            mc.Compact(1.0);
        }
        
        return Ok(new { message = "System cache cleared successfully." });
    }


    [HttpPost("enrich/retry-failed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> RetryFailedEnrichments(
        [FromQuery] bool resetPermanentlyFailed = false, 
        [FromQuery] int batchSize = 50, 
        CancellationToken cancellationToken = default)
    {
        var recoveredCount = await _mediator.Send(new EnrichFailedMoviesCommand(batchSize, resetPermanentlyFailed), cancellationToken);
        return Ok(recoveredCount);
    }
}
