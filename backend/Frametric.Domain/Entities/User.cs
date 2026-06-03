namespace Frametric.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public ICollection<MovieLike> MovieLikes { get; private set; } = new List<MovieLike>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<ImportHistory> ImportHistories { get; private set; } = new List<ImportHistory>();

    private User() { } // For EF Core

    public User(Guid id, string username, string email, string passwordHash)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }
}
