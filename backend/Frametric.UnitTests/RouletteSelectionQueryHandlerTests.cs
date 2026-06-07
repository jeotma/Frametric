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
using Frametric.Application.Interfaces.Discovery;
using Frametric.Application.Queries.Discovery;
using Frametric.Domain.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class RouletteSelectionQueryHandlerTests
{
    private readonly Mock<IDiscoveryQueries> _discoveryQueriesMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly Mock<ILogger<RouletteSelectionQueryHandler>> _loggerMock = new();

    private RouletteSelectionQueryHandler CreateHandler()
    {
        return new RouletteSelectionQueryHandler(
            _discoveryQueriesMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    private static List<DiscoveryMoviePoolItemDto> CreatePool(int count)
    {
        return Enumerable.Range(1, count).Select(i => new DiscoveryMoviePoolItemDto(
            Guid.NewGuid(),
            $"Movie {i}",
            "Director",
            2000 + i,
            100 + i,
            null,
            7.0,
            50.0,
            7.5,
            "Drama",
            null,
            null,
            "English",
            "USA"
        )).ToList();
    }

    [Fact]
    public async Task Handle_ShouldReturnMovie_WhenPoolHasItems()
    {
        var pool = CreatePool(10);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new RouletteSelectionQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains(pool, m => m.MovieId == result.MovieId);
        Assert.Equal("Roulette selected a random movie from the discovery pool.", result.SelectionMechanismMetadata);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPoolIsEmpty()
    {
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DiscoveryMoviePoolItemDto>());

        var handler = CreateHandler();
        var query = new RouletteSelectionQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCustomCollectionWithoutIds()
    {
        var handler = CreateHandler();
        var query = new RouletteSelectionQuery(Guid.NewGuid(), DiscoveryDataSourceScope.CustomCollection);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnPersistenceMetadata_WhenThresholdSet()
    {
        var pool = CreatePool(5);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        _cacheMock
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("3"));

        var handler = CreateHandler();
        var query = new RouletteSelectionQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, PersistenceThreshold: 3);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("consensus threshold", result.SelectionMechanismMetadata);
    }
}
