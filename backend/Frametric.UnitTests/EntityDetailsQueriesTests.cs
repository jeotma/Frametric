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
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.EntityDetails;
using Frametric.Application.Interfaces;
using Frametric.Infrastructure.Queries;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class EntityDetailsQueriesTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;

    public EntityDetailsQueriesTests()
    {
        _dbConnectionFactoryMock = new Mock<IDbConnectionFactory>();
    }

    [Fact]
    public async Task GetMovieDetailsAsync_ShouldReturnMovieDetails()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movieBase = new[] { new { Id = movieId, Title = "Movie 1", ReleaseYear = 2020, RuntimeMinutes = 120, PosterUrl = "poster", Overview = "Overview 1", TmdbRating = 8.5, UserAverageScore = 9.0, IsWatched = true } };
        var genres = new[] { new { Id = Guid.NewGuid(), Name = "Action" } };
        var directors = new[] { new { Id = Guid.NewGuid(), Name = "Director A" } };
        var actors = new[] { new { Id = Guid.NewGuid(), Name = "Actor A" } };
        var diaryEntries = new[] { new { Id = Guid.NewGuid(), DateWatched = "2023-01-01", IsRewatch = false, Rating = 9.0 } };

        var reader = new MultiResultSetDbDataReader(new IEnumerable<object>[] { movieBase, genres, directors, actors, diaryEntries });
        var dbCommand = new TestDbCommand(reader, 0);
        var dbConnection = new TestDbConnection(dbCommand);
        _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(dbConnection);

        var queries = new EntityDetailsQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetMovieDetailsAsync(Guid.NewGuid(), movieId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Movie 1", result.Title);
        Assert.Single(result.Genres);
        Assert.Equal("Action", result.Genres.First().Name);
        Assert.Single(result.Directors);
        Assert.Equal("Director A", result.Directors.First().Name);
        Assert.Single(result.Actors);
        Assert.Equal("Actor A", result.Actors.First().Name);
        Assert.Single(result.DiaryEntries);
        Assert.Equal("2023-01-01", result.DiaryEntries.First().DateWatched);
    }

    [Fact]
    public async Task GetActorDetailsAsync_ShouldReturnActorDetails()
    {
        // Arrange
        var actorId = Guid.NewGuid();
        var actorBase = new[] { new { Id = actorId, Name = "Actor Name", ProfilePath = "profile", TmdbId = 12345, DirectorId = (Guid?)null } };
        var actingMovies = new[] { new { Id = Guid.NewGuid(), Title = "Movie Star", ReleaseYear = 2018, PosterPath = "path", IsWatched = true } };
        var directedMovies = new[] { new { Id = Guid.NewGuid(), Title = "Dummy", ReleaseYear = 2020, PosterPath = "path", IsWatched = false } };
        var avgRating = new[] { new { Rating = 8.0 } };
        var watchCount = new[] { new { Count = 5 } };
        var watchlistMovieTitles = new[] { new { Id = Guid.NewGuid(), Title = "Movie Star", ReleaseYear = 2018, PosterPath = "path", IsWatched = true } };
        var likedMovieTitles = new[] { new { Id = Guid.NewGuid(), Title = "Movie Star", ReleaseYear = 2018, PosterPath = "path", IsWatched = true } };

        var reader = new MultiResultSetDbDataReader(new IEnumerable<object>[] { actorBase, actingMovies, directedMovies, avgRating, watchCount, watchlistMovieTitles, likedMovieTitles });
        var dbCommand = new TestDbCommand(reader, 0);
        var dbConnection = new TestDbConnection(dbCommand);
        _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(dbConnection);

        var queries = new EntityDetailsQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetActorDetailsAsync(Guid.NewGuid(), actorId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Actor Name", result.Name);
        Assert.Equal(8.0, result.AverageRating);
        Assert.Equal(5, result.WatchCount);
        Assert.Equal("profile", result.ProfilePath);
        Assert.Single(result.Movies);
        Assert.Equal("Movie Star", result.Movies.First().Title);
        Assert.True(result.Movies.First().IsWatched);
    }

    [Fact]
    public async Task GetDirectorDetailsAsync_ShouldReturnDirectorDetails()
    {
        // Arrange
        var directorId = Guid.NewGuid();
        var directorBase = new[] { new { Id = directorId, Name = "Director Name", ProfilePath = "profile", TmdbId = 12345, ActorId = (Guid?)null } };
        var directingMovies = new[] { new { Id = Guid.NewGuid(), Title = "Directorial Masterpiece", ReleaseYear = 2019, PosterPath = "path", IsWatched = true } };
        var actingMovies = new[] { new { Id = Guid.NewGuid(), Title = "Dummy", ReleaseYear = 2020, PosterPath = "path", IsWatched = false } };
        var avgRating = new[] { new { Rating = 9.2 } };
        var watchCount = new[] { new { Count = 3 } };
        var watchlistMovieTitles = new[] { new { Id = Guid.NewGuid(), Title = "Directorial Masterpiece", ReleaseYear = 2019, PosterPath = "path", IsWatched = true } };
        var likedMovieTitles = new[] { new { Id = Guid.NewGuid(), Title = "Directorial Masterpiece", ReleaseYear = 2019, PosterPath = "path", IsWatched = true } };

        var reader = new MultiResultSetDbDataReader(new IEnumerable<object>[] { directorBase, directingMovies, actingMovies, avgRating, watchCount, watchlistMovieTitles, likedMovieTitles });
        var dbCommand = new TestDbCommand(reader, 0);
        var dbConnection = new TestDbConnection(dbCommand);
        _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(dbConnection);

        var queries = new EntityDetailsQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetDirectorDetailsAsync(Guid.NewGuid(), directorId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Director Name", result.Name);
        Assert.Equal(9.2, result.AverageRating);
        Assert.Equal(3, result.WatchCount);
        Assert.Equal("profile", result.ProfilePath);
        Assert.Single(result.Movies);
        Assert.Equal("Directorial Masterpiece", result.Movies.First().Title);
        Assert.True(result.Movies.First().IsWatched);
    }

    [Fact]
    public async Task SearchEntitiesAsync_ShouldReturnEntities()
    {
        // Arrange
        var localId = Guid.NewGuid();
        var results = new[]
        {
            new
            {
                LocalId = (Guid?)localId,
                TmdbId = (int?)12345,
                EntityType = "Movie",
                TitleOrName = "Inception",
                ReleaseYear = (int?)2010,
                ImageUrl = "http://image.url",
                IsLocal = true
            }
        };

        var reader = new MultiResultSetDbDataReader(new IEnumerable<object>[] { results });
        var dbCommand = new TestDbCommand(reader, 0);
        var dbConnection = new TestDbConnection(dbCommand);
        _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(dbConnection);

        var queries = new EntityDetailsQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.SearchEntitiesAsync(Guid.NewGuid(), "Inception", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Single(list);
        Assert.Equal(localId, list[0].LocalId);
        Assert.Equal(12345, list[0].TmdbId);
        Assert.Equal("Movie", list[0].EntityType);
        Assert.Equal("Inception", list[0].TitleOrName);
    }
}
