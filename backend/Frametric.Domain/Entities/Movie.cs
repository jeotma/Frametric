using Frametric.Domain.Enums;
using Frametric.Domain.ValueObjects;

namespace Frametric.Domain.Entities;

public class Movie
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int? ReleaseYear { get; private set; }
    public int? RuntimeMinutes { get; private set; }
    public string? PosterUrl { get; private set; }
    public EnrichmentStatus EnrichmentStatus { get; private set; }
    public ExternalReference ExternalReference { get; private set; }
    public bool IsDocumentary { get; private set; }

    // Navigation properties
    public ICollection<Genre> Genres { get; private set; } = new List<Genre>();
    public ICollection<Director> Directors { get; private set; } = new List<Director>();
    public ICollection<Actor> Actors { get; private set; } = new List<Actor>();
    public ICollection<DiaryEntry> DiaryEntries { get; private set; } = new List<DiaryEntry>();
    public ICollection<MovieRating> MovieRatings { get; private set; } = new List<MovieRating>();
    public ICollection<WatchlistItem> WatchlistItems { get; private set; } = new List<WatchlistItem>();
    public ICollection<MovieLike> MovieLikes { get; private set; } = new List<MovieLike>();

    private Movie() { } // EF Core

    public Movie(Guid id, string title, int? releaseYear, ExternalReference externalReference)
    {
        Id = id;
        Title = title;
        ReleaseYear = releaseYear;
        ExternalReference = externalReference;
        EnrichmentStatus = EnrichmentStatus.Pending;
    }

    public void EnrichMetadata(int runtimeMinutes, string posterUrl, List<Genre> genres, List<Director> directors, List<Actor> actors, bool isDocumentary)
    {
        RuntimeMinutes = runtimeMinutes;
        PosterUrl = posterUrl;
        IsDocumentary = isDocumentary;
        
        foreach (var genre in genres) Genres.Add(genre);
        foreach (var director in directors) Directors.Add(director);
        foreach (var actor in actors) Actors.Add(actor);

        EnrichmentStatus = EnrichmentStatus.Completed;
    }

    public void MarkEnrichmentFailed()
    {
        EnrichmentStatus = EnrichmentStatus.Failed;
    }

    public void MarkAsNotFound()
    {
        EnrichmentStatus = EnrichmentStatus.NotFound;
    }
}
