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
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces.Discovery;
using Frametric.Application.Queries.Discovery;
using Frametric.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class RouletteSelectionQueryHandlerTests
{
    private readonly Mock<IDiscoveryQueries> _discoveryQueriesMock = new();
    private readonly Mock<ILogger<RouletteSelectionQueryHandler>> _loggerMock = new();

    private RouletteSelectionQueryHandler CreateHandler()
    {
        return new RouletteSelectionQueryHandler(
            _discoveryQueriesMock.Object,
            _loggerMock.Object);
    }

    private static List<DiscoveryMoviePoolItemDto> CreatePool(int count)
    {
        return Enumerable.Range(1, count).Select(i => new DiscoveryMoviePoolItemDto
        {
            MovieId = Guid.NewGuid(),
            Title = $"Movie {i}",
            DirectorName = "Director",
            ReleaseYear = 2000 + i,
            RuntimeMinutes = 100 + i,
            PosterUrl = null,
            TmdbRating = 7.0,
            TmdbPopularity = 50.0,
            CustomAverageRating = 7.5,
            Genres = "Drama",
            Keywords = null,
            Overview = null,
            Language = "English",
            Country = "USA"
        }).ToList();
    }

    [Fact]
    public async Task Handle_ShouldReturnRaceResult_WhenPoolHasItems()
    {
        var pool = CreatePool(10);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new RouletteSelectionQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, 3);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Winner);
        Assert.NotNull(result.SpinSequence);
        Assert.True(result.SpinSequence.Count() >= 3);
        Assert.Contains(pool, m => m.MovieId == result.Winner.MovieId);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPoolIsEmpty()
    {
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DiscoveryMoviePoolItemDto>());

        var handler = CreateHandler();
        var query = new RouletteSelectionQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, 3);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldLimitPoolTo50_WhenNonCustomScopeAndPoolSizeIsGreaterThan50()
    {
        var pool = CreatePool(60);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new RouletteSelectionQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, 1);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        // Under WinningThreshold = 1, the spin sequence length should be exactly 50.
        Assert.Equal(50, result.SpinSequence.Count());
    }

    [Fact]
    public async Task Handle_ShouldNotLimitPoolTo50_WhenCustomScopeAndPoolSizeIsGreaterThan50()
    {
        var pool = CreatePool(60);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        // CustomCollection is custom scope
        var query = new RouletteSelectionQuery(
            Guid.NewGuid(), 
            DiscoveryDataSourceScope.CustomCollection, 
            1, 
            CustomSourceIds: new[] { Guid.NewGuid() });

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        // Under WinningThreshold = 1, the spin sequence length should be exactly 60 (unlimited).
        Assert.Equal(60, result.SpinSequence.Count());
    }
}
