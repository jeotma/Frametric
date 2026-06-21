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

/// <summary>
/// Removes a single diary entry for the current user.
/// If the deleted entry is NOT a rewatch (i.e. it is the canonical first watch),
/// ALL user associations with the film are also purged: WatchedMovie, MovieRating, and MovieLike.
/// Rewatches are considered bonus entries — deleting one leaves the rest of the data intact.
/// </summary>
public record UnlogMovieWatchCommand(Guid UserId, Guid MovieId, Guid DiaryEntryId) : IRequest<bool>;

public class UnlogMovieWatchCommandHandler : IRequestHandler<UnlogMovieWatchCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public UnlogMovieWatchCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(UnlogMovieWatchCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the diary entry — must belong to the requesting user
        var entry = await _context.DiaryEntries
            .FirstOrDefaultAsync(
                d => d.Id == request.DiaryEntryId
                  && d.UserId == request.UserId
                  && d.MovieId == request.MovieId,
                cancellationToken);

        if (entry == null)
            return false;

        _context.DiaryEntries.Remove(entry);

        // 2. If this was not a rewatch it is the canonical watch — purge all user associations
        if (!entry.IsRewatch)
        {
            var watchedMovie = await _context.WatchedMovies
                .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.MovieId == request.MovieId, cancellationToken);
            if (watchedMovie != null)
                _context.WatchedMovies.Remove(watchedMovie);

            var rating = await _context.MovieRatings
                .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.MovieId == request.MovieId, cancellationToken);
            if (rating != null)
                _context.MovieRatings.Remove(rating);

            var like = await _context.MovieLikes
                .FirstOrDefaultAsync(l => l.UserId == request.UserId && l.MovieId == request.MovieId, cancellationToken);
            if (like != null)
                _context.MovieLikes.Remove(like);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate analytics caches
        _cacheService.RemoveByPrefix($"WrappedSummary_{request.UserId}");
        _cacheService.RemoveByPrefix($"WatchedMovies_{request.UserId}");

        return true;
    }
}
