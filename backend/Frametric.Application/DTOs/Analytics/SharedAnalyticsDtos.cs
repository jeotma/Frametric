namespace Frametric.Application.DTOs.Analytics;

public record MovieSimpleDto(Guid Id, string Title, int? ReleaseYear, string? PosterPath);
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
public record GenreStreakDto(string GenreName, int StreakLength, DateOnly StartDate, DateOnly EndDate);
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


