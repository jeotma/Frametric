using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.Enums;
using Frametric.Infrastructure.Persistence;
using Frametric.Infrastructure.Queries;
using Frametric.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Frametric.UnitTests;

public class TestDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    public TestDbConnectionFactory(string connectionString) => _connectionString = connectionString;
    public IDbConnection CreateConnection()
    {
        var connection = new Npgsql.NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}

public class QueryIntegrationTests : IClassFixture<PostgresTestFixture>
{
    private readonly PostgresTestFixture _fixture;

    public QueryIntegrationTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AllProjectQueries_ShouldExecuteWithoutPostgresErrors()
    {
        // 1. Arrange - Seed data using EF Core DbContext
        var options = new DbContextOptionsBuilder<FrametricDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        var userId = Guid.NewGuid();
        var partnerUserId = Guid.NewGuid();
        var movieId1 = Guid.NewGuid();
        var movieId2 = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var directorId = Guid.NewGuid();
        
        using (var context = new FrametricDbContext(options))
        {
            var user = new User(userId, "testuser", "test@example.com", "hash");
            var partnerUser = new User(partnerUserId, "partneruser", "partner@example.com", "hash");
            context.Users.AddRange(user, partnerUser);

            var movie1 = new Movie(movieId1, "Inception", 2010, new Frametric.Domain.ValueObjects.ExternalReference("Letterboxd", Guid.NewGuid().ToString()));
            var movie2 = new Movie(movieId2, "Interstellar", 2014, new Frametric.Domain.ValueObjects.ExternalReference("Letterboxd", Guid.NewGuid().ToString()));

            var genreAction = new Genre(Guid.NewGuid(), 28, "Action");
            var genreSciFi = new Genre(Guid.NewGuid(), 878, "Sci-Fi");
            context.Genres.AddRange(genreAction, genreSciFi);

            var directorNolan = new Director(directorId, 525, "Christopher Nolan");
            context.Directors.Add(directorNolan);

            var actorDiCaprio = new Actor(actorId, 6193, "Leonardo DiCaprio");
            var actorMcConaughey = new Actor(Guid.NewGuid(), 10297, "Matthew McConaughey");
            context.Actors.AddRange(actorDiCaprio, actorMcConaughey);

            movie1.Genres.Add(genreAction);
            movie1.Genres.Add(genreSciFi);
            movie1.Directors.Add(directorNolan);
            movie1.Actors.Add(actorDiCaprio);
            movie1.EnrichMetadata(148, "url", new List<Genre>(), new List<Director>(), new List<Actor>(), false);

            movie2.Genres.Add(genreSciFi);
            movie2.Directors.Add(directorNolan);
            movie2.Actors.Add(actorMcConaughey);
            movie2.EnrichMetadata(169, "url", new List<Genre>(), new List<Director>(), new List<Actor>(), false);

            context.Movies.AddRange(movie1, movie2);

            var watched1 = new WatchedMovie(Guid.NewGuid(), userId, movieId1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)));
            var watched2 = new WatchedMovie(Guid.NewGuid(), userId, movieId2, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));
            var partnerWatched = new WatchedMovie(Guid.NewGuid(), partnerUserId, movieId2, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));
            context.WatchedMovies.AddRange(watched1, watched2, partnerWatched);

            var rating1 = new MovieRating(Guid.NewGuid(), userId, movieId1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), 9.0m);
            var rating2 = new MovieRating(Guid.NewGuid(), userId, movieId2, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), 8.0m);
            context.MovieRatings.AddRange(rating1, rating2);

            var watchlist1 = new WatchlistItem(Guid.NewGuid(), userId, movieId1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)));
            var partnerWatchlist = new WatchlistItem(Guid.NewGuid(), partnerUserId, movieId2, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)));
            context.WatchlistItems.AddRange(watchlist1, partnerWatchlist);

            var diary1 = new DiaryEntry(Guid.NewGuid(), userId, movieId1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), null, false, null);
            context.DiaryEntries.Add(diary1);

            await context.SaveChangesAsync();
        }

        // 2. Act & Assert - Run queries using Dapper repositories
        var dapperConnectionFactory = new TestDbConnectionFactory(_fixture.ConnectionString);
        var watchedQueries = new WatchedQueriesImpl(dapperConnectionFactory);
        var watchlistQueries = new WatchlistQueriesImpl(dapperConnectionFactory);
        var bonusQueries = new BonusQueriesImpl(dapperConnectionFactory);
        var discoveryQueries = new DiscoveryQueriesImpl(dapperConnectionFactory);
        var detailsQueries = new EntityDetailsQueriesImpl(dapperConnectionFactory);
        var recommendationQueries = new RecommendationQueriesImpl(dapperConnectionFactory);
        var analyticsService = new DapperAnalyticsService(dapperConnectionFactory);

        var filter = new AnalyticsFilterDto { WatchYear = DateTime.UtcNow.Year };

        // --- TEST WATCHED QUERIES ---
        Assert.NotNull(await watchedQueries.GetMoviesAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetDirectorsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetActorsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetMoviesByGenreAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetCastingRepetitionsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetRatingEvolutionAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetGenreStreakAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetLongestWatchedMovieAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetShortestWatchedMovieAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetDirectorActorPairingsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetPrimeTimeStatsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchedQueries.GetGenresWithRatingAsync(userId, filter, CancellationToken.None));

        // --- TEST WATCHLIST QUERIES ---
        Assert.NotNull(await watchlistQueries.GetWatchlistAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetWatchlistDirectorsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetWatchlistActorsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetWatchlistByGenreAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetMostAnticipatedDirectorAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetMostAnticipatedActorAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetGenreProportionWatchlistVsWatchedAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetTotalPendingWatchtimeAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await watchlistQueries.GetOldestPendingMovieAsync(userId, filter, CancellationToken.None));

        // --- TEST BONUS QUERIES ---
        Assert.NotNull(await bonusQueries.GetWeekendWarriorStatsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await bonusQueries.GetHiddenGemsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await bonusQueries.GetWatchlistGraveyardAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await bonusQueries.GetCinematicFatigueExpandedAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await bonusQueries.GetBookendsAsync(userId, filter, CancellationToken.None));
        Assert.NotNull(await bonusQueries.GetMonthlyExtremesAsync(userId, filter, false, CancellationToken.None));
        Assert.NotNull(await bonusQueries.GetTopAndBottomRatedMoviesAsync(userId, filter, CancellationToken.None));
        // GetMostRewatchedMovieAsync can return null if no rewatches matching filter exist, which is fine, we just verify it executes
        await bonusQueries.GetMostRewatchedMovieAsync(userId, filter, CancellationToken.None);
        Assert.NotNull(await bonusQueries.GetBestRookiesAsync(userId, filter, CancellationToken.None));

        // --- TEST DISCOVERY QUERIES ---
        Assert.NotNull(await discoveryQueries.ResolveMovieIdsByTitlesAsync(new[] { "Inception" }, CancellationToken.None));
        Assert.NotNull(await discoveryQueries.GetDiscoveryPoolAsync(userId, DiscoveryDataSourceScope.Hybrid, null, true, null, CancellationToken.None));
        Assert.NotNull(await discoveryQueries.GetDiscoveryPoolAsync(userId, DiscoveryDataSourceScope.MergedWatchlists, null, true, partnerUserId, CancellationToken.None));
        Assert.NotNull(await discoveryQueries.GetUserIdByUsernameAsync("testuser", CancellationToken.None));
        Assert.NotNull(await discoveryQueries.GetAvailableCountriesAsync(CancellationToken.None));
        Assert.NotNull(await discoveryQueries.GetUserTopGenresAsync(userId, CancellationToken.None));

        // --- TEST ENTITY DETAILS QUERIES ---
        Assert.NotNull(await detailsQueries.GetMovieDetailsAsync(userId, movieId1, CancellationToken.None));
        Assert.NotNull(await detailsQueries.GetActorDetailsAsync(userId, actorId, CancellationToken.None));
        Assert.NotNull(await detailsQueries.GetDirectorDetailsAsync(userId, directorId, CancellationToken.None));
        Assert.NotNull(await detailsQueries.SearchEntitiesAsync(userId, "Incept", CancellationToken.None));

        // --- TEST RECOMMENDATION QUERIES ---
        Assert.NotNull(await recommendationQueries.GetWatchedMovieDetailsAsync(userId, CancellationToken.None));
        Assert.NotNull(await recommendationQueries.GetCandidateMoviesAsync(userId, RecommendationScope.Hybrid, null, null, CancellationToken.None));

        // --- TEST DAPPER ANALYTICS SERVICE (TO_CHAR / WEEKLY / MONTHLY ACTIVITY) ---
        Assert.NotNull(await analyticsService.GetMonthlyActivityAsync(userId, DateTime.UtcNow.Year, CancellationToken.None));
    }
}
