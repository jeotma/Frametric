// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.EntityDetails;

public class RemoveMovieFromWatchlistCommandHandler : IRequestHandler<RemoveMovieFromWatchlistCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveMovieFromWatchlistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveMovieFromWatchlistCommand request, CancellationToken cancellationToken)
    {
        var watchlistItem = await _context.WatchlistItems
            .FirstOrDefaultAsync(w => w.MovieId == request.MovieId && w.UserId == request.UserId, cancellationToken);

        if (watchlistItem == null) return false;

        _context.WatchlistItems.Remove(watchlistItem);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
