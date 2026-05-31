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
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
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
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE(AVG(mr.""Score""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM AllWatched w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
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
            SELECT a.""Name"" AS ActorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE(AVG(mr.""Score""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM AllWatched w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
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
            SELECT g.""Name"" AS GenreName, CAST(COUNT(*) AS INTEGER) AS Count
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
            SELECT CAST(FLOOR(m.""ReleaseYear"" / 10) * 10 AS INTEGER) AS Decade, CAST(COUNT(*) AS INTEGER) AS Count
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
            SELECT a.""Name"" AS ActorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE(AVG(mr.""Score""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM AllWatched w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
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
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE(AVG(mr.""Score""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM AllWatched w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
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
                CAST(COUNT(*) AS INTEGER) AS Count
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
            WITH DirectorStats AS (
                SELECT dr.""Id"" AS DirectorId, dr.""Name"" AS Name, CAST(COUNT(mr.""Id"") AS INTEGER) AS WatchCount, CAST(COALESCE(AVG(mr.""Score""), 0) AS DOUBLE PRECISION) AS AverageRating
                FROM ""MovieRatings"" mr
                JOIN ""MovieDirector"" md ON mr.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                WHERE mr.""UserId"" = @userId AND mr.""Score"" IS NOT NULL
                GROUP BY dr.""Id"", dr.""Name""
                HAVING COUNT(mr.""Id"") >= 2
            ),
            HighestRated AS (
                SELECT DISTINCT ON (dr.""Id"")
                    dr.""Id"" AS DirectorId,
                    m.""Title"" AS HighestRatedMovieTitle
                FROM ""MovieRatings"" mr
                JOIN ""MovieDirector"" md ON mr.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                JOIN ""Movies"" m ON mr.""MovieId"" = m.""Id""
                WHERE mr.""UserId"" = @userId AND mr.""Score"" IS NOT NULL
                ORDER BY dr.""Id"", mr.""Score"" DESC, mr.""DateRated"" DESC
            )
            SELECT ds.DirectorId, ds.Name, ds.WatchCount, ds.AverageRating, hr.HighestRatedMovieTitle
            FROM DirectorStats ds
            JOIN HighestRated hr ON ds.DirectorId = hr.DirectorId
            ORDER BY ds.AverageRating DESC, ds.WatchCount DESC
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
            SELECT 
                @filterName AS Name,
                CAST(COALESCE(SUM(m.""RuntimeMinutes"" * GREATEST(1, 
                    (SELECT COUNT(*) FROM ""DiaryEntries"" de WHERE de.""MovieId"" = w.""MovieId"" AND de.""UserId"" = @userId)
                )), 0) AS INTEGER) AS TotalMinutes,
                CAST(COALESCE(SUM(m.""RuntimeMinutes"" * GREATEST(1, 
                    (SELECT COUNT(*) FROM ""DiaryEntries"" de WHERE de.""MovieId"" = w.""MovieId"" AND de.""UserId"" = @userId)
                )) / 60, 0) AS INTEGER) AS TotalHours
            FROM ""WatchedMovies"" w
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
            SELECT TRIM(TO_CHAR(WatchDate, 'Day')) AS DayOfWeek, CAST(COUNT(*) AS INTEGER) AS WatchCount
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
                    CAST(COUNT(*) AS INTEGER) AS StreakLength
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
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
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
            SELECT CAST(EXTRACT(MONTH FROM ""DateRated"") AS INTEGER) AS Month, CAST(AVG(""Score"") AS DOUBLE PRECISION) AS AverageRating
            FROM ""MovieRatings""
            WHERE ""UserId"" = @userId AND EXTRACT(YEAR FROM ""DateRated"") = @year AND ""Score"" IS NOT NULL
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
                CAST(COUNT(*) AS INTEGER) AS CollaborationCount
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


