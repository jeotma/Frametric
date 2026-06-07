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
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Queries.Discovery;

public class RevealMysteryBoxQueryHandler : IRequestHandler<RevealMysteryBoxQuery, SelectionResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RevealMysteryBoxQueryHandler> _logger;

    public RevealMysteryBoxQueryHandler(IApplicationDbContext context, ILogger<RevealMysteryBoxQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SelectionResultDto> Handle(RevealMysteryBoxQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Revealing mystery box {BoxId} for user {UserId}", request.BoxId, request.UserId);

        var movie = await _context.Movies
            .Where(m => m.Id == request.BoxId)
            .Select(m => new SelectionResultDto(
                m.Id,
                m.Title,
                m.Directors.Any()
                    ? string.Join(", ", m.Directors.Select(d => d.Name))
                    : "Unknown",
                m.ReleaseYear ?? 0,
                "MysteryBoxReveal",
                m.PosterUrl,
                m.RuntimeMinutes))
            .SingleOrDefaultAsync(cancellationToken);

        if (movie == null)
        {
            throw new InvalidOperationException($"The mystery box {request.BoxId} could not be opened — movie not found.");
        }

        return movie;
    }
}
