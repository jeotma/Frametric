using System.Reflection;
using Frametric.Application.Interfaces;
using Frametric.Domain.Discovery.Entities;
using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Infrastructure.Persistence;

public class FrametricDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public FrametricDbContext(
        DbContextOptions<FrametricDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
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
    public DbSet<CustomList> CustomLists => Set<CustomList>();
    public DbSet<CustomListItem> CustomListItems => Set<CustomListItem>();
    public DbSet<DiscoveryObjective> DiscoveryObjectives => Set<DiscoveryObjective>();
    public DbSet<EntityRevision> EntityRevisions => Set<EntityRevision>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var revisions = new List<EntityRevision>();
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => (e.State == EntityState.Modified || e.State == EntityState.Deleted) && 
                        (e.Entity is Movie || e.Entity is Actor || e.Entity is Director || e.Entity is User))
            .ToList();

        var userId = _currentUserService?.UserId?.ToString() ?? "System/Database";

        foreach (var entry in modifiedEntries)
        {
            var idProperty = entry.Property("Id");
            var entityId = idProperty != null && idProperty.CurrentValue != null ? (Guid)idProperty.CurrentValue : Guid.Empty;

            var valuesDict = new Dictionary<string, object?>();
            foreach (var prop in entry.OriginalValues.Properties)
            {
                valuesDict[prop.Name] = entry.OriginalValues[prop];
            }

            var stateJson = System.Text.Json.JsonSerializer.Serialize(valuesDict);
            var entityType = entry.Entity.GetType().Name;
            if (entityType.Contains('_'))
            {
                entityType = entityType.Split('_')[0];
            }

            var revision = new EntityRevision(Guid.NewGuid(), entityType, entityId, userId, stateJson);
            revisions.Add(revision);
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (revisions.Any())
        {
            EntityRevisions.AddRange(revisions);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
