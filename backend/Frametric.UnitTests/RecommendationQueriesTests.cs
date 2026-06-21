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
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Domain.Enums;
using Frametric.Infrastructure.Queries;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class RecommendationQueriesTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;

    public RecommendationQueriesTests()
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
    public async Task GetWatchedMovieDetailsAsync_ShouldReturnDetails()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var details = new[]
        {
            new
            {
                MovieId = movieId,
                ReleaseYear = (int?)2010,
                RuntimeMinutes = (int?)148,
                Genres = "Action,Sci-Fi",
                Directors = "Christopher Nolan",
                Actors = "Leonardo DiCaprio",
                UserRating = (double?)4.5,
                WatchDate = DateTime.UtcNow,
                Keywords = "dream,subconscious"
            }
        };

        var reader = CreateDataReader(details);
        SetupDb(reader);
        var queries = new RecommendationQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetWatchedMovieDetailsAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Single(list);
        Assert.Equal(movieId, list[0].MovieId);
        Assert.Equal("Action,Sci-Fi", list[0].Genres);
    }

    [Theory]
    [InlineData(RecommendationScope.WatchlistOnly)]
    [InlineData(RecommendationScope.DatabaseOnly)]
    [InlineData(RecommendationScope.Hybrid)]
    public async Task GetCandidateMoviesAsync_ShouldReturnCandidates_ForDifferentScopes(RecommendationScope scope)
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var candidates = new[]
        {
            new
            {
                MovieId = movieId,
                Title = "Inception",
                ReleaseYear = (int?)2010,
                RuntimeMinutes = (int?)148,
                PosterUrl = "url",
                TmdbRating = (double?)8.8,
                TmdbPopularity = (double?)120.5,
                CustomAverageRating = (double?)8.5,
                Genres = "Action,Sci-Fi",
                Directors = "Christopher Nolan",
                Actors = "Leonardo DiCaprio",
                Keywords = "dream",
                Awards = "4 Oscars",
                Writers = "Christopher Nolan",
                Language = "English",
                Country = "USA",
                BoxOffice = "828M",
                Certification = "PG-13",
                StreamingProviders = "Netflix",
                Overview = "A thief who steals corporate secrets...",
                ImdbRating = (double?)8.8,
                RottenTomatoesRating = (double?)8.7,
                MetacriticRating = (double?)8.4,
                WatchlistAddedDate = (DateTime?)DateTime.UtcNow
            }
        };

        var reader = CreateDataReader(candidates);
        SetupDb(reader);
        var queries = new RecommendationQueriesImpl(_dbConnectionFactoryMock.Object);

        // Act
        var result = await queries.GetCandidateMoviesAsync(Guid.NewGuid(), scope, 150, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Single(list);
        Assert.Equal(movieId, list[0].MovieId);
        Assert.Equal("Inception", list[0].Title);
    }
}
