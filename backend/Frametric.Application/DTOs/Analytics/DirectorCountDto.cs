namespace Frametric.Application.DTOs.Analytics;

public record DirectorCountDto(
    string DirectorName,
    int Count,
    double AverageRating = 0,
    int WatchedCount = 0,
    Guid? Id = null,
    string? ProfilePath = null)
{
    // Secondary constructor for Dapper queries that don't yet return Id/ProfilePath
    public DirectorCountDto(string directorName, int count, double averageRating)
        : this(directorName, count, averageRating, 0, null, null) {}

    public DirectorCountDto(string directorName, int count, double averageRating, int watchedCount)
        : this(directorName, count, averageRating, watchedCount, null, null) {}
}

