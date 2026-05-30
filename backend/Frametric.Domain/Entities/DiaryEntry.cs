namespace Frametric.Domain.Entities;

public class DiaryEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MovieId { get; private set; }
    public Guid? ImportHistoryId { get; private set; }
    public DateOnly LogDate { get; private set; }
    public DateOnly WatchedDate { get; private set; }
    public decimal? Rating { get; private set; }
    public bool IsRewatch { get; private set; }
    public string? Tags { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;
    public ImportHistory? ImportHistory { get; private set; }

    private DiaryEntry() { }

    public DiaryEntry(Guid id, Guid userId, Guid movieId, DateOnly logDate, DateOnly watchedDate, decimal? rating, bool isRewatch, string? tags, Guid? importHistoryId = null)
    {
        Id = id;
        UserId = userId;
        MovieId = movieId;
        LogDate = logDate;
        WatchedDate = watchedDate;
        Rating = rating;
        IsRewatch = isRewatch;
        Tags = tags;
        ImportHistoryId = importHistoryId;
    }
}
