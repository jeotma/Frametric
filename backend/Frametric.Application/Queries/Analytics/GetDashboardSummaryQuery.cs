// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using MediatR;

namespace Frametric.Application.Queries.Analytics;

public record GetDashboardSummaryQuery(Guid UserId) : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IAnalyticsService _analyticsService;

    public GetDashboardSummaryQueryHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsService.GetDashboardSummaryAsync(request.UserId, cancellationToken);
    }
}
