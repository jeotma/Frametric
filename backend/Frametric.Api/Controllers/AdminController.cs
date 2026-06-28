// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Recommendations;

using Microsoft.EntityFrameworkCore;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users/{userId}/viewing-profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserViewingProfile>> GetUserViewingProfile(
        Guid userId,
        [FromServices] IUserViewingProfileService profileService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId;
        if (currentUserId == null) return Forbid();

        var actor = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) return Forbid();

        if (actor.Role != Frametric.Domain.Enums.UserRole.SuperAdmin && 
            !(actor.Role == Frametric.Domain.Enums.UserRole.Admin && (actor.CanAddUsers || actor.CanDeleteUsers)))
        {
            return Forbid();
        }

        var users = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        var userExists = users.Any(u => u.Id == userId);
        if (!userExists) return NotFound("User not found.");

        var profile = await profileService.GetOrCreateProfileAsync(userId);
        return Ok(profile);
    }

    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] AdminCreateUserRequest request, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(new CreateUserCommand(request.Username, request.Email, request.Password), cancellationToken);
        return Ok(userId);
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

    [HttpPost("users/{userId}/demote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DemoteUser(Guid userId, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new DemoteUserCommand(userId), cancellationToken);
        if (!success) return NotFound("User not found.");
        return Ok(new { message = "User demoted to standard user successfully." });
    }

    [HttpDelete("users/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new DeleteUserCommand(userId), cancellationToken);
        if (!success) return NotFound("User not found.");
        return Ok(new { message = "User deleted successfully." });
    }

    [HttpPost("users/{userId}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserPermissions(Guid userId, [FromBody] UpdatePermissionsRequest request, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new UpdateUserPermissionsCommand(
            userId,
            request.CanManageCatalog,
            request.CanAddUsers,
            request.CanDeleteUsers,
            request.CanPromoteToAdmin
        ), cancellationToken);
        if (!success) return NotFound("User not found.");
        return Ok(new { message = "User permissions updated successfully." });
    }

    [HttpPost("catalog/movies/{movieId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMovie(Guid movieId, [FromBody] AdminUpdateMovieRequest request, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new UpdateMovieCommand(
            movieId,
            request.Title,
            request.Overview,
            request.ReleaseYear,
            request.RuntimeMinutes
        ), cancellationToken);
        if (!success) return NotFound("Movie not found.");
        return Ok(new { message = "Movie updated successfully." });
    }

    [HttpPost("catalog/actors/{actorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateActor(Guid actorId, [FromBody] AdminUpdateContributorRequest request, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new UpdateActorCommand(actorId, request.Name), cancellationToken);
        if (!success) return NotFound("Actor not found.");
        return Ok(new { message = "Actor updated successfully." });
    }

    [HttpPost("catalog/directors/{directorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDirector(Guid directorId, [FromBody] AdminUpdateContributorRequest request, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new UpdateDirectorCommand(directorId, request.Name), cancellationToken);
        if (!success) return NotFound("Director not found.");
        return Ok(new { message = "Director updated successfully." });
    }

    [HttpGet("catalog/revisions/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RevisionDto>>> GetEntityRevisions(string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEntityRevisionsQuery(entityType, entityId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("catalog/revisions/{revisionId}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreRevision(Guid revisionId, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new RestoreRevisionCommand(revisionId), cancellationToken);
        if (!success) return NotFound("Revision or entity not found.");
        return Ok(new { message = "Entity state restored successfully." });
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
    public async Task<IActionResult> ClearCache(
        [FromServices] IMemoryCache? memoryCache,
        [FromServices] IDistributedCache distributedCache,
        CancellationToken cancellationToken)
    {
        if (memoryCache is MemoryCache mc)
        {
            mc.Compact(1.0);
        }

        if (distributedCache is MemoryDistributedCache)
        {
            // In-memory distributed cache doesn't need explicit clearing
        }
        else
        {
            try
            {
                var connectionString = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Redis:ConnectionString"];
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    var redis = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(connectionString);
                    var server = redis.GetServer(redis.GetEndPoints().First());
                    await server.FlushDatabaseAsync();
                    await redis.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminController>>();
                logger.LogWarning(ex, "Failed to clear distributed cache.");
            }
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

public record AdminCreateUserRequest(string Username, string Email, string Password);
public record UpdatePermissionsRequest(bool CanManageCatalog, bool CanAddUsers, bool CanDeleteUsers, bool CanPromoteToAdmin);
public record AdminUpdateMovieRequest(string Title, string? Overview, int? ReleaseYear, int? RuntimeMinutes);
public record AdminUpdateContributorRequest(string Name);
