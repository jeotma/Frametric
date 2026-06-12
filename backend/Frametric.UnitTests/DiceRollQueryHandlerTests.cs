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
using Frametric.Domain.Discovery.Enums;
using Frametric.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class DiceRollQueryHandlerTests
{
    private readonly Mock<IDiscoveryQueries> _discoveryQueriesMock = new();
    private readonly Mock<ILogger<DiceRollQueryHandler>> _loggerMock = new();

    private DiceRollQueryHandler CreateHandler()
    {
        return new DiceRollQueryHandler(_discoveryQueriesMock.Object, _loggerMock.Object);
    }

    private static List<DiscoveryMoviePoolItemDto> CreatePool(int count)
    {
        return Enumerable.Range(1, count).Select(i => new DiscoveryMoviePoolItemDto
        {
            MovieId = Guid.NewGuid(),
            Title = $"Movie {i}",
            DirectorName = "Director",
            ReleaseYear = 2000 + i,
            RuntimeMinutes = 100,
            PosterUrl = null,
            TmdbRating = Math.Min(10.0, 5.0 + (i * 0.3)),
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
    public async Task Handle_ShouldReturnResult_WithDiceRolls()
    {
        var pool = CreatePool(20);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new DiceRollQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.DiceResults);
        Assert.Equal(5, result.DiceResults.Count);
        Assert.All(result.DiceResults, d => Assert.InRange(d.RollValue, 1, 8));
    }

    [Fact]
    public async Task Handle_ShouldRespectSpecifiedDiceTypes()
    {
        var pool = CreatePool(20);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var diceTypes = new List<DiceType> { DiceType.Quality, DiceType.Risk };
        var query = new DiceRollQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, diceTypes);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.DiceResults.Count);
        Assert.Contains(result.DiceResults, d => d.DiceType == DiceType.Quality);
        Assert.Contains(result.DiceResults, d => d.DiceType == DiceType.Risk);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPoolIsEmpty()
    {
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DiscoveryMoviePoolItemDto>());

        var handler = CreateHandler();
        var query = new DiceRollQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldDetectCriticalSuccess()
    {
        var pool = Enumerable.Range(1, 100).Select(i => new DiscoveryMoviePoolItemDto
        {
            MovieId = Guid.NewGuid(),
            Title = $"Movie {i}",
            DirectorName = "Director",
            ReleaseYear = 2000,
            RuntimeMinutes = 100,
            PosterUrl = null,
            TmdbRating = 9.0,
            TmdbPopularity = 50.0,
            CustomAverageRating = 9.0,
            Genres = "Drama",
            Keywords = null,
            Overview = null,
            Language = "English",
            Country = "USA"
        }).ToList();
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new DiceRollQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, new[] { DiceType.Quality });

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.DiceResults);
        Assert.Contains(result.DiceResults, d => d.DiceType == DiceType.Quality);
    }
}
