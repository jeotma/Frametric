using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Movie> Movies { get; }
    DbSet<Genre> Genres { get; }
    DbSet<Director> Directors { get; }
    DbSet<Actor> Actors { get; }
    DbSet<DiaryEntry> DiaryEntries { get; }
    DbSet<MovieRating> MovieRatings { get; }
    DbSet<WatchlistItem> WatchlistItems { get; }
    DbSet<MovieLike> MovieLikes { get; }
    DbSet<WatchedMovie> WatchedMovies { get; }
    DbSet<TvShow> TvShows { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ImportHistory> ImportHistories { get; }
    DbSet<Frametric.Domain.Discovery.Entities.DiscoveryObjective> DiscoveryObjectives { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
