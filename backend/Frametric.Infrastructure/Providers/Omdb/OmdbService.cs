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
using System.Net.Http;
using System.Net.Http.Json;
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

    public OmdbService(HttpClient httpClient, IConfiguration configuration, ILogger<OmdbService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Omdb:ApiKey"] ?? configuration["Omdb:Apikey"]; // Support case variations
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

        try
        {
            var url = $"?apikey={_apiKey}&i={Uri.EscapeDataString(imdbId)}";
            var response = await _httpClient.GetFromJsonAsync<OmdbResponse>(url, cancellationToken);

            if (response == null || !string.Equals(response.Response, "True", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("OMDb search failed or movie not found for IMDb ID: {ImdbId}", imdbId);
                return null;
            }

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

            return new OmdbRatingsDto(imdbRating, rottenTomatoesRating, metacriticRating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ratings from OMDb for IMDb ID: {ImdbId}", imdbId);
            return null;
        }
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
}

public class OmdbRatingItem
{
    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    [JsonPropertyName("Value")]
    public string? Value { get; set; }
}
