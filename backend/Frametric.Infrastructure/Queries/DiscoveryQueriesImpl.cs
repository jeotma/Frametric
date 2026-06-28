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
using Dapper;
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces.Discovery;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;

namespace Frametric.Infrastructure.Queries;

public class DiscoveryQueriesImpl : IDiscoveryQueries
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DiscoveryQueriesImpl(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<Guid>> ResolveMovieIdsByTitlesAsync(IEnumerable<string> titles, CancellationToken ct = default)
    {
        var normalizedTitles = titles
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Select(title => title.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();

        if (!normalizedTitles.Any())
        {
            return Array.Empty<Guid>();
        }

        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
            SELECT m.""Id"" AS MovieId
            FROM ""Movies"" m
            WHERE lower(trim(m.""Title"")) = ANY(@titles)
                AND m.""EnrichmentStatus"" = 'Completed'
        ";

        return (await connection.QueryAsync<Guid>(sql, new { titles = normalizedTitles })).ToArray();
    }

    public async Task<IEnumerable<DiscoveryMoviePoolItemDto>> GetDiscoveryPoolAsync(Guid userId, DiscoveryDataSourceScope scope, IEnumerable<Guid>? customSourceIds, bool excludeWatched = true, Guid? partnerUserId = null, CancellationToken ct = default)
    {
        if (scope == DiscoveryDataSourceScope.CustomCollection)
        {
            if (customSourceIds == null || !customSourceIds.Any())
            {
                return Array.Empty<DiscoveryMoviePoolItemDto>();
            }
        }

        using var connection = _dbConnectionFactory.CreateConnection();
        var currentYear = DateTime.UtcNow.Year;
        var currentDate = DateTime.UtcNow.Date;

        if (scope == DiscoveryDataSourceScope.MergedWatchlists)
        {
            if (partnerUserId == null)
            {
                return Array.Empty<DiscoveryMoviePoolItemDto>();
            }
            const string mergedSql = @"
                WITH UserMovies AS (
                    SELECT m.*
                    FROM ""WatchlistItems"" wl
                    JOIN ""Movies"" m ON wl.""MovieId"" = m.""Id""
                    WHERE wl.""UserId"" = @userId
                      AND m.""EnrichmentStatus"" = 'Completed'
                      AND m.""ReleaseYear"" > 0
                    ORDER BY RANDOM()
                    LIMIT 25
                ),
                PartnerMovies AS (
                    SELECT m.*
                    FROM ""WatchlistItems"" wl
                    JOIN ""Movies"" m ON wl.""MovieId"" = m.""Id""
                    WHERE wl.""UserId"" = @partnerUserId
                      AND m.""EnrichmentStatus"" = 'Completed'
                      AND m.""ReleaseYear"" > 0
                    ORDER BY RANDOM()
                    LIMIT 25
                ),
                MergedPool AS (
                    SELECT * FROM UserMovies
                    UNION ALL
                    SELECT * FROM PartnerMovies
                )
                SELECT DISTINCT ON (mp.""Id"")
                       mp.""Id"" AS MovieId,
                       mp.""Title"" AS Title,
                       CAST(mp.""ReleaseYear"" AS INTEGER) AS ReleaseYear,
                       mp.""RuntimeMinutes"" AS RuntimeMinutes,
                       mp.""PosterUrl"" AS PosterUrl,
                       CAST(mp.""TmdbRating"" AS DOUBLE PRECISION) AS TmdbRating,
                       CAST(mp.""TmdbPopularity"" AS DOUBLE PRECISION) AS TmdbPopularity,
                       CAST(mp.""CustomAverageRating"" AS DOUBLE PRECISION) AS CustomAverageRating,
                       (SELECT STRING_AGG(d.""Name"", ',') FROM ""MovieDirector"" md JOIN ""Directors"" d ON md.""DirectorsId"" = d.""Id"" WHERE md.""MoviesId"" = mp.""Id"") AS DirectorName,
                       (SELECT STRING_AGG(a.""Name"", ',') FROM ""MovieActor"" ma JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id"" WHERE ma.""MoviesId"" = mp.""Id"") AS ActorNames,
                       (SELECT STRING_AGG(g.""Name"", ',') FROM ""MovieGenre"" mg JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id"" WHERE mg.""MoviesId"" = mp.""Id"") AS Genres,
                       mp.""Keywords"" AS Keywords,
                       mp.""Overview"" AS Overview,
                       mp.""Language"" AS Language,
                       mp.""Country"" AS Country
                FROM MergedPool mp
                ORDER BY mp.""Id"", RANDOM()";
            return await connection.QueryAsync<DiscoveryMoviePoolItemDto>(mergedSql, new { userId, partnerUserId });
        }

        var sql = BuildScopeSql(scope, excludeWatched);
        return await connection.QueryAsync<DiscoveryMoviePoolItemDto>(sql, new { userId, customSourceIds, currentYear, currentDate });
    }

    private string BuildScopeSql(DiscoveryDataSourceScope scope, bool excludeWatched)
    {
        string sourceSql;

        switch (scope)
        {
            case DiscoveryDataSourceScope.WatchlistOnly:
                sourceSql = @"
                    SELECT m.*
                    FROM ""WatchlistItems"" wl
                    JOIN ""Movies"" m ON wl.""MovieId"" = m.""Id""
                    WHERE wl.""UserId"" = @userId
                ";
                break;
            case DiscoveryDataSourceScope.DatabaseOnly:
                sourceSql = @"
                    SELECT m.*
                    FROM ""Movies"" m
                ";
                break;
            case DiscoveryDataSourceScope.Hybrid:
                sourceSql = @"
                    SELECT m.*
                    FROM ""WatchlistItems"" wl
                    JOIN ""Movies"" m ON wl.""MovieId"" = m.""Id""
                    WHERE wl.""UserId"" = @userId
                    UNION
                    SELECT m.*
                    FROM ""Movies"" m
                ";
                break;
            case DiscoveryDataSourceScope.CustomCollection:
                sourceSql = @"
                    SELECT m.*
                    FROM ""Movies"" m
                    WHERE m.""Id"" = ANY(@customSourceIds)
                ";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
        }

        return $"""
            WITH SourceSet AS (
                {sourceSql}
            ),
            ValidDiscoveryPool AS (
                SELECT *
                FROM SourceSet
                WHERE "EnrichmentStatus" = 'Completed'
                  AND "ReleaseYear" > 0
                  AND (
                      "ReleaseYear" < @currentYear
                      OR ("ReleaseYear" = @currentYear AND "ReleaseDate" IS NOT NULL AND "ReleaseDate" <= @currentDate)
                  )
                  {(excludeWatched && scope != DiscoveryDataSourceScope.CustomCollection ? @"AND ""Id"" NOT IN (
                    SELECT w.""MovieId"" FROM ""WatchedMovies"" w WHERE w.""UserId"" = @userId
                    UNION
                    SELECT de.""MovieId"" FROM ""DiaryEntries"" de WHERE de.""UserId"" = @userId
                    UNION
                    SELECT mr.""MovieId"" FROM ""MovieRatings"" mr WHERE mr.""UserId"" = @userId
                )" : "")}
            )
            SELECT vdp."Id" AS MovieId,
                   vdp."Title" AS Title,
                   CAST(vdp."ReleaseYear" AS INTEGER) AS ReleaseYear,
                   vdp."RuntimeMinutes" AS RuntimeMinutes,
                   vdp."PosterUrl" AS PosterUrl,
                   CAST(vdp."TmdbRating" AS DOUBLE PRECISION) AS TmdbRating,
                   CAST(vdp."TmdbPopularity" AS DOUBLE PRECISION) AS TmdbPopularity,
                   CAST(vdp."CustomAverageRating" AS DOUBLE PRECISION) AS CustomAverageRating,
                   (SELECT STRING_AGG(d."Name", ',') FROM "MovieDirector" md JOIN "Directors" d ON md."DirectorsId" = d."Id" WHERE md."MoviesId" = vdp."Id") AS DirectorName,
                   (SELECT STRING_AGG(a."Name", ',') FROM "MovieActor" ma JOIN "Actors" a ON ma."ActorsId" = a."Id" WHERE ma."MoviesId" = vdp."Id") AS ActorNames,
                   (SELECT STRING_AGG(g."Name", ',') FROM "MovieGenre" mg JOIN "Genres" g ON mg."GenresId" = g."Id" WHERE mg."MoviesId" = vdp."Id") AS Genres,
                   vdp."Keywords" AS Keywords,
                   vdp."Overview" AS Overview,
                   vdp."Language" AS Language,
                   vdp."Country" AS Country
            FROM ValidDiscoveryPool vdp
            ORDER BY RANDOM()
        """;
    }

    public async Task<Guid?> GetUserIdByUsernameAsync(string username, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Guid?>(
            @"SELECT ""Id"" FROM ""Users"" WHERE ""Username"" = @username",
            new { username });
    }

    public async Task<IEnumerable<string>> GetAvailableCountriesAsync(CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var sql = @"
            SELECT DISTINCT trim(unnest(string_to_array(m.""Country"", ','))) AS CleanCountry
            FROM ""Movies"" m
            WHERE m.""Country"" IS NOT NULL AND m.""Country"" <> ''
            ORDER BY CleanCountry
        ";
        return (await connection.QueryAsync<string>(sql)).ToArray();
    }
    public async Task<IReadOnlyList<string>> GetUserTopGenresAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT g.""Name"" AS GenreName
            FROM ""WatchedMovies"" w
            JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            WHERE w.""UserId"" = @userId
            GROUP BY g.""Name""
            ORDER BY COUNT(DISTINCT w.""MovieId"") DESC, g.""Name""
        ";
        return (await connection.QueryAsync<string>(sql, new { userId })).ToList();
    }
}
