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
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.Discovery;

public record GetUserBingoBoardsQuery(Guid UserId) : IRequest<IEnumerable<BingoBoardDto>>;

public class GetUserBingoBoardsQueryHandler : IRequestHandler<GetUserBingoBoardsQuery, IEnumerable<BingoBoardDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserBingoBoardsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<BingoBoardDto>> Handle(GetUserBingoBoardsQuery request, CancellationToken cancellationToken)
    {
        var objectives = await _dbContext.DiscoveryObjectives
            .Where(o => o.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (!objectives.Any())
        {
            return Enumerable.Empty<BingoBoardDto>();
        }

        // Group objectives by BoardId
        return objectives
            .GroupBy(o => o.BoardId)
            .Select(g =>
            {
                var total = g.Count();
                var achieved = g.Count(o => o.IsAchieved);
                var isCompleted = achieved == total;
                var sampleObjective = g.First();
                
                // Estimate creation timestamp based on StartDate, or default to CompletionDate, or default to MinValue.
                // We'll use MinValue if none exist, but in practice objectives will have StartDate or CompletionDate or we'll fallback to UtcNow
                var createdAt = sampleObjective.StartDate ?? DateTime.UtcNow;

                return new BingoBoardDto(
                    BoardId: g.Key,
                    GridSize: sampleObjective.GridSize,
                    StartDate: sampleObjective.StartDate,
                    EndDate: sampleObjective.EndDate,
                    IsCompleted: isCompleted,
                    CompletedSquares: achieved,
                    TotalSquares: total,
                    CreatedAt: createdAt
                );
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToList();
    }
}
