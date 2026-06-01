namespace Frametric.Application.DTOs.Analytics;

public record MovieSimpleDto(Guid Id, string Title, int? ReleaseYear, string? PosterPath);
public record WrappedMovieDto(Guid Id, string Title, int? ReleaseYear, string? PosterPath, int? RuntimeMinutes, double? Rating)
{
    public bool Liked { get; init; } = false;
}
public record DirectorSimpleDto(Guid Id, string Name);
public record ActorSimpleDto(Guid Id, string Name);
public record GenreSimpleDto(Guid Id, string Name);

public class TimeInvestedDto
{
    public string Name { get; set; } = string.Empty;
    public int TotalMinutes { get; set; }
    public int TotalHours { get; set; }

    public TimeInvestedDto() { }
    public TimeInvestedDto(string name, int totalMinutes, int totalHours)
    {
        Name = name;
        TotalMinutes = totalMinutes;
        TotalHours = totalHours;
    }
}
public record CollectionProgressDto(string CollectionName, int WatchedCount, int TotalCount, double ProgressPercentage);
public record PreferredDayDto(string DayOfWeek, int WatchCount);
public record GenreStreakDto(string GenreName, int StreakLength, DateTime StartDate, DateTime EndDate);
public record RatingEvolutionDto(int Month, double AverageRating);
public record LanguageDiversityDto(string Language, int Count);
public record CastingPairDto(string Actor1Name, string Actor2Name, int CollaborationCount);
public record EraBreakdownDto(string EraName, int Count);
public record GenreProportionDto(string GenreName, int WatchedCount, int PendingCount);
public record GoldenDirectorDto(string DirectorName, double AverageRatingInHistory, int PendingMoviesCount);
public record DurationBalanceDto(string DurationCategory, int Count);
public record GhostActorDto(string ActorName, int PendingMoviesCount);
public record WeekendWarriorDto(int WeekendWatches, int WeekdayWatches);
public record CinematicFatigueDto(double AvgRatingLightDays, double AvgRatingHeavyDays);

// --- Final Cut Expansion DTOs ---

// For The Bookends slide: first and last movie of the year
public record BookendsDto(
    WrappedMovieDto? OpeningScene,
    WrappedMovieDto? FadeToBlack
);

// For The Monthly Extremes slide: best and worst movie per month
public record MonthlyExtremeDto(
    int Month,
    string MonthName,
    WrappedMovieDto? BestMovie,
    WrappedMovieDto? WorstMovie
);

// For The Hall of Fame / Golden Raspberry: top and bottom rated movies
public record TopBottomMoviesDto(
    IEnumerable<WrappedMovieDto> TopRated,
    IEnumerable<WrappedMovieDto> BottomRated
);

// For The Return of the King: most rewatched
public record MostRewatchedDto(string Title, string? PosterPath, int? ReleaseYear, int RewatchCount);

// For The David and Goliath: shortest watched movie
public record DavidAndGoliathDto(
    WrappedMovieDto? Shortest,
    WrappedMovieDto? Longest
);

// For The Best Rookies: actors/directors new to the user this year
public record RookieDto(string Name, int MoviesWatchedThisYear, double AverageRating);
public record BestRookiesDto(
    IEnumerable<RookieDto> NewDirectors,
    IEnumerable<RookieDto> NewActors
);

// For The Genre Landscape (Expanded): top by volume + best/worst rated
public record GenreWithRatingDto(string GenreName, int Count, double AverageRating);

// For Dynamic Duos & Perfect Pairs: director-actor collaborations
public record DirectorActorPairDto(string DirectorName, string ActorName, int CollaborationCount);

// For The Prime Time Blockbuster: expanded stats on peak habits
public record MonthActivityCountDto(int Month, string MonthName, int WatchCount);
public record PrimeTimeStatsDto(
    string? PeakDay,
    int PeakDayCount,
    string? PeakMonth,
    int PeakMonthCount,
    string? SlumpDay,
    int SlumpDayCount,
    string? SlumpMonth,
    int SlumpMonthCount
);

// For Cinematic Fatigue (Expanded)
public record CinematicFatigueExpandedDto(
    double AvgRatingLightDays,
    double AvgRatingHeavyDays,
    string? SlumpDay,
    int SlumpDayWatchCount,
    string? SlumpMonth,
    int SlumpMonthWatchCount
);
