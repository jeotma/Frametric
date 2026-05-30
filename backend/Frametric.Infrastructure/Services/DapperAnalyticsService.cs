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

    public async Task<WrappedSummaryDto> GetWrappedSummaryAsync(Guid userId, int year, CancellationToken cancellationToken)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new { userId, year };

        // 1. Total Watchtime
        const string watchtimeSql = @"
            SELECT COALESCE(SUM(m.""RuntimeMinutes""), 0)
            FROM ""DiaryEntries"" d
            JOIN ""Movies"" m ON d.""MovieId"" = m.""Id""
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year";
        var totalWatchtime = await connection.ExecuteScalarAsync<int>(watchtimeSql, parameters);

        // 2. Total Watches
        const string totalWatchesSql = @"
            SELECT COUNT(*)
            FROM ""DiaryEntries"" d
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year";
        var totalWatches = await connection.ExecuteScalarAsync<int>(totalWatchesSql, parameters);

        // 3. Unique Movies Count
        const string uniqueMoviesSql = @"
            SELECT COUNT(DISTINCT d.""MovieId"")
            FROM ""DiaryEntries"" d
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year";
        var uniqueMovies = await connection.ExecuteScalarAsync<int>(uniqueMoviesSql, parameters);

        // 4. Top Genres
        const string topGenresSql = @"
            SELECT g.""Name"" AS GenreName, COUNT(*) AS Count
            FROM ""DiaryEntries"" d
            JOIN ""MovieGenre"" mg ON d.""MovieId"" = mg.""MoviesId""
            JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year
            GROUP BY g.""Name""
            ORDER BY Count DESC, g.""Name""
            LIMIT 5";
        var topGenres = (await connection.QueryAsync<GenreCountDto>(topGenresSql, parameters)).ToList();

        // 5. Top Directors
        const string topDirectorsSql = @"
            SELECT dr.""Name"" AS DirectorName, COUNT(*) AS Count, COALESCE(AVG(d.""Rating""), 0) AS AverageRating
            FROM ""DiaryEntries"" d
            JOIN ""MovieDirector"" md ON d.""MovieId"" = md.""MoviesId""
            JOIN ""Directors"" dr ON md.""DirectorsId"" = dr.""Id""
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year
            GROUP BY dr.""Name""
            ORDER BY Count DESC, AverageRating DESC
            LIMIT 5";
        var topDirectors = (await connection.QueryAsync<DirectorCountDto>(topDirectorsSql, parameters)).ToList();

        // 6. Top Actors
        const string topActorsSql = @"
            SELECT a.""Name"" AS ActorName, COUNT(*) AS Count, COALESCE(AVG(d.""Rating""), 0) AS AverageRating
            FROM ""DiaryEntries"" d
            JOIN ""MovieActor"" ma ON d.""MovieId"" = ma.""MoviesId""
            JOIN ""Actors"" a ON ma.""ActorsId"" = a.""Id""
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year
            GROUP BY a.""Name""
            ORDER BY Count DESC, AverageRating DESC
            LIMIT 5";
        var topActors = (await connection.QueryAsync<ActorCountDto>(topActorsSql, parameters)).ToList();

        // 7. Decade Breakdown
        const string decadeSql = @"
            SELECT CAST(FLOOR(m.""ReleaseYear"" / 10) * 10 AS INTEGER) AS Decade, COUNT(*) AS Count
            FROM ""DiaryEntries"" d
            JOIN ""Movies"" m ON d.""MovieId"" = m.""Id""
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year AND m.""ReleaseYear"" IS NOT NULL
            GROUP BY Decade
            ORDER BY Decade ASC";
        var decadeBreakdown = (await connection.QueryAsync<DecadeCountDto>(decadeSql, parameters)).ToList();

        // 8. Monthly Activity
        const string monthlySql = @"
            SELECT CAST(EXTRACT(MONTH FROM d.""WatchedDate"") AS INTEGER) AS Month, COUNT(*) AS Count
            FROM ""DiaryEntries"" d
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year
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

        const string monthlySql = @"
            SELECT CAST(EXTRACT(MONTH FROM d.""WatchedDate"") AS INTEGER) AS Month, COUNT(*) AS Count
            FROM ""DiaryEntries"" d
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year
            GROUP BY Month
            ORDER BY Month ASC";
        var monthlyList = (await connection.QueryAsync<MonthlyWatchesDto>(monthlySql, parameters)).ToList();

        const string weeklySql = @"
            SELECT TRIM(TO_CHAR(d.""WatchedDate"", 'Day')) AS DayOfWeek, COUNT(*) AS Count,
                   CAST(EXTRACT(ISODOW FROM d.""WatchedDate"") AS INTEGER) AS DayIndex
            FROM ""DiaryEntries"" d
            WHERE d.""UserId"" = @userId AND EXTRACT(YEAR FROM d.""WatchedDate"") = @year
            GROUP BY DayOfWeek, DayIndex
            ORDER BY DayIndex ASC";
        var weeklyList = (await connection.QueryAsync<dynamic>(weeklySql, parameters))
            .Select(x => new WeeklyWatchesDto((string)x.dayofweek, (long)x.count))
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
            SELECT dr.""Id"" AS DirectorId, dr.""Name"" AS Name, COUNT(*) AS WatchCount, COALESCE(AVG(d.""Rating""), 0) AS AverageRating
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
