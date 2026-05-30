namespace Frametric.Domain.Entities;

public class Genre
{
    public Guid Id { get; private set; }
    public int TmdbId { get; private set; }
    public string Name { get; private set; }

    // Navigation property
    public ICollection<Movie> Movies { get; private set; } = new List<Movie>();

    private Genre() { } // EF Core

    public Genre(Guid id, int tmdbId, string name)
    {
        Id = id;
        TmdbId = tmdbId;
        Name = name;
    }
}
