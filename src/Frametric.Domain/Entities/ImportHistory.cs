namespace Frametric.Domain.Entities;

public class ImportHistory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ImportDate { get; private set; }
    public int RowCount { get; private set; }
    public string Status { get; private set; } = null!; // Success, Enriching, Failed
    public string ProviderSource { get; private set; } = null!;
    public string? ErrorMessage { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<DiaryEntry> DiaryEntries { get; private set; } = new List<DiaryEntry>();
    public ICollection<MovieRating> MovieRatings { get; private set; } = new List<MovieRating>();
    public ICollection<WatchlistItem> WatchlistItems { get; private set; } = new List<WatchlistItem>();
    public ICollection<MovieLike> MovieLikes { get; private set; } = new List<MovieLike>();

    private ImportHistory() { } // For EF Core

    public ImportHistory(Guid id, Guid userId, int rowCount, string status, string providerSource)
    {
        Id = id;
        UserId = userId;
        ImportDate = DateTime.UtcNow;
        RowCount = rowCount;
        Status = status;
        ProviderSource = providerSource;
    }

    public void UpdateStatus(string status, string? errorMessage = null)
    {
        Status = status;
        ErrorMessage = errorMessage;
    }

    public void SetRowCount(int rowCount)
    {
        RowCount = rowCount;
    }
}
