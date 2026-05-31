namespace Frametric.Application.DTOs.Analytics;

public record GenreCountDto(string GenreName, int Count, int WatchedCount = 0)
{
    public GenreCountDto(string genreName, int count) : this(genreName, count, 0) {}
}

