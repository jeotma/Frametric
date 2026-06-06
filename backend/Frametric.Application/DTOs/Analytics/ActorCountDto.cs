namespace Frametric.Application.DTOs.Analytics;

public record ActorCountDto(
    string ActorName,
    int Count,
    double AverageRating = 0,
    int WatchedCount = 0,
    Guid? Id = null,
    string? ProfilePath = null)
{
    // Secondary constructor for Dapper queries that don't yet return Id/ProfilePath
    public ActorCountDto(string actorName, int count, double averageRating)
        : this(actorName, count, averageRating, 0, null, null) {}

    public ActorCountDto(string actorName, int count, double averageRating, int watchedCount)
        : this(actorName, count, averageRating, watchedCount, null, null) {}
}

