using Frametric.Application.DTOs.Analytics;

namespace Frametric.Application.Interfaces.Analytics;

public interface IWatchedBasicQueries
{
    Task<IEnumerable<WatchedMovieStatsDto>> GetMoviesAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<DirectorCountDto>> GetDirectorsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<ActorCountDto>> GetActorsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<GenreCountDto>> GetMoviesByGenreAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<DecadeCountDto>> GetMoviesByDecadeAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
}

public interface IWatchedAdvancedStatsQueries
{
    Task<ActorCountDto?> GetMostRepeatedActorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<DirectorCountDto?> GetMostWatchedDirectorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<EraBreakdownDto?> GetPredominantEraAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<DirectorLeaderboardDto>> GetDirectorRankingByRatingAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<TimeInvestedDto?> GetTotalTimeByDirectorOrGenreAsync(Guid userId, string filterType, string filterName, AnalyticsFilterDto filter, CancellationToken ct = default);
}

public interface IWatchedComplexCorrelationsQueries
{
    Task<IEnumerable<PreferredDayDto>> GetPreferredWatchDayOfWeekAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<GenreStreakDto>> GetGenreStreakAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<WrappedMovieDto?> GetLongestWatchedMovieAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<WrappedMovieDto?> GetShortestWatchedMovieAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<RatingEvolutionDto>> GetRatingEvolutionAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<CastingPairDto>> GetCastingRepetitionsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<DirectorActorPairDto>> GetDirectorActorPairingsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<PrimeTimeStatsDto?> GetPrimeTimeStatsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<GenreWithRatingDto>> GetGenresWithRatingAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
}

public interface IWatchlistBasicQueries
{
    Task<IEnumerable<WatchlistMovieStatsDto>> GetWatchlistAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<DirectorCountDto>> GetWatchlistDirectorsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<ActorCountDto>> GetWatchlistActorsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<GenreCountDto>> GetWatchlistByGenreAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<DecadeCountDto>> GetWatchlistByDecadeAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
}

public interface IWatchlistAdvancedStatsQueries
{
    Task<DirectorCountDto?> GetMostAnticipatedDirectorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<ActorCountDto?> GetMostAnticipatedActorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<TimeInvestedDto?> GetTotalPendingWatchtimeAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<MovieSimpleDto?> GetOldestPendingMovieAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<GenreProportionDto>> GetGenreProportionWatchlistVsWatchedAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
}

public interface IWatchlistComplexCorrelationsQueries
{
    Task<GoldenDirectorDto?> GetGoldenPendingDirectorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<DurationBalanceDto>> GetPendingDurationBalanceAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<EraBreakdownDto>> GetWatchlistByEraAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<GhostActorDto?> GetGhostActorAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
}

public interface IBonusQueries
{
    Task<WeekendWarriorDto?> GetWeekendWarriorStatsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<MovieSimpleDto>> GetHiddenGemsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<MovieSimpleDto>> GetWatchlistGraveyardAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<CinematicFatigueExpandedDto?> GetCinematicFatigueExpandedAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<BookendsDto?> GetBookendsAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<MonthlyExtremeDto>> GetMonthlyExtremesAsync(Guid userId, AnalyticsFilterDto filter, bool includeRewatches = false, CancellationToken ct = default);
    Task<TopBottomMoviesDto> GetTopAndBottomRatedMoviesAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<MostRewatchedDto?> GetMostRewatchedMovieAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
    Task<BestRookiesDto> GetBestRookiesAsync(Guid userId, AnalyticsFilterDto filter, CancellationToken ct = default);
}
