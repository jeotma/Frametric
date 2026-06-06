// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Domain.Enums;

namespace Frametric.Infrastructure.Queries;

public class RecommendationQueriesImpl : IRecommendationQueries
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RecommendationQueriesImpl(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<WatchedMovieDetailDto>> GetWatchedMovieDetailsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
            SELECT m.""Id"" AS MovieId,
                   m.""ReleaseYear"",
                   m.""RuntimeMinutes"",
                   (SELECT STRING_AGG(g.""Name"", ',') FROM ""MovieGenre"" mg JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id"" WHERE mg.""MoviesId"" = m.""Id"") AS Genres,
                   (SELECT STRING_AGG(d.""Name"", ',') FROM ""MovieDirector"" md JOIN ""Directors"" d ON md.""DirectorsId"" = d.""Id"" WHERE md.""MoviesId"" = m.""Id"") AS Directors,
                   (SELECT STRING_AGG(a.""Name"", ',') FROM ""MovieActor"" ma JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id"" WHERE ma.""MoviesId"" = m.""Id"") AS Actors,
                   CAST(mr.""Score"" AS DOUBLE PRECISION) AS UserRating,
                   CAST(COALESCE(de.""WatchedDate"", w.""Date"") AS TIMESTAMP) AS WatchDate,
                   m.""Keywords""
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
            LEFT JOIN ""DiaryEntries"" de ON de.""MovieId"" = w.""MovieId"" AND de.""UserId"" = @userId
            WHERE w.""UserId"" = @userId";

        return await connection.QueryAsync<WatchedMovieDetailDto>(sql, new { userId });
    }

    public async Task<IEnumerable<CandidateMovieDto>> GetCandidateMoviesAsync(Guid userId, RecommendationScope scope, int? maxRuntimeMinutes, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        string cteSql;
        if (scope == RecommendationScope.WatchlistOnly)
        {
            cteSql = @"
                SELECT m.*
                FROM ""WatchlistItems"" wl
                JOIN ""Movies"" m ON wl.""MovieId"" = m.""Id""
                WHERE wl.""UserId"" = @userId";
        }
        else if (scope == RecommendationScope.DatabaseOnly)
        {
            cteSql = @"
                SELECT m.*
                FROM ""Movies"" m";
        }
        else // Hybrid
        {
            cteSql = @"
                SELECT m.*
                FROM ""WatchlistItems"" wl
                JOIN ""Movies"" m ON wl.""MovieId"" = m.""Id""
                WHERE wl.""UserId"" = @userId
                UNION
                SELECT m.*
                FROM ""Movies"" m";
        }

        var sql = $@"
            WITH Candidates AS (
                {cteSql}
            )
            SELECT c.""Id"" AS MovieId,
                   c.""Title"",
                   c.""ReleaseYear"",
                   c.""RuntimeMinutes"",
                   c.""PosterUrl"",
                   CAST(c.""TmdbRating"" AS DOUBLE PRECISION) AS TmdbRating,
                   CAST(c.""TmdbPopularity"" AS DOUBLE PRECISION) AS TmdbPopularity,
                   CAST(c.""CustomAverageRating"" AS DOUBLE PRECISION) AS CustomAverageRating,
                   (SELECT STRING_AGG(g.""Name"", ',') FROM ""MovieGenre"" mg JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id"" WHERE mg.""MoviesId"" = c.""Id"") AS Genres,
                   (SELECT STRING_AGG(d.""Name"", ',') FROM ""MovieDirector"" md JOIN ""Directors"" d ON md.""DirectorsId"" = d.""Id"" WHERE md.""MoviesId"" = c.""Id"") AS Directors,
                   (SELECT STRING_AGG(a.""Name"", ',') FROM ""MovieActor"" ma JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id"" WHERE ma.""MoviesId"" = c.""Id"") AS Actors,
                   c.""Keywords"",
                   c.""Awards"",
                   c.""Writers"",
                   c.""Language"",
                   c.""Country"",
                   c.""BoxOffice"",
                   c.""Certification"",
                   c.""StreamingProviders"",
                   c.""Overview"",
                   CAST(c.""ImdbRating"" AS DOUBLE PRECISION) AS ImdbRating,
                   CAST(c.""RottenTomatoesRating"" AS DOUBLE PRECISION) AS RottenTomatoesRating,
                   CAST(c.""MetacriticRating"" AS DOUBLE PRECISION) AS MetacriticRating,
                   wl.""DateAdded"" AS WatchlistAddedDate
            FROM Candidates c
            LEFT JOIN ""WatchlistItems"" wl ON c.""Id"" = wl.""MovieId"" AND wl.""UserId"" = @userId
            WHERE NOT EXISTS (
                SELECT 1 FROM ""WatchedMovies"" wm WHERE wm.""MovieId"" = c.""Id"" AND wm.""UserId"" = @userId
            )
            AND NOT EXISTS (
                SELECT 1 FROM ""DiaryEntries"" de WHERE de.""MovieId"" = c.""Id"" AND de.""UserId"" = @userId
            )
            AND c.""EnrichmentStatus"" = 'Completed'
            AND (@maxRuntimeMinutes IS NULL OR c.""RuntimeMinutes"" <= @maxRuntimeMinutes)
            AND c.""ReleaseYear"" > 0
            AND (
                c.""ReleaseYear"" < @currentYear
                OR (c.""ReleaseYear"" = @currentYear AND c.""ReleaseDate"" IS NOT NULL AND c.""ReleaseDate"" <= @currentDate)
            )";

        var currentYear = DateTime.UtcNow.Year;
        var currentDate = DateTime.UtcNow.Date;
        return await connection.QueryAsync<CandidateMovieDto>(sql, new { userId, maxRuntimeMinutes, currentYear, currentDate });
    }
}
