// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsApplication _analyticsApplication;
    private readonly ICurrentUserService _currentUserService;

    public AnalyticsController(IAnalyticsApplication analyticsApplication, ICurrentUserService currentUserService)
    {
        _analyticsApplication = analyticsApplication;
        _currentUserService = currentUserService;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        var result = await _analyticsApplication.GetDashboardSummaryAsync(userId.Value, cancellationToken);
        return Ok(result);
    }

    [HttpGet("wrapped/{year:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WrappedSummaryDto>> GetWrappedSummary(int year, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        var result = await _analyticsApplication.GetWrappedSummaryAsync(userId.Value, year, cancellationToken);
        return Ok(result);
    }

    [HttpGet("monthly-activity/{year:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MonthlyActivityResponseDto>> GetMonthlyActivity(int year, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        var result = await _analyticsApplication.GetMonthlyActivityAsync(userId.Value, year, cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-directors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<DirectorLeaderboardDto>>> GetTopDirectors([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        var result = await _analyticsApplication.GetTopDirectorsAsync(userId.Value, limit, cancellationToken);
        return Ok(result);
    }
}
