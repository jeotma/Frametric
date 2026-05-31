using Dapper;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;

namespace Frametric.Infrastructure.Queries;

public class WatchedQueriesImpl : IWatchedBasicQueries, IWatchedAdvancedStatsQueries, IWatchedComplexCorrelationsQueries
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public WatchedQueriesImpl(IDbConnectionFactory db) => _dbConnectionFactory = db;

    // --- Basic Queries ---
    public async Task<IEnumerable<MovieSimpleDto>> GetMoviesByReleaseYearAsync(Guid userId, int releaseYear, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterPath""
            FROM AllWatched w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE m.""ReleaseYear"" = @releaseYear";
        return await connection.QueryAsync<MovieSimpleDto>(sql, new { userId, releaseYear });
    }

    public async Task<IEnumerable<DirectorCountDto>> GetDirectorsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT dr.""Name"" AS DirectorName, COUNT(*) AS Count, 0.0 AS AverageRating
            FROM AllWatched w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            GROUP BY dr.""Name""
            ORDER BY Count DESC, dr.""Name""";
        return await connection.QueryAsync<DirectorCountDto>(sql, new { userId });
    }

    public async Task<IEnumerable<ActorCountDto>> GetActorsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT a.""Name"" AS ActorName, COUNT(*) AS Count, 0.0 AS AverageRating
            FROM AllWatched w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            GROUP BY a.""Name""
            ORDER BY Count DESC, a.""Name""";
        return await connection.QueryAsync<ActorCountDto>(sql, new { userId });
    }

    public async Task<IEnumerable<GenreCountDto>> GetMoviesByGenreAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT g.""Name"" AS GenreName, COUNT(*) AS Count
            FROM AllWatched w
            JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            GROUP BY g.""Name""
            ORDER BY Count DESC, g.""Name""";
        return await connection.QueryAsync<GenreCountDto>(sql, new { userId });
    }

    public async Task<IEnumerable<DecadeCountDto>> GetMoviesByDecadeAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT CAST(FLOOR(m.""ReleaseYear"" / 10) * 10 AS INTEGER) AS Decade, COUNT(*) AS Count
            FROM AllWatched w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE m.""ReleaseYear"" IS NOT NULL
            GROUP BY Decade
            ORDER BY Decade ASC";
        return await connection.QueryAsync<DecadeCountDto>(sql, new { userId });
    }

    // --- Advanced Stats ---
    public async Task<ActorCountDto?> GetMostRepeatedActorAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT a.""Name"" AS ActorName, COUNT(*) AS Count, 0.0 AS AverageRating
            FROM AllWatched w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            GROUP BY a.""Name""
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<ActorCountDto>(sql, new { userId });
    }

    public async Task<DirectorCountDto?> GetMostWatchedDirectorAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT dr.""Name"" AS DirectorName, COUNT(*) AS Count, 0.0 AS AverageRating
            FROM AllWatched w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            GROUP BY dr.""Name""
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<DirectorCountDto>(sql, new { userId });
    }

    public async Task<EraBreakdownDto?> GetPredominantEraAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT 
                CASE WHEN m.""ReleaseYear"" <= 1980 THEN 'Classic (Pre-1980)' ELSE 'Modern (Post-1980)' END AS EraName,
                COUNT(*) AS Count
            FROM AllWatched w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE m.""ReleaseYear"" IS NOT NULL
            GROUP BY EraName
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<EraBreakdownDto>(sql, new { userId });
    }

    public async Task<IEnumerable<DirectorLeaderboardDto>> GetDirectorRankingByRatingAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT dr.""Id"" AS DirectorId, dr.""Name"" AS Name, COUNT(*) AS WatchCount, COALESCE(AVG(d.""Rating""), 0) AS AverageRating
            FROM ""DiaryEntries"" d
            JOIN ""MovieDirector"" md ON d.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            WHERE d.""UserId"" = @userId AND d.""Rating"" IS NOT NULL
            GROUP BY dr.""Id"", dr.""Name""
            HAVING COUNT(*) >= 2
            ORDER BY AverageRating DESC, WatchCount DESC
            LIMIT 20";
        return await connection.QueryAsync<DirectorLeaderboardDto>(sql, new { userId });
    }

    public async Task<TimeInvestedDto?> GetTotalTimeByDirectorOrGenreAsync(Guid userId, string filterType, string filterName, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        string joinSql = "";
        string whereSql = "";
        
        if (filterType.Equals("Director", StringComparison.OrdinalIgnoreCase))
        {
            joinSql = @"JOIN ""MovieDirector"" md ON m.""Id"" = md.""MoviesId"" JOIN ""Directors"" f ON md.""DirectorsId"" = f.""Id""";
            whereSql = @"f.""Name"" ILIKE @filterNamePattern";
        }
        else if (filterType.Equals("Genre", StringComparison.OrdinalIgnoreCase))
        {
            joinSql = @"JOIN ""MovieGenre"" mg ON m.""Id"" = mg.""MoviesId"" JOIN ""Genres"" f ON mg.""GenresId"" = f.""Id""";
            whereSql = @"f.""Name"" ILIKE @filterNamePattern";
        }
        else
        {
            throw new ArgumentException("FilterType must be Director or Genre");
        }

        string sql = $@"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT 
                CAST(COALESCE(SUM(m.""RuntimeMinutes""), 0) AS INTEGER) AS TotalMinutes,
                CAST(COALESCE(SUM(m.""RuntimeMinutes"") / 60, 0) AS INTEGER) AS TotalHours,
                @filterName AS Name
            FROM AllWatched w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            {joinSql}
            WHERE {whereSql}";

        return await connection.QuerySingleOrDefaultAsync<TimeInvestedDto>(sql, new { userId, filterNamePattern = $"%{filterName}%", filterName });
    }

    // --- Complex Correlations ---
    public async Task<IEnumerable<PreferredDayDto>> GetPreferredWatchDayOfWeekAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""WatchedDate"" AS WatchDate FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION ALL
                SELECT ""Date"" AS WatchDate FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT TRIM(TO_CHAR(WatchDate, 'Day')) AS DayOfWeek, COUNT(*) AS WatchCount
            FROM AllWatched
            GROUP BY DayOfWeek, EXTRACT(ISODOW FROM WatchDate)
            ORDER BY EXTRACT(ISODOW FROM WatchDate) ASC";
        return await connection.QueryAsync<PreferredDayDto>(sql, new { userId });
    }

    public async Task<IEnumerable<GenreStreakDto>> GetGenreStreakAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH WatchedWithGenre AS (
                SELECT 
                    w.WatchDate, 
                    g.""Name"" AS GenreName,
                    ROW_NUMBER() OVER (ORDER BY w.WatchDate) as rn1,
                    ROW_NUMBER() OVER (PARTITION BY g.""Name"" ORDER BY w.WatchDate) as rn2
                FROM (
                    SELECT ""MovieId"", ""WatchedDate"" AS WatchDate FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                    UNION
                    SELECT ""MovieId"", ""Date"" AS WatchDate FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
                ) w
                JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
                JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            ),
            Streaks AS (
                SELECT 
                    GenreName, 
                    MIN(WatchDate) as StartDate, 
                    MAX(WatchDate) as EndDate, 
                    COUNT(*) as StreakLength
                FROM WatchedWithGenre
                GROUP BY GenreName, rn1 - rn2
            )
            SELECT GenreName, StreakLength, CAST(StartDate AS DATE), CAST(EndDate AS DATE)
            FROM Streaks
            WHERE StreakLength >= 3
            ORDER BY StreakLength DESC
            LIMIT 10";
        return await connection.QueryAsync<GenreStreakDto>(sql, new { userId });
    }

    public async Task<MovieSimpleDto?> GetLongestWatchedMovieAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            )
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterPath""
            FROM AllWatched w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            ORDER BY m.""RuntimeMinutes"" DESC NULLS LAST
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<MovieSimpleDto>(sql, new { userId });
    }

    public async Task<IEnumerable<RatingEvolutionDto>> GetRatingEvolutionAsync(Guid userId, int year, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT CAST(EXTRACT(MONTH FROM ""WatchedDate"") AS INTEGER) AS Month, CAST(AVG(""Rating"") AS DOUBLE PRECISION) AS AverageRating
            FROM ""DiaryEntries""
            WHERE ""UserId"" = @userId AND EXTRACT(YEAR FROM ""WatchedDate"") = @year AND ""Rating"" IS NOT NULL
            GROUP BY Month
            ORDER BY Month ASC";
        return await connection.QueryAsync<RatingEvolutionDto>(sql, new { userId, year });
    }

    public async Task<IEnumerable<CastingPairDto>> GetCastingRepetitionsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""MovieId"" FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION
                SELECT ""MovieId"" FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            ),
            WatchedActors AS (
                SELECT w.""MovieId"", ma.""ActorsId""
                FROM AllWatched w
                JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            )
            SELECT 
                a1.""Name"" AS Actor1Name, 
                a2.""Name"" AS Actor2Name, 
                COUNT(*) AS CollaborationCount
            FROM WatchedActors w1
            JOIN WatchedActors w2 ON w1.""MovieId"" = w2.""MovieId"" AND w1.""ActorsId"" < w2.""ActorsId""
            JOIN ""Actors"" a1 ON w1.""ActorsId"" = a1.""Id""
            JOIN ""Actors"" a2 ON w2.""ActorsId"" = a2.""Id""
            GROUP BY a1.""Name"", a2.""Name""
            HAVING COUNT(*) >= 2
            ORDER BY CollaborationCount DESC
            LIMIT 10";
        return await connection.QueryAsync<CastingPairDto>(sql, new { userId });
    }
}
