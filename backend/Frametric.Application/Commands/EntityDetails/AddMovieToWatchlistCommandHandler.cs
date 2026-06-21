// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.EntityDetails;

public class AddMovieToWatchlistCommandHandler : IRequestHandler<AddMovieToWatchlistCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AddMovieToWatchlistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AddMovieToWatchlistCommand request, CancellationToken cancellationToken)
    {
        var movieExists = await _context.Movies.AnyAsync(m => m.Id == request.MovieId, cancellationToken);
        if (!movieExists) return false;

        var alreadyInWatchlist = await _context.WatchlistItems
            .AnyAsync(w => w.MovieId == request.MovieId && w.UserId == request.UserId, cancellationToken);

        if (alreadyInWatchlist) return true; // Idempotent success

        var watchlistItem = new WatchlistItem(
            Guid.NewGuid(),
            request.UserId,
            request.MovieId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null
        );

        _context.WatchlistItems.Add(watchlistItem);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
