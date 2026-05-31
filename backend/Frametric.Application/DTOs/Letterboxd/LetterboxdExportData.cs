namespace Frametric.Application.DTOs.Letterboxd;

public class LetterboxdExportData
{
    public IReadOnlyCollection<ParsedDiaryDto> DiaryEntries { get; }
    public IReadOnlyCollection<ParsedRatingDto> Ratings { get; }
    public IReadOnlyCollection<ParsedWatchlistItemDto> Watchlist { get; }
    public IReadOnlyCollection<ParsedLikeDto> Likes { get; }
    public IReadOnlyCollection<ParsedWatchedDto> Watched { get; }

    public LetterboxdExportData(
        IEnumerable<ParsedDiaryDto> diaryEntries,
        IEnumerable<ParsedRatingDto> ratings,
        IEnumerable<ParsedWatchlistItemDto> watchlist,
        IEnumerable<ParsedLikeDto> likes,
        IEnumerable<ParsedWatchedDto> watched)
    {
        DiaryEntries = diaryEntries.ToList().AsReadOnly();
        Ratings = ratings.ToList().AsReadOnly();
        Watchlist = watchlist.ToList().AsReadOnly();
        Likes = likes.ToList().AsReadOnly();
        Watched = watched.ToList().AsReadOnly();
    }
}

public class ParsedWatchedDto
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string LetterboxdUri { get; set; } = string.Empty;
}
