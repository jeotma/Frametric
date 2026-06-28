// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Frametric.Infrastructure.Providers.Omdb;

public class OmdbService : IOmdbService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly ILogger<OmdbService> _logger;
    private readonly string _cacheDirectory;

    public OmdbService(HttpClient httpClient, IConfiguration configuration, ILogger<OmdbService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Omdb:ApiKey"] ?? configuration["Omdb:Apikey"]; // Support case variations
        _cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), configuration["Omdb:CacheDirectory"] ?? "omdb_cache");
    }

    public async Task<OmdbRatingsDto?> GetMovieRatingsAsync(string imdbId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("OMDb API key is not configured. Skipping OMDb rating enrichment.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(imdbId))
        {
            return null;
        }

        if (!Directory.Exists(_cacheDirectory))
        {
            try
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create OMDb cache directory.");
            }
        }

        var cacheFile = Path.Combine(_cacheDirectory, $"{imdbId}.json");
        if (File.Exists(cacheFile))
        {
            try
            {
                var cachedJson = await File.ReadAllTextAsync(cacheFile, cancellationToken);
                var cachedResponse = System.Text.Json.JsonSerializer.Deserialize<OmdbResponse>(cachedJson);
                if (cachedResponse != null && string.Equals(cachedResponse.Response, "True", StringComparison.OrdinalIgnoreCase))
                {
                    return MapResponse(cachedResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read cached OMDb response for {ImdbId}", imdbId);
            }
        }

        try
        {
            var url = $"?apikey={_apiKey}&i={Uri.EscapeDataString(imdbId)}";
            var responseString = await _httpClient.GetStringAsync(url, cancellationToken);

            var response = System.Text.Json.JsonSerializer.Deserialize<OmdbResponse>(responseString);

            if (response == null || !string.Equals(response.Response, "True", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("OMDb search failed or movie not found for IMDb ID: {ImdbId}", imdbId);
                return null;
            }

            // Save raw response to cache file
            try
            {
                await File.WriteAllTextAsync(cacheFile, responseString, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save OMDb response to cache for {ImdbId}", imdbId);
            }

            return MapResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ratings from OMDb for IMDb ID: {ImdbId}", imdbId);
            return null;
        }
    }

    private OmdbRatingsDto MapResponse(OmdbResponse response)
    {
        double? imdbRating = ParseImdbRating(response.ImdbRating);
        double? rottenTomatoesRating = null;
        double? metacriticRating = null;

        if (response.Ratings != null)
        {
            foreach (var ratingItem in response.Ratings)
            {
                if (string.Equals(ratingItem.Source, "Internet Movie Database", StringComparison.OrdinalIgnoreCase))
                {
                    var parsed = ParseImdbRating(ratingItem.Value);
                    if (parsed.HasValue) imdbRating = parsed;
                }
                else if (string.Equals(ratingItem.Source, "Rotten Tomatoes", StringComparison.OrdinalIgnoreCase))
                {
                    rottenTomatoesRating = ParseRottenTomatoesRating(ratingItem.Value);
                }
                else if (string.Equals(ratingItem.Source, "Metacritic", StringComparison.OrdinalIgnoreCase))
                {
                    metacriticRating = ParseMetacriticRating(ratingItem.Value);
                }
            }
        }

        return new OmdbRatingsDto(
            imdbRating, 
            rottenTomatoesRating, 
            metacriticRating,
            Writers: response.Writer,
            Awards: response.Awards,
            BoxOffice: response.BoxOffice,
            Language: response.Language,
            Country: response.Country,
            Rated: response.Rated
        );
    }

    private static double? ParseImdbRating(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            return null;

        var clean = value.Split('/').First().Trim();
        if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
        {
            return parsed;
        }
        return null;
    }

    private static double? ParseRottenTomatoesRating(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            return null;

        var clean = value.Replace("%", "").Trim();
        if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
        {
            return parsed / 10.0; // Normalize percentage to a 10-point scale
        }
        return null;
    }

    private static double? ParseMetacriticRating(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            return null;

        var clean = value.Split('/').First().Trim();
        if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
        {
            return parsed / 10.0; // Normalize 100-point scale to a 10-point scale
        }
        return null;
    }
}

public class OmdbResponse
{
    [JsonPropertyName("imdbRating")]
    public string? ImdbRating { get; set; }

    [JsonPropertyName("Ratings")]
    public List<OmdbRatingItem>? Ratings { get; set; }

    [JsonPropertyName("Response")]
    public string? Response { get; set; }

    [JsonPropertyName("Writer")]
    public string? Writer { get; set; }

    [JsonPropertyName("Awards")]
    public string? Awards { get; set; }

    [JsonPropertyName("BoxOffice")]
    public string? BoxOffice { get; set; }

    [JsonPropertyName("Language")]
    public string? Language { get; set; }

    [JsonPropertyName("Country")]
    public string? Country { get; set; }

    [JsonPropertyName("Rated")]
    public string? Rated { get; set; }
}

public class OmdbRatingItem
{
    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    [JsonPropertyName("Value")]
    public string? Value { get; set; }
}
