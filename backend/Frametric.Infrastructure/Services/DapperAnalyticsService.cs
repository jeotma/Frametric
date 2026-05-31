using System.Data;
using Dapper;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;

namespace Frametric.Infrastructure.Services;

public class DapperAnalyticsService : IAnalyticsService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DapperAnalyticsService(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new { userId };

        // 1. Total Watchtime
        const string watchtimeSql = @"
            SELECT CAST(COALESCE(SUM(m.""RuntimeMinutes"" * GREATEST(1, 
                (SELECT COUNT(*) FROM ""DiaryEntries"" de WHERE de.""MovieId"" = w.""MovieId"" AND de.""UserId"" = @userId)
            )), 0) AS INTEGER)
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId";
        var totalWatchtime = await connection.ExecuteScalarAsync<int>(watchtimeSql, parameters);

        // 2. Total Watches
        const string totalWatchesSql = @"
            SELECT CAST(COALESCE(SUM(GREATEST(1, 
                (SELECT COUNT(*) FROM ""DiaryEntries"" de WHERE de.""MovieId"" = w.""MovieId"" AND de.""UserId"" = @userId)
            )), 0) AS INTEGER)
            FROM ""WatchedMovies"" w
            WHERE w.""UserId"" = @userId";
        var totalWatches = await connection.ExecuteScalarAsync<int>(totalWatchesSql, parameters);

        // 3. Unique Movies Count
        const string uniqueMoviesSql = @"
            SELECT CAST(COUNT(DISTINCT w.""MovieId"") AS INTEGER)
            FROM ""WatchedMovies"" w
            WHERE w.""UserId"" = @userId";
        var uniqueMovies = await connection.ExecuteScalarAsync<int>(uniqueMoviesSql, parameters);

        // 4. Top Genres
        const string topGenresSql = @"
            SELECT g.""Name"" AS GenreName, CAST(COUNT(DISTINCT w.""MovieId"") AS INTEGER) AS Count
            FROM ""WatchedMovies"" w
            JOIN ""MovieGenre"" mg ON w.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            WHERE w.""UserId"" = @userId
            GROUP BY g.""Name""
            ORDER BY Count DESC, g.""Name""
            LIMIT 5";
        var topGenres = (await connection.QueryAsync<GenreCountDto>(topGenresSql, parameters)).ToList();

        // 5. Top Directors
        const string topDirectorsSql = @"
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(DISTINCT w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE(AVG((SELECT ""Score"" FROM ""MovieRatings"" mr WHERE mr.""MovieId"" = w.""MovieId"" AND mr.""UserId"" = @userId LIMIT 1)), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM ""WatchedMovies"" w
            JOIN ""MovieDirector"" md ON w.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            WHERE w.""UserId"" = @userId
            GROUP BY dr.""Name""
            ORDER BY Count DESC, AverageRating DESC
            LIMIT 5";
        var topDirectors = (await connection.QueryAsync<DirectorCountDto>(topDirectorsSql, parameters)).ToList();

        // 6. Top Actors
        const string topActorsSql = @"
            SELECT a.""Name"" AS ActorName, CAST(COUNT(DISTINCT w.""MovieId"") AS INTEGER) AS Count, CAST(COALESCE(AVG((SELECT ""Score"" FROM ""MovieRatings"" mr WHERE mr.""MovieId"" = w.""MovieId"" AND mr.""UserId"" = @userId LIMIT 1)), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM ""WatchedMovies"" w
            JOIN ""MovieActor"" ma ON w.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            WHERE w.""UserId"" = @userId
            GROUP BY a.""Name""
            ORDER BY Count DESC, AverageRating DESC
            LIMIT 5";
        var topActors = (await connection.QueryAsync<ActorCountDto>(topActorsSql, parameters)).ToList();

        // 7. Decade Breakdown
        const string decadeSql = @"
            SELECT CAST(FLOOR(m.""ReleaseYear"" / 10) * 10 AS INTEGER) AS Decade, CAST(COUNT(DISTINCT w.""MovieId"") AS INTEGER) AS Count
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId AND m.""ReleaseYear"" IS NOT NULL
            GROUP BY Decade
            ORDER BY Decade ASC";
        var decadeBreakdown = (await connection.QueryAsync<DecadeCountDto>(decadeSql, parameters)).ToList();

        return new DashboardSummaryDto(
            totalWatchtime,
            totalWatches,
            uniqueMovies,
            topGenres,
            topDirectors,
            topActors,
            decadeBreakdown
        );
    }

    public async Task<WrappedSummaryDto> GetWrappedSummaryAsync(Guid userId, int year, CancellationToken cancellationToken)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new { userId, year };

        string yearlyWatchesCte = @"
            WITH YearlyWatches AS (
                SELECT d.""MovieId"", d.""WatchedDate"" AS WatchDate, d.""Rating""
                FROM ""DiaryEntries"" d
                WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year

                UNION ALL

                SELECT w.""MovieId"", w.""Date"" AS WatchDate, NULL AS Rating
                FROM ""WatchedMovies"" w
                WHERE w.""UserId"" = @userId AND EXTRACT(YEAR FROM w.""Date"") = @year
                AND NOT EXISTS (
                    SELECT 1 FROM ""DiaryEntries"" d2
                    WHERE d2.""UserId"" = w.""UserId"" AND d2.""MovieId"" = w.""MovieId"" 
                    AND EXTRACT(YEAR FROM d2.""WatchedDate"") = @year
                )
            )
        ";

        // 1. Total Watchtime
        string watchtimeSql = yearlyWatchesCte + @"
            SELECT COALESCE(SUM(CAST(m.""RuntimeMinutes"" AS BIGINT)), 0)
            FROM YearlyWatches yw
            JOIN ""Movies"" m ON yw.""MovieId"" = m.""Id""";
        var totalWatchtime = await connection.ExecuteScalarAsync<int>(watchtimeSql, parameters);

        // 2. Total Watches
        string totalWatchesSql = yearlyWatchesCte + @"
            SELECT COUNT(*)
            FROM YearlyWatches yw";
        var totalWatches = await connection.ExecuteScalarAsync<int>(totalWatchesSql, parameters);

        // 3. Unique Movies Count
        string uniqueMoviesSql = yearlyWatchesCte + @"
            SELECT COUNT(DISTINCT yw.""MovieId"")
            FROM YearlyWatches yw";
        var uniqueMovies = await connection.ExecuteScalarAsync<int>(uniqueMoviesSql, parameters);

        // 4. Top Genres
        string topGenresSql = yearlyWatchesCte + @"
            SELECT g.""Name"" AS GenreName, CAST(COUNT(*) AS INTEGER) AS Count
            FROM YearlyWatches yw
            JOIN ""MovieGenre"" mg ON yw.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            GROUP BY g.""Name""
            ORDER BY Count DESC, g.""Name""
            LIMIT 5";
        var topGenres = (await connection.QueryAsync<GenreCountDto>(topGenresSql, parameters)).ToList();

        // 5. Top Directors
        string topDirectorsSql = yearlyWatchesCte + @"
            SELECT dr.""Name"" AS DirectorName, CAST(COUNT(*) AS INTEGER) AS Count, CAST(COALESCE(AVG(yw.""Rating""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM YearlyWatches yw
            JOIN ""MovieDirector"" md ON yw.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            GROUP BY dr.""Name""
            ORDER BY Count DESC, AverageRating DESC
            LIMIT 5";
        var topDirectors = (await connection.QueryAsync<DirectorCountDto>(topDirectorsSql, parameters)).ToList();

        // 6. Top Actors
        string topActorsSql = yearlyWatchesCte + @"
            SELECT a.""Name"" AS ActorName, CAST(COUNT(*) AS INTEGER) AS Count, CAST(COALESCE(AVG(yw.""Rating""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM YearlyWatches yw
            JOIN ""MovieActor"" ma ON yw.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            GROUP BY a.""Name""
            ORDER BY Count DESC, AverageRating DESC
            LIMIT 5";
        var topActors = (await connection.QueryAsync<ActorCountDto>(topActorsSql, parameters)).ToList();

        // 7. Decade Breakdown
        string decadeSql = yearlyWatchesCte + @"
            SELECT CAST(FLOOR(m.""ReleaseYear"" / 10) * 10 AS INTEGER) AS Decade, CAST(COUNT(*) AS INTEGER) AS Count
            FROM YearlyWatches yw
            JOIN ""Movies"" m ON yw.""MovieId"" = m.""Id""
            WHERE m.""ReleaseYear"" IS NOT NULL
            GROUP BY Decade
            ORDER BY Decade ASC";
        var decadeBreakdown = (await connection.QueryAsync<DecadeCountDto>(decadeSql, parameters)).ToList();

        // 8. Monthly Activity
        string monthlySql = yearlyWatchesCte + @"
            SELECT CAST(EXTRACT(MONTH FROM yw.WatchDate) AS INTEGER) AS Month, CAST(COUNT(*) AS INTEGER) AS Count
            FROM YearlyWatches yw
            GROUP BY Month
            ORDER BY Month ASC";
        var monthlyActivity = (await connection.QueryAsync<MonthlyActivityDto>(monthlySql, parameters)).ToList();

        // Ensure all 12 months are represented in the output list for the UI
        var fullMonthlyActivity = Enumerable.Range(1, 12)
            .Select(m => monthlyActivity.FirstOrDefault(ma => ma.Month == m) ?? new MonthlyActivityDto(m, 0))
            .ToList();

        return new WrappedSummaryDto(
            year,
            totalWatchtime,
            totalWatches,
            uniqueMovies,
            topGenres,
            topDirectors,
            topActors,
            decadeBreakdown,
            fullMonthlyActivity
        );
    }

    public async Task<MonthlyActivityResponseDto> GetMonthlyActivityAsync(Guid userId, int year, CancellationToken cancellationToken)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new { userId, year };

        string yearlyWatchesCte = @"
            WITH YearlyWatches AS (
                SELECT d.""MovieId"", d.""WatchedDate"" AS WatchDate
                FROM ""DiaryEntries"" d
                WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year

                UNION ALL

                SELECT w.""MovieId"", w.""Date"" AS WatchDate
                FROM ""WatchedMovies"" w
                WHERE w.""UserId"" = @userId AND EXTRACT(YEAR FROM w.""Date"") = @year
                AND NOT EXISTS (
                    SELECT 1 FROM ""DiaryEntries"" d2
                    WHERE d2.""UserId"" = w.""UserId"" AND d2.""MovieId"" = w.""MovieId"" 
                    AND EXTRACT(YEAR FROM d2.""WatchedDate"") = @year
                )
            )
        ";

        string monthlySql = yearlyWatchesCte + @"
            SELECT CAST(EXTRACT(MONTH FROM yw.WatchDate) AS INTEGER) AS Month, CAST(COUNT(*) AS INTEGER) AS Count
            FROM YearlyWatches yw
            GROUP BY Month
            ORDER BY Month ASC";
        var monthlyList = (await connection.QueryAsync<MonthlyWatchesDto>(monthlySql, parameters)).ToList();

        string weeklySql = yearlyWatchesCte + @"
            SELECT TRIM(TO_CHAR(yw.WatchDate, 'Day')) AS DayOfWeek, CAST(COUNT(*) AS INTEGER) AS Count,
                   CAST(EXTRACT(ISODOW FROM yw.WatchDate) AS INTEGER) AS DayIndex
            FROM YearlyWatches yw
            GROUP BY DayOfWeek, DayIndex
            ORDER BY DayIndex ASC";
        var weeklyList = (await connection.QueryAsync<dynamic>(weeklySql, parameters))
            .Select(x => new WeeklyWatchesDto((string)x.dayofweek, (int)(long)x.count))
            .ToList();

        // Fill in missing months
        var fullMonthly = Enumerable.Range(1, 12)
            .Select(m => monthlyList.FirstOrDefault(ml => ml.Month == m) ?? new MonthlyWatchesDto(m, 0))
            .ToList();

        return new MonthlyActivityResponseDto(fullMonthly, weeklyList);
    }

    public async Task<List<DirectorLeaderboardDto>> GetTopDirectorsAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new { userId, limit };

        const string sql = @"
            SELECT dr.""Id"" AS DirectorId, dr.""Name"" AS Name, CAST(COUNT(*) AS INTEGER) AS WatchCount, COALESCE(AVG(d.""Rating""), 0) AS AverageRating
            FROM ""DiaryEntries"" d
            JOIN ""MovieDirector"" md ON d.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            WHERE d.""UserId"" = @userId
            GROUP BY dr.""Id"", dr.""Name""
            ORDER BY WatchCount DESC, AverageRating DESC
            LIMIT @limit";

        return (await connection.QueryAsync<DirectorLeaderboardDto>(sql, parameters)).ToList();
    }
}
