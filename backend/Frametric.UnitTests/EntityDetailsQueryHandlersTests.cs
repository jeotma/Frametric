// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.DTOs.EntityDetails;
using Frametric.Application.Interfaces.EntityDetails;
using Frametric.Application.Queries.EntityDetails;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class EntityDetailsQueryHandlersTests
{
    private readonly Mock<IEntityDetailsQueries> _queriesMock;

    public EntityDetailsQueryHandlersTests()
    {
        _queriesMock = new Mock<IEntityDetailsQueries>();
    }

    [Fact]
    public async Task GetActorDetailsQueryHandler_ShouldCallGetActorDetailsAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var query = new GetActorDetailsQuery(userId, actorId);
        var expectedDto = new ActorDetailsDto(actorId, "Actor A", 8.5, 3, new List<MovieSimpleDto>());

        _queriesMock
            .Setup(q => q.GetActorDetailsAsync(userId, actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var handler = new GetActorDetailsQueryHandler(_queriesMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Actor A", result.Name);
        _queriesMock.Verify(q => q.GetActorDetailsAsync(userId, actorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDirectorDetailsQueryHandler_ShouldCallGetDirectorDetailsAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var directorId = Guid.NewGuid();
        var query = new GetDirectorDetailsQuery(userId, directorId);
        var expectedDto = new DirectorDetailsDto(directorId, "Director D", 9.0, 5, new List<MovieSimpleDto>());

        _queriesMock
            .Setup(q => q.GetDirectorDetailsAsync(userId, directorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var handler = new GetDirectorDetailsQueryHandler(_queriesMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Director D", result.Name);
        _queriesMock.Verify(q => q.GetDirectorDetailsAsync(userId, directorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMovieDetailsQueryHandler_ShouldCallGetMovieDetailsAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var query = new GetMovieDetailsQuery(userId, movieId);
        var expectedDto = new MovieDetailsDto(
            movieId, "Movie Title", 2022, 110, "poster", "overview", 8.0, 8.5,
            new List<GenreSimpleDto>(), new List<DirectorSimpleDto>(), new List<ActorSimpleDto>(), new List<MovieDiaryEntryDto>()
        );

        _queriesMock
            .Setup(q => q.GetMovieDetailsAsync(userId, movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var handler = new GetMovieDetailsQueryHandler(_queriesMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Movie Title", result.Title);
        _queriesMock.Verify(q => q.GetMovieDetailsAsync(userId, movieId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
