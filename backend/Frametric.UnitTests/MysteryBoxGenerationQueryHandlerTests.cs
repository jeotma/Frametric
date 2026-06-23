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

public class MysteryBoxGenerationQueryHandlerTests
{
    private readonly Mock<IDiscoveryQueries> _discoveryQueriesMock = new();
    private readonly Mock<ILogger<MysteryBoxGenerationQueryHandler>> _loggerMock = new();

    private MysteryBoxGenerationQueryHandler CreateHandler()
    {
        return new MysteryBoxGenerationQueryHandler(
            _discoveryQueriesMock.Object,
            _loggerMock.Object);
    }

    private static List<DiscoveryMoviePoolItemDto> CreatePool(int count, string? genre = "Drama")
    {
        return Enumerable.Range(1, count).Select(i => new DiscoveryMoviePoolItemDto
        {
            MovieId = Guid.NewGuid(),
            Title = $"Movie {i}",
            DirectorName = "Director",
            ReleaseYear = 2000 + i,
            RuntimeMinutes = 100 + i,
            PosterUrl = null,
            TmdbRating = 7.0 + (i * 0.1),
            TmdbPopularity = 50.0 + i,
            CustomAverageRating = 7.5,
            Genres = genre ?? "Drama",
            Keywords = null,
            Overview = null,
            Language = "English",
            Country = "USA"
        }).ToList();
    }

    [Fact]
    public async Task Handle_ShouldReturnFiveBoxes_ByDefault()
    {
        var pool = CreatePool(20);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new MysteryBoxGenerationQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, MysteryBoxVariant.Standard);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5, result.BoxIds.Count);
        Assert.Equal(MysteryBoxVariant.Standard, result.Variant);
    }

    [Fact]
    public async Task Handle_ShouldReturnRequestedBoxCount()
    {
        var pool = CreatePool(30);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new MysteryBoxGenerationQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, MysteryBoxVariant.Standard, BoxCount: 3);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(3, result.BoxIds.Count);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPoolIsEmpty()
    {
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DiscoveryMoviePoolItemDto>());

        var handler = CreateHandler();
        var query = new MysteryBoxGenerationQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, MysteryBoxVariant.Standard);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThematicVariant_ShouldReturnBoxes()
    {
        var pool = CreatePool(40, "Horror");
        pool.AddRange(CreatePool(10, "Comedy"));
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new MysteryBoxGenerationQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, MysteryBoxVariant.Thematic);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(5, result.BoxIds.Count);
    }

    [Fact]
    public async Task Handle_PremiumVariant_ShouldReturnBoxes()
    {
        var pool = CreatePool(30);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new MysteryBoxGenerationQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, MysteryBoxVariant.ActorFocus);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(5, result.BoxIds.Count);
    }

    [Fact]
    public async Task Handle_DirectorFocusVariant_ShouldReturnBoxes()
    {
        var pool = CreatePool(30);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new MysteryBoxGenerationQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, MysteryBoxVariant.DirectorFocus);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(5, result.BoxIds.Count);
    }

    [Fact]
    public async Task Handle_StrategyVariant_ShouldIncludeHints()
    {
        var pool = CreatePool(30);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new MysteryBoxGenerationQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly, MysteryBoxVariant.Strategy);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(5, result.BoxIds.Count);
        Assert.NotNull(result.Hints);
        Assert.Equal(5, result.Hints.Count);
        Assert.All(result.Hints, h => Assert.Contains("Genre hint:", h.Hint));
    }
}
