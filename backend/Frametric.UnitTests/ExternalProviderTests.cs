// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Infrastructure.Providers.Omdb;
using Frametric.Infrastructure.Providers.Tmdb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Frametric.UnitTests;

public class ExternalProviderTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<OmdbService>> _omdbLoggerMock;

    public ExternalProviderTests()
    {
        _configMock = new Mock<IConfiguration>();
        _omdbLoggerMock = new Mock<ILogger<OmdbService>>();

        // Setup OMDb API key
        _configMock.Setup(c => c["Omdb:ApiKey"]).Returns("test_api_key");
    }

    [Fact]
    public async Task OmdbService_ShouldFetchAndMapRatingsSuccessfully()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""Response"": ""True"",
            ""imdbRating"": ""8.5"",
            ""Writer"": ""Christopher Nolan"",
            ""Awards"": ""Won 4 Oscars."",
            ""BoxOffice"": ""$800,000,000"",
            ""Language"": ""English"",
            ""Country"": ""USA"",
            ""Rated"": ""PG-13"",
            ""Ratings"": [
                { ""Source"": ""Internet Movie Database"", ""Value"": ""8.5/10"" },
                { ""Source"": ""Rotten Tomatoes"", ""Value"": ""92%"" },
                { ""Source"": ""Metacritic"", ""Value"": ""90/100"" }
            ]
        }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://www.omdbapi.com/")
        };

        var service = new OmdbService(httpClient, _configMock.Object, _omdbLoggerMock.Object);

        // Act
        var result = await service.GetMovieRatingsAsync("tt1375666", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8.5, result.ImdbRating);
        Assert.Equal(9.2, result.RottenTomatoesRating); // Normalized: 92% -> 9.2
        Assert.Equal(9.0, result.MetacriticRating); // Normalized: 90/100 -> 9.0
        Assert.Equal("Christopher Nolan", result.Writers);
        Assert.Equal("Won 4 Oscars.", result.Awards);
    }

    [Fact]
    public async Task TmdbService_ShouldMapSearchMultiCorrectly()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""results"": [
                {
                    ""id"": 101,
                    ""media_type"": ""movie"",
                    ""title"": ""Inception"",
                    ""release_date"": ""2010-07-16"",
                    ""poster_path"": ""/inception.jpg""
                },
                {
                    ""id"": 202,
                    ""media_type"": ""person"",
                    ""name"": ""Leonardo DiCaprio"",
                    ""profile_path"": ""/dicaprio.jpg""
                }
            ]
        }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };

        var service = new TmdbService(httpClient);

        // Act
        var results = (await service.SearchMultiAsync("inception", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        
        var movie = results.First(r => r.EntityType == "Movie");
        Assert.Equal("Inception", movie.TitleOrName);
        Assert.Equal(2010, movie.ReleaseYear);
        Assert.Contains("/inception.jpg", movie.ImageUrl);

        var actor = results.First(r => r.EntityType == "Actor");
        Assert.Equal("Leonardo DiCaprio", actor.TitleOrName);
        Assert.Contains("/dicaprio.jpg", actor.ImageUrl);
    }

    [Fact]
    public async Task OmdbService_ShouldReturnNull_WhenApiKeyMissing()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Omdb:ApiKey"]).Returns((string)null);

        var service = new OmdbService(new HttpClient(), configMock.Object, _omdbLoggerMock.Object);

        // Act
        var result = await service.GetMovieRatingsAsync("tt1234567", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task OmdbService_ShouldReturnNull_WhenImdbIdMissing()
    {
        // Arrange
        var service = new OmdbService(new HttpClient(), _configMock.Object, _omdbLoggerMock.Object);

        // Act
        var result1 = await service.GetMovieRatingsAsync(null, CancellationToken.None);
        var result2 = await service.GetMovieRatingsAsync("", CancellationToken.None);

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public async Task OmdbService_ShouldReturnNull_WhenResponseIsFalse()
    {
        // Arrange
        var jsonResponse = "{\"Response\":\"False\"}";
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://www.omdbapi.com/")
        };

        var service = new OmdbService(httpClient, _configMock.Object, _omdbLoggerMock.Object);

        // Act
        var result = await service.GetMovieRatingsAsync("ttNonExistent_" + Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TmdbService_SearchAndGetMovieDetailsAsync_ShouldReturnMovie_WhenExactMatchFound()
    {
        // Arrange
        var searchMovieJson = "{\"results\":[{\"id\":101,\"title\":\"Inception\",\"popularity\":10.0}]}";
        var movieDetailsJson = "{\"id\":101,\"runtime\":148,\"poster_path\":\"/inception.jpg\",\"genres\":[{\"id\":1,\"name\":\"Action\"}],\"credits\":{\"crew\":[{\"id\":2,\"name\":\"Christopher Nolan\",\"job\":\"Director\"}],\"cast\":[{\"id\":3,\"name\":\"Leonardo DiCaprio\",\"order\":0}]},\"vote_average\":8.8,\"popularity\":120.5,\"imdb_id\":\"tt1375666\",\"release_date\":\"2010-07-16\",\"overview\":\"dream\"}";
        var keywordsJson = "{\"keywords\":[{\"id\":1,\"name\":\"dream\"}]}";
        var providersJson = "{\"results\":{\"US\":{\"flatrate\":[{\"provider_name\":\"Netflix\"}]}}}";

        var handlerMock = new Mock<HttpMessageHandler>();
        var seq = handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        // search/movie exact match
        seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(searchMovieJson) });
        // movie/{id} details
        seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(movieDetailsJson) });
        // movie/{id}/keywords
        seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(keywordsJson) });
        // movie/{id}/watch/providers
        seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(providersJson) });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.themoviedb.org/3/") };
        var service = new TmdbService(httpClient);

        // Act
        var result = await service.SearchAndGetMovieDetailsAsync("Inception", 2010, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(101, result.TmdbId);
        Assert.Equal("Action", result.Genres[0].Name);
        Assert.Equal("Christopher Nolan", result.Directors[0].Name);
        Assert.Equal("Leonardo DiCaprio", result.Actors[0].Name);
        Assert.Equal("dream", result.Keywords);
        Assert.Equal("Netflix", result.StreamingProviders);
    }

    [Fact]
    public async Task TmdbService_SearchAndGetMovieDetailsAsync_ShouldReturnTvShow_WhenSubtitleMatchFound()
    {
        // Arrange
        // We will trigger a TV show search with subtitle. Let's pass title: "Euphoria: Trouble Don't Last Always"
        // progressive fallback triggers: TrySearchMovie (requireExactMatch: true) -> TrySearchTv (requireExactMatch: true) -> ...
        // Eventually we mock searches. Let's return empty results for everything until it hits subtitle search fallback or loose searches.
        // Let's just return empty results for first 4 calls, then return a TV search result for the subtitle search call.
        var emptyJson = "{\"results\":[]}";
        var tvSearchJson = "{\"results\":[{\"id\":202,\"name\":\"Euphoria\",\"popularity\":25.0}]}";
        var tvDetailsJson = "{\"id\":202,\"name\":\"Euphoria\",\"episode_run_time\":[55],\"poster_path\":\"/euphoria.jpg\",\"genres\":[{\"id\":2,\"name\":\"Drama\"}],\"created_by\":[{\"id\":10,\"name\":\"Sam Levinson\"}],\"credits\":{\"cast\":[{\"id\":11,\"name\":\"Zendaya\",\"order\":0}]},\"first_air_date\":\"2019-06-16\",\"seasons\":[{\"season_number\":1}]}";
        var seasonDetailsJson = "{\"episodes\":[{\"name\":\"Trouble Don't Last Always\",\"runtime\":57}]}";

        var handlerMock = new Mock<HttpMessageHandler>();
        var seq = handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        // Let's mock 8 fallbacks as empty, then the subtitle-based TV search
        for (int i = 0; i < 8; i++)
        {
            seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(emptyJson) });
        }
        
        // Subtitle TV search: search/tv
        seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(tvSearchJson) });
        // tv/{id} details
        seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(tvDetailsJson) });
        // tv/{id}/season/{number} details
        seq.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(seasonDetailsJson) });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.themoviedb.org/3/") };
        var service = new TmdbService(httpClient);

        // Act
        var result = await service.SearchAndGetMovieDetailsAsync("Euphoria: Trouble Don't Last Always", 2019, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(202, result.TmdbId);
        Assert.True(result.IsTvShow);
        Assert.Equal(57, result.RuntimeMinutes); // episode runtime mapped
    }

    [Fact]
    public async Task TmdbService_SearchMultiAsync_ShouldReturnEmpty_WhenNoResults()
    {
        // Arrange
        var jsonResponse = "{\"results\":[]}";
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.themoviedb.org/3/") };
        var service = new TmdbService(httpClient);

        // Act
        var results = await service.SearchMultiAsync("nonexistent", CancellationToken.None);

        // Assert
        Assert.Empty(results);
    }
}
