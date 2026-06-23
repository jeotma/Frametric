// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Discovery;

public record DeleteBingoBoardCommand(Guid UserId, Guid BoardId) : IRequest<bool>;

public class DeleteBingoBoardCommandHandler : IRequestHandler<DeleteBingoBoardCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteBingoBoardCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteBingoBoardCommand request, CancellationToken cancellationToken)
    {
        var boardObjectives = await _dbContext.DiscoveryObjectives
            .Where(o => o.UserId == request.UserId && o.BoardId == request.BoardId)
            .ToListAsync(cancellationToken);

        if (!boardObjectives.Any())
        {
            return false;
        }

        _dbContext.DiscoveryObjectives.RemoveRange(boardObjectives);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
