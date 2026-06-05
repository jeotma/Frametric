namespace Frametric.Application.DTOs.Analytics;

public record MovieSimpleDto(Guid Id, string Title, int? ReleaseYear, string? PosterPath, bool IsWatched = false)
{
    public MovieSimpleDto(Guid id, string title, int? releaseYear, string? posterPath) : this(id, title, releaseYear, posterPath, false) {}
}
public record WatchedMovieStatsDto(string Title, int? ReleaseYear, string Director, double Rating, bool Liked, double? CustomAverageRating = null, string? PosterUrl = null);
public record WatchlistMovieStatsDto(string Title, int? ReleaseYear, string Director, string DateAdded, double? CustomAverageRating = null, string? PosterUrl = null);
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
public class TopBottomMoviesDto
{
    public IEnumerable<WrappedMovieDto> TopRated { get; set; } = new List<WrappedMovieDto>();
    public IReadOnlyCollection<WrappedMovieDto> BottomRated { get; set; } = new List<WrappedMovieDto>();

    public TopBottomMoviesDto(IEnumerable<WrappedMovieDto> topRated, IEnumerable<WrappedMovieDto> bottomRated)
    {
        TopRated = topRated;
        BottomRated = bottomRated.ToList();
    }
}

// For The Return of the King: most rewatched
public record MostRewatchedDto(string Title, string? PosterPath, int? ReleaseYear, int RewatchCount);

// For The David and Goliath: shortest watched movie
public record DavidAndGoliathDto(
    WrappedMovieDto? Shortest,
    WrappedMovieDto? Longest
);

// For The Best Rookies: actors/directors new to the user this year
public record RookieDto(string Name, int MoviesWatchedThisYear, double AverageRating);
public class BestRookiesDto
{
    public IEnumerable<RookieDto> NewDirectors { get; set; } = new List<RookieDto>();
    public IReadOnlyCollection<RookieDto> NewActors { get; set; } = new List<RookieDto>();

    public BestRookiesDto(IEnumerable<RookieDto> newDirectors, IEnumerable<RookieDto> newActors)
    {
        NewDirectors = newDirectors;
        NewActors = newActors.ToList();
    }
}

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
