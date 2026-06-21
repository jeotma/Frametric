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
using Frametric.Application.Interfaces.EntityDetails;
using Frametric.Application.Queries.EntityDetails;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class GlobalSearchQueryHandlerTests
{
    private readonly Mock<IEntityDetailsQueries> _queriesMock;
    private readonly Mock<ITmdbService> _tmdbServiceMock;
    private readonly GlobalSearchQueryHandler _handler;

    public GlobalSearchQueryHandlerTests()
    {
        _queriesMock = new Mock<IEntityDetailsQueries>();
        _tmdbServiceMock = new Mock<ITmdbService>();
        _handler = new GlobalSearchQueryHandler(_queriesMock.Object, _tmdbServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLocalResultsExist_ShouldReturnLocalResultsAndNotCallTmdb()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var queryText = "Inception";
        var query = new GlobalSearchQuery(userId, queryText);

        var localResults = new List<GlobalSearchResultDto>
        {
            new GlobalSearchResultDto(Guid.NewGuid(), 123, "Movie", "Inception", 2010, "url", true)
        };

        _queriesMock
            .Setup(q => q.SearchEntitiesAsync(userId, queryText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(localResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Inception", resultList[0].TitleOrName);

        _queriesMock.Verify(q => q.SearchEntitiesAsync(userId, queryText, It.IsAny<CancellationToken>()), Times.Once);
        _tmdbServiceMock.Verify(t => t.SearchMultiAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoLocalResultsExist_ShouldFallbackToTmdb()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var queryText = "Avatar";
        var query = new GlobalSearchQuery(userId, queryText);

        _queriesMock
            .Setup(q => q.SearchEntitiesAsync(userId, queryText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<GlobalSearchResultDto>());

        var tmdbResults = new List<GlobalSearchResultDto>
        {
            new GlobalSearchResultDto(Guid.NewGuid(), 456, "Movie", "Avatar", 2009, "url2", false)
        };

        _tmdbServiceMock
            .Setup(t => t.SearchMultiAsync(queryText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Avatar", resultList[0].TitleOrName);

        _queriesMock.Verify(q => q.SearchEntitiesAsync(userId, queryText, It.IsAny<CancellationToken>()), Times.Once);
        _tmdbServiceMock.Verify(t => t.SearchMultiAsync(queryText, It.IsAny<CancellationToken>()), Times.Once);
    }
}
