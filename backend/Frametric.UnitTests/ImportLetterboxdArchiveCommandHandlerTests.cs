using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Commands.ImportData;
using Frametric.Application.DTOs.Letterboxd;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class ImportLetterboxdArchiveCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FrametricDbContext> _options;
    private readonly Mock<ILetterboxdImporter> _importerMock;
    private readonly Mock<ITmdbEnrichmentTrigger> _triggerMock;

    public ImportLetterboxdArchiveCommandHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<FrametricDbContext>()
            .UseSqlite(_connection)
            .Options;

        using (var context = new FrametricDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _importerMock = new Mock<ILetterboxdImporter>();
        _triggerMock = new Mock<ITmdbEnrichmentTrigger>();
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
    public async Task Handle_ShouldThrowArgumentException_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new ImportLetterboxdArchiveCommandHandler(_importerMock.Object, context, _triggerMock.Object);
        var command = new ImportLetterboxdArchiveCommand(Guid.NewGuid(), new MemoryStream());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldImportSuccessfullyAndTriggerEnrichment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var user = new User(userId, "dan", "dan@example.com", "hash");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var diary = new List<ParsedDiaryDto>
        {
            new ParsedDiaryDto { Name = "Inception", Year = 2010, Date = today, WatchedDate = today, Rating = 4.5m, Rewatch = false }
        };
        var ratings = new List<ParsedRatingDto>
        {
            new ParsedRatingDto { Name = "Inception", Year = 2010, Date = today, Rating = 4.5m }
        };
        var watchlist = new List<ParsedWatchlistItemDto>
        {
            new ParsedWatchlistItemDto { Name = "Interstellar", Year = 2014, Date = today }
        };
        var likes = new List<ParsedLikeDto>
        {
            new ParsedLikeDto { Name = "Inception", Year = 2010, Date = today }
        };
        var watched = new List<ParsedWatchedDto>();
        var exportData = new LetterboxdExportData(diary, ratings, watchlist, likes, watched);

        _importerMock.Setup(i => i.ImportFromZipAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exportData);

        using var actContext = CreateContext();
        var handler = new ImportLetterboxdArchiveCommandHandler(_importerMock.Object, actContext, _triggerMock.Object);
        var command = new ImportLetterboxdArchiveCommand(userId, new MemoryStream());

        // Act
        var importId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, importId);

        using var assertContext = CreateContext();
        var importHistory = await assertContext.ImportHistories.FindAsync(importId);
        Assert.NotNull(importHistory);
        Assert.Equal(userId, importHistory.UserId);
        Assert.Equal(2, importHistory.RowCount);
        Assert.Equal("Enriching", importHistory.Status);
        Assert.Equal("Letterboxd", importHistory.ProviderSource);

        // Movies Inception and Interstellar should be created in context
        var inception = await assertContext.Movies.FirstOrDefaultAsync(m => m.Title == "Inception" && m.ReleaseYear == 2010);
        Assert.NotNull(inception);
        var interstellar = await assertContext.Movies.FirstOrDefaultAsync(m => m.Title == "Interstellar" && m.ReleaseYear == 2014);
        Assert.NotNull(interstellar);

        // Verify dependent entities
        var hasDiary = await assertContext.DiaryEntries.AnyAsync(de => de.ImportHistoryId == importId && de.MovieId == inception.Id);
        Assert.True(hasDiary);

        var hasRating = await assertContext.MovieRatings.AnyAsync(mr => mr.ImportHistoryId == importId && mr.MovieId == inception.Id);
        Assert.True(hasRating);

        var hasWatchlist = await assertContext.WatchlistItems.AnyAsync(wi => wi.ImportHistoryId == importId && wi.MovieId == interstellar.Id);
        Assert.True(hasWatchlist);

        var hasLike = await assertContext.MovieLikes.AnyAsync(ml => ml.ImportHistoryId == importId && ml.MovieId == inception.Id);
        Assert.True(hasLike);

        // Verify enrichment background job was triggered
        _triggerMock.Verify(t => t.TriggerEnrichment(), Times.Once);
    }
}
