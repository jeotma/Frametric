using Dapper;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;

namespace Frametric.Infrastructure.Queries;

public class BonusQueriesImpl : IBonusQueries
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public BonusQueriesImpl(IDbConnectionFactory db) => _dbConnectionFactory = db;

    public async Task<WeekendWarriorDto?> GetWeekendWarriorStatsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH AllWatched AS (
                SELECT ""WatchedDate"" AS WatchDate FROM ""DiaryEntries"" WHERE ""UserId"" = @userId
                UNION ALL
                SELECT ""Date"" AS WatchDate FROM ""WatchedMovies"" WHERE ""UserId"" = @userId
            ),
            Categorized AS (
                SELECT 
                    CASE WHEN EXTRACT(ISODOW FROM WatchDate) >= 6 THEN 'Weekend' ELSE 'Weekday' END AS DayType,
                    CAST(COUNT(*) AS INTEGER) AS Count
                FROM AllWatched
                GROUP BY DayType
            )
            SELECT 
                COALESCE(MAX(CASE WHEN DayType = 'Weekend' THEN Count END), 0) AS WeekendWatches,
                COALESCE(MAX(CASE WHEN DayType = 'Weekday' THEN Count END), 0) AS WeekdayWatches
            FROM Categorized";
        return await connection.QuerySingleOrDefaultAsync<WeekendWarriorDto>(sql, new { userId });
    }

    public async Task<IEnumerable<MovieSimpleDto>> GetHiddenGemsAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
            FROM ""DiaryEntries"" d
            JOIN ""Movies"" m ON d.""MovieId"" = m.""Id""
            WHERE d.""UserId"" = @userId AND d.""Rating"" >= 4.5 AND m.""ReleaseYear"" < EXTRACT(YEAR FROM CURRENT_DATE) - 30
            ORDER BY d.""Rating"" DESC, m.""ReleaseYear"" ASC
            LIMIT 5";
        return await connection.QueryAsync<MovieSimpleDto>(sql, new { userId });
    }

    public async Task<IEnumerable<MovieSimpleDto>> GetWatchlistGraveyardAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            WHERE w.""UserId"" = @userId
            ORDER BY w.""DateAdded"" ASC
            LIMIT 10";
        return await connection.QueryAsync<MovieSimpleDto>(sql, new { userId });
    }

    public async Task<CinematicFatigueDto?> GetCinematicFatigueAsync(Guid userId, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            WITH DailyCounts AS (
                SELECT ""WatchedDate"", CAST(COUNT(*) AS INTEGER) AS MoviesWatched, AVG(""Rating"") AS DailyAvgRating
                FROM ""DiaryEntries""
                WHERE ""UserId"" = @userId AND ""Rating"" IS NOT NULL
                GROUP BY ""WatchedDate""
            )
            SELECT 
                CAST(COALESCE(AVG(CASE WHEN MoviesWatched <= 1 THEN DailyAvgRating END), 0) AS DOUBLE PRECISION) AS AvgRatingLightDays,
                CAST(COALESCE(AVG(CASE WHEN MoviesWatched >= 3 THEN DailyAvgRating END), 0) AS DOUBLE PRECISION) AS AvgRatingHeavyDays
            FROM DailyCounts";
        return await connection.QuerySingleOrDefaultAsync<CinematicFatigueDto>(sql, new { userId });
    }
}
