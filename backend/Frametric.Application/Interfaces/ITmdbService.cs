using Frametric.Application.DTOs;

namespace Frametric.Application.Interfaces;

public interface ITmdbService
{
    Task<TmdbMovieResultDto?> SearchAndGetMovieDetailsAsync(string title, int? year, CancellationToken cancellationToken);
}
