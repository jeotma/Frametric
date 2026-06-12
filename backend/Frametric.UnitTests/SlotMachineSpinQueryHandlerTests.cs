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

public class SlotMachineSpinQueryHandlerTests
{
    private readonly Mock<IDiscoveryQueries> _discoveryQueriesMock = new();
    private readonly Mock<ILogger<SlotMachineSpinQueryHandler>> _loggerMock = new();

    private SlotMachineSpinQueryHandler CreateHandler()
    {
        return new SlotMachineSpinQueryHandler(_discoveryQueriesMock.Object, _loggerMock.Object);
    }

    private static List<DiscoveryMoviePoolItemDto> CreatePool(int count)
    {
        return Enumerable.Range(1, count).Select(i => new DiscoveryMoviePoolItemDto
        {
            MovieId = Guid.NewGuid(),
            Title = $"Movie {i}",
            DirectorName = "Director",
            ReleaseYear = 2000 + (i % 5),
            RuntimeMinutes = 100 + i,
            PosterUrl = null,
            TmdbRating = 7.0,
            TmdbPopularity = 50.0,
            CustomAverageRating = 7.5,
            Genres = i % 2 == 0 ? "Drama" : "Comedy",
            Keywords = null,
            Overview = null,
            Language = "English",
            Country = i % 2 == 0 ? "USA" : "France"
        }).ToList();
    }

    [Fact]
    public async Task Handle_ShouldReturnResult_WithReelValues()
    {
        var pool = CreatePool(20);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new SlotMachineSpinQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.ReelResults);
        Assert.Equal(5, result.ReelResults.Count);
    }

    [Fact]
    public async Task Handle_ShouldUseProvidedReelValues()
    {
        var pool = CreatePool(20);
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pool);

        var handler = CreateHandler();
        var query = new SlotMachineSpinQuery(
            Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly,
            Genre: "Comedy", Decade: 2000, Country: "France");

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        var genreReel = result.ReelResults[0];
        var decadeReel = result.ReelResults[1];
        var countryReel = result.ReelResults[4];
        Assert.Equal("Comedy", genreReel.Value);
        Assert.Equal("2000s", decadeReel.Value);
        Assert.Equal("France", countryReel.Value);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPoolIsEmpty()
    {
        _discoveryQueriesMock
            .Setup(x => x.GetDiscoveryPoolAsync(It.IsAny<Guid>(), It.IsAny<DiscoveryDataSourceScope>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DiscoveryMoviePoolItemDto>());

        var handler = CreateHandler();
        var query = new SlotMachineSpinQuery(Guid.NewGuid(), DiscoveryDataSourceScope.DatabaseOnly);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }
}
