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
        var (mainTitle, subtitle) = ParseTitleAndSubtitle(title);
        var simplifiedTitle = mainTitle;

        // Progressive fallback chain: most specific → least specific
        // Each step relaxes one constraint to maximise the chance of finding the entry.
        var movieResult = await TrySearchMovieAsync(title, year, cancellationToken)
                       ?? await TrySearchTvAsync(title, year, cancellationToken)
                       ?? await TrySearchMovieAsync(title, year: null, cancellationToken)
                       ?? await TrySearchTvAsync(title, year: null, cancellationToken)
                       // Try searching TV show first with subtitle matching if a subtitle was detected
                       ?? (!string.IsNullOrEmpty(subtitle) ? await TrySearchTvAsync(simplifiedTitle, year: null, cancellationToken, subtitle) : null)
                       // Standard simplified title fallbacks
                       ?? (simplifiedTitle != title ? await TrySearchMovieAsync(simplifiedTitle, year: null, cancellationToken) : null)
                       ?? (simplifiedTitle != title ? await TrySearchTvAsync(simplifiedTitle, year: null, cancellationToken) : null);

        return movieResult;
    }

    // ── Search helpers ──────────────────────────────────────────────────────────

    private async Task<TmdbMovieResultDto?> TrySearchMovieAsync(string title, int? year, CancellationToken cancellationToken)
    {
        var url = $"search/movie?query={Uri.EscapeDataString(title)}&language=en-US";
        if (year.HasValue) url += $"&year={year.Value}";

        var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(url, cancellationToken);
        var result = response?.Results?.FirstOrDefault();
        if (result == null) return null;

        var details = await _httpClient.GetFromJsonAsync<TmdbMovieDetails>(
            $"movie/{result.Id}?append_to_response=credits&language=en-US", cancellationToken);

        return details == null ? null : MapMovieDetails(details);
    }

    private async Task<TmdbMovieResultDto?> TrySearchTvAsync(string title, int? year, CancellationToken cancellationToken, string? subtitle = null)
    {
        var url = $"search/tv?query={Uri.EscapeDataString(title)}&language=en-US";
        if (year.HasValue) url += $"&first_air_date_year={year.Value}";

        var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(url, cancellationToken);
        var result = response?.Results?.FirstOrDefault();
        if (result == null) return null;

        var details = await _httpClient.GetFromJsonAsync<TmdbTvDetails>(
            $"tv/{result.Id}?append_to_response=credits&language=en-US", cancellationToken);

        if (details == null) return null;

        if (!string.IsNullOrEmpty(subtitle))
        {
            var (hasEpisode, episodeRuntime) = await FindEpisodeAsync(details.Id, details.Seasons, subtitle, cancellationToken);
            if (!hasEpisode)
            {
                return null; // Reject: subtitle did not match any episode in this TV show
            }

            var mapped = MapTvDetails(details);
            if (episodeRuntime.HasValue && episodeRuntime.Value > 0)
            {
                mapped = new TmdbMovieResultDto(
                    mapped.TmdbId,
                    episodeRuntime.Value,
                    mapped.PosterUrl,
                    mapped.Genres,
                    mapped.Directors,
                    mapped.Actors,
                    IsTvShow: true,
                    Title: mapped.Title,
                    FirstAirYear: mapped.FirstAirYear);
            }
            return mapped;
        }

        return MapTvDetails(details);
    }

    private async Task<(bool HasEpisode, int? Runtime)> FindEpisodeAsync(int tvShowId, List<TmdbSeasonItem>? seasons, string subtitle, CancellationToken cancellationToken)
    {
        if (seasons == null || seasons.Count == 0) return (false, null);

        var tasks = seasons.Select(async s =>
        {
            try
            {
                var url = $"tv/{tvShowId}/season/{s.SeasonNumber}?language=en-US";
                return await _httpClient.GetFromJsonAsync<TmdbSeasonDetails>(url, cancellationToken);
            }
            catch
            {
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        foreach (var season in results)
        {
            if (season?.Episodes == null) continue;
            foreach (var ep in season.Episodes)
            {
                if (string.Equals(ep.Name, subtitle, StringComparison.OrdinalIgnoreCase))
                {
                    return (true, ep.Runtime);
                }
            }
        }

        return (false, null);
    }


    // ── Mapping helpers ─────────────────────────────────────────────────────────

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

        var runtime = details.EpisodeRunTime?.FirstOrDefault();

        var genres = details.Genres.Select(g => new TmdbGenreDto(g.Id, g.Name)).ToList();

        var directors = details.CreatedBy
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList();

        var actors = details.Credits?.Cast?
            .OrderBy(c => c.Order)
            .Take(10)
            .Select(c => new TmdbPersonDto(c.Id, c.Name))
            .ToList() ?? new List<TmdbPersonDto>();

        int? firstAirYear = null;
        if (!string.IsNullOrEmpty(details.FirstAirDate) && DateTime.TryParse(details.FirstAirDate, out var date))
        {
            firstAirYear = date.Year;
        }

        return new TmdbMovieResultDto(
            details.Id,
            runtime,
            posterUrl,
            genres,
            directors,
            actors,
            IsTvShow: true,
            Title: details.Name,
            FirstAirYear: firstAirYear);
    }

    // ── Utility ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Strips subtitles after ':' or ' - ' to get a simpler search term.
    /// e.g. "Euphoria: Trouble Don't Last Always" → "Euphoria"
    /// </summary>
    private static string SimplifyTitle(string title)
    {
        var (mainTitle, _) = ParseTitleAndSubtitle(title);
        return mainTitle;
    }

    private static (string MainTitle, string? Subtitle) ParseTitleAndSubtitle(string title)
    {
        var colonIndex = title.IndexOf(':');
        if (colonIndex > 0)
        {
            return (title[..colonIndex].Trim(), title[(colonIndex + 1)..].Trim());
        }

        var dashIndex = title.IndexOf(" - ", StringComparison.Ordinal);
        if (dashIndex > 0)
        {
            return (title[..dashIndex].Trim(), title[(dashIndex + 3)..].Trim());
        }

        return (title, null);
    }
}

