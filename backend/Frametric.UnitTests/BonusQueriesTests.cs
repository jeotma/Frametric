// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Data;
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

public class BonusQueriesTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;

    public BonusQueriesTests()
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
    public async Task GetWeekendWarriorStatsAsync_ShouldReturnStats()
    {
        // Arrange
        var stats = new[] { new { WeekendWatches = 10, WeekdayWatches = 5 } };
        var reader = CreateDataReader(stats);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWeekendWarriorStatsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.WeekendWatches);
        Assert.Equal(5, result.WeekdayWatches);
    }

    [Fact]
    public async Task GetHiddenGemsAsync_ShouldReturnGems()
    {
        // Arrange
        var movies = new[]
        {
            new { Id = Guid.NewGuid(), Title = "Hidden Gem 1", ReleaseYear = 1980, PosterPath = "path1" },
            new { Id = Guid.NewGuid(), Title = "Hidden Gem 2", ReleaseYear = 1975, PosterPath = "path2" }
        };
        var reader = CreateDataReader(movies);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetHiddenGemsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Hidden Gem 1", result.First().Title);
    }

    [Fact]
    public async Task GetWatchlistGraveyardAsync_ShouldReturnMovies()
    {
        // Arrange
        var movies = new[]
        {
            new { Id = Guid.NewGuid(), Title = "Old Watchlist Item", ReleaseYear = 2010, PosterPath = "path1" }
        };
        var reader = CreateDataReader(movies);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchlistGraveyardAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Old Watchlist Item", result.First().Title);
    }

    [Fact]
    public async Task GetCinematicFatigueExpandedAsync_ShouldReturnFatigueStats()
    {
        // Arrange
        var fatigue = new[]
        {
            new
            {
                AvgRatingLightDays = 4.2,
                AvgRatingHeavyDays = 3.5,
                SlumpDay = "Monday",
                SlumpDayWatchCount = 2,
                SlumpMonth = "January",
                SlumpMonthWatchCount = 5
            }
        };
        var reader = CreateDataReader(fatigue);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetCinematicFatigueExpandedAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4.2, result.AvgRatingLightDays);
        Assert.Equal(3.5, result.AvgRatingHeavyDays);
        Assert.Equal("Monday", result.SlumpDay);
    }

    [Fact]
    public async Task GetBookendsAsync_ShouldReturnBookends()
    {
        // Arrange
        var movie1Id = Guid.NewGuid();
        var movie2Id = Guid.NewGuid();
        var data = new[]
        {
            new { Which = "first", Id = movie1Id, Title = "First Movie", ReleaseYear = (int?)2000, PosterPath = "p1", RuntimeMinutes = (int?)120, Rating = (double?)4.5 },
            new { Which = "last", Id = movie2Id, Title = "Last Movie", ReleaseYear = (int?)2023, PosterPath = "p2", RuntimeMinutes = (int?)90, Rating = (double?)3.0 }
        };
        var reader = CreateDataReader(data);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetBookendsAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("First Movie", result.OpeningScene?.Title);
        Assert.Equal("Last Movie", result.FadeToBlack?.Title);
    }

    [Fact]
    public async Task GetMonthlyExtremesAsync_ShouldReturnExtremes()
    {
        // Arrange
        var movie1Id = Guid.NewGuid();
        var movie2Id = Guid.NewGuid();
        var data = new[]
        {
            new { Kind = "best", Month = 1, Id = movie1Id, Title = "Best Jan", ReleaseYear = (int?)2000, PosterPath = "p1", RuntimeMinutes = (int?)120, Rating = (double?)4.5 },
            new { Kind = "worst", Month = 1, Id = movie2Id, Title = "Worst Jan", ReleaseYear = (int?)2001, PosterPath = "p2", RuntimeMinutes = (int?)90, Rating = (double?)1.5 }
        };
        var reader = CreateDataReader(data);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMonthlyExtremesAsync(Guid.NewGuid(), new AnalyticsFilterDto(), false, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Best Jan", result.First().BestMovie?.Title);
        Assert.Equal("Worst Jan", result.First().WorstMovie?.Title);
    }

    [Fact]
    public async Task GetTopAndBottomRatedMoviesAsync_ShouldReturnTopBottom()
    {
        // Arrange
        var movie1Id = Guid.NewGuid();
        var movie2Id = Guid.NewGuid();
        var data = new[]
        {
            new { Kind = "top", Id = movie1Id, Title = "Best Film", ReleaseYear = (int?)2020, PosterPath = "p1", RuntimeMinutes = (int?)150, Rating = (double?)5.0, Liked = true },
            new { Kind = "bottom", Id = movie2Id, Title = "Worst Film", ReleaseYear = (int?)2021, PosterPath = "p2", RuntimeMinutes = (int?)80, Rating = (double?)0.5, Liked = false }
        };
        var reader = CreateDataReader(data);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetTopAndBottomRatedMoviesAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.TopRated);
        Assert.Single(result.BottomRated);
        Assert.Equal("Best Film", result.TopRated.First().Title);
        Assert.Equal("Worst Film", result.BottomRated.First().Title);
    }

    [Fact]
    public async Task GetMostRewatchedMovieAsync_ShouldReturnMostRewatched()
    {
        // Arrange
        var data = new[]
        {
            new { Title = "Inception", PosterPath = "path", ReleaseYear = 2010, RewatchCount = 5 }
        };
        var reader = CreateDataReader(data);
        SetupDb(reader);
        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMostRewatchedMovieAsync(Guid.NewGuid(), new AnalyticsFilterDto(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Inception", result.Title);
        Assert.Equal(5, result.RewatchCount);
    }

    [Fact]
    public async Task GetBestRookiesAsync_ShouldReturnRookies()
    {
        // Arrange
        var directors = new[]
        {
            new { Name = "Nolan", MoviesWatchedThisYear = 3, AverageRating = 4.8 }
        };
        var actors = new[]
        {
            new { Name = "DiCaprio", MoviesWatchedThisYear = 4, AverageRating = 4.7 }
        };

        var dirReader = CreateDataReader(directors);
        var actReader = CreateDataReader(actors);

        var connectionMock = new Mock<IDbConnection>();
        var commandMock = new Mock<IDbCommand>();

        int callCount = 0;
        var mockDbCommand = new Mock<DbCommand>();
        
        var customReaderMock = new Mock<DbDataReader> { CallBase = true };
        
        var cmd1 = new TestDbCommand(dirReader, 0);
        var cmd2 = new TestDbCommand(actReader, 0);
        int cmdIndex = 0;

        var connection = new Mock<DbConnection>();
        var dbConnection = new SequenceDbConnection(new[] { cmd1, cmd2 });
        _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(dbConnection);

        var queries = new BonusQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetBestRookiesAsync(Guid.NewGuid(), new AnalyticsFilterDto { WatchYear = 2023 }, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.NewDirectors);
        Assert.Single(result.NewActors);
        Assert.Equal("Nolan", result.NewDirectors.First().Name);
        Assert.Equal("DiCaprio", result.NewActors.First().Name);
    }
}

public class SequenceDbConnection : DbConnection
{
    private readonly DbCommand[] _commands;
    private int _index = 0;

    public SequenceDbConnection(DbCommand[] commands)
    {
        _commands = commands;
    }

    protected override DbCommand CreateDbCommand()
    {
        var cmd = _commands[_index];
        _index = (_index + 1) % _commands.Length;
        return cmd;
    }

    public override string ConnectionString { get; set; } = "";
    public override string Database => "";
    public override string DataSource => "";
    public override string ServerVersion => "";
    public override ConnectionState State => ConnectionState.Open;
    public override void Open() { }
    public override void Close() { }
    public override void ChangeDatabase(string databaseName) { }
    protected override DbTransaction BeginDbTransaction(IsolationLevel il) => throw new NotImplementedException();
}
