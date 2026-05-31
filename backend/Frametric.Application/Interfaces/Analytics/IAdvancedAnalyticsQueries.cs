using Frametric.Application.DTOs.Analytics;

namespace Frametric.Application.Interfaces.Analytics;

public interface IWatchedBasicQueries
{
    Task<IEnumerable<MovieSimpleDto>> GetMoviesByReleaseYearAsync(Guid userId, int releaseYear, CancellationToken ct = default);
    Task<IEnumerable<DirectorCountDto>> GetDirectorsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<ActorCountDto>> GetActorsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<GenreCountDto>> GetMoviesByGenreAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<DecadeCountDto>> GetMoviesByDecadeAsync(Guid userId, CancellationToken ct = default);
}

public interface IWatchedAdvancedStatsQueries
{
    Task<ActorCountDto?> GetMostRepeatedActorAsync(Guid userId, CancellationToken ct = default);
    Task<DirectorCountDto?> GetMostWatchedDirectorAsync(Guid userId, CancellationToken ct = default);
    Task<EraBreakdownDto?> GetPredominantEraAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<DirectorLeaderboardDto>> GetDirectorRankingByRatingAsync(Guid userId, CancellationToken ct = default);
    Task<TimeInvestedDto?> GetTotalTimeByDirectorOrGenreAsync(Guid userId, string filterType, string filterName, CancellationToken ct = default);
}

public interface IWatchedComplexCorrelationsQueries
{
    Task<IEnumerable<PreferredDayDto>> GetPreferredWatchDayOfWeekAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<GenreStreakDto>> GetGenreStreakAsync(Guid userId, CancellationToken ct = default);
    Task<MovieSimpleDto?> GetLongestWatchedMovieAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<RatingEvolutionDto>> GetRatingEvolutionAsync(Guid userId, int year, CancellationToken ct = default);
    Task<IEnumerable<CastingPairDto>> GetCastingRepetitionsAsync(Guid userId, CancellationToken ct = default);
}

public interface IWatchlistBasicQueries
{
    Task<IEnumerable<MovieSimpleDto>> GetWatchlistByYearAsync(Guid userId, int releaseYear, CancellationToken ct = default);
    Task<IEnumerable<DirectorCountDto>> GetWatchlistDirectorsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<ActorCountDto>> GetWatchlistActorsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<GenreCountDto>> GetWatchlistByGenreAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<DecadeCountDto>> GetWatchlistByDecadeAsync(Guid userId, CancellationToken ct = default);
}

public interface IWatchlistAdvancedStatsQueries
{
    Task<DirectorCountDto?> GetMostAnticipatedDirectorAsync(Guid userId, CancellationToken ct = default);
    Task<ActorCountDto?> GetMostAnticipatedActorAsync(Guid userId, CancellationToken ct = default);
    Task<TimeInvestedDto?> GetTotalPendingWatchtimeAsync(Guid userId, CancellationToken ct = default);
    Task<MovieSimpleDto?> GetOldestPendingMovieAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<GenreProportionDto>> GetGenreProportionWatchlistVsWatchedAsync(Guid userId, CancellationToken ct = default);
}

public interface IWatchlistComplexCorrelationsQueries
{
    Task<GoldenDirectorDto?> GetGoldenPendingDirectorAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<DurationBalanceDto>> GetPendingDurationBalanceAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<EraBreakdownDto>> GetWatchlistByEraAsync(Guid userId, CancellationToken ct = default);
    Task<GhostActorDto?> GetGhostActorAsync(Guid userId, CancellationToken ct = default);
}

public interface IBonusQueries
{
    Task<WeekendWarriorDto?> GetWeekendWarriorStatsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<MovieSimpleDto>> GetHiddenGemsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<MovieSimpleDto>> GetWatchlistGraveyardAsync(Guid userId, CancellationToken ct = default);
    Task<CinematicFatigueDto?> GetCinematicFatigueAsync(Guid userId, CancellationToken ct = default);
}
