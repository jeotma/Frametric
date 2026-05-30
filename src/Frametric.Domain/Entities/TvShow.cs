namespace Frametric.Domain.Entities;

public class TvShow
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int? FirstAirYear { get; private set; }
    public int TmdbId { get; private set; }
    public string? PosterUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Ef Core constructor
    protected TvShow() { }

    public TvShow(Guid id, string title, int? firstAirYear, int tmdbId, string? posterUrl)
    {
        Id = id;
        Title = title;
        FirstAirYear = firstAirYear;
        TmdbId = tmdbId;
        PosterUrl = posterUrl;
        CreatedAt = DateTime.UtcNow;
    }
}
