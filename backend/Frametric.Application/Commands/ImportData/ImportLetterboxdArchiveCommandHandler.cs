using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.ImportData;

public class ImportLetterboxdArchiveCommandHandler : IRequestHandler<ImportLetterboxdArchiveCommand, Guid>
{
    private readonly ILetterboxdImporter _importer;
    private readonly IApplicationDbContext _context;
    private readonly ITmdbEnrichmentTrigger _trigger;

    public ImportLetterboxdArchiveCommandHandler(ILetterboxdImporter importer, IApplicationDbContext context, ITmdbEnrichmentTrigger trigger)
    {
        _importer = importer;
        _context = context;
        _trigger = trigger;
    }

    public async Task<Guid> Handle(ImportLetterboxdArchiveCommand request, CancellationToken cancellationToken)
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
            .Union(exportData.Watched.Select(x => GenerateMovieKey(x.Name, x.Year)))
            .Distinct()
            .ToList();

        var existingMovies = await _context.Movies
            .Where(m => m.ExternalReference.Source == "Letterboxd" && allKeys.Contains(m.ExternalReference.ExternalId))
            .ToDictionaryAsync(m => m.ExternalReference.ExternalId, m => m, cancellationToken);

        var uniqueFilmsCount = allKeys.Count;
        bool needsEnrichment = uniqueFilmsCount > existingMovies.Count || 
                               existingMovies.Values.Any(m => m.EnrichmentStatus == Frametric.Domain.Enums.EnrichmentStatus.Pending);

        var importStatus = needsEnrichment ? "Enriching" : "Completed";

        var importHistory = new ImportHistory(Guid.NewGuid(), user.Id, uniqueFilmsCount, importStatus, "Letterboxd");
        _context.ImportHistories.Add(importHistory);

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
            var normalizedRating = entry.Rating.HasValue ? entry.Rating.Value : (decimal?)null;
            var diaryEntry = new DiaryEntry(Guid.NewGuid(), user.Id, movie.Id, entry.Date, entry.WatchedDate, normalizedRating, entry.Rewatch, entry.Tags, importHistory.Id);
            _context.DiaryEntries.Add(diaryEntry);
        }

        // Process Ratings
        foreach (var rating in exportData.Ratings)
        {
            var movie = GetOrCreateMovie(rating.Name, rating.Year);
            var normalizedRating = rating.Rating;
            var movieRating = new MovieRating(Guid.NewGuid(), user.Id, movie.Id, rating.Date, normalizedRating, importHistory.Id);
            _context.MovieRatings.Add(movieRating);
        }

        // Process Watchlist
        foreach (var wItem in exportData.Watchlist)
        {
            var movie = GetOrCreateMovie(wItem.Name, wItem.Year);
            var watchlistItem = new WatchlistItem(Guid.NewGuid(), user.Id, movie.Id, wItem.Date, importHistory.Id);
            _context.WatchlistItems.Add(watchlistItem);
        }

        // Process Likes
        foreach (var like in exportData.Likes)
        {
            var movie = GetOrCreateMovie(like.Name, like.Year);
            var movieLike = new MovieLike(Guid.NewGuid(), user.Id, movie.Id, like.Date, importHistory.Id);
            _context.MovieLikes.Add(movieLike);
        }

        // Process Watched
        foreach (var watchedItem in exportData.Watched)
        {
            var movie = GetOrCreateMovie(watchedItem.Name, watchedItem.Year);
            var watchedMovie = new WatchedMovie(Guid.NewGuid(), user.Id, movie.Id, watchedItem.Date, importHistory.Id);
            _context.WatchedMovies.Add(watchedMovie);
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        // Disparar el enriquecimiento en background tras una importación exitosa
        _trigger.TriggerEnrichment();
        
        return importHistory.Id;
    }
}
