// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Data;
using Dapper;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.DTOs.EntityDetails;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.EntityDetails;

namespace Frametric.Infrastructure.Queries;

public class EntityDetailsQueriesImpl : IEntityDetailsQueries
{
    private readonly IDbConnectionFactory _db;

    public EntityDetailsQueriesImpl(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<MovieDetailsDto?> GetMovieDetailsAsync(Guid userId, Guid movieId, CancellationToken cancellationToken)
    {
        using var connection = _db.CreateConnection();
        
        var sql = @"
            SELECT 
                m.""Id"", 
                m.""Title"", 
                m.""ReleaseYear"", 
                m.""RuntimeMinutes"", 
                m.""PosterUrl"", 
                m.""Overview"", 
                m.""TmdbRating"",
                (SELECT r.""Score"" FROM ""MovieRatings"" r WHERE r.""MovieId"" = m.""Id"" AND r.""UserId"" = @UserId LIMIT 1) AS ""UserAverageScore""
            FROM ""Movies"" m
            WHERE m.""Id"" = @MovieId;

            SELECT g.""Id"", g.""Name"" 
            FROM ""Genres"" g
            INNER JOIN ""MovieGenre"" gm ON gm.""GenresId"" = g.""Id""
            WHERE gm.""MoviesId"" = @MovieId;

            SELECT d.""Id"", d.""Name"" 
            FROM ""Directors"" d
            INNER JOIN ""MovieDirector"" dm ON dm.""DirectorsId"" = d.""Id""
            WHERE dm.""MoviesId"" = @MovieId;

            SELECT a.""Id"", a.""Name"" 
            FROM ""Actors"" a
            INNER JOIN ""MovieActor"" am ON am.""ActorsId"" = a.""Id""
            WHERE am.""MoviesId"" = @MovieId;

            SELECT 
                d.""Id"", 
                TO_CHAR(d.""WatchedDate"", 'YYYY-MM-DD') AS ""DateWatched"", 
                d.""IsRewatch"", 
                CAST(r.""Score"" AS DOUBLE PRECISION) AS ""Rating""
            FROM ""DiaryEntries"" d
            LEFT JOIN ""MovieRatings"" r ON r.""MovieId"" = d.""MovieId"" AND r.""UserId"" = d.""UserId""
            WHERE d.""MovieId"" = @MovieId AND d.""UserId"" = @UserId
            ORDER BY d.""WatchedDate"" DESC;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { UserId = userId, MovieId = movieId });
        
        var movieBase = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (movieBase == null) return null;

        var genres = await multi.ReadAsync<GenreSimpleDto>();
        var directors = await multi.ReadAsync<DirectorSimpleDto>();
        var actors = await multi.ReadAsync<ActorSimpleDto>();
        var diaryEntries = await multi.ReadAsync<MovieDiaryEntryDto>();

        return new MovieDetailsDto(
            (Guid)movieBase.Id,
            (string)movieBase.Title,
            (int?)movieBase.ReleaseYear,
            (int?)movieBase.RuntimeMinutes,
            (string?)movieBase.PosterUrl,
            (string?)movieBase.Overview,
            (double?)movieBase.TmdbRating,
            (double?)movieBase.UserAverageScore,
            genres,
            directors,
            actors,
            diaryEntries
        );
    }

    public async Task<ActorDetailsDto?> GetActorDetailsAsync(Guid userId, Guid actorId, CancellationToken cancellationToken)
    {
        using var connection = _db.CreateConnection();
        var sql = @"
            SELECT 
                a.""Id"", 
                a.""Name"",
                a.""ProfilePath"",
                CAST(COALESCE((
                    SELECT AVG(r.""Score"") 
                    FROM ""MovieActor"" am
                    INNER JOIN ""MovieRatings"" r ON r.""MovieId"" = am.""MoviesId""
                    WHERE am.""ActorsId"" = a.""Id"" AND r.""UserId"" = @UserId
                ), 0) AS DOUBLE PRECISION) AS ""AverageRating"",
                CAST((
                    SELECT COUNT(DISTINCT am.""MoviesId"")
                    FROM ""MovieActor"" am
                    INNER JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = am.""MoviesId""
                    WHERE am.""ActorsId"" = a.""Id"" AND wm.""UserId"" = @UserId
                ) AS INTEGER) AS ""WatchCount""
            FROM ""Actors"" a
            WHERE a.""Id"" = @ActorId;

            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieActor"" am ON am.""MoviesId"" = m.""Id""
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE am.""ActorsId"" = @ActorId
            ORDER BY m.""ReleaseYear"" DESC;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { UserId = userId, ActorId = actorId });
        var actorBase = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (actorBase == null) return null;

        var movies = await multi.ReadAsync<MovieSimpleDto>();

        return new ActorDetailsDto(
            (Guid)actorBase.Id,
            (string)actorBase.Name,
            (double)actorBase.AverageRating,
            (int)actorBase.WatchCount,
            movies,
            (string?)actorBase.ProfilePath
        );
    }

    public async Task<DirectorDetailsDto?> GetDirectorDetailsAsync(Guid userId, Guid directorId, CancellationToken cancellationToken)
    {
        using var connection = _db.CreateConnection();
        var sql = @"
            SELECT 
                d.""Id"", 
                d.""Name"",
                d.""ProfilePath"",
                CAST(COALESCE((
                    SELECT AVG(r.""Score"") 
                    FROM ""MovieDirector"" dm
                    INNER JOIN ""MovieRatings"" r ON r.""MovieId"" = dm.""MoviesId""
                    WHERE dm.""DirectorsId"" = d.""Id"" AND r.""UserId"" = @UserId
                ), 0) AS DOUBLE PRECISION) AS ""AverageRating"",
                CAST((
                    SELECT COUNT(DISTINCT dm.""MoviesId"")
                    FROM ""MovieDirector"" dm
                    INNER JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = dm.""MoviesId""
                    WHERE dm.""DirectorsId"" = d.""Id"" AND wm.""UserId"" = @UserId
                ) AS INTEGER) AS ""WatchCount""
            FROM ""Directors"" d
            WHERE d.""Id"" = @DirectorId;

            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieDirector"" dm ON dm.""MoviesId"" = m.""Id""
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE dm.""DirectorsId"" = @DirectorId
            ORDER BY m.""ReleaseYear"" DESC;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { UserId = userId, DirectorId = directorId });
        var dirBase = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (dirBase == null) return null;

        var movies = await multi.ReadAsync<MovieSimpleDto>();

        return new DirectorDetailsDto(
            (Guid)dirBase.Id,
            (string)dirBase.Name,
            (double)dirBase.AverageRating,
            (int)dirBase.WatchCount,
            movies,
            (string?)dirBase.ProfilePath
        );
    }

    public async Task<IEnumerable<GlobalSearchResultDto>> SearchEntitiesAsync(Guid userId, string query, CancellationToken cancellationToken)
    {
        using var connection = _db.CreateConnection();
        var searchPattern = $"%{query}%";

        var sql = @"
            SELECT 
                ""Id"" AS ""LocalId"", 
                CASE WHEN LOWER(""ExternalSource"") = 'tmdb' AND ""ExternalId"" ~ '^[0-9]+$' THEN CAST(""ExternalId"" AS INTEGER) ELSE NULL END AS ""TmdbId"", 
                'Movie' AS ""EntityType"", 
                ""Title"" AS ""TitleOrName"", 
                ""ReleaseYear"", 
                ""PosterUrl"" AS ""ImageUrl"", 
                true AS ""IsLocal""
            FROM ""Movies""
            WHERE ""Title"" ILIKE @SearchPattern
            UNION ALL
            SELECT ""Id"" AS ""LocalId"", ""TmdbId"", 'Actor' AS ""EntityType"", ""Name"" AS ""TitleOrName"", NULL AS ""ReleaseYear"", NULL AS ""ImageUrl"", true AS ""IsLocal""
            FROM ""Actors""
            WHERE ""Name"" ILIKE @SearchPattern
            UNION ALL
            SELECT ""Id"" AS ""LocalId"", ""TmdbId"", 'Director' AS ""EntityType"", ""Name"" AS ""TitleOrName"", NULL AS ""ReleaseYear"", NULL AS ""ImageUrl"", true AS ""IsLocal""
            FROM ""Directors""
            WHERE ""Name"" ILIKE @SearchPattern
            ORDER BY ""TitleOrName""
            LIMIT 20;
        ";

        return await connection.QueryAsync<GlobalSearchResultDto>(sql, new { SearchPattern = searchPattern });
    }
}
