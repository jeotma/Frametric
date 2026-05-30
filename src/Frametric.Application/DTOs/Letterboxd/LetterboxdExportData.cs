namespace Frametric.Application.DTOs.Letterboxd;

public class LetterboxdExportData
{
    public IReadOnlyCollection<ParsedDiaryDto> DiaryEntries { get; }
    public IReadOnlyCollection<ParsedRatingDto> Ratings { get; }
    public IReadOnlyCollection<ParsedWatchlistItemDto> Watchlist { get; }
    public IReadOnlyCollection<ParsedLikeDto> Likes { get; }

    public LetterboxdExportData(
        IEnumerable<ParsedDiaryDto> diaryEntries,
        IEnumerable<ParsedRatingDto> ratings,
        IEnumerable<ParsedWatchlistItemDto> watchlist,
        IEnumerable<ParsedLikeDto> likes)
    {
        DiaryEntries = diaryEntries.ToList().AsReadOnly();
        Ratings = ratings.ToList().AsReadOnly();
        Watchlist = watchlist.ToList().AsReadOnly();
        Likes = likes.ToList().AsReadOnly();
    }
}
