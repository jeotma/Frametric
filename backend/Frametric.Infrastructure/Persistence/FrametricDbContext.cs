using System.Reflection;
using Frametric.Application.Interfaces;
using Frametric.Domain.Discovery.Entities;
using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Infrastructure.Persistence;

public class FrametricDbContext : DbContext, IApplicationDbContext
{
    public FrametricDbContext(DbContextOptions<FrametricDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Director> Directors => Set<Director>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<MovieRating> MovieRatings => Set<MovieRating>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<MovieLike> MovieLikes => Set<MovieLike>();
    public DbSet<WatchedMovie> WatchedMovies => Set<WatchedMovie>();
    public DbSet<TvShow> TvShows => Set<TvShow>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ImportHistory> ImportHistories => Set<ImportHistory>();
    public DbSet<DiscoveryObjective> DiscoveryObjectives => Set<DiscoveryObjective>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
