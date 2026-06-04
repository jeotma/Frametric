namespace Frametric.Domain.Entities;

public class MovieRating
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MovieId { get; private set; }
    public Guid? ImportHistoryId { get; private set; }
    public DateOnly DateRated { get; private set; }
    public decimal Score { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;
    public ImportHistory? ImportHistory { get; private set; }

    private MovieRating() { }

    public MovieRating(Guid id, Guid userId, Guid movieId, DateOnly dateRated, decimal score, Guid? importHistoryId = null)
    {
        Id = id;
        UserId = userId;
        MovieId = movieId;
        DateRated = dateRated;
        Score = score;
        ImportHistoryId = importHistoryId;
    }

    public void UpdateScore(decimal newScore)
    {
        Score = newScore;
    }
}
