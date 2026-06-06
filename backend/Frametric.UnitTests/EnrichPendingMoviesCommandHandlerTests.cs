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

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenNoPendingMoviesExist()
    {
        // Arrange
        using var actContext = CreateContext();
        var handler = new EnrichPendingMoviesCommandHandler(actContext, _tmdbServiceMock.Object, _omdbServiceMock.Object);
        var command = new EnrichPendingMoviesCommand(10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Handle_ShouldConvertMovieToTvShow_WhenTvShowAlreadyExists()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var movie = new Movie(movieId, "Existing TV Show", 2010, new ExternalReference("letterboxd", "444"));
            context.Movies.Add(movie);
            
            var existingTv = new TvShow(Guid.NewGuid(), "Existing TV Show", 2010, 99999, null, false);
            context.TvShows.Add(existingTv);

            await context.SaveChangesAsync();
        }

        var tmdbDto = new TmdbMovieResultDto(
            TmdbId: 99999,
            RuntimeMinutes: 45,
            PosterUrl: "/existingtv.jpg",
            Genres: new List<TmdbGenreDto>(),
            Directors: new List<TmdbPersonDto>(),
            Actors: new List<TmdbPersonDto>(),
            IsTvShow: true,
            Title: "Existing TV Show",
            FirstAirYear: 2010
        );

        _tmdbServiceMock.Setup(s => s.SearchAndGetMovieDetailsAsync("Existing TV Show", 2010, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbDto);

        using var actContext = CreateContext();
        var handler = new EnrichPendingMoviesCommandHandler(actContext, _tmdbServiceMock.Object, _omdbServiceMock.Object);
        var command = new EnrichPendingMoviesCommand(10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result);
        using var assertContext = CreateContext();
        var movieExists = await assertContext.Movies.AnyAsync(m => m.Id == movieId);
        Assert.False(movieExists);
    }

    [Fact]
    public async Task Handle_ShouldEnrichMovieWithOmdbDataAndClippedLengthsAndLocalEntityLookups()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var movie = new Movie(movieId, "Inception", 2010, new ExternalReference("letterboxd", "333"));
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
        }

        var newMovieId = Guid.NewGuid();
        using (var context = CreateContext())
        {
            var movie = new Movie(newMovieId, "The Dark Knight", 2008, new ExternalReference("letterboxd", "555"));
            // Seed existing Genre, Director, Actor in DB
            var genre = new Genre(Guid.NewGuid(), 28, "Action");
            var director = new Director(Guid.NewGuid(), 525, "Christopher Nolan");
            var actor = new Actor(Guid.NewGuid(), 6193, "Leonardo DiCaprio");
            
            context.Movies.Add(movie);
            context.Genres.Add(genre);
            context.Directors.Add(director);
            context.Actors.Add(actor);
            await context.SaveChangesAsync();
        }

        var tmdbDto = new TmdbMovieResultDto(
            TmdbId: 155,
            RuntimeMinutes: 152,
            PosterUrl: "/tdk.jpg",
            Genres: new List<TmdbGenreDto> { new TmdbGenreDto(28, "Action") },
            Directors: new List<TmdbPersonDto> { new TmdbPersonDto(525, "Christopher Nolan") },
            Actors: new List<TmdbPersonDto> { new TmdbPersonDto(6193, "Leonardo DiCaprio") },
            IsTvShow: false,
            ImdbId: "tt0468569",
            ReleaseDate: "2008-07-18",
            Overview: new string('O', 5000),
            StreamingProviders: new string('P', 2000),
            Keywords: new string('K', 5000)
        );

        _tmdbServiceMock.Setup(s => s.SearchAndGetMovieDetailsAsync("The Dark Knight", 2008, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbDto);

        var omdbRatings = new OmdbRatingsDto(
            ImdbRating: 9.0,
            RottenTomatoesRating: 9.4,
            MetacriticRating: 8.4,
            Writers: new string('W', 1500),
            Awards: new string('A', 1500),
            BoxOffice: new string('B', 200),
            Language: new string('L', 200),
            Country: new string('C', 300),
            Rated: new string('R', 100)
        );

        _omdbServiceMock.Setup(s => s.GetMovieRatingsAsync("tt0468569", It.IsAny<CancellationToken>()))
            .ReturnsAsync(omdbRatings);

        using var actContext = CreateContext();
        var handler = new EnrichPendingMoviesCommandHandler(actContext, _tmdbServiceMock.Object, _omdbServiceMock.Object);
        var command = new EnrichPendingMoviesCommand(10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result);
        using var assertContext = CreateContext();
        var enriched = await assertContext.Movies
            .Include(m => m.Genres)
            .Include(m => m.Directors)
            .Include(m => m.Actors)
            .FirstOrDefaultAsync(m => m.Id == newMovieId);

        Assert.NotNull(enriched);
        Assert.Equal(152, enriched.RuntimeMinutes);
        
        // Local genre/director/actor lookup assertion
        Assert.Single(enriched.Genres);
        Assert.Single(enriched.Directors);
        Assert.Single(enriched.Actors);

        // String truncation assertions
        Assert.Equal(4000, enriched.Overview?.Length);
        Assert.Equal(1000, enriched.StreamingProviders?.Length);
        Assert.Equal(4000, enriched.Keywords?.Length);
        Assert.Equal(1000, enriched.Writers?.Length);
        Assert.Equal(1000, enriched.Awards?.Length);
        Assert.Equal(100, enriched.BoxOffice?.Length);
        Assert.Equal(100, enriched.Language?.Length);
        Assert.Equal(200, enriched.Country?.Length);
        Assert.Equal(50, enriched.Certification?.Length);

        // Release date and score calculations
        Assert.Equal(new DateOnly(2008, 7, 18), enriched.ReleaseDate);
        Assert.NotNull(enriched.CustomAverageRating);
    }
}

