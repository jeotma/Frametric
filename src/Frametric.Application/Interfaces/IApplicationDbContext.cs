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
    DbSet<TvShow> TvShows { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
