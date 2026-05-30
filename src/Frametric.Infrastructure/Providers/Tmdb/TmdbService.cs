using System.Net.Http.Json;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;

namespace Frametric.Infrastructure.Providers.Tmdb;

public class TmdbService : ITmdbService
{
    private readonly HttpClient _httpClient;

    public TmdbService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TmdbMovieResultDto?> SearchAndGetMovieDetailsAsync(string title, int? year, CancellationToken cancellationToken)
    {
        var encodedTitle = Uri.EscapeDataString(title);
        var searchUrl = $"search/movie?query={encodedTitle}&language=en-US";
        if (year.HasValue) searchUrl += $"&year={year.Value}";

        var searchResponse = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(searchUrl, cancellationToken);
        var firstResult = searchResponse?.Results?.FirstOrDefault();

        if (firstResult == null) return null;

        var detailsUrl = $"movie/{firstResult.Id}?append_to_response=credits&language=en-US";
        var detailsResponse = await _httpClient.GetFromJsonAsync<TmdbMovieDetails>(detailsUrl, cancellationToken);

        if (detailsResponse == null) return null;

        var posterUrl = !string.IsNullOrEmpty(detailsResponse.PosterPath) 
            ? $"https://image.tmdb.org/t/p/w500{detailsResponse.PosterPath}" 
            : null;

        var genres = detailsResponse.Genres.Select(g => new TmdbGenreDto(g.Id, g.Name)).ToList();
        
        var directors = detailsResponse.Credits?.Crew?
            .Where(c => c.Job == "Director")
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList() ?? new List<TmdbPersonDto>();

        var actors = detailsResponse.Credits?.Cast?
            .OrderBy(c => c.Order)
            .Take(10) // Restrict to top 10 actors to avoid database bloat
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList() ?? new List<TmdbPersonDto>();

        return new TmdbMovieResultDto(
            detailsResponse.Id,
            detailsResponse.Runtime,
            posterUrl,
            genres,
            directors,
            actors
        );
    }
}
