namespace Frametric.Application.DTOs;

public record TmdbCollectionResultDto(
    int Id,
    string Name,
    string? Overview,
    string? PosterUrl,
    string? BackdropUrl,
    List<TmdbCollectionPartDto> Parts
);

public record TmdbCollectionPartDto(
    int Id,
    string Title,
    string? ReleaseDate,
    string? PosterUrl,
    bool IsInDatabase
);
