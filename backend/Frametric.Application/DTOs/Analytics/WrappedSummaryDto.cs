namespace Frametric.Application.DTOs.Analytics;

public record WrappedSummaryDto(
    int? Year,
    int TotalWatchtimeMinutes,
    int TotalWatches,
    int UniqueMoviesCount,
    List<GenreCountDto> TopGenres,
    List<DirectorCountDto> TopDirectors,
    List<ActorCountDto> TopActors,
    List<DecadeCountDto> DecadeBreakdown,
    List<MonthlyActivityDto> MonthlyActivity
);

