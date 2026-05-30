namespace Frametric.Domain.Entities;

public class MovieLike
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MovieId { get; private set; }
    public Guid? ImportHistoryId { get; private set; }
    public DateOnly DateLiked { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;
    public ImportHistory? ImportHistory { get; private set; }

    private MovieLike() { }

    public MovieLike(Guid id, Guid userId, Guid movieId, DateOnly dateLiked, Guid? importHistoryId = null)
    {
        Id = id;
        UserId = userId;
        MovieId = movieId;
        DateLiked = dateLiked;
        ImportHistoryId = importHistoryId;
    }
}
