using System.Net.Http.Json;
using Frametric.Application.DTOs;
using Frametric.Application.DTOs.EntityDetails;
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

        // Progressive fallback chain prioritizing exact matches first:
        var movieResult = await TrySearchMovieAsync(title, year, cancellationToken, requireExactMatch: true)
                       ?? await TrySearchTvAsync(title, year, cancellationToken, requireExactMatch: true)
                       ?? await TrySearchMovieAsync(title, year: null, cancellationToken, requireExactMatch: true)
                       ?? await TrySearchTvAsync(title, year: null, cancellationToken, requireExactMatch: true)
                       
                       // If no exact match is found, fallback to loose matches (first search result):
                       ?? await TrySearchMovieAsync(title, year, cancellationToken, requireExactMatch: false)
                       ?? await TrySearchTvAsync(title, year, cancellationToken, requireExactMatch: false)
                       ?? await TrySearchMovieAsync(title, year: null, cancellationToken, requireExactMatch: false)
                       ?? await TrySearchTvAsync(title, year: null, cancellationToken, requireExactMatch: false)
                       
                       // Try searching TV show first with subtitle matching if a subtitle was detected:
                       ?? (!string.IsNullOrEmpty(subtitle) ? await TrySearchTvAsync(simplifiedTitle, year: null, cancellationToken, subtitle) : null)
                       
                       // Standard simplified title fallbacks:
                       ?? (simplifiedTitle != title ? await TrySearchMovieAsync(simplifiedTitle, year: null, cancellationToken) : null)
                       ?? (simplifiedTitle != title ? await TrySearchTvAsync(simplifiedTitle, year: null, cancellationToken) : null);

        return movieResult;
    }

    public async Task<IEnumerable<GlobalSearchResultDto>> SearchMultiAsync(string query, CancellationToken cancellationToken)
    {
        var url = $"search/multi?query={Uri.EscapeDataString(query)}&language=en-US";
        var response = await GetWithRetryAsync<TmdbMultiSearchResponse>(url, cancellationToken);
        
        if (response?.Results == null || !response.Results.Any())
            return Enumerable.Empty<GlobalSearchResultDto>();

        return response.Results
            .Where(r => r.MediaType == "movie" || r.MediaType == "person")
            .Select(r => new GlobalSearchResultDto(
                null,
                r.Id,
                r.MediaType == "movie" ? "Movie" : "Actor", // TMDB uses person for both actor and director
                r.Title ?? r.Name ?? "Unknown",
                DateTime.TryParse(r.ReleaseDate, out var date) ? date.Year : null,
                !string.IsNullOrEmpty(r.PosterPath ?? r.ProfilePath) ? $"https://image.tmdb.org/t/p/w500{(r.PosterPath ?? r.ProfilePath)}" : null,
                false
            ))
            .Take(10)
            .ToList();
    }

    // ── Search helpers ──────────────────────────────────────────────────────────

    private async Task<TmdbMovieResultDto?> TrySearchMovieAsync(string title, int? year, CancellationToken cancellationToken, bool requireExactMatch = false)
    {
        var url = $"search/movie?query={Uri.EscapeDataString(title)}&language=en-US";
        if (year.HasValue) url += $"&year={year.Value}";

        var response = await GetWithRetryAsync<TmdbSearchResponse>(url, cancellationToken);
        if (response?.Results == null || !response.Results.Any()) return null;

        var result = requireExactMatch
            ? response.Results
                .Where(r => string.Equals(r.Title, title, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Popularity ?? 0.0)
                .FirstOrDefault()
            : response.Results
                .OrderByDescending(r => r.Popularity ?? 0.0)
                .FirstOrDefault();

        if (result == null) return null;

        return await GetMovieDetailsByIdAsync(result.Id, cancellationToken);
    }

    public async Task<TmdbMovieResultDto?> GetMovieDetailsByIdAsync(int tmdbId, CancellationToken cancellationToken)
    {
        var detailsTask = GetWithRetryAsync<TmdbMovieDetails>(
            $"movie/{tmdbId}?append_to_response=credits&language=en-US", cancellationToken);

        var keywordsTask = GetWithRetryAsync<TmdbKeywordsResponse>(
            $"movie/{tmdbId}/keywords", cancellationToken);

        var providersTask = GetWithRetryAsync<TmdbWatchProvidersResponse>(
            $"movie/{tmdbId}/watch/providers", cancellationToken);

        try
        {
            await Task.WhenAll(detailsTask, keywordsTask, providersTask);
        }
        catch
        {
            // Fallback: if keywords or providers fail, ensure detailsTask is still awaited or let it bubble up
        }

        var details = await detailsTask;
        if (details == null) return null;

        // Parse keywords
        string? keywords = null;
        try
        {
            var keywordsResp = await keywordsTask;
            if (keywordsResp?.Keywords != null && keywordsResp.Keywords.Any())
            {
                keywords = string.Join(";", keywordsResp.Keywords.Select(k => k.Name));
            }
        }
        catch { /* ignore keywords failure */ }

        // Parse providers
        string? providers = null;
        try
        {
            var providersResp = await providersTask;
            if (providersResp?.Results != null)
            {
                List<TmdbWatchProviderItem>? providersList = null;
                if (providersResp.Results.TryGetValue("ES", out var esProviders))
                {
                    providersList = esProviders.Flatrate;
                }
                if ((providersList == null || !providersList.Any()) && providersResp.Results.TryGetValue("US", out var usProviders))
                {
                    providersList = usProviders.Flatrate;
                }

                if (providersList != null && providersList.Any())
                {
                    providers = string.Join(";", providersList.Select(p => p.ProviderName));
                }
            }
        }
        catch { /* ignore providers failure */ }

        return MapMovieDetails(details, keywords, providers);
    }

    private async Task<TmdbMovieResultDto?> TrySearchTvAsync(string title, int? year, CancellationToken cancellationToken, string? subtitle = null, bool requireExactMatch = false)
    {
        var url = $"search/tv?query={Uri.EscapeDataString(title)}&language=en-US";
        if (year.HasValue) url += $"&first_air_date_year={year.Value}";

        var response = await GetWithRetryAsync<TmdbSearchResponse>(url, cancellationToken);
        if (response?.Results == null || !response.Results.Any()) return null;

        var result = requireExactMatch
            ? response.Results
                .Where(r => string.Equals(r.Name, title, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Popularity ?? 0.0)
                .FirstOrDefault()
            : response.Results
                .OrderByDescending(r => r.Popularity ?? 0.0)
                .FirstOrDefault();

        if (result == null) return null;

        var details = await GetWithRetryAsync<TmdbTvDetails>(
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
                return await GetWithRetryAsync<TmdbSeasonDetails>(url, cancellationToken);
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

    private static TmdbMovieResultDto MapMovieDetails(TmdbMovieDetails details, string? keywords = null, string? providers = null)
    {
        var posterUrl = !string.IsNullOrEmpty(details.PosterPath)
            ? $"https://image.tmdb.org/t/p/w500{details.PosterPath}"
            : null;

        var genres = details.Genres.Select(g => new TmdbGenreDto(g.Id, g.Name)).ToList();

        var directors = details.Credits?.Crew?
            .Where(c => c.Job == "Director")
            .Select(c => new TmdbPersonDto(c.Id, c.Name, !string.IsNullOrEmpty(c.ProfilePath) ? $"https://image.tmdb.org/t/p/w500{c.ProfilePath}" : null))
            .ToList() ?? new List<TmdbPersonDto>();

        var actors = details.Credits?.Cast?
            .OrderBy(c => c.Order)
            .Take(10)
            .Select(c => new TmdbPersonDto(c.Id, c.Name, !string.IsNullOrEmpty(c.ProfilePath) ? $"https://image.tmdb.org/t/p/w500{c.ProfilePath}" : null))
            .ToList() ?? new List<TmdbPersonDto>();

        return new TmdbMovieResultDto(
            details.Id, 
            details.Runtime, 
            posterUrl, 
            genres, 
            directors, 
            actors, 
            IsTvShow: false,
            TmdbRating: details.VoteAverage,
            TmdbPopularity: details.Popularity,
            ImdbId: details.ImdbId,
            ReleaseDate: details.ReleaseDate,
            Keywords: keywords,
            StreamingProviders: providers,
            Overview: details.Overview);
    }

    private static TmdbMovieResultDto MapTvDetails(TmdbTvDetails details)
    {
        var posterUrl = !string.IsNullOrEmpty(details.PosterPath)
            ? $"https://image.tmdb.org/t/p/w500{details.PosterPath}"
            : null;

        var runtime = details.EpisodeRunTime?.FirstOrDefault();

        var genres = details.Genres.Select(g => new TmdbGenreDto(g.Id, g.Name)).ToList();

        var directors = details.CreatedBy
            .Select(c => new TmdbPersonDto(c.Id, c.Name, !string.IsNullOrEmpty(c.ProfilePath) ? $"https://image.tmdb.org/t/p/w500{c.ProfilePath}" : null))
            .ToList();

        var actors = details.Credits?.Cast?
            .OrderBy(c => c.Order)
            .Take(10)
            .Select(c => new TmdbPersonDto(c.Id, c.Name, !string.IsNullOrEmpty(c.ProfilePath) ? $"https://image.tmdb.org/t/p/w500{c.ProfilePath}" : null))
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
    private async Task<T?> GetWithRetryAsync<T>(string url, CancellationToken cancellationToken, int maxRetries = 3)
    {
        int delayMs = 1000;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<T>(url, cancellationToken);
            }
            catch (HttpRequestException ex) when (i < maxRetries - 1 && (ex.StatusCode == System.Net.HttpStatusCode.BadGateway || ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || ex.StatusCode == System.Net.HttpStatusCode.GatewayTimeout || (int?)ex.StatusCode == 429))
            {
                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2; // Exponential backoff
            }
        }
        return await _httpClient.GetFromJsonAsync<T>(url, cancellationToken);
    }
}

