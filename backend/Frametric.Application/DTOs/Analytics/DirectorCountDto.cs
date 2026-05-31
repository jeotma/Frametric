namespace Frametric.Application.DTOs.Analytics;

public record DirectorCountDto(string DirectorName, int Count, double AverageRating = 0, int WatchedCount = 0)
{
    public DirectorCountDto(string directorName, int count, double averageRating) : this(directorName, count, averageRating, 0) {}
}

