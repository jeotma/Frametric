using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Commands.EnrichMovies;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.Enums;
using Frametric.Domain.ValueObjects;
using Frametric.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class EnrichPendingMoviesCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FrametricDbContext> _options;
    private readonly Mock<ITmdbService> _tmdbServiceMock;
    private readonly Mock<IOmdbService> _omdbServiceMock;

    public EnrichPendingMoviesCommandHandlerTests()
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

        _tmdbServiceMock = new Mock<ITmdbService>();
        _omdbServiceMock = new Mock<IOmdbService>();
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
    public async Task Handle_ShouldMarkMovieAsNotFound_WhenTmdbReturnsNull()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var movie = new Movie(movieId, "NonExistentMovie", 2025, new ExternalReference("letterboxd", "111"));
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
        }

        _tmdbServiceMock.Setup(s => s.SearchAndGetMovieDetailsAsync("NonExistentMovie", 2025, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbMovieResultDto?)null);

        using var actContext = CreateContext();
        var handler = new EnrichPendingMoviesCommandHandler(actContext, _tmdbServiceMock.Object, _omdbServiceMock.Object);
        var command = new EnrichPendingMoviesCommand(10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result); // No successful enrichments
        using var assertContext = CreateContext();
        var updatedMovie = await assertContext.Movies.FindAsync(movieId);
        Assert.NotNull(updatedMovie);
        Assert.Equal(EnrichmentStatus.NotFound, updatedMovie.EnrichmentStatus);
    }

    [Fact]
    public async Task Handle_ShouldConvertMovieToTvShow_WhenTmdbReturnsIsTvShow()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var movie = new Movie(movieId, "Breaking Bad", 2008, new ExternalReference("letterboxd", "222"));
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
        }

        var tmdbDto = new TmdbMovieResultDto(
            TmdbId: 1399,
            RuntimeMinutes: 49,
            PosterUrl: "/breakingbad.jpg",
            Genres: new List<TmdbGenreDto> { new TmdbGenreDto(80, "Crime") },
            Directors: new List<TmdbPersonDto>(),
            Actors: new List<TmdbPersonDto>(),
            IsTvShow: true,
            Title: "Breaking Bad",
            FirstAirYear: 2008
        );

        _tmdbServiceMock.Setup(s => s.SearchAndGetMovieDetailsAsync("Breaking Bad", 2008, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbDto);

        using var actContext = CreateContext();
        var handler = new EnrichPendingMoviesCommandHandler(actContext, _tmdbServiceMock.Object, _omdbServiceMock.Object);
        var command = new EnrichPendingMoviesCommand(10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result);

        using var assertContext = CreateContext();
        // Movie should be deleted/removed
        var movieExists = await assertContext.Movies.AnyAsync(m => m.Id == movieId);
        Assert.False(movieExists);

        // TV Show should be created
        var tvShow = await assertContext.TvShows.FirstOrDefaultAsync(t => t.TmdbId == 1399);
        Assert.NotNull(tvShow);
        Assert.Equal("Breaking Bad", tvShow.Title);
        Assert.Equal(2008, tvShow.FirstAirYear);
        Assert.Equal("/breakingbad.jpg", tvShow.PosterUrl);
    }

    [Fact]
    public async Task Handle_ShouldEnrichMovieMetadataAndAddGenresActorsDirectors_WhenTmdbReturnsValidMovie()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var movie = new Movie(movieId, "Inception", 2010, new ExternalReference("letterboxd", "333"));
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
        }

        var tmdbDto = new TmdbMovieResultDto(
            TmdbId: 27205,
            RuntimeMinutes: 148,
            PosterUrl: "/inception.jpg",
            Genres: new List<TmdbGenreDto> { new TmdbGenreDto(28, "Action"), new TmdbGenreDto(878, "Sci-Fi") },
            Directors: new List<TmdbPersonDto> { new TmdbPersonDto(525, "Christopher Nolan") },
            Actors: new List<TmdbPersonDto> { new TmdbPersonDto(6193, "Leonardo DiCaprio") },
            IsTvShow: false
        );

        _tmdbServiceMock.Setup(s => s.SearchAndGetMovieDetailsAsync("Inception", 2010, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbDto);

        using var actContext = CreateContext();
        var handler = new EnrichPendingMoviesCommandHandler(actContext, _tmdbServiceMock.Object, _omdbServiceMock.Object);
        var command = new EnrichPendingMoviesCommand(10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result);

        using var assertContext = CreateContext();
        var enrichedMovie = await assertContext.Movies
            .Include(m => m.Genres)
            .Include(m => m.Directors)
            .Include(m => m.Actors)
            .FirstOrDefaultAsync(m => m.Id == movieId);

        Assert.NotNull(enrichedMovie);
        Assert.Equal(EnrichmentStatus.Completed, enrichedMovie.EnrichmentStatus);
        Assert.Equal(148, enrichedMovie.RuntimeMinutes);
        Assert.Equal("/inception.jpg", enrichedMovie.PosterUrl);
        
        // Assert Genres
        Assert.Equal(2, enrichedMovie.Genres.Count);
        Assert.Contains(enrichedMovie.Genres, g => g.Name == "Action" && g.TmdbId == 28);
        Assert.Contains(enrichedMovie.Genres, g => g.Name == "Sci-Fi" && g.TmdbId == 878);

        // Assert Directors
        Assert.Single(enrichedMovie.Directors);
        Assert.Equal("Christopher Nolan", enrichedMovie.Directors.First().Name);

        // Assert Actors
        Assert.Single(enrichedMovie.Actors);
        Assert.Equal("Leonardo DiCaprio", enrichedMovie.Actors.First().Name);
    }
}
