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

public record LogMovieWatchCommand(
    Guid UserId, 
    Guid MovieId, 
    DateOnly DateWatched, 
    double? Rating, 
    bool IsRewatch) : IRequest<bool>;

public class LogMovieWatchCommandHandler : IRequestHandler<LogMovieWatchCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public LogMovieWatchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(LogMovieWatchCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies.FindAsync(new object[] { request.MovieId }, cancellationToken);
        if (movie == null)
            return false;

        // 1. Handle Rating (update or create)
        if (request.Rating.HasValue)
        {
            var existingRating = await _context.MovieRatings
                .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.MovieId == request.MovieId, cancellationToken);
            
            if (existingRating != null)
            {
                existingRating.UpdateScore((decimal)request.Rating.Value);
            }
            else
            {
                _context.MovieRatings.Add(new MovieRating(Guid.NewGuid(), request.UserId, request.MovieId, request.DateWatched, (decimal)request.Rating.Value, null));
            }
        }

        // 2. Handle Diary Entry
        var existingEntry = await _context.DiaryEntries
            .FirstOrDefaultAsync(d => d.UserId == request.UserId && d.MovieId == request.MovieId && d.WatchedDate == request.DateWatched, cancellationToken);

        if (existingEntry != null)
        {
            // If it already exists on this exact date, we might just update the rewatch flag if needed
            // However, domain model might not support mutating IsRewatch directly if it's read-only.
            // Assuming we just log a new one if not exact match or ignore if identical.
        }
        else
        {
            _context.DiaryEntries.Add(new DiaryEntry(
                Guid.NewGuid(), 
                request.UserId, 
                request.MovieId, 
                DateOnly.FromDateTime(DateTime.UtcNow), // logDate
                request.DateWatched, 
                request.Rating.HasValue ? (decimal?)request.Rating.Value : null, 
                request.IsRewatch,
                null, // tags
                null  // importHistoryId
            ));
        }

        // 3. Ensure WatchedMovie record exists (so it shows up in library)
        var watchedMovie = await _context.WatchedMovies
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.MovieId == request.MovieId, cancellationToken);

        if (watchedMovie == null)
        {
            _context.WatchedMovies.Add(new WatchedMovie(
                Guid.NewGuid(),
                request.UserId,
                request.MovieId,
                request.DateWatched
                // ImportHistoryId is null — this is a manually logged watch, not from an import
            ));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
