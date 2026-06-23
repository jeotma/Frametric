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
    public async Task<IEnumerable<WatchedMovieStatsDto>> GetMoviesAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: true, isRatingsJoined: true, ratingAlias: "mr2");
        string sql = $@"
            SELECT m.""Title"", 
                   m.""ReleaseYear"", 
                   (SELECT STRING_AGG(dr.""Name"", ', ') FROM ""MovieDirector"" md JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id"" WHERE md.""MoviesId"" = m.""Id"") AS Director, 
                   CAST(COALESCE(MAX(mr2.""Score""), 0) AS DOUBLE PRECISION) AS Rating, 
                   CAST(CASE WHEN COUNT(ml.""Id"") > 0 THEN 1 ELSE 0 END AS BOOLEAN) AS Liked,
                   m.""CustomAverageRating"",
                   m.""PosterUrl""
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            LEFT JOIN ""MovieRatings"" mr2 ON m.""Id"" = mr2.""MovieId"" AND mr2.""UserId"" = @userId
            LEFT JOIN ""MovieLikes"" ml ON m.""Id"" = ml.""MovieId"" AND ml.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId
            {filterBuilder.BuildWhereClause()}
            GROUP BY w.""Id"", m.""Id"", m.""Title"", m.""ReleaseYear"", w.""Date""
            ORDER BY COALESCE((SELECT MIN(de.""WatchedDate"") FROM ""DiaryEntries"" de WHERE de.""MovieId"" = w.""MovieId"" AND de.""UserId"" = w.""UserId""), w.""Date"") DESC
            ";
        return await connection.QueryAsync<WatchedMovieStatsDto>(sql, parameters);
    }

    public async Task<IEnumerable<DirectorCountDto>> GetDirectorsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false, isRatingsJoined: true, ratingAlias: "mr");
        string sql = $@"
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE((AVG(mr.""Score"") * 2), 0) AS DOUBLE PRECISION) AS AverageRating,
                   CAST(0 AS INTEGER) AS WatchedCount, dr.""Id"" AS Id, dr.""ProfilePath"" AS ProfilePath
            FROM ""WatchedMovies"" w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId 
            
            {filterBuilder.BuildWhereClause()}
            GROUP BY dr.""Id"", dr.""Name"", dr.""ProfilePath""
            ORDER BY Count DESC, dr.""Name""";
        return await connection.QueryAsync<DirectorCountDto>(sql, parameters);
    }

    public async Task<IEnumerable<ActorCountDto>> GetActorsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false, isRatingsJoined: true, ratingAlias: "mr");
        string sql = $@"
            SELECT a.""Name"" AS ActorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE((AVG(mr.""Score"") * 2), 0) AS DOUBLE PRECISION) AS AverageRating,
                   CAST(0 AS INTEGER) AS WatchedCount, a.""Id"" AS Id, a.""ProfilePath"" AS ProfilePath
            FROM ""WatchedMovies"" w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            {filterBuilder.BuildJoins()}
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
            WHERE w.""UserId"" = @userId 
            {filterBuilder.BuildWhereClause()}
            GROUP BY a.""Id"", a.""Name"", a.""ProfilePath""
            ORDER BY Count DESC, a.""Name""";
        return await connection.QueryAsync<ActorCountDto>(sql, parameters);
    }

    public async Task<IEnumerable<GenreCountDto>> GetMoviesByGenreAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT g.""Name"" AS GenreName, CAST(COUNT(*) AS INTEGER) AS Count
            FROM ""WatchedMovies"" w
            JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId 
            {filterBuilder.BuildWhereClause()}
            GROUP BY g.""Name""
            ORDER BY Count DESC, g.""Name""";
        return await connection.QueryAsync<GenreCountDto>(sql, parameters);
    }

    public async Task<IEnumerable<DecadeCountDto>> GetMoviesByDecadeAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: true);
        string sql = $@"
            SELECT CAST(FLOOR(m.""ReleaseYear"" / 10) * 10 AS INTEGER) AS Decade, CAST(COUNT(*) AS INTEGER) AS Count
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId AND m.""ReleaseYear"" IS NOT NULL 
            {filterBuilder.BuildWhereClause()}
            GROUP BY Decade
            ORDER BY Decade ASC";
        return await connection.QueryAsync<DecadeCountDto>(sql, parameters);
    }

    // --- Advanced Stats ---
    public async Task<ActorCountDto?> GetMostRepeatedActorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT a.""Name"" AS ActorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE((AVG(mr.""Score"") * 2), 0) AS DOUBLE PRECISION) AS AverageRating,
                   CAST(0 AS INTEGER) AS WatchedCount, a.""Id"" AS Id, a.""ProfilePath"" AS ProfilePath
            FROM ""WatchedMovies"" w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId
            
            {filterBuilder.BuildWhereClause()}
            GROUP BY a.""Id"", a.""Name"", a.""ProfilePath""
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<ActorCountDto>(sql, parameters);
    }

    public async Task<DirectorCountDto?> GetMostWatchedDirectorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE((AVG(mr.""Score"") * 2), 0) AS DOUBLE PRECISION) AS AverageRating,
                   CAST(0 AS INTEGER) AS WatchedCount, dr.""Id"" AS Id, dr.""ProfilePath"" AS ProfilePath
            FROM ""WatchedMovies"" w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId
            
            {filterBuilder.BuildWhereClause()}
            GROUP BY dr.""Id"", dr.""Name"", dr.""ProfilePath""
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<DirectorCountDto>(sql, parameters);
    }

    public async Task<EraBreakdownDto?> GetPredominantEraAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT 
                CASE WHEN m.""ReleaseYear"" <= 1980 THEN 'Classic (Pre-1980)' ELSE 'Modern (Post-1980)' END AS EraName,
                CAST(COUNT(*) AS INTEGER) AS Count
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId  AND m.""ReleaseYear"" IS NOT NULL
            
            {filterBuilder.BuildWhereClause()}
            GROUP BY EraName
            ORDER BY Count DESC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<EraBreakdownDto>(sql, parameters);
    }

    public async Task<IEnumerable<DirectorLeaderboardDto>> GetDirectorRankingByRatingAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "mr", "DateRated", isMoviesJoined: false);
        string sql = $@"
            WITH DirectorStats AS (
                SELECT dr.""Id"" AS DirectorId, dr.""Name"" AS Name, CAST(COUNT(DISTINCT w.""MovieId"") AS INTEGER) AS WatchCount, CAST(COALESCE((AVG(mr.""Score"") * 2), 0) AS DOUBLE PRECISION) AS AverageRating, CAST(COALESCE(AVG(m.""CustomAverageRating""), 0) AS DOUBLE PRECISION) AS CustomAverageRating
                FROM ""WatchedMovies"" w
                JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
                LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = w.""UserId""
                
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId
                
            {filterBuilder.BuildWhereClause()}
            GROUP BY dr.""Id"", dr.""Name""
            ),
            HighestRated AS (
                SELECT DISTINCT ON (dr.""Id"")
                    dr.""Id"" AS DirectorId,
                    m.""Title"" AS HighestRatedMovieTitle
                FROM ""WatchedMovies"" w
                JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
                JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
                JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
                LEFT JOIN ""MovieRatings"" mr ON w.""MovieId"" = mr.""MovieId"" AND mr.""UserId"" = w.""UserId""
                WHERE w.""UserId"" = @userId
                ORDER BY dr.""Id"", mr.""Score"" DESC NULLS LAST, mr.""DateRated"" DESC NULLS LAST
            )
            SELECT ds.DirectorId, ds.Name, ds.WatchCount, ds.AverageRating, hr.HighestRatedMovieTitle, ds.CustomAverageRating
            FROM DirectorStats ds
            JOIN HighestRated hr ON ds.DirectorId = hr.DirectorId
            ORDER BY ds.AverageRating DESC, ds.WatchCount DESC";
        return await connection.QueryAsync<DirectorLeaderboardDto>(sql, parameters);
    }

    public async Task<TimeInvestedDto?> GetTotalTimeByDirectorOrGenreAsync(Guid userId, string filterType, string filterName, AnalyticsFilterDto filter, CancellationToken ct = default)
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
        else if (filterType.Equals("Actor", StringComparison.OrdinalIgnoreCase))
        {
            joinSql = @"JOIN ""MovieActor"" ma ON m.""Id"" = ma.""MoviesId"" JOIN ""Actors"" f ON ma.""ActorsId"" = f.""Id""";
            whereSql = @"f.""Name"" ILIKE @filterNamePattern";
        }
        else
        {
            throw new ArgumentException("FilterType must be Director, Genre, or Actor");
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
    public async Task<IEnumerable<PreferredDayDto>> GetPreferredWatchDayOfWeekAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "WatchDate", isMoviesJoined: false);
        string sql = $@"
            WITH AllWatched AS (
                SELECT ""MovieId"", CAST(""WatchedDate"" AS DATE) AS ""WatchDate""
                FROM ""DiaryEntries""
                WHERE ""UserId"" = @userId

                UNION ALL

                SELECT ""MovieId"", CAST(""Date"" AS DATE) AS ""WatchDate""
                FROM ""WatchedMovies"" w
                WHERE w.""UserId"" = @userId
                AND NOT EXISTS (
                    SELECT 1 FROM ""DiaryEntries"" d2
                    WHERE d2.""UserId"" = w.""UserId"" AND d2.""MovieId"" = w.""MovieId""
                )
            ),
            FilteredWatched AS (
                SELECT w.""MovieId"", w.""WatchDate""
                FROM AllWatched w
                {filterBuilder.BuildJoins()}
                WHERE 1=1 {filterBuilder.BuildWhereClause()}
            )
            SELECT TRIM(TO_CHAR(""WatchDate"", 'Day')) AS DayOfWeek, CAST(COUNT(*) AS INTEGER) AS Count
            FROM FilteredWatched
            GROUP BY DayOfWeek, EXTRACT(ISODOW FROM ""WatchDate"")
            ORDER BY EXTRACT(ISODOW FROM ""WatchDate"") ASC";
        return await connection.QueryAsync<PreferredDayDto>(sql, parameters);
    }

    public async Task<IEnumerable<GenreStreakDto>> GetGenreStreakAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "WatchDate", isMoviesJoined: false);
        string sql = $@"
            WITH AllWatched AS (
                SELECT ""MovieId"", CAST(""WatchedDate"" AS DATE) AS ""WatchDate""
                FROM ""DiaryEntries""
                WHERE ""UserId"" = @userId 

                UNION ALL

                SELECT ""MovieId"", CAST(""Date"" AS DATE) AS ""WatchDate""
                FROM ""WatchedMovies"" w
                WHERE w.""UserId"" = @userId 
                AND NOT EXISTS (
                    SELECT 1 FROM ""DiaryEntries"" d2
                    WHERE d2.""UserId"" = w.""UserId"" AND d2.""MovieId"" = w.""MovieId""
                )
            ),
            FilteredWatched AS (
                SELECT w.""MovieId"", w.""WatchDate""
                FROM AllWatched w
                {filterBuilder.BuildJoins()}
                WHERE 1=1 {filterBuilder.BuildWhereClause()}
            ),
            WatchedWithGenre AS (
                SELECT 
                    fw.""WatchDate"", 
                    g.""Name"" AS GenreName,
                    ROW_NUMBER() OVER (ORDER BY fw.""WatchDate"") as rn1,
                    ROW_NUMBER() OVER (PARTITION BY g.""Name"" ORDER BY fw.""WatchDate"") as rn2
                FROM FilteredWatched fw
                JOIN ""MovieGenre"" mg ON fw.""MovieId"" = mg.""MoviesId""
                JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            ),
            Streaks AS (
                SELECT 
                    GenreName, 
                    MIN(""WatchDate"") as StartDate, 
                    MAX(""WatchDate"") as EndDate, 
                    CAST(COUNT(*) AS INTEGER) AS StreakLength
                FROM WatchedWithGenre
                GROUP BY GenreName, rn1 - rn2
            ),
            MaxStreaks AS (
                SELECT GenreName, StreakLength, StartDate, EndDate,
                       ROW_NUMBER() OVER (PARTITION BY GenreName ORDER BY StreakLength DESC, StartDate DESC) as rank
                FROM Streaks
                WHERE StreakLength >= 2
            )
            SELECT GenreName, StreakLength, CAST(StartDate AS DATE), CAST(EndDate AS DATE)
            FROM MaxStreaks
            WHERE rank = 1
            ORDER BY StreakLength DESC
            LIMIT 10";
        return await connection.QueryAsync<GenreStreakDto>(sql, parameters);
    }

    public async Task<WrappedMovieDto?> GetLongestWatchedMovieAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS PosterPath,
                   m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS Rating
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId  AND m.""RuntimeMinutes"" IS NOT NULL
            
            {filterBuilder.BuildWhereClause()}
            ORDER BY m.""RuntimeMinutes"" DESC NULLS LAST
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<WrappedMovieDto>(sql, parameters);
    }

    public async Task<WrappedMovieDto?> GetShortestWatchedMovieAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS PosterPath,
                   m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS Rating
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId  AND m.""RuntimeMinutes"" IS NOT NULL AND m.""RuntimeMinutes"" > 0
            
            {filterBuilder.BuildWhereClause()}
            ORDER BY m.""RuntimeMinutes"" ASC
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<WrappedMovieDto>(sql, parameters);
    }

    public async Task<IEnumerable<DirectorActorPairDto>> GetDirectorActorPairingsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "d", "WatchedDate", isMoviesJoined: false);
        string sql = $@"
            SELECT 
                dr.""Name"" AS DirectorName,
                a.""Name"" AS ActorName,
                CAST(COUNT(d.""Id"") AS INTEGER) AS CollaborationCount,
                dr.""ProfilePath"" AS DirectorProfilePath,
                a.""ProfilePath"" AS ActorProfilePath
            FROM ""DiaryEntries"" d
            JOIN ""MovieDirector"" md ON d.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            JOIN ""MovieActor"" ma ON d.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            
            {filterBuilder.BuildJoins()}
            WHERE d.""UserId"" = @userId 
            
            {filterBuilder.BuildWhereClause()}
            GROUP BY dr.""Name"", a.""Name"", dr.""ProfilePath"", a.""ProfilePath""
            HAVING COUNT(d.""Id"") >= 2
            ORDER BY CollaborationCount DESC
            LIMIT 10";
        return await connection.QueryAsync<DirectorActorPairDto>(sql, parameters);
    }

    public async Task<PrimeTimeStatsDto?> GetPrimeTimeStatsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "WatchDate", isMoviesJoined: false);
        string sql = $@"
            WITH AllWatched AS (
                SELECT ""MovieId"", CAST(""WatchedDate"" AS DATE) AS ""WatchDate""
                FROM ""DiaryEntries""
                WHERE ""UserId"" = @userId

                UNION ALL

                SELECT ""MovieId"", CAST(""Date"" AS DATE) AS ""WatchDate""
                FROM ""WatchedMovies"" w
                WHERE w.""UserId"" = @userId
                AND NOT EXISTS (
                    SELECT 1 FROM ""DiaryEntries"" d2
                    WHERE d2.""UserId"" = w.""UserId"" AND d2.""MovieId"" = w.""MovieId""
                )
            ),
            FilteredWatched AS (
                SELECT w.""MovieId"", w.""WatchDate""
                FROM AllWatched w
                {filterBuilder.BuildJoins()}
                WHERE 1=1 {filterBuilder.BuildWhereClause()}
            ),
            ByDay AS (
                SELECT TRIM(TO_CHAR(""WatchDate"", 'Day')) AS DayName, EXTRACT(ISODOW FROM ""WatchDate"") AS DowNum, CAST(COUNT(*) AS INTEGER) AS Cnt
                FROM FilteredWatched
                GROUP BY DayName, DowNum
            ),
            ByMonth AS (
                SELECT TO_CHAR(""WatchDate"", 'Month') AS MonthName, EXTRACT(MONTH FROM ""WatchDate"") AS MonthNum, CAST(COUNT(*) AS INTEGER) AS Cnt
                FROM FilteredWatched
                GROUP BY MonthName, MonthNum
            )
            SELECT
                (SELECT TRIM(DayName) FROM ByDay ORDER BY Cnt DESC LIMIT 1) AS PeakDay,
                (SELECT Cnt FROM ByDay ORDER BY Cnt DESC LIMIT 1) AS PeakDayCount,
                (SELECT TRIM(MonthName) FROM ByMonth ORDER BY Cnt DESC LIMIT 1) AS PeakMonth,
                (SELECT Cnt FROM ByMonth ORDER BY Cnt DESC LIMIT 1) AS PeakMonthCount,
                (SELECT TRIM(DayName) FROM ByDay ORDER BY Cnt ASC LIMIT 1) AS SlumpDay,
                (SELECT Cnt FROM ByDay ORDER BY Cnt ASC LIMIT 1) AS SlumpDayCount,
                (SELECT TRIM(MonthName) FROM ByMonth ORDER BY Cnt ASC LIMIT 1) AS SlumpMonth,
                (SELECT Cnt FROM ByMonth ORDER BY Cnt ASC LIMIT 1) AS SlumpMonthCount";
        return await connection.QuerySingleOrDefaultAsync<PrimeTimeStatsDto>(sql, parameters);
    }

    public async Task<IEnumerable<GenreWithRatingDto>> GetGenresWithRatingAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT 
                g.""Name"" AS GenreName,
                CAST(COUNT(DISTINCT w.""MovieId"") AS INTEGER) AS Count,
                CAST(COALESCE((AVG(mr.""Score"") * 2), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM ""WatchedMovies"" w
            JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = w.""MovieId"" AND mr.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId 
            
            {filterBuilder.BuildWhereClause()}
            GROUP BY g.""Name""
            ORDER BY Count DESC, g.""Name""";
        return await connection.QueryAsync<GenreWithRatingDto>(sql, parameters);
    }

    public async Task<IEnumerable<RatingEvolutionDto>> GetRatingEvolutionAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "\"MovieRatings\"", "DateRated", isMoviesJoined: false);
        string sql = $@"
            SELECT CAST(EXTRACT(MONTH FROM ""DateRated"") AS INTEGER) AS Month, MIN(TO_CHAR(""DateRated"", 'Month')) AS MonthName, CAST((AVG(""Score"") * 2) AS DOUBLE PRECISION) AS AverageRating
            FROM ""MovieRatings""
            
            {filterBuilder.BuildJoins()}
            WHERE ""UserId"" = @userId  AND ""Score"" IS NOT NULL
            
            {filterBuilder.BuildWhereClause()}
            GROUP BY Month
            ORDER BY Month ASC";
        return await connection.QueryAsync<RatingEvolutionDto>(sql, parameters);
    }

    public async Task<IEnumerable<CastingPairDto>> GetCastingRepetitionsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "WatchedDate", isMoviesJoined: false);
        string sql = $@"
            WITH AllWatched AS (
                SELECT ""MovieId""
                FROM ""DiaryEntries""
                WHERE ""UserId"" = @userId

                UNION ALL

                SELECT ""MovieId""
                FROM ""WatchedMovies"" wm
                WHERE wm.""UserId"" = @userId
                AND NOT EXISTS (
                    SELECT 1 FROM ""DiaryEntries"" d2
                    WHERE d2.""UserId"" = wm.""UserId"" AND d2.""MovieId"" = wm.""MovieId""
                )
            ),
            WatchedActors AS (
                SELECT w.""MovieId"", ma.""ActorsId""
                FROM AllWatched w
                JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
                {filterBuilder.BuildJoins()}
                WHERE 1=1 {filterBuilder.BuildWhereClause()}
            )
            SELECT 
                a1.""Name"" AS Actor1Name, 
                a2.""Name"" AS Actor2Name, 
                CAST(COUNT(*) AS INTEGER) AS CollaborationCount,
                a1.""ProfilePath"" AS Actor1ProfilePath,
                a2.""ProfilePath"" AS Actor2ProfilePath
            FROM WatchedActors w1
            JOIN WatchedActors w2 ON w1.""MovieId"" = w2.""MovieId"" AND w1.""ActorsId"" < w2.""ActorsId""
            JOIN ""Actors"" a1 ON w1.""ActorsId"" = a1.""Id""
            JOIN ""Actors"" a2 ON w2.""ActorsId"" = a2.""Id""
            
            GROUP BY a1.""Name"", a2.""Name"", a1.""ProfilePath"", a2.""ProfilePath""
            HAVING COUNT(*) >= 2
            ORDER BY CollaborationCount DESC
            LIMIT 10";
        return await connection.QueryAsync<CastingPairDto>(sql, parameters);
    }
}



