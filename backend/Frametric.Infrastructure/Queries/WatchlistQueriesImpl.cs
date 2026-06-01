using Dapper;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;

namespace Frametric.Infrastructure.Queries;

public class WatchlistQueriesImpl : IWatchlistBasicQueries, IWatchlistAdvancedStatsQueries, IWatchlistComplexCorrelationsQueries
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public WatchlistQueriesImpl(IDbConnectionFactory db) => _dbConnectionFactory = db;

    // --- Basic Queries ---
    public async Task<IEnumerable<MovieSimpleDto>> GetWatchlistByYearAsync(Guid userId, int releaseYear, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId AND m.""ReleaseYear"" = @releaseYear";
        return await connection.QueryAsync<MovieSimpleDto>(sql, new { userId, releaseYear });
    }

    public async Task<IEnumerable<DirectorCountDto>> GetWatchlistDirectorsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH UserDirectorRatings AS (
                SELECT dr.""Id"" AS DirectorId, AVG(mr.""Score"") AS AverageRating
                FROM ""MovieRatings"" mr
                JOIN ""MovieDirector"" md ON mr.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                WHERE mr.""UserId"" = @userId AND mr.""Score"" IS NOT NULL
                GROUP BY dr.""Id""
            ),
            WatchedDirectors AS (
                SELECT dr.""Id"" AS DirectorId, COUNT(DISTINCT w.""MovieId"") AS WatchedCount
                FROM (
                    SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                    UNION
                    SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
                ) w
                JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                GROUP BY dr.""Id""
            )
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(w.""Id"") AS INTEGER) AS Count, CAST(COALESCE(udr.AverageRating, 0.0) AS DOUBLE PRECISION) AS AverageRating, CAST(COALESCE(wd.WatchedCount, 0) AS INTEGER) AS WatchedCount
            FROM ""WatchlistItems"" w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            LEFT JOIN UserDirectorRatings udr ON dr.""Id"" = udr.DirectorId
            LEFT JOIN WatchedDirectors wd ON dr.""Id"" = wd.DirectorId
            WHERE w.""UserId"" = @userId
            GROUP BY dr.""Id"", dr.""Name"", udr.AverageRating, wd.WatchedCount
            ORDER BY Count DESC, dr.""Name""";
        return await connection.QueryAsync<DirectorCountDto>(sql, new { userId });
    }

    public async Task<IEnumerable<ActorCountDto>> GetWatchlistActorsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH UserActorRatings AS (
                SELECT a.""Id"" AS ActorId, AVG(mr.""Score"") AS AverageRating
                FROM ""MovieRatings"" mr
                JOIN ""MovieActor"" ma ON mr.""MovieId"" = ma.""MoviesId""
                JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
                WHERE mr.""UserId"" = @userId AND mr.""Score"" IS NOT NULL
                GROUP BY a.""Id""
            ),
            WatchedActors AS (
                SELECT a.""Id"" AS ActorId, COUNT(DISTINCT w.""MovieId"") AS WatchedCount
                FROM (
                    SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                    UNION
                    SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
                ) w
                JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
                JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
                GROUP BY a.""Id""
            )
            SELECT a.""Name"" AS ActorName, CAST(COUNT(w.""Id"") AS INTEGER) AS Count, CAST(COALESCE(uar.AverageRating, 0.0) AS DOUBLE PRECISION) AS AverageRating, CAST(COALESCE(wa.WatchedCount, 0) AS INTEGER) AS WatchedCount
            FROM ""WatchlistItems"" w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            LEFT JOIN UserActorRatings uar ON a.""Id"" = uar.ActorId
            LEFT JOIN WatchedActors wa ON a.""Id"" = wa.ActorId
            WHERE w.""UserId"" = @userId
            GROUP BY a.""Id"", a.""Name"", uar.AverageRating, wa.WatchedCount
            ORDER BY Count DESC, a.""Name""";
        return await connection.QueryAsync<ActorCountDto>(sql, new { userId });
    }

    public async Task<IEnumerable<GenreCountDto>> GetWatchlistByGenreAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH WatchedGenres AS (
                SELECT g.""Id"" AS GenreId, COUNT(DISTINCT w.""MovieId"") AS WatchedCount
                FROM ""WatchedMovies"" w
                JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
                JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
                WHERE w.""UserId"" = @userId
                GROUP BY g.""Id""
            )
            SELECT g.""Name"" AS GenreName, CAST(COUNT(w.""Id"") AS INTEGER) AS Count, CAST(COALESCE(wg.WatchedCount, 0) AS INTEGER) AS WatchedCount
            FROM ""WatchlistItems"" w
            JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            LEFT JOIN WatchedGenres wg ON g.""Id"" = wg.GenreId
            WHERE w.""UserId"" = @userId
            GROUP BY g.""Id"", g.""Name"", wg.WatchedCount
            ORDER BY Count DESC, g.""Name""";
        return await connection.QueryAsync<GenreCountDto>(sql, new { userId });
    }

    public async Task<IEnumerable<DecadeCountDto>> GetWatchlistByDecadeAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT CAST(FLOOR(m.""ReleaseYear"" / 10) * 10 AS INTEGER) AS Decade, CAST(COUNT(*) AS INTEGER) AS Count
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId AND m.""ReleaseYear"" IS NOT NULL
            GROUP BY Decade
            ORDER BY Decade ASC";
        return await connection.QueryAsync<DecadeCountDto>(sql, new { userId });
    }

    // --- Advanced Stats ---
    public async Task<DirectorCountDto?> GetMostAnticipatedDirectorAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH UserDirectorRatings AS (
                SELECT dr.""Id"" AS DirectorId, AVG(mr.""Score"") AS AverageRating
                FROM ""MovieRatings"" mr
                JOIN ""MovieDirector"" md ON mr.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                WHERE mr.""UserId"" = @userId AND mr.""Score"" IS NOT NULL
                GROUP BY dr.""Id""
            ),
            WatchedDirectors AS (
                SELECT dr.""Id"" AS DirectorId, COUNT(DISTINCT w.""MovieId"") AS WatchedCount
                FROM ""WatchedMovies"" w
                JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                WHERE w.""UserId"" = @userId
                GROUP BY dr.""Id""
            )
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(w.""Id"") AS INTEGER) AS Count, CAST(COALESCE(udr.AverageRating, 0.0) AS DOUBLE PRECISION) AS AverageRating, CAST(COALESCE(wd.WatchedCount, 0) AS INTEGER) AS WatchedCount
            FROM ""WatchlistItems"" w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            LEFT JOIN UserDirectorRatings udr ON dr.""Id"" = udr.DirectorId
            LEFT JOIN WatchedDirectors wd ON dr.""Id"" = wd.DirectorId
            WHERE w.""UserId"" = @userId
            GROUP BY dr.""Id"", dr.""Name"", udr.AverageRating, wd.WatchedCount
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<DirectorCountDto>(sql, new { userId });
    }

    public async Task<ActorCountDto?> GetMostAnticipatedActorAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH UserActorRatings AS (
                SELECT a.""Id"" AS ActorId, AVG(mr.""Score"") AS AverageRating
                FROM ""MovieRatings"" mr
                JOIN ""MovieActor"" ma ON mr.""MovieId"" = ma.""MoviesId""
                JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
                WHERE mr.""UserId"" = @userId AND mr.""Score"" IS NOT NULL
                GROUP BY a.""Id""
            ),
            WatchedActors AS (
                SELECT a.""Id"" AS ActorId, COUNT(DISTINCT w.""MovieId"") AS WatchedCount
                FROM ""WatchedMovies"" w
                JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
                JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
                WHERE w.""UserId"" = @userId
                GROUP BY a.""Id""
            )
            SELECT a.""Name"" AS ActorName, CAST(COUNT(w.""Id"") AS INTEGER) AS Count, CAST(COALESCE(uar.AverageRating, 0.0) AS DOUBLE PRECISION) AS AverageRating, CAST(COALESCE(wa.WatchedCount, 0) AS INTEGER) AS WatchedCount
            FROM ""WatchlistItems"" w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            LEFT JOIN UserActorRatings uar ON a.""Id"" = uar.ActorId
            LEFT JOIN WatchedActors wa ON a.""Id"" = wa.ActorId
            WHERE w.""UserId"" = @userId
            GROUP BY a.""Id"", a.""Name"", uar.AverageRating, wa.WatchedCount
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<ActorCountDto>(sql, new { userId });
    }

    public async Task<TimeInvestedDto?> GetTotalPendingWatchtimeAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                'Total Watchlist' AS Name,
                CAST(COALESCE(SUM(m.""RuntimeMinutes""), 0) AS INTEGER) AS TotalMinutes,
                CAST(COALESCE(SUM(m.""RuntimeMinutes"") / 60, 0) AS INTEGER) AS TotalHours
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId";
        return await connection.QuerySingleOrDefaultAsync<TimeInvestedDto>(sql, new { userId });
    }

    public async Task<MovieSimpleDto?> GetOldestPendingMovieAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId
            ORDER BY w.""DateAdded"" ASC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<MovieSimpleDto>(sql, new { userId });
    }

    public async Task<IEnumerable<GenreProportionDto>> GetGenreProportionWatchlistVsWatchedAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH WatchedGenres AS (
                SELECT g.""Name"" AS GenreName, CAST(COUNT(*) AS INTEGER) AS WatchedCount
                FROM ""WatchedMovies"" w
                JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
                JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
                WHERE w.""UserId"" = @userId
                GROUP BY g.""Name""
            ),
            WatchlistGenres AS (
                SELECT g.""Name"" AS GenreName, CAST(COUNT(*) AS INTEGER) AS PendingCount
                FROM ""WatchlistItems"" w
                JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
                JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
                WHERE w.""UserId"" = @userId
                GROUP BY g.""Name""
            )
            SELECT 
                COALESCE(w.GenreName, wl.GenreName) AS GenreName,
                COALESCE(w.WatchedCount, 0) AS WatchedCount,
                COALESCE(wl.PendingCount, 0) AS PendingCount
            FROM WatchedGenres w
            FULL OUTER JOIN WatchlistGenres wl ON w.GenreName = wl.GenreName
            ORDER BY (COALESCE(wl.PendingCount, 0) + COALESCE(w.WatchedCount, 0)) DESC";
        return await connection.QueryAsync<GenreProportionDto>(sql, new { userId });
    }

    // --- Complex Correlations ---
    public async Task<GoldenDirectorDto?> GetGoldenPendingDirectorAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH PendingDirectors AS (
                SELECT DISTINCT dr.""Id"", dr.""Name"", CAST(COUNT(*) AS INTEGER) AS PendingCount
                FROM ""WatchlistItems"" w
                JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                WHERE w.""UserId"" = @userId
                GROUP BY dr.""Id"", dr.""Name""
            ),
            WatchedDirectorRatings AS (
                SELECT dr.""Id"", AVG(mr.""Score"") AS AvgRating, CAST(COUNT(mr.""Id"") AS INTEGER) AS WatchedCount
                FROM ""MovieRatings"" mr
                JOIN ""MovieDirector"" md ON mr.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                WHERE mr.""UserId"" = @userId AND mr.""Score"" IS NOT NULL
                GROUP BY dr.""Id""
            )
            SELECT 
                p.""Name"" AS DirectorName, 
                CAST(COALESCE(w.AvgRating, 0) AS DOUBLE PRECISION) AS AverageRatingInHistory,
                CAST(p.PendingCount AS INTEGER) AS PendingMoviesCount
            FROM PendingDirectors p
            JOIN WatchedDirectorRatings w ON p.""Id"" = w.""Id""
            WHERE w.WatchedCount >= 2
            ORDER BY w.AvgRating DESC, p.PendingCount DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<GoldenDirectorDto>(sql, new { userId });
    }

    public async Task<IEnumerable<DurationBalanceDto>> GetPendingDurationBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                CASE 
                    WHEN m.""RuntimeMinutes"" < 90 THEN 'Short (< 90m)'
                    WHEN m.""RuntimeMinutes"" BETWEEN 90 AND 140 THEN 'Medium (90m - 140m)'
                    ELSE 'Long (> 140m)' 
                END AS DurationCategory,
                CAST(COUNT(*) AS INTEGER) AS Count
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId AND m.""RuntimeMinutes"" IS NOT NULL
            GROUP BY DurationCategory
            ORDER BY Count DESC";
        return await connection.QueryAsync<DurationBalanceDto>(sql, new { userId });
    }

    public async Task<IEnumerable<EraBreakdownDto>> GetWatchlistByEraAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                CASE 
                    WHEN m.""ReleaseYear"" < 1960 THEN 'Golden Age (<1960)'
                    WHEN m.""ReleaseYear"" BETWEEN 1960 AND 1989 THEN 'Classic (1960-1989)'
                    WHEN m.""ReleaseYear"" BETWEEN 1990 AND 2009 THEN 'Modern (1990-2009)'
                    ELSE 'Contemporary (2010+)'
                END AS EraName,
                CAST(COUNT(*) AS INTEGER) AS Count
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId AND m.""ReleaseYear"" IS NOT NULL
            GROUP BY EraName
            ORDER BY Count DESC";
        return await connection.QueryAsync<EraBreakdownDto>(sql, new { userId });
    }

    public async Task<GhostActorDto?> GetGhostActorAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH PendingActors AS (
                SELECT a.""Id"", a.""Name"", CAST(COUNT(*) AS INTEGER) AS PendingCount
                FROM ""WatchlistItems"" w
                JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
                JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
                WHERE w.""UserId"" = @userId
                GROUP BY a.""Id"", a.""Name""
            ),
            WatchedActors AS (
                SELECT DISTINCT a.""Id""
                FROM ""WatchedMovies"" w
                JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
                JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
                WHERE w.""UserId"" = @userId
            )
            SELECT 
                p.""Name"" AS ActorName, 
                CAST(p.PendingCount AS INTEGER) AS PendingMoviesCount
            FROM PendingActors p
            LEFT JOIN WatchedActors w ON p.""Id"" = w.""Id""
            WHERE w.""Id"" IS NULL
            ORDER BY p.PendingCount DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<GhostActorDto>(sql, new { userId });
    }
}

