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
using Frametric.Application.Queries.Discovery;
using Frametric.Domain.Discovery.Entities;
using Frametric.Domain.Entities;
using Frametric.Domain.ValueObjects;
using Frametric.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class GetBingoGridQueryHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FrametricDbContext> _options;
    private readonly Mock<ILogger<GetBingoGridQueryHandler>> _loggerMock;

    public GetBingoGridQueryHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<FrametricDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new FrametricDbContext(_options);
        context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<GetBingoGridQueryHandler>>();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private FrametricDbContext CreateContext() => new FrametricDbContext(_options);

    [Fact]
    public async Task Handle_ShouldCreateDefaultObjectives_WhenNoObjectivesExist()
    {
        var userId = Guid.NewGuid();

        using var context = CreateContext();
        var handler = new GetBingoGridQueryHandler(context, _loggerMock.Object);
        var query = new GetBingoGridQuery(userId, 3, AutoEvaluate: true);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.GridSize);
        Assert.Equal(9, result.Squares.Count);
        Assert.All(result.Squares, square => Assert.False(string.IsNullOrWhiteSpace(square.Description)));

        using var verifyContext = CreateContext();
        var persistedCount = await verifyContext.DiscoveryObjectives.CountAsync(o => o.UserId == userId && o.GridSize == 3);
        Assert.Equal(9, persistedCount);
    }

    [Fact]
    public async Task Handle_ShouldMarkMatchingObjectiveAsAchieved_WhenDiaryEntryMatchesRequirement()
    {
        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var diaryEntryId = Guid.NewGuid();
        var boardId = Guid.NewGuid();

        using (var context = CreateContext())
        {
            var user = new User(userId, "bingo_user", "bingo@example.com", "hash-value");
            var movie = new Movie(movieId, "Short Horror", 1975, new ExternalReference("tmdb", "123"));
            movie.EnrichMetadata(
                runtimeMinutes: 85,
                posterUrl: "http://example.com/poster.jpg",
                genres: new List<Genre> { new Genre(Guid.NewGuid(), 0, "Horror") },
                directors: new List<Director>(),
                actors: new List<Actor>(),
                isDocumentary: false,
                tmdbRating: 7.8,
                language: "English",
                country: "USA");

            var diaryEntry = new DiaryEntry(
                diaryEntryId,
                userId,
                movieId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow),
                8.0m,
                false,
                null);

            var objective = new DiscoveryObjective(
                Guid.NewGuid(),
                userId,
                boardId,
                3, // GridSize
                1, // Row
                1, // Column
                "Genre == 'Horror'",
                "Watch a horror film",
                null,
                null);

            context.Users.Add(user);
            context.Movies.Add(movie);
            context.DiaryEntries.Add(diaryEntry);
            context.DiscoveryObjectives.Add(objective);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        using var actContext = CreateContext();
        var handler = new GetBingoGridQueryHandler(actContext, _loggerMock.Object);
        var result = await handler.Handle(new GetBingoGridQuery(userId, 3, AutoEvaluate: true), CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Squares.Any(square => square.IsCompleted));
        Assert.All(result.Squares.Where(square => square.IsCompleted), square => Assert.NotNull(square.CompletionDate));

        using var verifyContext = CreateContext();
        var achievedObjective = await verifyContext.DiscoveryObjectives.FirstOrDefaultAsync(o => o.UserId == userId && o.IsAchieved);
        Assert.NotNull(achievedObjective);
        Assert.Equal(diaryEntryId, achievedObjective.FulfillingDiaryEntryId);
    }
}

