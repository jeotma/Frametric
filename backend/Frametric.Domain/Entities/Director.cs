namespace Frametric.Domain.Entities;

public class Director
{
    public Guid Id { get; private set; }
    public int TmdbId { get; private set; }
    public string Name { get; private set; } = null!;

    // Navigation property
    public ICollection<Movie> Movies { get; private set; } = new List<Movie>();

    private Director() { } // EF Core

    public Director(Guid id, int tmdbId, string name)
    {
        Id = id;
        TmdbId = tmdbId;
        Name = name;
    }
}
