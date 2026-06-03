using Dapper;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;

namespace Frametric.Infrastructure.Queries;

public class BonusQueriesImpl : IBonusQueries
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public BonusQueriesImpl(IDbConnectionFactory db) => _dbConnectionFactory = db;

    public async Task<WeekendWarriorDto?> GetWeekendWarriorStatsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
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
            Categorized AS (
                SELECT 
                    CASE WHEN EXTRACT(ISODOW FROM ""WatchDate"") >= 6 THEN 'Weekend' ELSE 'Weekday' END AS DayType,
                    CAST(COUNT(*) AS INTEGER) AS Count
                FROM FilteredWatched
                GROUP BY DayType
            )
            SELECT 
                COALESCE(MAX(CASE WHEN DayType = 'Weekend' THEN Count END), 0) AS WeekendWatches,
                COALESCE(MAX(CASE WHEN DayType = 'Weekday' THEN Count END), 0) AS WeekdayWatches
            FROM Categorized";
        return await connection.QuerySingleOrDefaultAsync<WeekendWarriorDto>(sql, parameters);
    }

    public async Task<IEnumerable<MovieSimpleDto>> GetHiddenGemsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
            FROM ""WatchedMovies"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId  AND mr.""Score"" >= 4.5 AND m.""ReleaseYear"" < EXTRACT(YEAR FROM CURRENT_DATE) - 30
            
            {filterBuilder.BuildWhereClause()}
            ORDER BY mr.""Score"" DESC, m.""ReleaseYear"" ASC
            LIMIT 5";
        return await connection.QueryAsync<MovieSimpleDto>(sql, parameters);
    }

    public async Task<IEnumerable<MovieSimpleDto>> GetWatchlistGraveyardAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "Date", isMoviesJoined: false);
        string sql = $@"
            SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath""
            FROM ""WatchlistItems"" w
            JOIN ""Movies"" m ON w.""MovieId"" = m.""Id""
            
            {filterBuilder.BuildJoins()}
            WHERE w.""UserId"" = @userId
            
            {filterBuilder.BuildWhereClause()}
            ORDER BY w.""DateAdded"" ASC
            LIMIT 10";
        return await connection.QueryAsync<MovieSimpleDto>(sql, parameters);
    }

    public async Task<CinematicFatigueExpandedDto?> GetCinematicFatigueExpandedAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
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
            DailyCounts AS (
                SELECT fw.""WatchDate"", 
                       CAST(COUNT(DISTINCT fw.""MovieId"") AS INTEGER) AS MoviesWatched, 
                       CAST(AVG(mr.""Score"") AS DOUBLE PRECISION) AS DailyAvgRating
                FROM FilteredWatched fw
                LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = fw.""MovieId"" AND mr.""UserId"" = @userId
                GROUP BY fw.""WatchDate""
            ),
            ByDay AS (
                SELECT TRIM(TO_CHAR(""WatchDate"", 'Day')) AS DayName, EXTRACT(ISODOW FROM ""WatchDate"") AS DowNum, CAST(COUNT(*) AS INTEGER) AS Cnt
                FROM FilteredWatched
                GROUP BY DayName, DowNum
            ),
            ByMonth AS (
                SELECT TRIM(TO_CHAR(""WatchDate"", 'Month')) AS MonthName, EXTRACT(MONTH FROM ""WatchDate"") AS MonthNum, CAST(COUNT(*) AS INTEGER) AS Cnt
                FROM FilteredWatched
                GROUP BY MonthName, MonthNum
            )
            SELECT 
                CAST(COALESCE(AVG(CASE WHEN MoviesWatched <= 1 THEN DailyAvgRating END), 0) AS DOUBLE PRECISION) AS AvgRatingLightDays,
                CAST(COALESCE(AVG(CASE WHEN MoviesWatched >= 2 THEN DailyAvgRating END), 0) AS DOUBLE PRECISION) AS AvgRatingHeavyDays,
                (SELECT TRIM(DayName) FROM ByDay ORDER BY Cnt ASC LIMIT 1) AS SlumpDay,
                (SELECT Cnt FROM ByDay ORDER BY Cnt ASC LIMIT 1) AS SlumpDayWatchCount,
                (SELECT TRIM(MonthName) FROM ByMonth ORDER BY Cnt ASC LIMIT 1) AS SlumpMonth,
                (SELECT Cnt FROM ByMonth ORDER BY Cnt ASC LIMIT 1) AS SlumpMonthWatchCount
            FROM DailyCounts";
        return await connection.QuerySingleOrDefaultAsync<CinematicFatigueExpandedDto>(sql, parameters);
    }

    public async Task<BookendsDto?> GetBookendsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
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
            FirstWatch AS (
                SELECT ""MovieId"" FROM FilteredWatched
                ORDER BY ""WatchDate"" ASC LIMIT 1
            ),
            LastWatch AS (
                SELECT ""MovieId"" FROM FilteredWatched
                ORDER BY ""WatchDate"" DESC LIMIT 1
            )
            SELECT 
                'first' AS ""Which"",
                m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS ""Rating""
            FROM FirstWatch fw JOIN ""Movies"" m ON fw.""MovieId"" = m.""Id""
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
            UNION ALL
            SELECT 
                'last' AS ""Which"",
                m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS ""Rating""
            FROM LastWatch lw JOIN ""Movies"" m ON lw.""MovieId"" = m.""Id""
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId";

        var rows = await connection.QueryAsync<dynamic>(sql, parameters);
        WrappedMovieDto? opening = null;
        WrappedMovieDto? fade = null;
        foreach (var row in rows)
        {
            var dto = new WrappedMovieDto(
                (Guid)row.Id,
                (string)row.Title,
                (int?)row.ReleaseYear,
                (string?)row.PosterPath,
                (int?)row.RuntimeMinutes,
                (double?)row.Rating
            );
            if ((string)row.Which == "first") opening = dto;
            else fade = dto;
        }
        return new BookendsDto(opening, fade);
    }

    public async Task<IEnumerable<MonthlyExtremeDto>> GetMonthlyExtremesAsync(Guid userId, AnalyticsFilterDto filter, bool includeRewatches = false, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "WatchDate", isMoviesJoined: false);

        string rewatchFilter = includeRewatches ? "" : @" AND ""IsRewatch"" = false";
        string bestSql = $@"
            WITH AllWatched AS (
                SELECT ""MovieId"", CAST(""WatchedDate"" AS DATE) AS ""WatchDate""
                FROM ""DiaryEntries"" 
                WHERE ""UserId"" = @userId {rewatchFilter}
                
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
            Ranked AS (
                SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                       m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS ""Rating"",
                       EXTRACT(MONTH FROM fw.""WatchDate"") AS ""Month"",
                       ROW_NUMBER() OVER (PARTITION BY EXTRACT(MONTH FROM fw.""WatchDate"") ORDER BY mr.""Score"" DESC, fw.""WatchDate"" ASC) AS rn
                FROM FilteredWatched fw
                JOIN ""Movies"" m ON fw.""MovieId"" = m.""Id""
                JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
                AND mr.""Score"" IS NOT NULL
            )
            SELECT 'best' AS ""Kind"", CAST(""Month"" AS INTEGER) AS ""Month"", ""Id"", ""Title"", ""ReleaseYear"", ""PosterPath"", ""RuntimeMinutes"", ""Rating""
            FROM Ranked WHERE rn = 1
            UNION ALL
            SELECT 'worst' AS ""Kind"", CAST(""Month"" AS INTEGER) AS ""Month"", ""Id"", ""Title"", ""ReleaseYear"", ""PosterPath"", ""RuntimeMinutes"", ""Rating""
            FROM (
                SELECT m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                       m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS ""Rating"",
                       EXTRACT(MONTH FROM fw.""WatchDate"") AS ""Month"",
                       ROW_NUMBER() OVER (PARTITION BY EXTRACT(MONTH FROM fw.""WatchDate"") ORDER BY mr.""Score"" ASC, fw.""WatchDate"" ASC) AS rn
                FROM FilteredWatched fw
                JOIN ""Movies"" m ON fw.""MovieId"" = m.""Id""
                JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
                AND mr.""Score"" IS NOT NULL
            ) w WHERE rn = 1
            ORDER BY ""Month"" ASC";

        var rows = await connection.QueryAsync<dynamic>(bestSql, parameters);
        var monthNames = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;

        var grouped = rows.GroupBy(r => (int)r.Month);
        var result = new List<MonthlyExtremeDto>();
        foreach (var g in grouped.OrderBy(g => g.Key))
        {
            var monthNum = g.Key;
            WrappedMovieDto? best = null, worst = null;
            foreach (var row in g)
            {
                var dto = new WrappedMovieDto(
                    (Guid)row.Id, (string)row.Title, (int?)row.ReleaseYear,
                    (string?)row.PosterPath, (int?)row.RuntimeMinutes, (double?)row.Rating);
                if ((string)row.Kind == "best") best = dto;
                else worst = dto;
            }
            result.Add(new MonthlyExtremeDto(monthNum, monthNames[monthNum - 1], best, worst));
        }
        return result;
    }

    public async Task<TopBottomMoviesDto> GetTopAndBottomRatedMoviesAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        var filterBuilder = new SqlFilterBuilder(filter, parameters, "m", "w", "WatchDate", isMoviesJoined: false);

        string sql = $@"
            WITH AllWatched AS (
                SELECT ""MovieId"", CAST(""WatchedDate"" AS DATE) AS ""WatchDate""
                FROM ""DiaryEntries""
                WHERE ""UserId"" = @userId AND ""IsRewatch"" = false

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
            SELECT * FROM (
                SELECT 'top' AS ""Kind"", m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                       m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS ""Rating"",
                       CASE WHEN ml.""Id"" IS NOT NULL THEN TRUE ELSE FALSE END AS ""Liked""
                FROM FilteredWatched fw
                JOIN ""Movies"" m ON fw.""MovieId"" = m.""Id""
                JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
                LEFT JOIN ""MovieLikes"" ml ON ml.""MovieId"" = m.""Id"" AND ml.""UserId"" = @userId
                AND mr.""Score"" IS NOT NULL
                ORDER BY mr.""Score"" DESC, ""Liked"" DESC, RANDOM()
                LIMIT 5
            ) top_movies
            UNION ALL
            SELECT * FROM (
                SELECT 'bottom' AS ""Kind"", m.""Id"", m.""Title"", m.""ReleaseYear"", m.""PosterUrl"" AS ""PosterPath"",
                       m.""RuntimeMinutes"", CAST(mr.""Score"" AS DOUBLE PRECISION) AS ""Rating"",
                       CASE WHEN ml.""Id"" IS NOT NULL THEN TRUE ELSE FALSE END AS ""Liked""
                FROM FilteredWatched fw
                JOIN ""Movies"" m ON fw.""MovieId"" = m.""Id""
                JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
                LEFT JOIN ""MovieLikes"" ml ON ml.""MovieId"" = m.""Id"" AND ml.""UserId"" = @userId
                AND mr.""Score"" IS NOT NULL
                ORDER BY mr.""Score"" ASC, ""Liked"" ASC, RANDOM()
                LIMIT 5
            ) bottom_movies";

        var rows = await connection.QueryAsync<dynamic>(sql, parameters);
        var top = new List<WrappedMovieDto>();
        var bottom = new List<WrappedMovieDto>();
        foreach (var row in rows)
        {
            var dto = new WrappedMovieDto(
                (Guid)row.Id, (string)row.Title, (int?)row.ReleaseYear,
                (string?)row.PosterPath, (int?)row.RuntimeMinutes, (double?)row.Rating) { Liked = (bool)row.Liked };
            if ((string)row.Kind == "top") top.Add(dto);
            else bottom.Add(dto);
        }
        return new TopBottomMoviesDto(top, bottom);
    }

    public async Task<MostRewatchedDto?> GetMostRewatchedMovieAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        
        var cteFilter = new AnalyticsFilterDto { WatchYear = filter?.WatchYear };
        var cteFilterBuilder = new SqlFilterBuilder(cteFilter, parameters, "m", "de", "WatchedDate", isMoviesJoined: false);

        var mainFilter = new AnalyticsFilterDto { 
            ReleaseYear = filter?.ReleaseYear,
            MinRating = filter?.MinRating,
            MaxRating = filter?.MaxRating,
            Genre = filter?.Genre,
            Director = filter?.Director,
            Actor = filter?.Actor
        };
        var mainFilterBuilder = new SqlFilterBuilder(mainFilter, parameters, "m", "de", "WatchedDate", isMoviesJoined: true);

        string sql = $@"
            WITH RewatchCounts AS (
                SELECT ""MovieId"", CAST(COUNT(*) AS INTEGER) AS RewatchCount
                FROM ""DiaryEntries"" de
                WHERE ""UserId"" = @userId AND ""IsRewatch"" = true 
                {cteFilterBuilder.BuildWhereClause()}
                GROUP BY ""MovieId""
            )
            SELECT m.""Title"", m.""PosterUrl"" AS ""PosterPath"", m.""ReleaseYear"", rc.RewatchCount
            FROM RewatchCounts rc
            JOIN ""Movies"" m ON rc.""MovieId"" = m.""Id""
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = m.""Id"" AND mr.""UserId"" = @userId
            LEFT JOIN ""MovieLikes"" ml ON ml.""MovieId"" = m.""Id"" AND ml.""UserId"" = @userId
            {mainFilterBuilder.BuildJoins()}
            WHERE 1=1 {mainFilterBuilder.BuildWhereClause()}
            ORDER BY rc.RewatchCount DESC, mr.""Score"" DESC, CASE WHEN ml.""Id"" IS NOT NULL THEN 1 ELSE 0 END DESC, RANDOM()
            LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<MostRewatchedDto>(sql, parameters);
    }

    public async Task<BestRookiesDto> GetBestRookiesAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // Directors seen for the first time this year or last 12 months
        const string directorsSql = @"
            WITH ThisYear AS (
                SELECT DISTINCT md.""DirectorsId"" AS DirId
                FROM ""DiaryEntries"" de
                JOIN ""MovieDirector"" md ON de.""MovieId"" = md.""MoviesId""
                WHERE de.""UserId"" = @userId AND (
                    (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM de.""WatchedDate"") = CAST(@year AS INTEGER)) OR 
                    (CAST(@year AS INTEGER) IS NULL AND de.""WatchedDate"" >= CURRENT_DATE - INTERVAL '1 year')
                )
            ),
            PriorYears AS (
                SELECT DISTINCT md.""DirectorsId"" AS DirId
                FROM ""DiaryEntries"" de
                JOIN ""MovieDirector"" md ON de.""MovieId"" = md.""MoviesId""
                WHERE de.""UserId"" = @userId AND (
                    (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM de.""WatchedDate"") < CAST(@year AS INTEGER)) OR 
                    (CAST(@year AS INTEGER) IS NULL AND de.""WatchedDate"" < CURRENT_DATE - INTERVAL '1 year')
                )
                UNION
                SELECT DISTINCT md.""DirectorsId"" AS DirId
                FROM ""WatchedMovies"" wm
                JOIN ""MovieDirector"" md ON wm.""MovieId"" = md.""MoviesId""
                WHERE wm.""UserId"" = @userId AND (
                    (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM wm.""Date"") < CAST(@year AS INTEGER)) OR 
                    (CAST(@year AS INTEGER) IS NULL AND wm.""Date"" < CURRENT_DATE - INTERVAL '1 year')
                )
            ),
            Rookies AS (
                SELECT ty.DirId FROM ThisYear ty
                WHERE ty.DirId NOT IN (SELECT DirId FROM PriorYears)
            )
            SELECT dr.""Name"" AS Name,
                   CAST(COUNT(DISTINCT de.""MovieId"") AS INTEGER) AS MoviesWatchedThisYear,
                   CAST(COALESCE(AVG(mr.""Score""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM Rookies r
            JOIN ""Directors"" dr ON r.DirId = dr.""Id""
            JOIN ""MovieDirector"" md ON r.DirId = md.""DirectorsId""
            JOIN ""DiaryEntries"" de ON md.""MoviesId"" = de.""MovieId"" AND de.""UserId"" = @userId AND (
                (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM de.""WatchedDate"") = CAST(@year AS INTEGER)) OR 
                (CAST(@year AS INTEGER) IS NULL AND de.""WatchedDate"" >= CURRENT_DATE - INTERVAL '1 year')
            )
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = de.""MovieId"" AND mr.""UserId"" = @userId
            GROUP BY dr.""Name""
            ORDER BY MoviesWatchedThisYear DESC, AverageRating DESC
            LIMIT 5";

        // Actors seen for the first time this year or last 12 months
        const string actorsSql = @"
            WITH ThisYear AS (
                SELECT DISTINCT ma.""ActorsId"" AS ActId
                FROM ""DiaryEntries"" de
                JOIN ""MovieActor"" ma ON de.""MovieId"" = ma.""MoviesId""
                WHERE de.""UserId"" = @userId AND (
                    (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM de.""WatchedDate"") = CAST(@year AS INTEGER)) OR 
                    (CAST(@year AS INTEGER) IS NULL AND de.""WatchedDate"" >= CURRENT_DATE - INTERVAL '1 year')
                )
            ),
            PriorYears AS (
                SELECT DISTINCT ma.""ActorsId"" AS ActId
                FROM ""DiaryEntries"" de
                JOIN ""MovieActor"" ma ON de.""MovieId"" = ma.""MoviesId""
                WHERE de.""UserId"" = @userId AND (
                    (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM de.""WatchedDate"") < CAST(@year AS INTEGER)) OR 
                    (CAST(@year AS INTEGER) IS NULL AND de.""WatchedDate"" < CURRENT_DATE - INTERVAL '1 year')
                )
                UNION
                SELECT DISTINCT ma.""ActorsId"" AS ActId
                FROM ""WatchedMovies"" wm
                JOIN ""MovieActor"" ma ON wm.""MovieId"" = ma.""MoviesId""
                WHERE wm.""UserId"" = @userId AND (
                    (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM wm.""Date"") < CAST(@year AS INTEGER)) OR 
                    (CAST(@year AS INTEGER) IS NULL AND wm.""Date"" < CURRENT_DATE - INTERVAL '1 year')
                )
            ),
            Rookies AS (
                SELECT ty.ActId FROM ThisYear ty
                WHERE ty.ActId NOT IN (SELECT ActId FROM PriorYears)
            )
            SELECT a.""Name"" AS Name,
                   CAST(COUNT(DISTINCT de.""MovieId"") AS INTEGER) AS MoviesWatchedThisYear,
                   CAST(COALESCE(AVG(mr.""Score""), 0) AS DOUBLE PRECISION) AS AverageRating
            FROM Rookies r
            JOIN ""Actors"" a ON r.ActId = a.""Id""
            JOIN ""MovieActor"" ma ON r.ActId = ma.""ActorsId""
            JOIN ""DiaryEntries"" de ON ma.""MoviesId"" = de.""MovieId"" AND de.""UserId"" = @userId AND (
                (CAST(@year AS INTEGER) IS NOT NULL AND EXTRACT(YEAR FROM de.""WatchedDate"") = CAST(@year AS INTEGER)) OR 
                (CAST(@year AS INTEGER) IS NULL AND de.""WatchedDate"" >= CURRENT_DATE - INTERVAL '1 year')
            )
            LEFT JOIN ""MovieRatings"" mr ON mr.""MovieId"" = de.""MovieId"" AND mr.""UserId"" = @userId
            GROUP BY a.""Name""
            ORDER BY MoviesWatchedThisYear DESC, AverageRating DESC
            LIMIT 5";

        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);
        if (filter?.WatchYear.HasValue == true) parameters.Add("year", filter.WatchYear.Value);
        else parameters.Add("year", null);

        var directors = await connection.QueryAsync<RookieDto>(directorsSql, parameters);
        var actors = await connection.QueryAsync<RookieDto>(actorsSql, parameters);
        return new BestRookiesDto(directors, actors);
    }
}




