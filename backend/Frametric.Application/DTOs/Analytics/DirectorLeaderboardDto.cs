namespace Frametric.Application.DTOs.Analytics;

public record DirectorLeaderboardDto(Guid DirectorId, string Name, long WatchCount, decimal AverageRating);
