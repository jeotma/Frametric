// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Infrastructure.Queries;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class WatchlistQueriesTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;

    public WatchlistQueriesTests()
    {
        _dbConnectionFactoryMock = new Mock<IDbConnectionFactory>();
    }

    private static DbDataReader CreateDataReader<T>(IEnumerable<T> items)
    {
        return new MultiResultSetDbDataReader(new IEnumerable<object>[] { items.Cast<object>() });
    }

    private void SetupDb(DbDataReader reader, object scalarResult = null)
    {
        var dbCommand = new TestDbCommand(reader, scalarResult ?? 0);
        var dbConnection = new TestDbConnection(dbCommand);
        _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(dbConnection);
    }

    [Fact]
    public async Task GetWatchlistAsync_ShouldReturnWatchlist()
    {
        // Arrange
        var watchlist = new[]
        {
            new { Title = "Interstellar", ReleaseYear = 2014, Director = "Christopher Nolan", DateAdded = "June 2023", CustomAverageRating = 4.6, PosterUrl = "poster" }
        };
        var reader = CreateDataReader(watchlist);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchlistAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Interstellar", result.First().Title);
    }

    [Fact]
    public async Task GetWatchlistDirectorsAsync_ShouldReturnDirectors()
    {
        // Arrange
        var directors = new[]
        {
            new { DirectorName = "Spielberg", Count = 3, AverageRating = 4.2, WatchedCount = 10 }
        };
        var reader = CreateDataReader(directors);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchlistDirectorsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Spielberg", result.First().DirectorName);
    }

    [Fact]
    public async Task GetWatchlistActorsAsync_ShouldReturnActors()
    {
        // Arrange
        var actors = new[]
        {
            new { ActorName = "Hanks", Count = 4, AverageRating = 4.4, WatchedCount = 12 }
        };
        var reader = CreateDataReader(actors);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchlistActorsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Hanks", result.First().ActorName);
    }

    [Fact]
    public async Task GetWatchlistByGenreAsync_ShouldReturnGenres()
    {
        // Arrange
        var genres = new[]
        {
            new { GenreName = "Adventure", Count = 8, WatchedCount = 20 }
        };
        var reader = CreateDataReader(genres);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchlistByGenreAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Adventure", result.First().GenreName);
    }

    [Fact]
    public async Task GetWatchlistByDecadeAsync_ShouldReturnDecades()
    {
        // Arrange
        var decades = new[]
        {
            new { Decade = 2000, Count = 10 }
        };
        var reader = CreateDataReader(decades);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchlistByDecadeAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2000, result.First().Decade);
    }

    [Fact]
    public async Task GetMostAnticipatedDirectorAsync_ShouldReturnDirector()
    {
        // Arrange
        var director = new[]
        {
            new { DirectorName = "Fincher", Count = 3, AverageRating = 4.5, WatchedCount = 6 }
        };
        var reader = CreateDataReader(director);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMostAnticipatedDirectorAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fincher", result.DirectorName);
    }

    [Fact]
    public async Task GetMostAnticipatedActorAsync_ShouldReturnActor()
    {
        // Arrange
        var actor = new[]
        {
            new { ActorName = "Bale", Count = 4, AverageRating = 4.6, WatchedCount = 8 }
        };
        var reader = CreateDataReader(actor);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMostAnticipatedActorAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bale", result.ActorName);
    }

    [Fact]
    public async Task GetTotalPendingWatchtimeAsync_ShouldReturnWatchtime()
    {
        // Arrange
        var watchtime = new[]
        {
            new { Name = "Total Watchlist", TotalMinutes = 1200, TotalHours = 20 }
        };
        var reader = CreateDataReader(watchtime);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetTotalPendingWatchtimeAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Total Watchlist", result.Name);
        Assert.Equal(1200, result.TotalMinutes);
        Assert.Equal(20, result.TotalHours);
    }

    [Fact]
    public async Task GetOldestPendingMovieAsync_ShouldReturnMovie()
    {
        // Arrange
        var movie = new[]
        {
            new { Id = Guid.NewGuid(), Title = "Metropolis", ReleaseYear = 1927, PosterPath = "path" }
        };
        var reader = CreateDataReader(movie);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetOldestPendingMovieAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Metropolis", result.Title);
    }

    [Fact]
    public async Task GetGenreProportionWatchlistVsWatchedAsync_ShouldReturnProportions()
    {
        // Arrange
        var proportions = new[]
        {
            new { GenreName = "Fantasy", WatchedCount = 15, PendingCount = 5 }
        };
        var reader = CreateDataReader(proportions);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetGenreProportionWatchlistVsWatchedAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Fantasy", result.First().GenreName);
        Assert.Equal(15, result.First().WatchedCount);
        Assert.Equal(5, result.First().PendingCount);
    }

    [Fact]
    public async Task GetGoldenPendingDirectorAsync_ShouldReturnDirector()
    {
        // Arrange
        var director = new[]
        {
            new { DirectorName = "Villeneuve", AverageRatingInHistory = 4.7, PendingMoviesCount = 2 }
        };
        var reader = CreateDataReader(director);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetGoldenPendingDirectorAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Villeneuve", result.DirectorName);
        Assert.Equal(4.7, result.AverageRatingInHistory);
        Assert.Equal(2, result.PendingMoviesCount);
    }

    [Fact]
    public async Task GetPendingDurationBalanceAsync_ShouldReturnBalances()
    {
        // Arrange
        var balances = new[]
        {
            new { DurationCategory = "Medium (90m - 140m)", Count = 15 }
        };
        var reader = CreateDataReader(balances);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetPendingDurationBalanceAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Medium (90m - 140m)", result.First().DurationCategory);
    }

    [Fact]
    public async Task GetWatchlistByEraAsync_ShouldReturnEras()
    {
        // Arrange
        var eras = new[]
        {
            new { EraName = "Modern (1990-2009)", Count = 22 }
        };
        var reader = CreateDataReader(eras);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchlistByEraAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Modern (1990-2009)", result.First().EraName);
    }

    [Fact]
    public async Task GetGhostActorAsync_ShouldReturnActor()
    {
        // Arrange
        var actor = new[]
        {
            new { ActorName = "Brando", PendingMoviesCount = 3 }
        };
        var reader = CreateDataReader(actor);
        SetupDb(reader);
        var queries = new WatchlistQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetGhostActorAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Brando", result.ActorName);
        Assert.Equal(3, result.PendingMoviesCount);
    }
}
