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

public class WatchedQueriesTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;

    public WatchedQueriesTests()
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
    public async Task GetMoviesAsync_ShouldReturnMovies()
    {
        // Arrange
        var movies = new[]
        {
            new { Title = "Inception", ReleaseYear = (int?)2010, Director = "Christopher Nolan", Rating = 4.5, Liked = true, CustomAverageRating = (double?)4.3, PosterUrl = "poster" }
        };
        var reader = CreateDataReader(movies);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMoviesAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Inception", result.First().Title);
    }

    [Fact]
    public async Task GetDirectorsAsync_ShouldReturnDirectors()
    {
        // Arrange
        var directors = new[]
        {
            new { DirectorName = "Nolan", Count = 5, AverageRating = 4.8 }
        };
        var reader = CreateDataReader(directors);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetDirectorsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Nolan", result.First().DirectorName);
    }

    [Fact]
    public async Task GetActorsAsync_ShouldReturnActors()
    {
        // Arrange
        var actors = new[]
        {
            new { ActorName = "DiCaprio", Count = 6, AverageRating = 4.7 }
        };
        var reader = CreateDataReader(actors);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetActorsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("DiCaprio", result.First().ActorName);
    }

    [Fact]
    public async Task GetMoviesByGenreAsync_ShouldReturnGenres()
    {
        // Arrange
        var genres = new[]
        {
            new { GenreName = "Sci-Fi", Count = 12 }
        };
        var reader = CreateDataReader(genres);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMoviesByGenreAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Sci-Fi", result.First().GenreName);
    }

    [Fact]
    public async Task GetMoviesByDecadeAsync_ShouldReturnDecades()
    {
        // Arrange
        var decades = new[]
        {
            new { Decade = 2010, Count = 15 }
        };
        var reader = CreateDataReader(decades);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMoviesByDecadeAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2010, result.First().Decade);
    }

    [Fact]
    public async Task GetMostRepeatedActorAsync_ShouldReturnActor()
    {
        // Arrange
        var actor = new[]
        {
            new { ActorName = "Pitt", Count = 8, AverageRating = 4.2 }
        };
        var reader = CreateDataReader(actor);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMostRepeatedActorAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pitt", result.ActorName);
    }

    [Fact]
    public async Task GetMostWatchedDirectorAsync_ShouldReturnDirector()
    {
        // Arrange
        var director = new[]
        {
            new { DirectorName = "Tarantino", Count = 7, AverageRating = 4.6 }
        };
        var reader = CreateDataReader(director);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMostWatchedDirectorAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Tarantino", result.DirectorName);
    }

    [Fact]
    public async Task GetPredominantEraAsync_ShouldReturnEra()
    {
        // Arrange
        var era = new[]
        {
            new { EraName = "Modern (Post-1980)", Count = 42 }
        };
        var reader = CreateDataReader(era);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetPredominantEraAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Modern (Post-1980)", result.EraName);
    }

    [Fact]
    public async Task GetDirectorRankingByRatingAsync_ShouldReturnRanking()
    {
        // Arrange
        var ranking = new[]
        {
            new { DirectorId = Guid.NewGuid(), Name = "Scorsese", WatchCount = 8, AverageRating = 4.5, HighestRatedMovieTitle = "Goodfellas", CustomAverageRating = 4.4 }
        };
        var reader = CreateDataReader(ranking);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetDirectorRankingByRatingAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Scorsese", result.First().Name);
    }

    [Fact]
    public async Task GetTotalTimeByDirectorOrGenreAsync_ShouldReturnTimeInvested()
    {
        // Arrange
        var time = new[]
        {
            new { Name = "Action", TotalMinutes = 450, TotalHours = 7 }
        };
        var reader = CreateDataReader(time);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetTotalTimeByDirectorOrGenreAsync(Guid.NewGuid(), "Genre", "Action", new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Action", result.Name);
        Assert.Equal(450, result.TotalMinutes);
        Assert.Equal(7, result.TotalHours);
    }

    [Fact]
    public async Task GetPreferredWatchDayOfWeekAsync_ShouldReturnPreferredDays()
    {
        // Arrange
        var preferredDays = new[]
        {
            new { DayOfWeek = "Friday", Count = 20 }
        };
        var reader = CreateDataReader(preferredDays);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetPreferredWatchDayOfWeekAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Friday", result.First().DayOfWeek);
    }

    [Fact]
    public async Task GetGenreStreakAsync_ShouldReturnStreaks()
    {
        // Arrange
        var streaks = new[]
        {
            new { GenreName = "Drama", StreakLength = 5, StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2023, 1, 5) }
        };
        var reader = CreateDataReader(streaks);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetGenreStreakAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Drama", result.First().GenreName);
    }

    [Fact]
    public async Task GetLongestWatchedMovieAsync_ShouldReturnMovie()
    {
        // Arrange
        var movie = new[]
        {
            new { Id = Guid.NewGuid(), Title = "The Irishman", ReleaseYear = (int?)2019, PosterPath = "path", RuntimeMinutes = (int?)209, Rating = (double?)4.0 }
        };
        var reader = CreateDataReader(movie);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetLongestWatchedMovieAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Irishman", result.Title);
    }

    [Fact]
    public async Task GetShortestWatchedMovieAsync_ShouldReturnMovie()
    {
        // Arrange
        var movie = new[]
        {
            new { Id = Guid.NewGuid(), Title = "Short film", ReleaseYear = (int?)2020, PosterPath = "path", RuntimeMinutes = (int?)5, Rating = (double?)3.5 }
        };
        var reader = CreateDataReader(movie);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetShortestWatchedMovieAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Short film", result.Title);
    }

    [Fact]
    public async Task GetDirectorActorPairingsAsync_ShouldReturnPairings()
    {
        // Arrange
        var pairings = new[]
        {
            new { DirectorName = "Nolan", ActorName = "Caine", CollaborationCount = 8 }
        };
        var reader = CreateDataReader(pairings);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetDirectorActorPairingsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Nolan", result.First().DirectorName);
    }

    [Fact]
    public async Task GetPrimeTimeStatsAsync_ShouldReturnStats()
    {
        // Arrange
        var stats = new[]
        {
            new { PeakDay = "Saturday", PeakDayCount = 25, PeakMonth = "December", PeakMonthCount = 30, SlumpDay = "Tuesday", SlumpDayCount = 2, SlumpMonth = "April", SlumpMonthCount = 1 }
        };
        var reader = CreateDataReader(stats);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetPrimeTimeStatsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Saturday", result.PeakDay);
    }

    [Fact]
    public async Task GetGenresWithRatingAsync_ShouldReturnGenresRating()
    {
        // Arrange
        var genres = new[]
        {
            new { GenreName = "Horror", Count = 15, AverageRating = 3.9 }
        };
        var reader = CreateDataReader(genres);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetGenresWithRatingAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Horror", result.First().GenreName);
    }

    [Fact]
    public async Task GetRatingEvolutionAsync_ShouldReturnEvolution()
    {
        // Arrange
        var evolution = new[]
        {
            new { Month = 5, AverageRating = 4.2 }
        };
        var reader = CreateDataReader(evolution);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetRatingEvolutionAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(5, result.First().Month);
    }

    [Fact]
    public async Task GetCastingRepetitionsAsync_ShouldReturnRepetitions()
    {
        // Arrange
        var repetitions = new[]
        {
            new { Actor1Name = "Al Pacino", Actor2Name = "Robert De Niro", CollaborationCount = 4 }
        };
        var reader = CreateDataReader(repetitions);
        SetupDb(reader);
        var queries = new WatchedQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetCastingRepetitionsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Al Pacino", result.First().Actor1Name);
    }
}
