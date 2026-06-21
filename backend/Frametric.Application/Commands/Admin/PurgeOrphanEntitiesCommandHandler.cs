// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Admin;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public class PurgeOrphanEntitiesCommandHandler : IRequestHandler<PurgeOrphanEntitiesCommand, PurgeOrphanResultDto>
{
    private readonly IApplicationDbContext _context;

    public PurgeOrphanEntitiesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurgeOrphanResultDto> Handle(PurgeOrphanEntitiesCommand request, CancellationToken cancellationToken)
    {
        // Purge orphaned genres
        var orphanGenres = await _context.Genres
            .Where(g => !g.Movies.Any())
            .ToListAsync(cancellationToken);
        int genresCount = orphanGenres.Count;
        if (genresCount > 0)
        {
            _context.Genres.RemoveRange(orphanGenres);
        }

        // Purge orphaned directors
        var orphanDirectors = await _context.Directors
            .Where(d => !d.Movies.Any())
            .ToListAsync(cancellationToken);
        int directorsCount = orphanDirectors.Count;
        if (directorsCount > 0)
        {
            _context.Directors.RemoveRange(orphanDirectors);
        }

        // Purge orphaned actors
        var orphanActors = await _context.Actors
            .Where(a => !a.Movies.Any())
            .ToListAsync(cancellationToken);
        int actorsCount = orphanActors.Count;
        if (actorsCount > 0)
        {
            _context.Actors.RemoveRange(orphanActors);
        }

        if (genresCount > 0 || directorsCount > 0 || actorsCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new PurgeOrphanResultDto(genresCount, directorsCount, actorsCount);
    }
}
