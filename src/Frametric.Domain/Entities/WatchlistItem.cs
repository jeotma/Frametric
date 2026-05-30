namespace Frametric.Domain.Entities;

public class WatchlistItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MovieId { get; private set; }
    public DateOnly DateAdded { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;

    private WatchlistItem() { }

    public WatchlistItem(Guid id, Guid userId, Guid movieId, DateOnly dateAdded)
    {
        Id = id;
        UserId = userId;
        MovieId = movieId;
        DateAdded = dateAdded;
    }
}
