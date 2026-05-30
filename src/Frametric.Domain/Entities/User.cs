namespace Frametric.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public ICollection<MovieLike> MovieLikes { get; private set; } = new List<MovieLike>();

    private User() { } // For EF Core

    public User(Guid id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }
}
