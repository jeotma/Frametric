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

        // 1. Try as a Movie first
        var movieSearchUrl = $"search/movie?query={encodedTitle}&language=en-US";
        if (year.HasValue) movieSearchUrl += $"&year={year.Value}";

        var movieSearchResponse = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(movieSearchUrl, cancellationToken);
        var movieResult = movieSearchResponse?.Results?.FirstOrDefault();

        if (movieResult != null)
        {
            var detailsUrl = $"movie/{movieResult.Id}?append_to_response=credits&language=en-US";
            var details = await _httpClient.GetFromJsonAsync<TmdbMovieDetails>(detailsUrl, cancellationToken);
            if (details == null) return null;

            return MapMovieDetails(details);
        }

        // 2. Fallback: try as a TV Show (miniseries)
        var tvSearchUrl = $"search/tv?query={encodedTitle}&language=en-US";
        if (year.HasValue) tvSearchUrl += $"&first_air_date_year={year.Value}";

        var tvSearchResponse = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(tvSearchUrl, cancellationToken);
        var tvResult = tvSearchResponse?.Results?.FirstOrDefault();

        if (tvResult == null) return null;

        var tvDetailsUrl = $"tv/{tvResult.Id}?append_to_response=credits&language=en-US";
        var tvDetails = await _httpClient.GetFromJsonAsync<TmdbTvDetails>(tvDetailsUrl, cancellationToken);
        if (tvDetails == null) return null;

        return MapTvDetails(tvDetails);
    }

    private static TmdbMovieResultDto MapMovieDetails(TmdbMovieDetails details)
    {
        var posterUrl = !string.IsNullOrEmpty(details.PosterPath)
            ? $"https://image.tmdb.org/t/p/w500{details.PosterPath}"
            : null;

        var genres = details.Genres.Select(g => new TmdbGenreDto(g.Id, g.Name)).ToList();

        var directors = details.Credits?.Crew?
            .Where(c => c.Job == "Director")
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList() ?? new List<TmdbPersonDto>();

        var actors = details.Credits?.Cast?
            .OrderBy(c => c.Order)
            .Take(10)
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList() ?? new List<TmdbPersonDto>();

        return new TmdbMovieResultDto(details.Id, details.Runtime, posterUrl, genres, directors, actors, IsTvShow: false);
    }

    private static TmdbMovieResultDto MapTvDetails(TmdbTvDetails details)
    {
        var posterUrl = !string.IsNullOrEmpty(details.PosterPath)
            ? $"https://image.tmdb.org/t/p/w500{details.PosterPath}"
            : null;

        // TV shows return episode_run_time as an array; take the first value as a representative runtime
        var runtime = details.EpisodeRunTime?.FirstOrDefault();

        var genres = details.Genres.Select(g => new TmdbGenreDto(g.Id, g.Name)).ToList();

        // Creators (created_by) map to Directors in our domain
        var directors = details.CreatedBy
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList();

        var actors = details.Credits?.Cast?
            .OrderBy(c => c.Order)
            .Take(10)
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList() ?? new List<TmdbPersonDto>();

        return new TmdbMovieResultDto(details.Id, runtime, posterUrl, genres, directors, actors, IsTvShow: true);
    }
}
