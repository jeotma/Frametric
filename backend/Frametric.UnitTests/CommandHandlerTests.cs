using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Commands.Auth;
using Frametric.Application.Commands.Imports;
using Frametric.Application.Commands.EntityDetails;
using Frametric.Application.Queries.Imports;
using Frametric.Domain.Enums;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.ValueObjects;
using Frametric.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class CommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FrametricDbContext> _options;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public CommandHandlerTests()
    {
        // Open connection for Sqlite in-memory database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = OFF;";
            cmd.ExecuteNonQuery();
        }

        _options = new DbContextOptionsBuilder<FrametricDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Ensure database schema is created
        using (var context = new FrametricDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
    }

    private FrametricDbContext CreateContext()
    {
        return new FrametricDbContext(_options);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    [Fact]
    public async Task RegisterUserCommandHandler_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        _passwordHasherMock.Setup(h => h.Hash("securePassword123"))
            .Returns("hashedPasswordPart.saltPart");

        using var context = CreateContext();
        var handler = new RegisterUserCommandHandler(context, _passwordHasherMock.Object);
        var command = new RegisterUserCommand("alice", "alice@example.com", "securePassword123");

        // Act
        var userId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, userId);
        
        using var assertContext = CreateContext();
        var user = await assertContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(user);
        Assert.Equal("alice", user.Username);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("hashedPasswordPart.saltPart", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterUserCommandHandler_ShouldThrowException_WhenUserAlreadyExists()
    {
        // Arrange
        using (var context = CreateContext())
        {
            var existingUser = new User(Guid.NewGuid(), "alice", "alice@example.com", "hash");
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();
        }

        using var actContext = CreateContext();
        var handler = new RegisterUserCommandHandler(actContext, _passwordHasherMock.Object);
        var command = new RegisterUserCommand("alice", "alice_new@example.com", "password");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("A user with this username or email already exists.", ex.Message);
    }

    [Fact]
    public async Task LoginUserCommandHandler_ShouldLoginSuccessfully_WithCorrectCredentials()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var user = new User(userId, "bob", "bob@example.com", "hashedPassword");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        _passwordHasherMock.Setup(h => h.Verify("correctPassword", "hashedPassword"))
            .Returns(true);
        _tokenGeneratorMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("fakeAccessToken");
        _tokenGeneratorMock.Setup(t => t.GenerateRefreshToken())
            .Returns("fakeRefreshToken");

        using var actContext = CreateContext();
        var handler = new LoginUserCommandHandler(actContext, _tokenGeneratorMock.Object, _passwordHasherMock.Object);
        var command = new LoginUserCommand("bob@example.com", "correctPassword");

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("fakeAccessToken", response.AccessToken);
        Assert.Equal("fakeRefreshToken", response.RefreshToken);

        using var assertContext = CreateContext();
        var savedToken = await assertContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId);
        Assert.NotNull(savedToken);
        Assert.Equal("fakeRefreshToken", savedToken.Token);
    }

    [Fact]
    public async Task LoginUserCommandHandler_ShouldThrowException_WithIncorrectPassword()
    {
        // Arrange
        using (var context = CreateContext())
        {
            var user = new User(Guid.NewGuid(), "bob", "bob@example.com", "hashedPassword");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        _passwordHasherMock.Setup(h => h.Verify("wrongPassword", "hashedPassword"))
            .Returns(false);

        using var actContext = CreateContext();
        var handler = new LoginUserCommandHandler(actContext, _tokenGeneratorMock.Object, _passwordHasherMock.Object);
        var command = new LoginUserCommand("bob@example.com", "wrongPassword");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_ShouldRefreshTokensSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var user = new User(userId, "charlie", "charlie@example.com", "hash");
            var activeToken = new RefreshToken(Guid.NewGuid(), userId, "validRefreshToken", DateTime.UtcNow.AddDays(1));
            context.Users.Add(user);
            context.RefreshTokens.Add(activeToken);
            await context.SaveChangesAsync();
        }

        _tokenGeneratorMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("newAccessToken");
        _tokenGeneratorMock.Setup(t => t.GenerateRefreshToken())
            .Returns("newRefreshToken");

        using var actContext = CreateContext();
        var handler = new RefreshTokenCommandHandler(actContext, _tokenGeneratorMock.Object);
        var command = new RefreshTokenCommand("validRefreshToken");

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("newAccessToken", response.AccessToken);
        Assert.Equal("newRefreshToken", response.RefreshToken);

        using var assertContext = CreateContext();
        // Verify old token is revoked
        var updatedOldToken = await assertContext.RefreshTokens.FirstAsync(rt => rt.Token == "validRefreshToken");
        Assert.NotNull(updatedOldToken.RevokedAt);
        Assert.False(updatedOldToken.IsActive);

        // Verify new token is added
        var savedNewToken = await assertContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "newRefreshToken");
        Assert.NotNull(savedNewToken);
        Assert.Equal(userId, savedNewToken.UserId);
    }

    [Fact]
    public async Task DeleteImportCommandHandler_ShouldDeleteImportAndCascadeDependentData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var importId = Guid.NewGuid();

        using (var context = CreateContext())
        {
            var user = new User(userId, "dan", "dan@example.com", "hash");
            context.Users.Add(user);

            var importHistory = new ImportHistory(importId, userId, 10, "Success", "Letterboxd");
            context.ImportHistories.Add(importHistory);

            var externalRef = new ExternalReference("tmdb", "12345");
            var movie = new Movie(Guid.NewGuid(), "Inception", 2010, externalRef);
            context.Movies.Add(movie);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Add related logs tied to this import history
            var diaryEntry = new DiaryEntry(Guid.NewGuid(), userId, movie.Id, today, today, 4.5m, false, "Sci-Fi", importId);
            var rating = new MovieRating(Guid.NewGuid(), userId, movie.Id, today, 4.5m, importId);
            var like = new MovieLike(Guid.NewGuid(), userId, movie.Id, today, importId);
            var watchlistItem = new WatchlistItem(Guid.NewGuid(), userId, movie.Id, today, importId);

            context.DiaryEntries.Add(diaryEntry);
            context.MovieRatings.Add(rating);
            context.MovieLikes.Add(like);
            context.WatchlistItems.Add(watchlistItem);

            await context.SaveChangesAsync();
        }

        // Act
        using (var actContext = CreateContext())
        {
            var handler = new DeleteImportCommandHandler(actContext);
            var command = new DeleteImportCommand(userId, importId);
            var result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result);
        }

        // Assert
        using (var assertContext = CreateContext())
        {
            // Verify ImportHistory is deleted
            var exists = await assertContext.ImportHistories.AnyAsync(ih => ih.Id == importId);
            Assert.False(exists);

            // Verify all related dependent logs are cascade deleted
            var diaryExists = await assertContext.DiaryEntries.AnyAsync(de => de.ImportHistoryId == importId);
            Assert.False(diaryExists);

            var ratingExists = await assertContext.MovieRatings.AnyAsync(mr => mr.ImportHistoryId == importId);
            Assert.False(ratingExists);

            var likeExists = await assertContext.MovieLikes.AnyAsync(ml => ml.ImportHistoryId == importId);
            Assert.False(likeExists);

            var watchlistExists = await assertContext.WatchlistItems.AnyAsync(wi => wi.ImportHistoryId == importId);
            Assert.False(watchlistExists);
        }
    }

    [Fact]
    public async Task LogMovieWatchCommandHandler_ShouldLogWatchSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        using (var context = CreateContext())
        {
            var user = new User(userId, "alex", "alex@example.com", "hash");
            var movie = new Movie(movieId, "Goodfellas", 1990, new ExternalReference("tmdb", "111"));
            var systemImport = new ImportHistory(Guid.Empty, userId, 0, "System", "System");
            context.Users.Add(user);
            context.Movies.Add(movie);
            context.ImportHistories.Add(systemImport);
            await context.SaveChangesAsync();
        }

        using var actContext = CreateContext();
        var handler = new LogMovieWatchCommandHandler(actContext);
        var command = new LogMovieWatchCommand(userId, movieId, DateOnly.FromDateTime(DateTime.UtcNow), 4.5, false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        using var assertContext = CreateContext();
        var rating = await assertContext.MovieRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);
        Assert.NotNull(rating);
        Assert.Equal(4.5m, rating.Score);

        var diary = await assertContext.DiaryEntries.FirstOrDefaultAsync(d => d.UserId == userId && d.MovieId == movieId);
        Assert.NotNull(diary);
        Assert.False(diary.IsRewatch);

        var watched = await assertContext.WatchedMovies.FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);
        Assert.NotNull(watched);
    }

    [Fact]
    public async Task MarkImportsCompletedCommandHandler_ShouldMarkEnrichingAsCompleted_WhenNoPendingMovies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var user = new User(userId, "danielle", "danielle@example.com", "hash");
            context.Users.Add(user);

            var movie = new Movie(Guid.NewGuid(), "Inception", 2010, new ExternalReference("tmdb", "123"));
            movie.EnrichMetadata(120, "poster.jpg", new List<Genre>(), new List<Director>(), new List<Actor>(), false);
            context.Movies.Add(movie);

            var enrichingImport = new ImportHistory(Guid.NewGuid(), userId, 5, "Enriching", "Letterboxd");
            context.ImportHistories.Add(enrichingImport);

            await context.SaveChangesAsync();
        }

        using var actContext = CreateContext();
        var handler = new MarkImportsCompletedCommandHandler(actContext);
        var command = new MarkImportsCompletedCommand();

        // Act
        var count = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, count);

        using var assertContext = CreateContext();
        var import = await assertContext.ImportHistories.FirstAsync();
        Assert.Equal("Completed", import.Status);
    }

    [Fact]
    public async Task GetImportHistoryQueryHandler_ShouldReturnHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var user = new User(userId, "felix", "felix@example.com", "hash");
            context.Users.Add(user);

            var import = new ImportHistory(Guid.NewGuid(), userId, 10, "Success", "Letterboxd");
            import.UpdateStatus("Success", "None");
            context.ImportHistories.Add(import);

            await context.SaveChangesAsync();
        }

        using var actContext = CreateContext();
        var handler = new GetImportHistoryQueryHandler(actContext);
        var query = new GetImportHistoryQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Letterboxd", result[0].ProviderSource);
    }
}
