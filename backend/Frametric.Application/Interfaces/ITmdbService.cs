using Frametric.Application.DTOs;
using Frametric.Application.DTOs.EntityDetails;

namespace Frametric.Application.Interfaces;

public interface ITmdbService
{
    Task<TmdbMovieResultDto?> SearchAndGetMovieDetailsAsync(string title, int? year, CancellationToken cancellationToken);
    Task<TmdbMovieResultDto?> GetMovieDetailsByIdAsync(int tmdbId, CancellationToken cancellationToken);
    Task<IEnumerable<GlobalSearchResultDto>> SearchMultiAsync(string query, CancellationToken cancellationToken);
}
