namespace Frametric.Application.DTOs;

public record TmdbMovieResultDto(
    int TmdbId,
    int? RuntimeMinutes,
    string? PosterUrl,
    List<TmdbGenreDto> Genres,
    List<TmdbPersonDto> Directors,
    List<TmdbPersonDto> Actors,
    bool IsTvShow = false,
    string? Title = null,
    int? FirstAirYear = null,
    double? TmdbRating = null,
    double? TmdbPopularity = null,
    string? ImdbId = null,
    string? ReleaseDate = null,
    string? Keywords = null,
    string? StreamingProviders = null,
    string? Overview = null
)
{
    public const int DocumentaryGenreId = 99;

    public bool IsDocumentary => Genres.Any(g => g.Id == DocumentaryGenreId);
}

public record TmdbGenreDto(int Id, string Name);
public record TmdbPersonDto(int Id, string Name);
