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
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Discovery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Frametric.Application.Commands.Discovery;
using Frametric.Application.Interfaces.Discovery;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/v1/discovery")]
[Authorize]
public class DiscoveryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DiscoveryController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    private Guid GetUserIdOrThrow()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    [HttpPost("roulette")]
    public async Task<ActionResult<RouletteRaceResultDto>> Roulette([FromBody] RouletteRequest request, CancellationToken cancellationToken)
    {
        var query = new RouletteSelectionQuery(
            GetUserIdOrThrow(),
            request.Scope,
            request.WinningThreshold,
            request.CustomSourceIds,
            request.CustomSourceTitles,
            request.ExcludeWatched,
            request.CustomAliases,
            request.PartnerUsername,
            request.AllowMultipleWinners,
            request.WinnerCount);

        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("mystery-box")]
    public async Task<ActionResult<MysteryBoxDto>> MysteryBox([FromBody] MysteryBoxRequest request, CancellationToken cancellationToken)
    {
        var query = new MysteryBoxGenerationQuery(
            GetUserIdOrThrow(),
            request.Scope,
            request.Variant,
            request.BoxCount,
            request.CustomSourceIds,
            request.CustomSourceTitles,
            request.ExcludeWatched,
            request.PartnerUsername);

        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("dice")]
    public async Task<ActionResult<DiceRollResultDto>> Dice([FromBody] DiceRollRequest request, CancellationToken cancellationToken)
    {
        var query = new DiceRollQuery(
            GetUserIdOrThrow(),
            request.Scope,
            request.DiceTypes,
            request.CustomSourceIds,
            request.CustomSourceTitles,
            request.ExcludeWatched,
            request.Presets,
            request.PartnerUsername);

        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("slot-machine")]
    public async Task<ActionResult<SlotMachineResultDto>> SlotMachine([FromBody] SlotMachineRequest request, CancellationToken cancellationToken)
    {
        var query = new SlotMachineSpinQuery(
            GetUserIdOrThrow(),
            request.Scope,
            request.Genre,
            request.Decade,
            request.Popularity,
            request.Rating,
            request.Country,
            request.CustomSourceIds,
            request.CustomSourceTitles,
            request.ExcludeWatched);

        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("mystery-box/{boxId}/reveal")]
    public async Task<ActionResult<SelectionResultDto>> RevealMysteryBox(Guid boxId, CancellationToken cancellationToken)
    {
        var query = new RevealMysteryBoxQuery(GetUserIdOrThrow(), boxId);

        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("bingo")]
    public async Task<ActionResult<BingoGridDto>> Bingo([FromBody] BingoRequest request, CancellationToken cancellationToken)
    {
        var query = new GetBingoGridQuery(
            GetUserIdOrThrow(), 
            request.GridSize, 
            request.Scope, 
            request.CustomSourceIds, 
            request.CustomSourceTitles, 
            request.ExcludeWatched,
            request.DurationDays,
            request.BoardId,
            request.AutoEvaluate);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("bingo/claim")]
    public async Task<ActionResult<BingoGridDto>> ClaimBingoObjective([FromBody] ClaimBingoObjectiveRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ClaimBingoObjectiveCommand(GetUserIdOrThrow(), request.ObjectiveId, request.DiaryEntryId);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("bingo/squares/{objectiveId}/candidates")]
    public async Task<ActionResult<IEnumerable<BingoCandidateDto>>> GetBingoObjectiveCandidates(Guid objectiveId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetBingoObjectiveCandidatesQuery(GetUserIdOrThrow(), objectiveId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("bingo/boards")]
    public async Task<ActionResult<IEnumerable<BingoBoardDto>>> GetBingoBoards(CancellationToken cancellationToken)
    {
        var query = new GetUserBingoBoardsQuery(GetUserIdOrThrow());
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("bingo/boards/{boardId}")]
    public async Task<ActionResult<bool>> DeleteBingoBoard(Guid boardId, CancellationToken cancellationToken)
    {
        var command = new DeleteBingoBoardCommand(GetUserIdOrThrow(), boardId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("bingo/reroll/{objectiveId}")]
    public async Task<ActionResult<BingoGridDto>> RerollBingoObjective(Guid objectiveId, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RerollBingoObjectiveCommand(GetUserIdOrThrow(), objectiveId);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("available-countries")]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableCountries([FromServices] IDiscoveryQueries discoveryQueries, CancellationToken cancellationToken)
    {
        var result = await discoveryQueries.GetAvailableCountriesAsync(cancellationToken);
        return Ok(result);
    }
}
