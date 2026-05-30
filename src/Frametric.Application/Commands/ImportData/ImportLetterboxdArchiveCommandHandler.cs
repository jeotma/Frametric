using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.ImportData;

public class ImportLetterboxdArchiveCommandHandler : IRequestHandler<ImportLetterboxdArchiveCommand, bool>
{
    private readonly ILetterboxdImporter _importer;
    private readonly IApplicationDbContext _context;

    public ImportLetterboxdArchiveCommandHandler(ILetterboxdImporter importer, IApplicationDbContext context)
    {
        _importer = importer;
        _context = context;
    }

    public async Task<bool> Handle(ImportLetterboxdArchiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) throw new ArgumentException("User not found");

        var exportData = await _importer.ImportFromZipAsync(request.ZipStream, cancellationToken);

        string GenerateMovieKey(string title, int? year)
        {
            var cleanTitle = title?.Trim().ToLowerInvariant() ?? "unknown";
            return $"{cleanTitle}-{year ?? 0}";
        }

        // Deduplication & Movie Creation
        var allKeys = exportData.DiaryEntries.Select(x => GenerateMovieKey(x.Name, x.Year))
            .Union(exportData.Ratings.Select(x => GenerateMovieKey(x.Name, x.Year)))
            .Union(exportData.Watchlist.Select(x => GenerateMovieKey(x.Name, x.Year)))
            .Union(exportData.Likes.Select(x => GenerateMovieKey(x.Name, x.Year)))
            .Distinct()
            .ToList();

        var existingMovies = await _context.Movies
            .Where(m => m.ExternalReference.Source == "Letterboxd" && allKeys.Contains(m.ExternalReference.ExternalId))
            .ToDictionaryAsync(m => m.ExternalReference.ExternalId, m => m, cancellationToken);

        var moviesByKey = new Dictionary<string, Movie>(existingMovies);

        // Helper to get or create Movie
        Movie GetOrCreateMovie(string title, int? year)
        {
            var key = GenerateMovieKey(title, year);
            if (moviesByKey.TryGetValue(key, out var existing)) return existing;
            
            var externalRef = new ExternalReference("Letterboxd", key);
            var newMovie = new Movie(Guid.NewGuid(), title, year, externalRef);
            
            moviesByKey[key] = newMovie;
            _context.Movies.Add(newMovie);
            return newMovie;
        }

        // Process Diary
        foreach (var entry in exportData.DiaryEntries)
        {
            var movie = GetOrCreateMovie(entry.Name, entry.Year);
            var diaryEntry = new DiaryEntry(Guid.NewGuid(), user.Id, movie.Id, entry.Date, entry.WatchedDate, entry.Rating, entry.Rewatch, entry.Tags);
            _context.DiaryEntries.Add(diaryEntry);
        }

        // Process Ratings
        foreach (var rating in exportData.Ratings)
        {
            var movie = GetOrCreateMovie(rating.Name, rating.Year);
            var movieRating = new MovieRating(Guid.NewGuid(), user.Id, movie.Id, rating.Date, rating.Rating);
            _context.MovieRatings.Add(movieRating);
        }

        // Process Watchlist
        foreach (var wItem in exportData.Watchlist)
        {
            var movie = GetOrCreateMovie(wItem.Name, wItem.Year);
            var watchlistItem = new WatchlistItem(Guid.NewGuid(), user.Id, movie.Id, wItem.Date);
            _context.WatchlistItems.Add(watchlistItem);
        }

        // Process Likes
        foreach (var like in exportData.Likes)
        {
            var movie = GetOrCreateMovie(like.Name, like.Year);
            var movieLike = new MovieLike(Guid.NewGuid(), user.Id, movie.Id, like.Date);
            _context.MovieLikes.Add(movieLike);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
