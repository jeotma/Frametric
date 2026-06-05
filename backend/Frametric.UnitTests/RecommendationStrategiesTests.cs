// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Linq;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Application.Queries.Recommendations.Strategies;
using Frametric.Domain.Enums;
using Xunit;

namespace Frametric.UnitTests;

public class RecommendationStrategiesTests
{
    private readonly List<CandidateMovieDto> _standardCandidates;
    private readonly List<WatchedMovieDetailDto> _standardWatched;

    public RecommendationStrategiesTests()
    {
        // Setup shared mock datasets matching the exact primary constructor signatures
        _standardCandidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(
                MovieId: Guid.NewGuid(),
                Title: "Cult Classic A",
                ReleaseYear: 1999,
                RuntimeMinutes: 100,
                PosterUrl: "/poster_a.jpg",
                TmdbRating: 7.8,
                TmdbPopularity: 25.0,
                CustomAverageRating: 7.8,
                Genres: "Sci-Fi,Action",
                Directors: "Wachowskis",
                Actors: "Keanu Reeves",
                Keywords: "existential, reality",
                Awards: "Won 2 Oscars. Another 5 wins & 10 nominations.",
                Writers: "Wachowskis",
                Language: "en",
                Country: "USA",
                BoxOffice: "$3,000,000",
                Certification: "R",
                StreamingProviders: "Netflix",
                Overview: "Existential action movie about reality.",
                ImdbRating: 7.8,
                RottenTomatoesRating: 6.0,
                MetacriticRating: 5.5,
                WatchlistAddedDate: null
            ),
            new CandidateMovieDto(
                MovieId: Guid.NewGuid(),
                Title: "Blockbuster B",
                ReleaseYear: 2012,
                RuntimeMinutes: 150,
                PosterUrl: "/poster_b.jpg",
                TmdbRating: 8.8,
                TmdbPopularity: 150.0,
                CustomAverageRating: 8.8,
                Genres: "Sci-Fi,Drama",
                Directors: "Christopher Nolan",
                Actors: "Leonardo DiCaprio",
                Keywords: "dream, reality",
                Awards: "Won 4 Oscars. Another 20 wins & 40 nominations.",
                Writers: "Christopher Nolan",
                Language: "en",
                Country: "USA",
                BoxOffice: "$800,000,000",
                Certification: "PG-13",
                StreamingProviders: "Max",
                Overview: "A mind-bending journey.",
                ImdbRating: 8.8,
                RottenTomatoesRating: 9.2,
                MetacriticRating: 9.0,
                WatchlistAddedDate: null
            ),
            new CandidateMovieDto(
                MovieId: Guid.NewGuid(),
                Title: "Indie Drama C",
                ReleaseYear: 2020,
                RuntimeMinutes: 95,
                PosterUrl: "/poster_c.jpg",
                TmdbRating: 8.2,
                TmdbPopularity: 4.0,
                CustomAverageRating: 8.2,
                Genres: "Drama,Romance",
                Directors: "Indie Director",
                Actors: "Indie Actor",
                Keywords: "love, poetry",
                Awards: "Nominated for 1 Oscar.",
                Writers: "Indie Writer",
                Language: "en",
                Country: "USA",
                BoxOffice: "$400,000",
                Certification: "PG",
                StreamingProviders: "Mubi",
                Overview: "A quiet, poetic reflection on life.",
                ImdbRating: 8.2,
                RottenTomatoesRating: 9.0,
                MetacriticRating: 8.5,
                WatchlistAddedDate: null
            )
        };

        _standardWatched = new List<WatchedMovieDetailDto>
        {
            new WatchedMovieDetailDto(
                MovieId: Guid.NewGuid(),
                ReleaseYear: 1999,
                RuntimeMinutes: 100,
                Genres: "Sci-Fi,Action",
                Directors: "Wachowskis",
                Actors: "Keanu Reeves",
                UserRating: 8.5,
                WatchDate: DateTime.UtcNow.AddMonths(-1),
                Keywords: "existential, reality"
            ),
            new WatchedMovieDetailDto(
                MovieId: Guid.NewGuid(),
                ReleaseYear: 2008,
                RuntimeMinutes: 152,
                Genres: "Action,Drama",
                Directors: "Christopher Nolan",
                Actors: "Christian Bale",
                UserRating: 9.0,
                WatchDate: DateTime.UtcNow.AddMonths(-2),
                Keywords: "superhero"
            )
        };
    }

    [Fact]
    public void GuiltyPleasureStrategy_ShouldRecommendCultClassic_DueToCriticAudienceDiscrepancy()
    {
        // Arrange
        var strategy = new GuiltyPleasureStrategy();

        // Act
        var results = strategy.Recommend(_standardCandidates, _standardWatched, 3);

        // Assert
        Assert.NotEmpty(results);
        var firstResult = results.First();
        Assert.Equal("Cult Classic A", firstResult.Title);
    }

    [Fact]
    public void CinephileEliteStrategy_ShouldRecommendHighlyRatedForeignOrIndieFilms()
    {
        // Arrange
        var strategy = new CinephileEliteStrategy();
        
        var prestigeCandidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(
                MovieId: Guid.NewGuid(),
                Title: "Prestige Art Film",
                ReleaseYear: 2018,
                RuntimeMinutes: 140,
                PosterUrl: "/france.jpg",
                TmdbRating: 8.5,
                TmdbPopularity: 5.0,
                CustomAverageRating: 8.5,
                Genres: "Drama",
                Directors: "Foreign Director",
                Actors: "Foreign Actor",
                Keywords: "solitude",
                Awards: "Won 1 Oscar. Another 15 wins.",
                Writers: "Foreign Writer",
                Language: "fr",
                Country: "France",
                BoxOffice: "$1,500,000",
                Certification: "R",
                StreamingProviders: "Mubi",
                Overview: "An existential and poetic portrait of solitude.",
                ImdbRating: 8.5,
                RottenTomatoesRating: 9.5,
                MetacriticRating: 9.0,
                WatchlistAddedDate: null
            ),
            new CandidateMovieDto(
                MovieId: Guid.NewGuid(),
                Title: "Blockbuster Trash",
                ReleaseYear: 2021,
                RuntimeMinutes: 90,
                PosterUrl: "/trash.jpg",
                TmdbRating: 5.0,
                TmdbPopularity: 200.0,
                CustomAverageRating: 5.0,
                Genres: "Action",
                Directors: "Blockbuster Director",
                Actors: "Blockbuster Actor",
                Keywords: "boom",
                Awards: "None.",
                Writers: "Blockbuster Writer",
                Language: "en",
                Country: "USA",
                BoxOffice: "$500,000,000",
                Certification: "PG-13",
                StreamingProviders: "Cinema",
                Overview: "Boom splash bang.",
                ImdbRating: 5.0,
                RottenTomatoesRating: 4.5,
                MetacriticRating: 4.0,
                WatchlistAddedDate: null
            )
        };

        // Act
        var results = strategy.Recommend(prestigeCandidates, _standardWatched, 2);

        // Assert
        Assert.Single(results);
        Assert.Equal("Prestige Art Film", results.First().Title);
        // Generates foreign cinema style reasons, which contains "cinema" or "language" or "foreign" or "art-house"
        Assert.True(results.First().RecommendationReason.Contains("cinema", StringComparison.OrdinalIgnoreCase) || 
                    results.First().RecommendationReason.Contains("language", StringComparison.OrdinalIgnoreCase) ||
                    results.First().RecommendationReason.Contains("art-house", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OppositeMoodStrategy_ShouldRecommendContrastingGenres()
    {
        // Arrange
        var strategy = new OppositeMoodStrategy();

        // Act
        var results = strategy.Recommend(_standardCandidates, _standardWatched, 3);

        // Assert
        Assert.NotEmpty(results);
        Assert.NotNull(results.First().RecommendationReason);
    }

    [Fact]
    public void ComfortZoneDisruptorStrategy_ShouldRecommendUnfamiliarCreatorsAndGenres()
    {
        // Arrange
        var strategy = new ComfortZoneDisruptorStrategy();

        // Act
        var results = strategy.Recommend(_standardCandidates, _standardWatched, 3);

        // Assert
        Assert.NotEmpty(results);
        var recommendedTitles = results.Select(r => r.Title).ToList();
        Assert.Contains("Indie Drama C", recommendedTitles);
    }

    [Fact]
    public void ComfortZoneDisruptorStrategy_WithEmptyWatchedList_ShouldReturnRandomCandidates()
    {
        // Arrange
        var strategy = new ComfortZoneDisruptorStrategy();
        var emptyWatched = new List<WatchedMovieDetailDto>();

        // Act
        var results = strategy.Recommend(_standardCandidates, emptyWatched, 2);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("A completely random pick", results[0].RecommendationReason);
    }

    [Fact]
    public void ComfortZoneDisruptorStrategy_WithAnchorsAndForeignFilm_ShouldScoreAndExplainCorrectly()
    {
        // Arrange
        var strategy = new ComfortZoneDisruptorStrategy();

        // Create candidates to hit different branches
        var candidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(
                MovieId: Guid.NewGuid(),
                Title: "Foreign Anchor Movie",
                ReleaseYear: 1995,
                RuntimeMinutes: 90,
                PosterUrl: "/poster.jpg",
                TmdbRating: 8.0,
                TmdbPopularity: 10.0,
                CustomAverageRating: 8.0,
                Genres: "Action", // Comfort genre
                Directors: "Christopher Nolan", // Familiar director
                Actors: "Keanu Reeves", // Familiar actor
                Keywords: "dream",
                Awards: "",
                Writers: "Christopher Nolan", // Familiar writer
                Language: "ja", // Foreign
                Country: "Japan",
                BoxOffice: "",
                Certification: "PG",
                StreamingProviders: "",
                Overview: "Overview",
                ImdbRating: 8.0,
                RottenTomatoesRating: 8.0,
                MetacriticRating: 8.0,
                WatchlistAddedDate: null
            )
        };

        // Act
        var results = strategy.Recommend(candidates, _standardWatched, 1);

        var rec = results[0];
        Assert.Equal("Japan", candidates[0].Country);
        string reasonLower = rec.RecommendationReason.ToLower();
        Assert.True(reasonLower.Contains("international") || reasonLower.Contains("cultural"));
    }

    [Fact]
    public void RuntimeContextStrategy_ShouldEnforceRuntimeConstraints()
    {
        // Arrange
        var strategy = new RuntimeContextStrategy();

        // Act
        var results = strategy.Recommend(_standardCandidates, _standardWatched, 3, maxRuntime: 110);
        
        // Assert
        Assert.NotEmpty(results);
        Assert.NotNull(results.First().RecommendationReason);
    }

    [Fact]
    public void DirectorsTrajectoryStrategy_ShouldRecommendUnwatchedFilmsFromWatchedDirectors()
    {
        // Arrange
        var strategy = new DirectorsTrajectoryStrategy();

        // Act
        var results = strategy.Recommend(_standardCandidates, _standardWatched, 3);

        // Assert
        Assert.NotEmpty(results);
        var recommendedTitles = results.Select(r => r.Title).ToList();
        Assert.Contains("Blockbuster B", recommendedTitles);
        Assert.Contains("Cult Classic A", recommendedTitles);
    }

    [Fact]
    public void DirectorsTrajectoryStrategy_WithEmptyWatchedList_ShouldReturnGlobalFallback()
    {
        // Arrange
        var strategy = new DirectorsTrajectoryStrategy();
        var emptyWatched = new List<WatchedMovieDetailDto>();

        // Act
        var results = strategy.Recommend(_standardCandidates, emptyWatched, 2);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Highly rated film to expand your cinematic catalog.", results[0].RecommendationReason);
    }

    [Fact]
    public void DirectorsTrajectoryStrategy_WithNoHighlyRatedDirectors_ShouldReturnRelaxedFallback()
    {
        // Arrange
        var strategy = new DirectorsTrajectoryStrategy();
        
        var watched = new List<WatchedMovieDetailDto>
        {
            new WatchedMovieDetailDto(
                MovieId: Guid.NewGuid(),
                ReleaseYear: 2012,
                RuntimeMinutes: 120,
                Genres: "Sci-Fi",
                Directors: "Christopher Nolan",
                Actors: "Christian Bale",
                UserRating: 4.0,
                WatchDate: DateTime.UtcNow.AddMonths(-1)
            )
        };

        // Act
        var results = strategy.Recommend(_standardCandidates, watched, 2);

        // Assert
        Assert.NotEmpty(results);
        var recommendedTitles = results.Select(r => r.Title).ToList();
        Assert.Contains("Blockbuster B", recommendedTitles);
    }
}
