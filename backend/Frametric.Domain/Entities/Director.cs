namespace Frametric.Domain.Entities;

public class Director
{
    public Guid Id { get; private set; }
    public int TmdbId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? ProfilePath { get; private set; }

    // Navigation property
    public ICollection<Movie> Movies { get; private set; } = new List<Movie>();

    private Director() { } // EF Core

    public Director(Guid id, int tmdbId, string name, string? profilePath = null)
    {
        Id = id;
        TmdbId = tmdbId;
        Name = name;
        ProfilePath = profilePath;
    }

    public void UpdateProfilePath(string? profilePath)
    {
        ProfilePath = profilePath;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name;
    }
}
