// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Admin;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.Admin;

public class GetDatabaseStatsQueryHandler : IRequestHandler<GetDatabaseStatsQuery, DatabaseStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDatabaseStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DatabaseStatsDto> Handle(GetDatabaseStatsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalMovies = await _context.Movies.CountAsync(cancellationToken);
        
        var pendingMovies = await _context.Movies.CountAsync(m => m.EnrichmentStatus == EnrichmentStatus.Pending, cancellationToken);
        var completedMovies = await _context.Movies.CountAsync(m => m.EnrichmentStatus == EnrichmentStatus.Completed, cancellationToken);
        var failedMovies = await _context.Movies.CountAsync(m => m.EnrichmentStatus == EnrichmentStatus.Failed, cancellationToken);
        var notFoundMovies = await _context.Movies.CountAsync(m => m.EnrichmentStatus == EnrichmentStatus.NotFound, cancellationToken);
        var permanentlyFailedMovies = await _context.Movies.CountAsync(m => m.EnrichmentStatus == EnrichmentStatus.PermanentlyFailed, cancellationToken);

        var totalTvShows = await _context.TvShows.CountAsync(cancellationToken);
        var totalGenres = await _context.Genres.CountAsync(cancellationToken);
        var totalDirectors = await _context.Directors.CountAsync(cancellationToken);
        var totalActors = await _context.Actors.CountAsync(cancellationToken);
        var totalDiaryEntries = await _context.DiaryEntries.CountAsync(cancellationToken);

        return new DatabaseStatsDto(
            totalUsers,
            totalMovies,
            pendingMovies,
            completedMovies,
            failedMovies,
            notFoundMovies,
            permanentlyFailedMovies,
            totalTvShows,
            totalGenres,
            totalDirectors,
            totalActors,
            totalDiaryEntries
        );
    }
}
