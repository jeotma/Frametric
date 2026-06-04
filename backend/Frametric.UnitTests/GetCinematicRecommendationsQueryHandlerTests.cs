// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Application.Queries.Recommendations;
using Frametric.Application.Queries.Recommendations.Strategies;
using Frametric.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Frametric.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Frametric.Application.Interfaces;
using Frametric.Infrastructure.Providers.Tmdb;
using Frametric.Infrastructure.Providers.Omdb;
using MediatR;

namespace Frametric.UnitTests;

public class GetCinematicRecommendationsQueryHandlerTests
{
    private readonly Mock<IRecommendationQueries> _queriesMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<GetCinematicRecommendationsQueryHandler>> _loggerMock;
    private readonly GetCinematicRecommendationsQueryHandler _handler;

    public GetCinematicRecommendationsQueryHandlerTests()
    {
        _queriesMock = new Mock<IRecommendationQueries>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<GetCinematicRecommendationsQueryHandler>>();

        var strategies = new List<IRecommendationStrategy>
        {
            new RecentMoodStrategy(),
            new OppositeMoodStrategy(),
            new ComfortZoneDisruptorStrategy(),
            new GuiltyPleasureStrategy(),
            new CinephileEliteStrategy(),
            new DirectorsTrajectoryStrategy(),
            new RuntimeContextStrategy(),
            new PureRandomStrategy()
        };

        _handler = new GetCinematicRecommendationsQueryHandler(
            _queriesMock.Object,
            _cacheMock.Object,
            _loggerMock.Object,
            strategies
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoCandidatesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queriesMock.Setup(q => q.GetWatchedMovieDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchedMovieDetailDto>());
        _queriesMock.Setup(q => q.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CandidateMovieDto>());

        var query = new GetCinematicRecommendationsQuery(userId, RecommendationStrategy.RecentMood, RecommendationScope.Hybrid, 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldRecommendHighlyRated_WhenUserHasNoWatchHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var candidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(Guid.NewGuid(), "Acclaimed Film A", 2020, 120, null, 8.5, 100, 8.5, "Drama", "Director A", "Actor A"),
            new CandidateMovieDto(Guid.NewGuid(), "Acclaimed Film B", 2021, 110, null, 8.0, 90, 8.0, "Comedy", "Director B", "Actor B")
        };

        _queriesMock.Setup(q => q.GetWatchedMovieDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchedMovieDetailDto>());
        _queriesMock.Setup(q => q.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        var query = new GetCinematicRecommendationsQuery(userId, RecommendationStrategy.RecentMood, RecommendationScope.Hybrid, 3);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Acclaimed Film A", result[0].Title);
        Assert.Equal("Acclaimed Film B", result[1].Title);
    }

    [Fact]
    public async Task Handle_ShouldFilterOutSkippedMovies_WhenIdsAreCached()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var skippedMovieId = Guid.NewGuid();
        var normalMovieId = Guid.NewGuid();

        var candidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(skippedMovieId, "Skipped Movie", 2020, 100, null, 7.5, 50, 7.5, "Drama", "Director A", "Actor A"),
            new CandidateMovieDto(normalMovieId, "Normal Movie", 2021, 100, null, 7.5, 50, 7.5, "Drama", "Director B", "Actor B")
        };

        _queriesMock.Setup(q => q.GetWatchedMovieDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchedMovieDetailDto>
            {
                new WatchedMovieDetailDto(Guid.NewGuid(), 2019, 100, "Drama", "Director C", "Actor C", 8.0, DateTime.UtcNow)
            });

        _queriesMock.Setup(q => q.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        // Setup cache mock: return value "skipped" only for the skippedMovieId
        _cacheMock.Setup(c => c.GetAsync($"skip_recommendation:{userId}:{skippedMovieId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("skipped"));
        _cacheMock.Setup(c => c.GetAsync($"skip_recommendation:{userId}:{normalMovieId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var query = new GetCinematicRecommendationsQuery(userId, RecommendationStrategy.RecentMood, RecommendationScope.Hybrid, 3);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(normalMovieId, result[0].MovieId);
        Assert.Equal("Normal Movie", result[0].Title);
    }

    [Fact]
    public async Task Handle_ShouldHaveUniqueMatchPercentages_EvenForSimilarCandidates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var candidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(Guid.NewGuid(), "Identical Film A", 2020, 100, null, 7.5, 50, 7.5, "Drama", "Director A", "Actor A"),
            new CandidateMovieDto(Guid.NewGuid(), "Identical Film B", 2020, 100, null, 7.5, 50, 7.5, "Drama", "Director A", "Actor A")
        };

        _queriesMock.Setup(q => q.GetWatchedMovieDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchedMovieDetailDto>
            {
                new WatchedMovieDetailDto(Guid.NewGuid(), 2020, 100, "Drama", "Director A", "Actor A", 8.0, DateTime.UtcNow)
            });

        _queriesMock.Setup(q => q.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        var query = new GetCinematicRecommendationsQuery(userId, RecommendationStrategy.RecentMood, RecommendationScope.Hybrid, 2);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].MatchPercentage >= result[1].MatchPercentage);
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenCandidateHasDuplicateKeywordsOrGenres()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var candidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(Guid.NewGuid(), "Duplicate Theme Film", 2020, 100, null, 7.5, 50, 7.5, 
                "Drama, Drama, Comedy", "Director A", "Actor A", "new york city, new york city, drama")
        };

        _queriesMock.Setup(q => q.GetWatchedMovieDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchedMovieDetailDto>
            {
                new WatchedMovieDetailDto(Guid.NewGuid(), 2020, 100, "Drama", "Director A", "Actor A", 8.0, DateTime.UtcNow, "new york city, drama")
            });

        _queriesMock.Setup(q => q.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        var query = new GetCinematicRecommendationsQuery(userId, RecommendationStrategy.RecentMood, RecommendationScope.Hybrid, 2);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Duplicate Theme Film", result[0].Title);
    }

    [Fact]
    public async Task Handle_ShouldTriggerWellnessCheck_WhenUserHasHeavyStreaks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var candidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(Guid.NewGuid(), "Acclaimed Film A", 2020, 120, null, 8.5, 100, 8.5, "Drama", "Director A", "Actor A")
        };

        var now = DateTime.UtcNow;
        var watched = new List<WatchedMovieDetailDto>
        {
            new WatchedMovieDetailDto(Guid.NewGuid(), 2010, 100, "Drama", "Director B", "Actor B", 8.0, now.AddHours(-5), "existential dread"),
            new WatchedMovieDetailDto(Guid.NewGuid(), 2011, 100, "Horror", "Director C", "Actor C", 8.0, now.AddHours(-3), "psychological insanity"),
            new WatchedMovieDetailDto(Guid.NewGuid(), 2012, 100, "Thriller", "Director D", "Actor D", 8.0, now.AddHours(-1), "disturbing dread")
        };

        _queriesMock.Setup(q => q.GetWatchedMovieDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watched);
        _queriesMock.Setup(q => q.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);
        _cacheMock.Setup(c => c.GetAsync($"skip_wellness_check:{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var query = new GetCinematicRecommendationsQuery(userId, RecommendationStrategy.RecentMood, RecommendationScope.Hybrid, 1);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.NotNull(result[0].WellnessCheckMessage);
        Assert.Contains("psychological or existential", result[0].WellnessCheckMessage);
    }

    [Fact]
    public async Task Handle_ShouldNotTriggerWellnessCheck_WhenDismissed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var candidates = new List<CandidateMovieDto>
        {
            new CandidateMovieDto(Guid.NewGuid(), "Acclaimed Film A", 2020, 120, null, 8.5, 100, 8.5, "Drama", "Director A", "Actor A")
        };

        var now = DateTime.UtcNow;
        var watched = new List<WatchedMovieDetailDto>
        {
            new WatchedMovieDetailDto(Guid.NewGuid(), 2010, 100, "Drama", "Director B", "Actor B", 8.0, now.AddHours(-5), "existential dread"),
            new WatchedMovieDetailDto(Guid.NewGuid(), 2011, 100, "Horror", "Director C", "Actor C", 8.0, now.AddHours(-3), "psychological insanity"),
            new WatchedMovieDetailDto(Guid.NewGuid(), 2012, 100, "Thriller", "Director D", "Actor D", 8.0, now.AddHours(-1), "disturbing dread")
        };

        _queriesMock.Setup(q => q.GetWatchedMovieDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(watched);
        _queriesMock.Setup(q => q.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);
        _cacheMock.Setup(c => c.GetAsync($"skip_wellness_check:{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("dismissed"));

        var query = new GetCinematicRecommendationsQuery(userId, RecommendationStrategy.RecentMood, RecommendationScope.Hybrid, 1);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Null(result[0].WellnessCheckMessage);
    }
}
