namespace Frametric.Application.DTOs.Analytics;

public record DirectorLeaderboardDto(Guid DirectorId, string Name, int WatchCount, double AverageRating, string HighestRatedMovieTitle);



