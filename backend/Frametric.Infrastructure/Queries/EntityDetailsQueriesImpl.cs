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

    private static T? SafeCast<T>(object value) where T : struct
    {
        if (value == null || Convert.IsDBNull(value))
            return null;
        return (T)Convert.ChangeType(value, typeof(T));
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
                (SELECT r.""Score"" * 2 FROM ""MovieRatings"" r WHERE r.""MovieId"" = m.""Id"" AND r.""UserId"" = @UserId LIMIT 1) AS ""UserAverageScore"",
                CASE WHEN EXISTS (SELECT 1 FROM ""WatchedMovies"" w WHERE w.""MovieId"" = m.""Id"" AND w.""UserId"" = @UserId) THEN true ELSE false END AS ""IsWatched""
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
                CAST(r.""Score"" * 2 AS DOUBLE PRECISION) AS ""Rating""
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
            SafeCast<double>(movieBase.TmdbRating),
            SafeCast<double>(movieBase.UserAverageScore),
            genres,
            directors,
            actors,
            diaryEntries,
            (bool)movieBase.IsWatched
        );
    }

    public async Task<ActorDetailsDto?> GetActorDetailsAsync(Guid userId, Guid actorId, CancellationToken cancellationToken)
    {
        using var connection = _db.CreateConnection();
        var sql = @"
            -- 1. Base actor details and check if director exists with same TmdbId
            SELECT 
                a.""Id"", 
                a.""Name"",
                a.""ProfilePath"",
                a.""TmdbId"",
                (SELECT d.""Id"" FROM ""Directors"" d WHERE d.""TmdbId"" = a.""TmdbId"" AND a.""TmdbId"" IS NOT NULL AND a.""TmdbId"" > 0 LIMIT 1) AS ""DirectorId""
            FROM ""Actors"" a
            WHERE a.""Id"" = @ActorId;

            -- 2. Acting movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieActor"" am ON am.""MoviesId"" = m.""Id""
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE am.""ActorsId"" = @ActorId
            ORDER BY m.""ReleaseYear"" DESC;

            -- 3. Directed movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieDirector"" dm ON dm.""MoviesId"" = m.""Id""
            INNER JOIN ""Directors"" d ON d.""Id"" = dm.""DirectorsId""
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE d.""TmdbId"" = (SELECT a2.""TmdbId"" FROM ""Actors"" a2 WHERE a2.""Id"" = @ActorId) AND d.""TmdbId"" IS NOT NULL AND d.""TmdbId"" > 0
            ORDER BY m.""ReleaseYear"" DESC;

            -- 4. Combined average rating
            SELECT CAST(COALESCE(AVG(r.""Score""), 0) * 2 AS DOUBLE PRECISION)
            FROM ""MovieRatings"" r
            WHERE r.""UserId"" = @UserId AND (
                r.""MovieId"" IN (SELECT am.""MoviesId"" FROM ""MovieActor"" am WHERE am.""ActorsId"" = @ActorId)
                OR
                r.""MovieId"" IN (
                    SELECT dm.""MoviesId"" 
                    FROM ""MovieDirector"" dm
                    INNER JOIN ""Directors"" d ON d.""Id"" = dm.""DirectorsId""
                    WHERE d.""TmdbId"" = (SELECT a2.""TmdbId"" FROM ""Actors"" a2 WHERE a2.""Id"" = @ActorId) AND d.""TmdbId"" IS NOT NULL AND d.""TmdbId"" > 0
                )
            );

            -- 5. Combined watch count
            SELECT CAST(COUNT(DISTINCT m.""Id"") AS INTEGER)
            FROM ""Movies"" m
            INNER JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE 
                m.""Id"" IN (SELECT am.""MoviesId"" FROM ""MovieActor"" am WHERE am.""ActorsId"" = @ActorId)
                OR
                m.""Id"" IN (
                    SELECT dm.""MoviesId"" 
                    FROM ""MovieDirector"" dm
                    INNER JOIN ""Directors"" d ON d.""Id"" = dm.""DirectorsId""
                    WHERE d.""TmdbId"" = (SELECT a2.""TmdbId"" FROM ""Actors"" a2 WHERE a2.""Id"" = @ActorId) AND d.""TmdbId"" IS NOT NULL AND d.""TmdbId"" > 0
                );

            -- 6. Combined watchlist movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""WatchlistItems"" wl ON wl.""MovieId"" = m.""Id"" AND wl.""UserId"" = @UserId
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE 
                m.""Id"" IN (SELECT am.""MoviesId"" FROM ""MovieActor"" am WHERE am.""ActorsId"" = @ActorId)
                OR
                m.""Id"" IN (
                    SELECT dm.""MoviesId"" 
                    FROM ""MovieDirector"" dm
                    INNER JOIN ""Directors"" d ON d.""Id"" = dm.""DirectorsId""
                    WHERE d.""TmdbId"" = (SELECT a2.""TmdbId"" FROM ""Actors"" a2 WHERE a2.""Id"" = @ActorId) AND d.""TmdbId"" IS NOT NULL AND d.""TmdbId"" > 0
                )
            ORDER BY m.""Title"";

            -- 7. Combined liked movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieLikes"" l ON l.""MovieId"" = m.""Id"" AND l.""UserId"" = @UserId
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE 
                m.""Id"" IN (SELECT am.""MoviesId"" FROM ""MovieActor"" am WHERE am.""ActorsId"" = @ActorId)
                OR
                m.""Id"" IN (
                    SELECT dm.""MoviesId"" 
                    FROM ""MovieDirector"" dm
                    INNER JOIN ""Directors"" d ON d.""Id"" = dm.""DirectorsId""
                    WHERE d.""TmdbId"" = (SELECT a2.""TmdbId"" FROM ""Actors"" a2 WHERE a2.""Id"" = @ActorId) AND d.""TmdbId"" IS NOT NULL AND d.""TmdbId"" > 0
                )
            ORDER BY m.""Title"";
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { UserId = userId, ActorId = actorId });
        var actorBase = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (actorBase == null) return null;

        var actingMovies = await multi.ReadAsync<MovieSimpleDto>();
        var directedMovies = await multi.ReadAsync<MovieSimpleDto>();
        var avgRating = await multi.ReadSingleOrDefaultAsync<double>();
        var watchCount = await multi.ReadSingleOrDefaultAsync<int>();
        var watchlistMovies = await multi.ReadAsync<MovieSimpleDto>();
        var watchlistList = watchlistMovies.ToList();
        var likedMovies = await multi.ReadAsync<MovieSimpleDto>();
        var likedList = likedMovies.ToList();

        var isDirector = actorBase.DirectorId != null;

        return new ActorDetailsDto(
            (Guid)actorBase.Id,
            (string)actorBase.Name,
            avgRating,
            watchCount,
            actingMovies,
            (string?)actorBase.ProfilePath,
            isDirector,
            isDirector ? directedMovies.ToList() : null,
            watchlistList.Count,
            likedList.Count,
            likedList,
            watchlistList
        );
    }

    public async Task<DirectorDetailsDto?> GetDirectorDetailsAsync(Guid userId, Guid directorId, CancellationToken cancellationToken)
    {
        using var connection = _db.CreateConnection();
        var sql = @"
            -- 1. Base director details and check if actor exists with same TmdbId
            SELECT 
                d.""Id"", 
                d.""Name"",
                d.""ProfilePath"",
                d.""TmdbId"",
                (SELECT a.""Id"" FROM ""Actors"" a WHERE a.""TmdbId"" = d.""TmdbId"" AND d.""TmdbId"" IS NOT NULL AND d.""TmdbId"" > 0 LIMIT 1) AS ""ActorId""
            FROM ""Directors"" d
            WHERE d.""Id"" = @DirectorId;

            -- 2. Directing movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieDirector"" dm ON dm.""MoviesId"" = m.""Id""
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE dm.""DirectorsId"" = @DirectorId
            ORDER BY m.""ReleaseYear"" DESC;

            -- 3. Acting movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieActor"" am ON am.""MoviesId"" = m.""Id""
            INNER JOIN ""Actors"" a ON a.""Id"" = am.""ActorsId""
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE a.""TmdbId"" = (SELECT d2.""TmdbId"" FROM ""Directors"" d2 WHERE d2.""Id"" = @DirectorId) AND a.""TmdbId"" IS NOT NULL AND a.""TmdbId"" > 0
            ORDER BY m.""ReleaseYear"" DESC;

            -- 4. Combined average rating
            SELECT CAST(COALESCE(AVG(r.""Score""), 0) * 2 AS DOUBLE PRECISION)
            FROM ""MovieRatings"" r
            WHERE r.""UserId"" = @UserId AND (
                r.""MovieId"" IN (SELECT dm.""MoviesId"" FROM ""MovieDirector"" dm WHERE dm.""DirectorsId"" = @DirectorId)
                OR
                r.""MovieId"" IN (
                    SELECT am.""MoviesId"" 
                    FROM ""MovieActor"" am
                    INNER JOIN ""Actors"" a ON a.""Id"" = am.""ActorsId""
                    WHERE a.""TmdbId"" = (SELECT d2.""TmdbId"" FROM ""Directors"" d2 WHERE d2.""Id"" = @DirectorId) AND a.""TmdbId"" IS NOT NULL AND a.""TmdbId"" > 0
                )
            );

            -- 5. Combined watch count
            SELECT CAST(COUNT(DISTINCT m.""Id"") AS INTEGER)
            FROM ""Movies"" m
            INNER JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE 
                m.""Id"" IN (SELECT dm.""MoviesId"" FROM ""MovieDirector"" dm WHERE dm.""DirectorsId"" = @DirectorId)
                OR
                m.""Id"" IN (
                    SELECT am.""MoviesId"" 
                    FROM ""MovieActor"" am
                    INNER JOIN ""Actors"" a ON a.""Id"" = am.""ActorsId""
                    WHERE a.""TmdbId"" = (SELECT d2.""TmdbId"" FROM ""Directors"" d2 WHERE d2.""Id"" = @DirectorId) AND a.""TmdbId"" IS NOT NULL AND a.""TmdbId"" > 0
                );

            -- 6. Combined watchlist movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""WatchlistItems"" wl ON wl.""MovieId"" = m.""Id"" AND wl.""UserId"" = @UserId
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE 
                m.""Id"" IN (SELECT dm.""MoviesId"" FROM ""MovieDirector"" dm WHERE dm.""DirectorsId"" = @DirectorId)
                OR
                m.""Id"" IN (
                    SELECT am.""MoviesId"" 
                    FROM ""MovieActor"" am
                    INNER JOIN ""Actors"" a ON a.""Id"" = am.""ActorsId""
                    WHERE a.""TmdbId"" = (SELECT d2.""TmdbId"" FROM ""Directors"" d2 WHERE d2.""Id"" = @DirectorId) AND a.""TmdbId"" IS NOT NULL AND a.""TmdbId"" > 0
                )
            ORDER BY m.""Title"";

            -- 7. Combined liked movies
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                   CASE WHEN wm.""MovieId"" IS NOT NULL THEN true ELSE false END AS ""IsWatched""
            FROM ""Movies"" m
            INNER JOIN ""MovieLikes"" l ON l.""MovieId"" = m.""Id"" AND l.""UserId"" = @UserId
            LEFT JOIN ""WatchedMovies"" wm ON wm.""MovieId"" = m.""Id"" AND wm.""UserId"" = @UserId
            WHERE 
                m.""Id"" IN (SELECT dm.""MoviesId"" FROM ""MovieDirector"" dm WHERE dm.""DirectorsId"" = @DirectorId)
                OR
                m.""Id"" IN (
                    SELECT am.""MoviesId"" 
                    FROM ""MovieActor"" am
                    INNER JOIN ""Actors"" a ON a.""Id"" = am.""ActorsId""
                    WHERE a.""TmdbId"" = (SELECT d2.""TmdbId"" FROM ""Directors"" d2 WHERE d2.""Id"" = @DirectorId) AND a.""TmdbId"" IS NOT NULL AND a.""TmdbId"" > 0
                )
            ORDER BY m.""Title"";
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { UserId = userId, DirectorId = directorId });
        var dirBase = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (dirBase == null) return null;

        var directingMovies = await multi.ReadAsync<MovieSimpleDto>();
        var actingMovies = await multi.ReadAsync<MovieSimpleDto>();
        var avgRating = await multi.ReadSingleOrDefaultAsync<double>();
        var watchCount = await multi.ReadSingleOrDefaultAsync<int>();
        var watchlistMovies = await multi.ReadAsync<MovieSimpleDto>();
        var watchlistList = watchlistMovies.ToList();
        var likedMovies = await multi.ReadAsync<MovieSimpleDto>();
        var likedList = likedMovies.ToList();

        var isActor = dirBase.ActorId != null;

        return new DirectorDetailsDto(
            (Guid)dirBase.Id,
            (string)dirBase.Name,
            avgRating,
            watchCount,
            directingMovies,
            (string?)dirBase.ProfilePath,
            isActor,
            isActor ? actingMovies.ToList() : null,
            watchlistList.Count,
            likedList.Count,
            likedList,
            watchlistList
        );
    }

    public async Task<IEnumerable<GlobalSearchResultDto>> SearchEntitiesAsync(Guid userId, string query, CancellationToken cancellationToken)
    {
        using var connection = _db.CreateConnection();
        var searchPattern = $"%{query}%";

        var sql = @"
            WITH RawPeople AS (
                SELECT 
                    ""Id"" AS ""ActorId"", 
                    NULL::uuid AS ""DirectorId"",
                    ""TmdbId"",
                    ""Name"",
                    ""ProfilePath"",
                    'Actor' AS ""Role""
                FROM ""Actors""
                WHERE ""Name"" ILIKE @SearchPattern
                UNION ALL
                SELECT 
                    NULL::uuid AS ""ActorId"", 
                    ""Id"" AS ""DirectorId"",
                    ""TmdbId"",
                    ""Name"",
                    ""ProfilePath"",
                    'Director' AS ""Role""
                FROM ""Directors""
                WHERE ""Name"" ILIKE @SearchPattern
            ),
            GroupedPeople AS (
                SELECT
                    COALESCE(NULLIF(""TmdbId"", 0)::text, LOWER(""Name"")) AS ""GroupKey"",
                    MAX(""ActorId""::text)::uuid AS ""ActorId"",
                    MAX(""DirectorId""::text)::uuid AS ""DirectorId"",
                    MAX(""TmdbId"") AS ""TmdbId"",
                    MAX(""Name"") AS ""Name"",
                    MAX(""ProfilePath"") AS ""ProfilePath""
                FROM RawPeople
                GROUP BY COALESCE(NULLIF(""TmdbId"", 0)::text, LOWER(""Name""))
            ),
            CombinedPeople AS (
                SELECT
                    COALESCE(""ActorId"", ""DirectorId"") AS ""LocalId"",
                    ""TmdbId"",
                    CASE 
                        WHEN ""ActorId"" IS NOT NULL AND ""DirectorId"" IS NOT NULL THEN 'Director / Actor'
                        WHEN ""ActorId"" IS NOT NULL THEN 'Actor'
                        ELSE 'Director'
                    END AS ""EntityType"",
                    ""Name"" AS ""TitleOrName"",
                    NULL::integer AS ""ReleaseYear"",
                    ""ProfilePath"" AS ""ImageUrl"",
                    true AS ""IsLocal"",
                    ""ActorId"",
                    ""DirectorId""
                FROM GroupedPeople
            )
            SELECT 
                ""Id"" AS ""LocalId"", 
                CASE WHEN LOWER(""ExternalSource"") = 'tmdb' AND ""ExternalId"" ~ '^[0-9]+$' THEN CAST(""ExternalId"" AS INTEGER) ELSE NULL END AS ""TmdbId"", 
                'Movie' AS ""EntityType"", 
                ""Title"" AS ""TitleOrName"", 
                ""ReleaseYear"", 
                ""PosterUrl"" AS ""ImageUrl"", 
                true AS ""IsLocal"",
                NULL::uuid AS ""ActorId"",
                NULL::uuid AS ""DirectorId""
            FROM ""Movies""
            WHERE ""Title"" ILIKE @SearchPattern
            UNION ALL
            SELECT 
                ""LocalId"",
                ""TmdbId"",
                ""EntityType"",
                ""TitleOrName"",
                ""ReleaseYear"",
                ""ImageUrl"",
                ""IsLocal"",
                ""ActorId"",
                ""DirectorId""
            FROM CombinedPeople
            ORDER BY ""TitleOrName""
            LIMIT 20;
        ";

        return await connection.QueryAsync<GlobalSearchResultDto>(sql, new { SearchPattern = searchPattern });
    }
}
