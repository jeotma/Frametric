namespace Frametric.Application.DTOs;

public record TmdbMovieResultDto(
    int TmdbId,
    int? RuntimeMinutes,
    string? PosterUrl,
    List<TmdbGenreDto> Genres,
    List<TmdbPersonDto> Directors,
    List<TmdbPersonDto> Actors,
    bool IsTvShow = false
);

public record TmdbGenreDto(int Id, string Name);
public record TmdbPersonDto(int Id, string Name);
