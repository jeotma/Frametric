namespace Frametric.Application.DTOs.Analytics;

public record ActorCountDto(string ActorName, int Count, double AverageRating = 0, int WatchedCount = 0)
{
    public ActorCountDto(string actorName, int count, double averageRating) : this(actorName, count, averageRating, 0) {}
}

